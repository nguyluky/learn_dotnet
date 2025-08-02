using test_api.Models.Entities;

namespace test_api.extensions
{
    public static class HttpContextExtensions
    {
        public static User? GetUser(this HttpContext context)
        {
            return context.Items["User"] as User;
        }

        public static int? GetUserId(this HttpContext context)
        {
            return context.Items["UserId"] as int?;
        }

        public static bool IsAuthenticated(this HttpContext context)
        {
            return context.Items["User"] != null;
        }
    }
}
