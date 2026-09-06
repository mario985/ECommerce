namespace ECommerce.Modules.Ordering.Domain.Orders;

public enum OrderStatus
{
    PendingInventory = 1,
    AwaitingPayment = 2,
    InventoryFailed = 3,
    Paid = 4,
    PaymentFailed = 5,
}
