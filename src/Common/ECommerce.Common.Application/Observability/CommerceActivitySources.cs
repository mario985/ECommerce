using System.Diagnostics;

namespace ECommerce.Common.Application.Observability;

public static class CommerceActivitySources
{
    public const string CartName = "ECommerce.Cart";
    public const string OrderingName = "ECommerce.Ordering";
    public const string InventoryName = "ECommerce.Inventory";
    public const string PaymentsName = "ECommerce.Payments";
    public const string MessagingName = "ECommerce.Messaging";

    public static readonly ActivitySource Cart = new(CartName);
    public static readonly ActivitySource Ordering = new(OrderingName);
    public static readonly ActivitySource Inventory = new(InventoryName);
    public static readonly ActivitySource Payments = new(PaymentsName);
    public static readonly ActivitySource Messaging = new(MessagingName);

}
