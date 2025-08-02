

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using test_api.Data;
using Microsoft.EntityFrameworkCore;
using test_api.Models.Entities;

namespace test_api.Middlewares
{
    public class DynamicAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public DynamicAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, StoreContext dbContext, ILogger<DynamicAuthorizationMiddleware> logger)
        {
            var endpoint = context.GetEndpoint() as RouteEndpoint;

            // 👇 Nếu có [AllowAnonymous] => bỏ qua
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                logger.LogInformation("Endpoint allows anonymous access, skipping authorization.");
                await _next(context);
                return;
            }

            var method = context.Request.Method.ToUpper();
            if (method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            // 👇 Nếu chưa đăng nhập
            if (!context.User.Identity!.IsAuthenticated)
            {
                logger.LogWarning("Unauthorized access attempt.");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
            // Lấy thông tin user và kiểm tra quyền
            var path =  "/" + endpoint?.RoutePattern.RawText;

            var userId = context.User.FindFirst("id")?.Value;
            if (userId == null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Missing user ID");
                return;
            }

            bool isAdmin = await dbContext.Users
                .Where(u => u.Id.ToString() == userId)
                .Select(u => u.Rule.IsAdmin)
                .AnyAsync(r => r == true);

            if (isAdmin)
            {
                // Admin users have access to all endpoints
                await _next(context);
                return;
            }

            User user = await dbContext.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("User not found");
                return;
            }

            Rule rule = await dbContext.Roles
                .Where(r => r.Id == user.RuleId)
                .Include(r => r.ActionPermissions)
                .FirstOrDefaultAsync();
            
            if (rule == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Role not found");
                return;
            }
            logger.LogInformation($"Check access for user {context.User.FindFirst("id").Value} to {method} {path}");

            // Check if the user has permission for the requested action
            bool hasAccess = rule.ActionPermissions.Any(ap =>
            {
                Console.WriteLine($"Checking permission: {ap.Method} {ap.Path}");
                return ap.Method == method && ap.Path == path;
            });
                
            



            if (!hasAccess)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            await _next(context);
        }
    }
}
