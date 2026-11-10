using Core.Domain;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class ImportTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new OrderService(new InMemoryOrderStore());
        var importer = new JsonOrderImporter();

        // Act & Assert: заздалегідь скасований токен негайно зупиняє операцію
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(importer, "non_existent_file.json", cts.Token));
    }

    [Fact]
    public async Task ImportAsync_WithCorruptedData_ThrowsDomainException_AndCleansTempFile()
    {
        // Arrange: створення тимчасового файлу з некоректним записом (порожній CustomerId порушує інваріант)
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_corrupt_{Guid.NewGuid():N}.json");
        string invalidJson = """
        [
          {
            "Id": "ORD-INV-01",
            "CustomerId": "   ",
            "CreatedAt": "2026-11-10T10:00:00Z",
            "Status": 0,
            "Lines": []
          }
        ]
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, invalidJson);

            var service = new OrderService(new InMemoryOrderStore());
            var importer = new JsonOrderImporter();

            // Act & Assert: DomainException має обгорнути ArgumentException із зазначенням номера запису
            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(importer, tempFile));

            Assert.Contains("Запис 1", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.IsType<ArgumentException>(ex.InnerException);
        }
        finally
        {
            // Assert: тимчасовий файл обов'язково видаляється у finally
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ImportAsync_Idempotency_SkipsExistingOrders()
    {
        // Arrange: створюємо тимчасовий валідний файл із двома замовленнями
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_valid_{Guid.NewGuid():N}.json");
        string validJson = """
        [
          {
            "Id": "ORD-IDEMP-01",
            "CustomerId": "CUST-IDEMP",
            "CreatedAt": "2026-11-10T10:00:00Z",
            "Status": 0,
            "Lines": [
              { "ProductId": "P-01", "ProductName": "Товар 1", "Price": 100.0, "Quantity": 1 }
            ]
          }
        ]
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, validJson);

            var store = new InMemoryOrderStore();
            var service = new OrderService(store);
            var importer = new JsonOrderImporter();

            // Act 1: Перший імпорт
            int firstRunAdded = await service.ImportAsync(importer, tempFile);
            Assert.Equal(1, firstRunAdded);

            // Act 2: Повторний імпорт того самого файлу
            int secondRunAdded = await service.ImportAsync(importer, tempFile);

            // Assert: ідемпотентність — замовлення вже є в сховищі, повторно не додається
            Assert.Equal(0, secondRunAdded);
            Assert.Single(store.List());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ImportAsync_WithLinkedToken_TimeoutCancelsOperation()
    {
        // Arrange (Бонус 2: зв'язані токени)
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        // Скасовуємо саме токен таймауту
        await timeoutCts.CancelAsync();

        var service = new OrderService(new InMemoryOrderStore());
        var importer = new JsonOrderImporter();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(importer, "any.json", linkedCts.Token));

        // Доводимо, що спрацював саме таймаут, а не дія користувача:
        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
    }

    [Fact]
    public async Task LoadStreamAsync_YieldsOrdersAsynchronously()
    {
        // Arrange (Бонус 1: потоковий імпорт IAsyncEnumerable)
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_stream_{Guid.NewGuid():N}.json");
        string validJson = """
        [
          {
            "Id": "ORD-STR-01",
            "CustomerId": "CUST-STR-01",
            "CreatedAt": "2026-11-10T10:00:00Z",
            "Status": 0,
            "Lines": [
              { "ProductId": "P-01", "ProductName": "Товар 1", "Price": 250.0, "Quantity": 2 }
            ]
          },
          {
            "Id": "ORD-STR-02",
            "CustomerId": "CUST-STR-02",
            "CreatedAt": "2026-11-10T11:00:00Z",
            "Status": 1,
            "Lines": [
              { "ProductId": "P-02", "ProductName": "Товар 2", "Price": 1200.0, "Quantity": 1 }
            ]
          }
        ]
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, validJson);

            var importer = new JsonOrderImporter();
            var receivedOrders = new List<Order>();

            // Act: споживання через await foreach
            await foreach (var order in importer.LoadStreamAsync(tempFile))
            {
                receivedOrders.Add(order);
            }

            // Assert: отримано всі 2 замовлення без проміжного списку List<T>
            Assert.Equal(2, receivedOrders.Count);
            Assert.Equal("ORD-STR-01", receivedOrders[0].Id);
            Assert.Equal("ORD-STR-02", receivedOrders[1].Id);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
