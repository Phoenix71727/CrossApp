namespace Core.Domain;

public static class OrderLimitService
{
    public const int MaxActiveOrdersPerCustomer = 3;

    public static bool CanPlaceOrder(Customer customer, IEnumerable<Order> existingOrders, out string? reason)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(existingOrders);

        int activeCount = existingOrders.Count(o => o.CustomerId == customer.Id && o.Status != OrderStatus.Cancelled);

        if (activeCount >= MaxActiveOrdersPerCustomer)
        {
            reason = $"Клієнт '{customer.FullName}' (ID: {customer.Id}) уже має {activeCount} активних замовлень (ліміт: {MaxActiveOrdersPerCustomer}).";
            return false;
        }

        reason = null;
        return true;
    }
}
