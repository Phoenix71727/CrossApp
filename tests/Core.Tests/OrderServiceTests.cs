using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public void Confirm_WhenEmpty_ThrowsDomainException_AndKeepsDraftState()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("CUST-01", "ORD-EMPTY");

        // Act & Assert
        Assert.Throws<DomainException>(() => service.Confirm(order.Id));
        Assert.Equal(OrderStatus.Draft, store.GetById(order.Id)!.Status);
    }

    [Fact]
    public void AddLine_ToConfirmedOrder_ThrowsDomainException_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("CUST-01", "ORD-CONF");
        service.AddLine(order.Id, "P-01", "Товар", 1000m, 1);
        service.Confirm(order.Id);

        decimal totalBefore = store.GetById(order.Id)!.Total;

        // Act & Assert (Варіант "Замовлення": DomainException «замовлення вже підтверджено»)
        Assert.Throws<DomainException>(() => service.AddLine(order.Id, "P-02", "Новий товар", 500m, 1));
        Assert.Equal(totalBefore, store.GetById(order.Id)!.Total);
    }

    [Fact]
    public void AddLine_IncreasesOrderTotal()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("CUST-01", "ORD-TOTAL");

        // Act (Варіант "Замовлення": тест на суму замовлення після додавання рядків)
        service.AddLine(order.Id, "P-01", "Клавіатура", 1500m, 2);
        service.AddLine(order.Id, "P-02", "Миша", 800m, 1);

        // Assert: 1500 * 2 + 800 * 1 = 3800
        var savedOrder = store.GetById(order.Id)!;
        Assert.Equal(3800m, savedOrder.Total);
        Assert.Equal(2, savedOrder.Lines.Count);
    }

    [Fact]
    public void CreateOrder_RaisesChangedExactlyOnce()
    {
        // Arrange
        var service = new OrderService(new InMemoryOrderStore());
        var events = new List<OrderChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        // Act
        service.CreateOrder("CUST-EVENT", "ORD-EV-01");

        // Assert
        Assert.Single(events);
        Assert.Equal(OrderChangeKind.OrderCreated, events[0].Kind);
        Assert.Equal("ORD-EV-01", events[0].OrderId);
    }
}
