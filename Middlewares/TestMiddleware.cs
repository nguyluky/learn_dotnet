
namespace test_api.Middlewares
{
    public class TestMiddleware
    {
        private readonly RequestDelegate _next;

        public TestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // make this to logging
            var request = context.Request;
            var method = request.Method;
            var path = request.Path;
            var query = request.QueryString;

            // Log the request details
            Console.WriteLine($"Request: {method} {path}{query}");
            await _next(context);

        }
    }
}
