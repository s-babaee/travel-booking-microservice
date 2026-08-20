using Flight.Api.Application.Exceptions;
using Flight.Api.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Infrastructure.Web;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            DomainException => (StatusCodes.Status400BadRequest, "Domain rule violated"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= 500
                ? "The request could not be completed."
                : exception.Message,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(problem);
    }
}
