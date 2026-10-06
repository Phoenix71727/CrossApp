namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(l => l.LineTotal);
    public bool IsConfirmed => Status == OrderStatus.Confirmed;

    private Order(string id, string customerId, DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        CreatedAt = createdAt;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id, string customerId, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта не може бути порожнім", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim(), createdAt ?? DateTime.UtcNow);
    }

    public static bool CanTransition(OrderStatus current, OrderStatus next) => (current, next) switch
    {
        (OrderStatus.Draft, OrderStatus.Confirmed) => true,
        (OrderStatus.Draft, OrderStatus.Cancelled) => true,
        _ => false
    };

    public void AddLine(string productId, string productName, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Не можна додавати рядки до замовлення зі статусом '{Status}'. Дозволено лише для 'Draft'.");

        OrderLine line = OrderLine.Create(productId, productName, price, quantity);
        _lines.Add(line);
    }

    public void Confirm()
    {
        if (!CanTransition(Status, OrderStatus.Confirmed))
            throw new InvalidOperationException($"Неможливо підтвердити замовлення зі статусу '{Status}'. Дозволено лише для 'Draft'.");

        if (_lines.Count == 0)
            throw new InvalidOperationException("Неможливо підтвердити порожнє замовлення: додайте хоча б один товар.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (!CanTransition(Status, OrderStatus.Cancelled))
            throw new InvalidOperationException($"Неможливо скасувати замовлення зі статусу '{Status}'. Перехід заборонено.");

        Status = OrderStatus.Cancelled;
    }

    public override string ToString() =>
        $"Замовлення #{Id} (Клієнт: {CustomerId}) [{Status}] — Рядків: {_lines.Count}, Сума: {Total:F2} грн";
}
