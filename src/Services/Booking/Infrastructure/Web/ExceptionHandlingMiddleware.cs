using Booking.Api.Application.Exceptions;
using Booking.Api.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Infrastructure.Web;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                UnauthorizedException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized"),
                NotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource not found"),
                ConflictException => (
                    StatusCodes.Status409Conflict,
                    "Conflict"),
                ValidationException => (
                    StatusCodes.Status400BadRequest,
                    "Validation failed"),
                DomainException => (
                    StatusCodes.Status400BadRequest,
                    "Domain rule violated"),
                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid argument"),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Unexpected error")
            };

            if (statusCode >= 500)
            {
                logger.LogError(
                    exception,
                    "Unhandled booking request exception.");
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode >= 500
                    ? "The request could not be completed."
                    : exception.Message,
                Instance = context.Request.Path
            });
        }
    }
}
