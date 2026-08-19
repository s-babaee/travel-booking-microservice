using System.Text.Json;
using Auth.Api.Application.Exceptions;

namespace Auth.Api.Infrastructure.Web;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await WriteErrorAsync(context, exception);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ExternalServiceException external when external.StatusCode is 400 or 401 =>
                (StatusCodes.Status401Unauthorized, "Identity provider rejected the request"),
            ExternalServiceException => (StatusCodes.Status502BadGateway, "Identity provider error"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(
            new
            {
                type = "about:blank",
                title,
                status = statusCode,
                detail = exception.Message,
                traceId = context.TraceIdentifier
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
