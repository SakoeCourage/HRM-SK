using HRM_SK.Middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public static class ApplicationMiddlewareExtension
{
    public static List<MiddlewareDelegate> Middlewares { get; } = new List<MiddlewareDelegate>();

    static ApplicationMiddlewareExtension()
    {
        
    }

    public static IApplicationBuilder UseBaseMiddleware(this IApplicationBuilder app)
    {
        foreach (var middleware in Middlewares)
        {
            app.Use(next => context => middleware(context, () => next(context)));
        }
        return app;
    }

    public static void AddMiddleware(MiddlewareDelegate middleware)
    {
        Middlewares.Add(middleware);
    }
}
