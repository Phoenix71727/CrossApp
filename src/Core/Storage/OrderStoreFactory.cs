using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Фабрика для створення сховища замовлень на основі аргументів командного рядка (патерн Simple Factory).
/// Інкапсулює логіку вибору та конфігурації сховища (InMemory, File, Caching Decorator).
/// </summary>
public static class OrderStoreFactory
{
    public static IOrderStore Create(
        string[] args,
        string? defaultDataPath = null,
        IEnumerable<Order>? seed = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cached");

        string dataPath = defaultDataPath ?? Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

        IOrderStore store = useFile
            ? new FileOrderStore(dataPath)
            : new InMemoryOrderStore(seed ?? SampleData.Orders());

        if (useCache)
        {
            store = new CachingOrderStore(store);
        }

        return store;
    }
}
