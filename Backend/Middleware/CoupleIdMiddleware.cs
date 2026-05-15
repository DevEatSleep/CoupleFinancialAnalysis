using System.Security.Claims;

namespace CoupleChat.Middleware;

public class CoupleIdMiddleware
{
    private readonly RequestDelegate _next;

    public CoupleIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract coupleId from JWT claims
        var coupleIdClaim = context.User.FindFirst("coupleId")?.Value;
        
        if (coupleIdClaim != null && int.TryParse(coupleIdClaim, out var coupleId))
        {
            context.Items["CoupleId"] = coupleId;
        }

        await _next(context);
    }
}

public static class CoupleIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCoupleIdMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CoupleIdMiddleware>();
    }
}
