using Microsoft.OpenApi.Any;

namespace ECommerce.Api.OpenApi;

internal static class OpenApiExamples
{
    public static readonly OpenApiObject ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://ecommerce/errors/cart/item-not-found"),
        ["title"] = new OpenApiString("Cart item not found"),
        ["status"] = new OpenApiInteger(404),
        ["detail"] = new OpenApiString("The requested cart item was not found."),
        ["errorCode"] = new OpenApiString("Cart.ItemNotFound"),
        ["correlationId"] = new OpenApiString("abc123"),
        ["traceId"] = new OpenApiString("00-00000000000000000000000000000000-0000000000000000-00"),
    };

    public static readonly OpenApiObject ValidationProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://ecommerce/errors/api/validation"),
        ["title"] = new OpenApiString("Validation failed"),
        ["status"] = new OpenApiInteger(400),
        ["errors"] = new OpenApiObject
        {
            ["quantity"] = new OpenApiArray { new OpenApiString("Quantity must be greater than zero.") },
            ["email"] = new OpenApiArray { new OpenApiString("Email is required.") },
        },
        ["errorCode"] = new OpenApiString("Api.Validation"),
        ["correlationId"] = new OpenApiString("abc123"),
    };

    public static readonly IReadOnlyDictionary<string, OpenApiObject> Requests =
        new Dictionary<string, OpenApiObject>(StringComparer.Ordinal)
        {
            ["Identity.Register"] = Credentials(),
            ["Identity.Login"] = Credentials(),
            ["CatalogAdmin.CreateProduct"] = ProductRequest(),
            ["CatalogAdmin.UpdateProduct"] = ProductRequest(),
            ["Cart.AddItem"] = new() { ["productId"] = new OpenApiString("00000000-0000-0000-0000-000000000001"), ["quantity"] = new OpenApiInteger(2) },
            ["Cart.UpdateItem"] = new() { ["quantity"] = new OpenApiInteger(2) },
            ["Inventory.Reserve"] = new() { ["quantity"] = new OpenApiInteger(2) },
            ["InventoryAdmin.AdjustStock"] = new() { ["quantity"] = new OpenApiInteger(10), ["type"] = new OpenApiString("Increase"), ["reason"] = new OpenApiString("Restock"), ["note"] = new OpenApiString("Example restock adjustment") },
            ["Reviews.CreateReview"] = new() { ["rating"] = new OpenApiInteger(5), ["comment"] = new OpenApiString("Excellent product") },
            ["OrderingAdmin.CreateShipment"] = new()
            {
                ["carrier"] = new OpenApiString("DHL"),
                ["trackingNumber"] = new OpenApiString("TRK123456"),
            },
            ["OrderingAdmin.UpdateShipmentStatus"] = new()
            {
                ["status"] = new OpenApiString("Shipped"),
            },
        };

    public static readonly IReadOnlyDictionary<string, OpenApiObject> Responses =
        new Dictionary<string, OpenApiObject>(StringComparer.Ordinal)
        {
            ["Catalog.GetProduct"] = Product(),
            ["Catalog.AdvancedSearchProducts"] = new()
            {
                ["items"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000001"),
                        ["name"] = new OpenApiString("Gaming Laptop"),
                        ["sku"] = new OpenApiString("LAPTOP-001"),
                        ["price"] = new OpenApiDouble(499.99),
                        ["currency"] = new OpenApiString("USD"),
                        ["categoryName"] = new OpenApiString("Laptops"),
                        ["averageRating"] = new OpenApiDouble(4.8),
                        ["reviewCount"] = new OpenApiInteger(120),
                        ["inStock"] = new OpenApiBoolean(true),
                        ["createdAtUtc"] = new OpenApiString("2026-08-30T12:00:00Z"),
                    },
                },
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20),
                ["totalCount"] = new OpenApiInteger(120),
                ["totalPages"] = new OpenApiInteger(6),
            },
            ["Cart.GetCart"] = Cart(),
            ["Cart.AddItem"] = Cart(),
            ["Ordering.GetOrder"] = Order(),
            ["Payments.GetPayment"] = Payment(),
            ["Reviews.GetProductReviews"] = new()
            {
                ["averageRating"] = new OpenApiDouble(4.5),
                ["totalReviews"] = new OpenApiInteger(2),
                ["items"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["rating"] = new OpenApiInteger(5),
                        ["comment"] = new OpenApiString("Excellent product"),
                        ["createdAtUtc"] = new OpenApiString("2026-08-27T12:00:00Z"),
                    },
                },
            },
            ["OrderingAdmin.CreateShipment"] = Shipment(),
            ["OrderingAdmin.UpdateShipmentStatus"] = Shipment(),
            ["OrderingAdmin.GetShipments"] = new()
            {
                ["items"] = new OpenApiArray { Shipment() },
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20),
                ["totalCount"] = new OpenApiInteger(1),
                ["totalPages"] = new OpenApiInteger(1),
            },
            ["Ordering.GetOrderTracking"] = new()
            {
                ["orderId"] = new OpenApiString("00000000-0000-0000-0000-000000000003"),
                ["status"] = new OpenApiString("Shipped"),
                ["carrier"] = new OpenApiString("DHL"),
                ["trackingNumber"] = new OpenApiString("TRK123456"),
                ["createdAtUtc"] = new OpenApiString("2026-08-27T12:00:00Z"),
                ["shippedAtUtc"] = new OpenApiString("2026-08-28T12:00:00Z"),
            },
        };

    private static OpenApiObject Credentials() => new()
    {
        ["email"] = new OpenApiString("user@example.com"),
        ["password"] = new OpenApiString("Password123!"),
    };

    private static OpenApiObject ProductRequest() => new()
    {
        ["name"] = new OpenApiString("Mechanical Keyboard"),
        ["sku"] = new OpenApiString("KEYBOARD-001"),
        ["description"] = new OpenApiString("RGB mechanical keyboard"),
        ["price"] = new OpenApiDouble(99.99),
        ["currency"] = new OpenApiString("USD"),
    };

    private static OpenApiObject Product() => new()
    {
        ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000001"),
        ["name"] = new OpenApiString("Mechanical Keyboard"),
        ["sku"] = new OpenApiString("KEYBOARD-001"),
        ["price"] = new OpenApiDouble(99.99),
        ["currency"] = new OpenApiString("USD"),
        ["isActive"] = new OpenApiBoolean(true),
    };

    private static OpenApiObject Cart() => new()
    {
        ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000002"),
        ["items"] = new OpenApiArray { new OpenApiObject { ["productId"] = new OpenApiString("00000000-0000-0000-0000-000000000001"), ["productName"] = new OpenApiString("Mechanical Keyboard"), ["quantity"] = new OpenApiInteger(2) } },
        ["totalAmount"] = new OpenApiDouble(199.98),
        ["currency"] = new OpenApiString("USD"),
    };

    private static OpenApiObject Order() => new()
    {
        ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000003"),
        ["status"] = new OpenApiString("AwaitingPayment"),
        ["totalAmount"] = new OpenApiDouble(199.98),
        ["currency"] = new OpenApiString("USD"),
    };

    private static OpenApiObject Payment() => new()
    {
        ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000004"),
        ["orderId"] = new OpenApiString("00000000-0000-0000-0000-000000000003"),
        ["status"] = new OpenApiString("Pending"),
        ["amount"] = new OpenApiDouble(199.98),
        ["currency"] = new OpenApiString("USD"),
    };

    private static OpenApiObject Shipment() => new()
    {
        ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000005"),
        ["orderId"] = new OpenApiString("00000000-0000-0000-0000-000000000003"),
        ["status"] = new OpenApiString("Shipped"),
        ["carrier"] = new OpenApiString("DHL"),
        ["trackingNumber"] = new OpenApiString("TRK123456"),
        ["createdAtUtc"] = new OpenApiString("2026-08-27T12:00:00Z"),
        ["shippedAtUtc"] = new OpenApiString("2026-08-28T12:00:00Z"),
    };
}
