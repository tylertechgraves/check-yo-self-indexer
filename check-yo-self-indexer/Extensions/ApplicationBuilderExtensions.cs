using System;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace check_yo_self_indexer.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {       
        // Configure XSRF middleware, This pattern is for SPA style applications where XSRF token is added on Index page 
        // load and passed back token on every subsequent async request            
        public static IApplicationBuilder UseXsrf(this IApplicationBuilder app)
        {
            var antiforgery = app.ApplicationServices.GetRequiredService<IAntiforgery>();

            app.Use(async (context, next) =>
            {
                if (string.Equals(context.Request.Path.Value, "/", StringComparison.OrdinalIgnoreCase))
                {
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
                }
                await next.Invoke();
            });

            return app;
        }
        public static IApplicationBuilder AddDevMiddlewares(this IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            return app;
        }
    }
}
