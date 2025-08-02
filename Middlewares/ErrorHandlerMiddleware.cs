
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using test_api.Models.Responses;

namespace test_api.Middleware
{

    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            var result = exception.Message;

            var apiResponse = ApiResponse<string>.ErrorResponse(result);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsJsonAsync(apiResponse);
        }
    }
}
