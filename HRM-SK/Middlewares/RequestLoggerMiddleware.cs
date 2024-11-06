using System.Diagnostics;

namespace HRM_SK.Middlewares;

public class RequestLoggerMiddleware: IAppMiddleware
{
    private readonly ILogger<RequestLoggerMiddleware> _logger;

    public RequestLoggerMiddleware(ILogger<RequestLoggerMiddleware> logger)
    {
        _logger = logger;
    }

    public MiddlewareDelegate Middleware => async (context, next) =>
    {
        var exemptPath = "/api/auth/user/login";

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Handling request: {RequestMethod} {RequestPath} at {StartTime}", context.Request.Method, context.Request.Path, DateTime.UtcNow);

        if (context.Request.Path.Value.Equals(exemptPath, StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthorized access attempt to:{RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var response = new { Error = "Unauthorized", StatusCode = StatusCodes.Status401Unauthorized };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonResponse);
            return;
        }
        await next();

        stopwatch.Stop();
        _logger.LogInformation("Finished handling request:{RequestMethod}  {RequestPath} in {Duration} ms",
            context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);

    };
}