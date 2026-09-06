using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommerce.Api.OpenApi;

internal sealed class EndpointDocumentationFilter : IOperationFilter
{
    private static readonly Dictionary<string, (string Summary, string Description)> Descriptions =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            ["Identity.Register"] = ("Register customer account", "Creates a customer account using the supplied email address and password."),
            ["Identity.Login"] = ("Authenticate customer", "Authenticates a customer and returns access and refresh tokens."),
            ["Identity.Refresh"] = ("Refresh access token", "Exchanges a valid refresh token for a new access token."),
            ["Identity.Logout"] = ("Sign out", "Revokes the supplied refresh token for the authenticated customer."),
            ["Identity.GetCurrentUser"] = ("Get current user", "Retrieves the authenticated customer's identity details."),
            ["Identity.AdminCheck"] = ("Check administrator access", "Verifies that the authenticated user has the administrator role."),
            ["Catalog.GetProduct"] = ("Get product", "Retrieves an active catalog product by its identifier."),
            ["Catalog.SearchProducts"] = ("Search products", "Searches the public catalog using optional filters and shared pagination."),
            ["Catalog.AdvancedSearchProducts"] = ("Search and filter products", "Searches active Catalog products by name, SKU, description, or category name; filters by category, price, availability, and rating; and applies an allowed sort with pagination."),
            ["Cart.GetCart"] = ("Get cart", "Retrieves the authenticated customer's current shopping cart."),
            ["Cart.AddItem"] = ("Add product to cart", "Adds a product snapshot from Catalog into the authenticated customer's cart."),
            ["Cart.UpdateItem"] = ("Update cart item quantity", "Changes the quantity of an existing product in the authenticated customer's cart."),
            ["Cart.RemoveItem"] = ("Remove cart item", "Removes a product from the authenticated customer's cart."),
            ["Cart.ClearCart"] = ("Clear cart", "Removes all items from the authenticated customer's cart."),
            ["Cart.Checkout"] = ("Start checkout", "Creates a checkout workflow and starts order processing."),
            ["Cart.GetCheckout"] = ("Get checkout", "Retrieves the state of a checkout workflow."),
            ["Ordering.GetOrder"] = ("Get order", "Retrieves an order owned by the authenticated customer."),
            ["Ordering.GetHistory"] = ("Get order history", "Retrieves the authenticated customer's order history using shared pagination."),
            ["Ordering.GetOrderTracking"] = ("Get order tracking", "Retrieves shipment tracking for an order owned by the authenticated customer."),
            ["OrderingAdmin.CreateShipment"] = ("Create an order shipment", "Creates a pending shipment for a paid order. Requires the Admin role."),
            ["OrderingAdmin.UpdateShipmentStatus"] = ("Update shipment status", "Moves a shipment through the allowed fulfillment lifecycle. Requires the Admin role."),
            ["OrderingAdmin.GetShipments"] = ("Get shipments", "Returns a paged shipment list with optional status filtering. Requires the Admin role."),
            ["Payments.GetPayment"] = ("Get payment", "Retrieves payment information for an order owned by the authenticated customer."),
            ["Payments.StripeWebhook"] = ("Process Stripe webhook", "Receives a Stripe-signed webhook event. This endpoint uses Stripe signature verification, not JWT authentication."),
            ["Inventory.GetStock"] = ("Get inventory stock", "Retrieves the stock quantities for a product."),
            ["Inventory.Reserve"] = ("Reserve inventory", "Reserves available inventory for a product."),
            ["Inventory.ReleaseReservation"] = ("Release inventory reservation", "Releases a previously created inventory reservation."),
            ["Inventory.ConfirmReservation"] = ("Confirm inventory reservation", "Confirms a reservation after order processing."),
            ["CatalogAdmin.SearchProducts"] = ("Search catalog products", "Searches catalog products for administrators using optional filters and shared pagination."),
            ["CatalogAdmin.CreateProduct"] = ("Create catalog product", "Creates a new catalog product. Requires the Admin role."),
            ["CatalogAdmin.UpdateProduct"] = ("Update catalog product", "Updates an existing catalog product. Requires the Admin role."),
            ["CatalogAdmin.ActivateProduct"] = ("Activate catalog product", "Makes a catalog product available to customers. Requires the Admin role."),
            ["CatalogAdmin.DeactivateProduct"] = ("Deactivate catalog product", "Removes a catalog product from customer visibility. Requires the Admin role."),
            ["CatalogAdmin.DeleteProduct"] = ("Delete catalog product", "Removes a catalog product. Requires the Admin role."),
            ["IdentityAdmin.SearchUsers"] = ("Search users", "Searches user accounts for administrators using optional filters and shared pagination."),
            ["IdentityAdmin.GetUser"] = ("Get user", "Retrieves an individual user account for administration."),
            ["IdentityAdmin.AssignRole"] = ("Assign user role", "Assigns a role to a user account. Requires the Admin role."),
            ["IdentityAdmin.RemoveRole"] = ("Remove user role", "Removes a role from a user account. Requires the Admin role."),
            ["IdentityAdmin.DisableUser"] = ("Disable user", "Disables a user account. Requires the Admin role."),
            ["IdentityAdmin.EnableUser"] = ("Enable user", "Enables a user account. Requires the Admin role."),
            ["InventoryAdmin.Search"] = ("Search inventory", "Searches inventory for administrators using optional filters and shared pagination."),
            ["InventoryAdmin.AdjustStock"] = ("Adjust stock", "Records an administrative stock adjustment for a product. Requires the Admin role."),
            ["InventoryAdmin.History"] = ("Get stock adjustment history", "Retrieves a product's stock adjustment history using shared pagination."),
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string operationId = operation.OperationId ?? GetFallbackOperationId(context.ApiDescription.RelativePath, context.ApiDescription.HttpMethod);
        operation.OperationId = operationId;

