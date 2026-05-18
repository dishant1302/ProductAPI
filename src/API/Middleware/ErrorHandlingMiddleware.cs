using System.Net;
using ProductAPI.Application.Common;
using ProductAPI.Domain.Exceptions;

namespace ProductAPI.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Bad request: {Message}", ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized: {Message}", ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Message}", ex.Message);
            var response = ApiResponse<object>.ErrorResult(
                "Validation failed",
                ex.Errors.SelectMany(e => e.Value));
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred processing {Path}", context.Request.Path);
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteResponseAsync(
        HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object>.ErrorResult(message);
        await context.Response.WriteAsJsonAsync(response);
    }
}