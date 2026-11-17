using System.Text;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void StorageOptions_ResolveFullPath_WithRelativePath_CombinesAndCreatesDirectory()
    {
        // Arrange
        string tempBase = Path.Combine(Path.GetTempPath(), $"crossapp_test_{Guid.NewGuid():N}");
        var options = new StorageOptions
        {
            DataPath = "custom_data",
            FileName = "custom_orders.json"
        };

        try
        {
            // Act
            string fullPath = options.ResolveFullPath(tempBase);

            // Assert
            string expectedDir = Path.GetFullPath(Path.Combine(tempBase, "custom_data"));
            string expectedFile = Path.Combine(expectedDir, "custom_orders.json");

            Assert.Equal(expectedFile, fullPath);
            Assert.True(Directory.Exists(expectedDir), "Каталог повинен бути автоматично створений.");
        }
        finally
        {
            if (Directory.Exists(tempBase))
                Directory.Delete(tempBase, recursive: true);
        }
    }

    [Fact]
    public void StorageOptions_ResolveFullPath_WithRootedPath_IgnoresBaseDirectory()
    {
        // Arrange
        string tempRoot = Path.Combine(Path.GetTempPath(), $"crossapp_rooted_{Guid.NewGuid():N}");
        string dummyBase = Path.Combine(Path.GetTempPath(), "should_not_be_used");

        var options = new StorageOptions
        {
            DataPath = tempRoot,
            FileName = "orders.json"
        };

        try
        {
            // Act
            string fullPath = options.ResolveFullPath(dummyBase);

            // Assert
            string expectedFile = Path.Combine(Path.GetFullPath(tempRoot), "orders.json");
            Assert.Equal(expectedFile, fullPath);
            Assert.True(Directory.Exists(tempRoot));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void AppOptions_HasExpectedDefaultValues()
    {
        // Arrange & Act
        var options = new AppOptions();

        // Assert
        Assert.Equal("Development", options.EnvironmentName);
        Assert.Equal("data", options.Storage.DataPath);
        Assert.Equal("orders.json", options.Storage.FileName);
        Assert.Equal("UAH", options.Orders.Currency);
        Assert.Equal("Information", options.Logging.MinimumLevel);
    }

    [Fact]
    public void FileOrderStore_WritesAtomically_AndCleansTmpFile()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"crossapp_atomic_{Guid.NewGuid():N}");
        string filePath = Path.Combine(tempDir, "orders.json");
        string tmpPath = filePath + ".tmp";

        try
        {
            var store = new FileOrderStore(filePath);
            var order = Order.Create("ORD-TEST-01", "CUST-001");
            order.AddLine("P-01", "Цемент М400", 250m, 10);

            // Act
            store.Add(order);

            // Assert
            Assert.True(File.Exists(filePath), "Цільовий файл повинен існувати.");
            Assert.False(File.Exists(tmpPath), "Тимчасовий .tmp файл має бути замінений через File.Move і не залишатися на диску.");

            var reloadedStore = new FileOrderStore(filePath);
            var loadedOrder = reloadedStore.GetById("ORD-TEST-01");
            Assert.NotNull(loadedOrder);
            Assert.Equal(order.CustomerId, loadedOrder.CustomerId);
            Assert.Single(loadedOrder.Lines);
            Assert.Equal(2500m, loadedOrder.Total);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void FileOrderStore_SavesCyrillicWithoutUnicodeEscapes()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"crossapp_cyrillic_{Guid.NewGuid():N}");
        string filePath = Path.Combine(tempDir, "orders.json");

        try
        {
            var store = new FileOrderStore(filePath);
            var order = Order.Create("ORD-CYR-01", "КЛІЄНТ-01");
            order.AddLine("P-01", "Цегла рядова М100 (Червона)", 15.5m, 100);

            // Act
            store.Add(order);

            // Assert
            string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

            // Перевіряємо, що кирилиця збереглася у прямому вигляді, а не у вигляді \u0426...
            Assert.Contains("Цегла рядова М100 (Червона)", jsonContent);
            Assert.DoesNotContain("\\u0426", jsonContent, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\\u", jsonContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void FileOrderStore_DeserializesBothPascalCaseAndCamelCase()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"crossapp_case_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "orders_pascal.json");

        // JSON із властивостями у PascalCase (як у попередніх лабораторних)
        string pascalJson = """
        [
          {
            "Id": "ORD-PASCAL-1",
            "CustomerId": "CUST-001",
            "CreatedAt": "2026-11-17T12:00:00Z",
            "Status": 0,
            "Lines": [
              { "ProductId": "P-01", "ProductName": "Товар 1", "Price": 100.0, "Quantity": 2 }
            ]
          }
        ]
        """;
        File.WriteAllText(filePath, pascalJson);

        try
        {
            // Act
            var store = new FileOrderStore(filePath);
            var orders = store.List();

            // Assert
            Assert.Single(orders);
            Assert.Equal("ORD-PASCAL-1", orders[0].Id);
            Assert.Equal("CUST-001", orders[0].CustomerId);
            Assert.Equal(200.0m, orders[0].Total);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void FileOrderStore_WithCorruptedJson_ThrowsFormatExceptionWithHelpfulMessage()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"crossapp_corrupt_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "corrupted_orders.json");
        File.WriteAllText(filePath, "{ this is not a valid json content ]]]");

        try
        {
            var store = new FileOrderStore(filePath);

            // Act & Assert
            var ex = Assert.Throws<FormatException>(() => store.List());
            Assert.Contains("пошкоджено або має некоректний JSON формат", ex.Message);
            Assert.Contains(filePath, ex.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
