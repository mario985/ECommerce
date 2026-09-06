using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommerce.Api.OpenApi;

public static class ApiDocumentationExtensions
{
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OpenApiOptions>()
            .Bind(configuration.GetSection(OpenApiOptions.SectionName))
            .ValidateDataAnnotations();

        OpenApiOptions options = configuration.GetSection(OpenApiOptions.SectionName).Get<OpenApiOptions>() ?? new();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc(options.Version, new OpenApiInfo
            {
                Title = options.Title,
                Version = options.Version,
                Description = options.Description,
            });
            swagger.DocInclusionPredicate((_, description) =>
                description.RelativePath?.StartsWith("api/v1/", StringComparison.OrdinalIgnoreCase) == true);
            swagger.AddSecurityDefinition(OpenApiConstants.BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT access token. Paste the token value; Swagger UI sends it as a Bearer token.",
            });
            swagger.OperationFilter<EndpointDocumentationFilter>();

            string xmlPath = Path.Combine(AppContext.BaseDirectory, "ECommerce.Api.xml");
            if (File.Exists(xmlPath))
            {
                swagger.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        OpenApiOptions options = app.Services.GetRequiredService<IOptions<OpenApiOptions>>().Value;
        if (!options.Enabled)
        {
            return app;
        }

        app.UseSwagger(swagger => swagger.RouteTemplate = "swagger/{documentName}/swagger.json");
        if (options.EnableSwaggerUi)
        {
            app.UseSwaggerUI(swagger =>
            {
                swagger.SwaggerEndpoint($"/swagger/{options.Version}/swagger.json", $"{options.Title} {options.Version}");
                swagger.RoutePrefix = "swagger";
                swagger.DisplayOperationId();
                swagger.EnableDeepLinking();
                swagger.DisplayRequestDuration();
                swagger.EnableFilter();
            });
        }

        return app;
    }
}

internal static class OpenApiConstants
{
    public const string BearerScheme = "Bearer";
}
