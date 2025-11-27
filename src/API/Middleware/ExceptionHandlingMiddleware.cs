using System.Diagnostics;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment
    )
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = MapExceptionToResponse(exception);

        LogException(exception, context);

        var problemDetails = CreateProblemDetails(exception, statusCode, title, context);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

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

    private void LogException(Exception exception, HttpContext context)
    {
        switch (exception)
        {
            case TaxRuleNotFoundException taxRuleNotFound:
                _logger.LogWarning(
                    exception,
                    "Tax rule not found. CityId: {CityId}, Year: {Year}, Path: {Path}",
                    taxRuleNotFound.CityId,
                    taxRuleNotFound.Year,
                    context.Request.Path
                );
                break;

            case CityNotFoundException cityNotFound:
                _logger.LogWarning(
                    exception,
                    "City not found. CityId: {CityId}, Path: {Path}",
                    cityNotFound.CityId,
                    context.Request.Path
                );
                break;

            case ValidationException validationException:
                _logger.LogWarning(
                    exception,
                    "Validation failed. Errors: {@Errors}, Path: {Path}",
                    validationException.Errors,
                    context.Request.Path
                );
                break;

            case NotFoundException:
                _logger.LogWarning(
                    exception,
                    "Resource not found. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                break;

            case DomainException:
                _logger.LogWarning(
                    exception,
                    "Domain exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                break;
        }
    }

    private ProblemDetails CreateProblemDetails(
        Exception exception,
        int statusCode,
        string title,
        HttpContext context
    )
    {
        var problemDetails = new ProblemDetails
        {
            Type = GetProblemType(statusCode),
            Title = title,
            Status = statusCode,
            Detail = GetDetailMessage(exception, statusCode),
            Instance = context.Request.Path,
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        if (
            exception is ValidationException validationException
            && validationException.Errors.Any()
        )
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        return problemDetails;
    }

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

    private string GetDetailMessage(Exception exception, int statusCode)
    {
        if (statusCode == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment())
        {
            return "An unexpected error occurred while processing your request. Please try again later.";
        }

        if (exception is DomainException)
        {
            return exception.Message;
        }

        if (_environment.IsDevelopment())
        {
            return exception.Message;
        }

        return "An error occurred while processing your request.";
    }
}
