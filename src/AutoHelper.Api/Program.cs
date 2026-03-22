using AutoHelper.Api.Extensions;
using Scalar.AspNetCore;
using Serilog;

// Bootstrap logger — captures startup failures before full Serilog is configured
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog from appsettings.json
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.AddServices();

    var app = builder.Build();

    // Middleware pipeline
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        // Scalar UI at /scalar/v1
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // Feature endpoints registered here as the solution grows
    // e.g.: app.MapAuthEndpoints();
    //       app.MapVehicleEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
