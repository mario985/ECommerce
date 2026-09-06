using Asp.Versioning;

namespace ECommerce.Api.Versioning;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddUrlApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        return services;
    }
}
