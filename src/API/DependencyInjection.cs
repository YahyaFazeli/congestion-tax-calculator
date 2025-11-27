using Scalar.AspNetCore;
using Serilog;

namespace API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddOpenApi();

        return services;
    }

    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "CongestionTaxCalculator")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
            )
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Add global exception handling
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerFeature =
                    context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogError(
                    exception,
                    "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        error = "An error occurred processing your request.",
                        traceId = System.Diagnostics.Activity.Current?.Id
                        ?? context.TraceIdentifier,
                    }
                );
            });
        });

        // Add Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set(
                    "UserAgent",
                    httpContext.Request.Headers["User-Agent"].ToString()
                );
            };
        });

        return app;
    }
}
