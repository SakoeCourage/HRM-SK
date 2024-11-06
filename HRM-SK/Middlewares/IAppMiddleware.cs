namespace HRM_SK.Middlewares;


public delegate Task MiddlewareDelegate(HttpContext context, Func<Task> next);

public interface IAppMiddleware
{
    MiddlewareDelegate Middleware { get; }
}