        if (Descriptions.TryGetValue(operationId, out (string Summary, string Description) details))
        {
            operation.Summary = details.Summary;
            operation.Description = details.Description;
        }
        else
        {
            operation.Summary = CreateSummary(operationId);
            operation.Description = $"{operation.Summary}.";
        }

        operation.Tags = [new OpenApiTag { Name = GetTag(context.ApiDescription.RelativePath) }];
        ConfigureSecurity(operation, context);
        ConfigureExamples(operation, operationId);
        ConfigureProblemDetails(operation, context, operationId);
        ConfigurePagination(operation, context, context.ApiDescription.RelativePath);
        ConfigureCatalogSearchParameters(operation, operationId);
    }

    private static void ConfigureCatalogSearchParameters(
        OpenApiOperation operation,
        string operationId)
    {
        if (operationId != "Catalog.AdvancedSearchProducts")
        {
            return;
        }

        Dictionary<string, (string Description, IOpenApiAny Example)> details =
            new Dictionary<string, (string, IOpenApiAny)>(StringComparer.OrdinalIgnoreCase)
            {
                ["q"] = ("Text matched against product name, SKU, description, and category name.", new OpenApiString("laptop")),
                ["categoryId"] = ("Catalog category identifier.", new OpenApiString("00000000-0000-0000-0000-000000000010")),
                ["minPrice"] = ("Inclusive minimum product price.", new OpenApiDouble(100)),
                ["maxPrice"] = ("Inclusive maximum product price.", new OpenApiDouble(500)),
                ["inStock"] = ("Filters by current Inventory availability.", new OpenApiBoolean(true)),
                ["minimumRating"] = ("Inclusive minimum approved-review average rating from 1 through 5.", new OpenApiDouble(4)),
                ["sort"] = ("Allowed values: price-asc, price-desc, newest, rating-desc, popularity.", new OpenApiString("price-desc")),
            };
        foreach (OpenApiParameter parameter in operation.Parameters)
        {
            if (details.TryGetValue(parameter.Name, out (string Description, IOpenApiAny Example) detail))
            {
                parameter.Description = detail.Description;
                parameter.Example = detail.Example;
            }
        }
    }

    private static void ConfigureSecurity(OpenApiOperation operation, OperationFilterContext context)
    {
        IEnumerable<object> metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata ?? [];
        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        IEnumerable<IAuthorizeData> authorization = metadata.OfType<IAuthorizeData>();
        if (!authorization.Any())
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = OpenApiConstants.BearerScheme },
                }] = [],
            },
        ];

        if (authorization.Any(item => string.Equals(item.Policy, "AdminOnly", StringComparison.Ordinal) ||
                                      item.Roles?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true))
        {
            operation.Description += " Requires the Admin role.";
        }
    }

    private static void ConfigureExamples(OpenApiOperation operation, string operationId)
    {
        if (operation.RequestBody is not null && OpenApiExamples.Requests.TryGetValue(operationId, out OpenApiObject? requestExample))
        {
            foreach (OpenApiMediaType mediaType in operation.RequestBody.Content.Values)
            {
                mediaType.Example = requestExample;
            }
        }

        if (OpenApiExamples.Responses.TryGetValue(operationId, out OpenApiObject? responseExample) &&
            (operation.Responses.TryGetValue("200", out OpenApiResponse? response) ||
             operation.Responses.TryGetValue("201", out response)))
        {
            foreach (OpenApiMediaType mediaType in response.Content.Values)
            {
                mediaType.Example = responseExample;
            }
        }
    }

    private static void ConfigureProblemDetails(OpenApiOperation operation, OperationFilterContext context, string operationId)
    {
        AddProblemResponse(operation, context, "400", "Validation or request error", OpenApiExamples.ValidationProblemDetails, typeof(ApiValidationProblemDetailsDocument));
        AddProblemResponse(operation, context, "500", "Unexpected server error", OpenApiExamples.ProblemDetails, typeof(ApiProblemDetailsDocument));

        if (operation.Security?.Count > 0)
        {
            AddProblemResponse(operation, context, "401", "Authentication is required", OpenApiExamples.ProblemDetails, typeof(ApiProblemDetailsDocument));
            AddProblemResponse(operation, context, "403", "The authenticated user is not authorized", OpenApiExamples.ProblemDetails, typeof(ApiProblemDetailsDocument));
        }

        if (operationId.Contains("Get", StringComparison.Ordinal) || operationId.Contains("Update", StringComparison.Ordinal) ||
            operationId.Contains("Remove", StringComparison.Ordinal) || operationId.Contains("Delete", StringComparison.Ordinal) ||
            operationId.Contains("Reservation", StringComparison.Ordinal) || operationId.Contains("Approve", StringComparison.Ordinal) ||
            operationId.Contains("Reject", StringComparison.Ordinal))
        {
            AddProblemResponse(operation, context, "404", "The requested resource was not found", OpenApiExamples.ProblemDetails, typeof(ApiProblemDetailsDocument));
        }

        if (operationId.Contains("Create", StringComparison.Ordinal) || operationId.Contains("Update", StringComparison.Ordinal) ||
            operationId.Contains("Checkout", StringComparison.Ordinal) || operationId.Contains("Reservation", StringComparison.Ordinal))
        {
            AddProblemResponse(operation, context, "409", "The request conflicts with the current resource state", OpenApiExamples.ProblemDetails, typeof(ApiProblemDetailsDocument));
        }
        else if (operationId.Contains("Approve", StringComparison.Ordinal) || operationId.Contains("Reject", StringComparison.Ordinal))
        {
            AddProblemResponse(operation, context, "409", "The request conflicts with the current resource state", OpenApiExamples.ProblemDetails, typeof(ApiProblemDetailsDocument));
        }
    }

    private static void AddProblemResponse(
        OpenApiOperation operation,
        OperationFilterContext context,
        string statusCode,
        string description,
        Microsoft.OpenApi.Any.IOpenApiAny example,
        Type documentType)
    {
        if (operation.Responses.ContainsKey(statusCode))
        {
            return;
        }

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(documentType, context.SchemaRepository),
                    Example = example,
                },
            },
        };
    }

    private static void ConfigurePagination(OpenApiOperation operation, OperationFilterContext context, string? path)
    {
        if (path is null || !path.Contains("/products", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith("orders", StringComparison.OrdinalIgnoreCase) &&
            !path.Contains("/users", StringComparison.OrdinalIgnoreCase) &&
            !path.Contains("/adjustments", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith("inventory", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith("shipments", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (OpenApiParameter parameter in operation.Parameters.Where(parameter => parameter.Name is "page" or "pageSize"))
        {
            parameter.Description = parameter.Name == "page"
                ? "Page number. Defaults to 1."
                : "Number of items per page. Defaults to 20 and cannot exceed 100.";
            parameter.Example = new Microsoft.OpenApi.Any.OpenApiInteger(parameter.Name == "page" ? 1 : 20);
        }

        if (operation.Responses.TryGetValue("200", out OpenApiResponse? response) && response.Content.Count == 0)
        {
            response.Content["application/json"] = new OpenApiMediaType
            {
                Schema = context.SchemaGenerator.GenerateSchema(typeof(ApiPagedResponseDocument), context.SchemaRepository),
                Example = new OpenApiObject
                {
                    ["items"] = new OpenApiArray(),
                    ["page"] = new OpenApiInteger(1),
                    ["pageSize"] = new OpenApiInteger(20),
                    ["totalCount"] = new OpenApiInteger(100),
                    ["totalPages"] = new OpenApiInteger(5),
                },
            };
        }
    }

    private static string GetTag(string? path) => path switch
    {
        not null when path.StartsWith("api/v1/admin/identity", StringComparison.OrdinalIgnoreCase) => "Admin Identity",
        not null when path.StartsWith("api/v1/admin/catalog", StringComparison.OrdinalIgnoreCase) => "Admin Catalog",
        not null when path.StartsWith("api/v1/admin/inventory", StringComparison.OrdinalIgnoreCase) => "Admin Inventory",
        not null when path.StartsWith("api/v1/admin/reviews", StringComparison.OrdinalIgnoreCase) => "Admin Reviews",
        not null when path.StartsWith("api/v1/admin/shipments", StringComparison.OrdinalIgnoreCase) ||
                      path.StartsWith("api/v1/admin/orders", StringComparison.OrdinalIgnoreCase) &&
                      path.Contains("shipment", StringComparison.OrdinalIgnoreCase) => "Admin Shipments",
        not null when path.StartsWith("api/v1/identity", StringComparison.OrdinalIgnoreCase) => "Identity",
        not null when path.StartsWith("api/v1/catalog", StringComparison.OrdinalIgnoreCase) => "Catalog",
        not null when path.StartsWith("api/v1/inventory", StringComparison.OrdinalIgnoreCase) => "Inventory",
        not null when path.StartsWith("api/v1/cart", StringComparison.OrdinalIgnoreCase) => "Cart",
        not null when path.StartsWith("api/v1/orders", StringComparison.OrdinalIgnoreCase) &&
                      path.Contains("tracking", StringComparison.OrdinalIgnoreCase) => "Order Tracking",
        not null when path.StartsWith("api/v1/orders", StringComparison.OrdinalIgnoreCase) => "Orders",
        not null when path.StartsWith("api/v1/payments", StringComparison.OrdinalIgnoreCase) => "Payments",
        not null when path.StartsWith("api/v1/wishlist", StringComparison.OrdinalIgnoreCase) => "Wishlists",
        not null when path.StartsWith("api/v1/products", StringComparison.OrdinalIgnoreCase) && path.Contains("reviews", StringComparison.OrdinalIgnoreCase) => "Reviews",
        _ => "API",
    };

    private static string GetFallbackOperationId(string? path, string? method) => (path, method) switch
    {
        ("api/v1/identity/logout", _) => "Identity.Logout",
        ("api/v1/identity/admin/check", _) => "Identity.AdminCheck",
        _ => $"{GetTag(path).Replace(" ", string.Empty, StringComparison.Ordinal)}.{method ?? "Operation"}",
    };

    private static string CreateSummary(string operationId) => operationId.Replace('.', ' ');
}
