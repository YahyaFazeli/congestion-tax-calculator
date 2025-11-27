using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
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

                if (exception is null)
                {
                    return;
                }

                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

                // Map exception to status code and title
                var (statusCode, title) = MapExceptionToResponse(exception);

                // Log the exception with appropriate level
                LogException(logger, exception, context);

                // Create Problem Details response
                var problemDetails = CreateProblemDetails(
                    exception,
                    statusCode,
                    title,
                    context,
                    environment
                );

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails);
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

    /// <summary>
    /// Maps an exception to an HTTP status code and title.
    /// </summary>
    private static (int StatusCode, string Title) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            DomainException => (StatusCodes.Status400BadRequest, "Bad Request"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };
    }

    /// <summary>
    /// Logs the exception with appropriate level and context.
    /// </summary>
    private static void LogException(
        Microsoft.Extensions.Logging.ILogger logger,
        Exception exception,
        HttpContext context
    )
    {
        switch (exception)
        {
            case TaxRuleNotFoundException taxRuleNotFound:
                logger.LogWarning(
                    exception,
                    "Tax rule not found. CityId: {CityId}, Year: {Year}, Path: {Path}",
                    taxRuleNotFound.CityId,
                    taxRuleNotFound.Year,
                    context.Request.Path
                );
                break;

            case CityNotFoundException cityNotFound:
                logger.LogWarning(
                    exception,
                    "City not found. CityId: {CityId}, Path: {Path}",
                    cityNotFound.CityId,
                    context.Request.Path
                );
                break;

            case ValidationException validationException:
                logger.LogWarning(
                    exception,
                    "Validation failed. Errors: {@Errors}, Path: {Path}",
                    validationException.Errors,
                    context.Request.Path
                );
                break;

            case NotFoundException:
                logger.LogWarning(
                    exception,
                    "Resource not found. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                break;

            case DomainException:
                logger.LogWarning(
                    exception,
                    "Domain exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                break;

            default:
                logger.LogError(
                    exception,
                    "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                break;
        }
    }

    /// <summary>
    /// Creates a Problem Details response following RFC 7807.
    /// </summary>
    private static ProblemDetails CreateProblemDetails(
        Exception exception,
        int statusCode,
        string title,
        HttpContext context,
        IHostEnvironment environment
    )
    {
        var problemDetails = new ProblemDetails
        {
            Type = GetProblemType(statusCode),
            Title = title,
            Status = statusCode,
            Detail = GetDetailMessage(exception, statusCode, environment),
            Instance = context.Request.Path,
        };

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

        // Add validation errors if applicable
        if (
            exception is ValidationException validationException
            && validationException.Errors.Any()
        )
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        return problemDetails;
    }

    /// <summary>
    /// Gets the RFC 7807 problem type URI for the status code.
    /// </summary>
    private static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "https://tools.ietf.org/html/rfc7231",
        };
    }

    /// <summary>
    /// Gets the detail message for the exception, filtering sensitive information in production.
    /// </summary>
    private static string GetDetailMessage(
        Exception exception,
        int statusCode,
        IHostEnvironment environment
    )
    {
        // For 500 errors in production, return generic message
        if (statusCode == StatusCodes.Status500InternalServerError && !environment.IsDevelopment())
        {
            return "An unexpected error occurred while processing your request. Please try again later.";
        }

        // For domain exceptions, return the exception message (already sanitized)
        if (exception is DomainException)
        {
            return exception.Message;
        }

        // For other exceptions in development, include the message
        if (environment.IsDevelopment())
        {
            return exception.Message;
        }

        // Default generic message for production
        return "An error occurred while processing your request.";
    }
}
