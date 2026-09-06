namespace ECommerce.Api.OpenApi;

/// <summary>
/// Configures the public OpenAPI document and Swagger UI exposure.
/// </summary>
public sealed class OpenApiOptions
{
    public const string SectionName = "OpenApi";

    public string Title { get; init; } = "ECommerce API";

    public string Description { get; init; } = "Modular monolith ecommerce backend API.";

    public string Version { get; init; } = "v1";

    public bool Enabled { get; init; }

    public bool EnableSwaggerUi { get; init; }
}
