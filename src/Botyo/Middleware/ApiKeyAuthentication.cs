using Microsoft.Extensions.Primitives;

namespace Botyo.Middleware
{
    public class ApiKeyAuthentication(RequestDelegate next, IConfiguration configuration)
    {
        public Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(new("/swagger")))
                return next(context);

            if (context.Request.Headers.TryGetValue("ApiKey", out StringValues values) && values.Any(x => x == configuration.GetValue<string>("ApiKey")))
                return next(context);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
    }
}