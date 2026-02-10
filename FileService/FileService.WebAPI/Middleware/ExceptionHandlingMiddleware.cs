using Microsoft.AspNetCore.Http;
using System.Net;

namespace FileService.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            await HandleExceptionAsync(context, HttpStatusCode.Forbidden, "Access denied");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found");
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, "File not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An error occurred while processing your request");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
