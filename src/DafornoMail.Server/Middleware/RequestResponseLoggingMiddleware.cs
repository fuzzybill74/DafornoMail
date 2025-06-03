using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DafornoMail.Server.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Incoming request: {method} {path}", context.Request.Method, context.Request.Path);
        await _next(context);
        _logger.LogInformation("Outgoing response: {statusCode}", context.Response.StatusCode);
    }
}
