using System.Net;
using System.Text.Json;

namespace TodoApi.Api.Middleware;

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            KeyNotFoundException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = exception.Message,
                Type = "NotFound"
            },
            ArgumentException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = exception.Message,
                Type = "BadRequest"
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Unauthorized access",
                Type = "Unauthorized"
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An unexpected error occurred. Please try again later.",
                Type = "InternalServerError"
            }
        };

        response.StatusCode = errorResponse.StatusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
