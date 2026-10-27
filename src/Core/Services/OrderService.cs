using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService(IOrderStore store, INotificationSink? notificationSink = null)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly INotificationSink? _sink = notificationSink;

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        Changed?.Invoke(this, new OrderChangedEventArgs(order.Id, kind, order.Lines.Count, order.Total));
        _sink?.Notify($"Подія {kind} для замовлення #{order.Id} (сума: {order.Total:F2} грн)");
    }

    public Order CreateOrder(string customerId, string? orderId = null)
    {
        string id = orderId ?? $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        Order order = Order.Create(id, customerId);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.OrderCreated);
        return order;
    }

    public void AddLine(string orderId, string productId, string productName, decimal price, int quantity)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id='{orderId}'.");

        order.AddLine(productId, productName, price, quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id='{orderId}'.");

        order.Confirm();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public void Cancel(string orderId)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id='{orderId}'.");

        order.Cancel();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderCancelled);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) => _store.Find(predicate);

    public IReadOnlyList<Order> FindAll(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }
}
