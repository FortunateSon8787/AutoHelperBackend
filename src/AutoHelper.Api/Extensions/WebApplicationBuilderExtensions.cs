using AutoHelper.Api.Common;
using AutoHelper.Application;
using AutoHelper.Infrastructure;

namespace AutoHelper.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Application and Infrastructure layers
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        // API-level services
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddOpenApi();

        // CORS — allow the Next.js frontend
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        // Auth — JWT Bearer
        services
            .AddAuthentication()
            .AddJwtBearer();

        services.AddAuthorization();

        // Health checks
        services
            .AddHealthChecks()
            .AddDbContextCheck<AutoHelper.Infrastructure.Persistence.AppDbContext>();

        return builder;
    }
}
