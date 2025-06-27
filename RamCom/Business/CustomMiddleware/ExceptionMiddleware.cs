using Business.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Business.CustomMiddleware
{
    //Should have only asynchronous menthods for nonblocking UI
    public class ExceptionMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(ResourseNotFoundException ex)
            {
                string errorMessage = $"Resource {ex.ResourceName} not found.";
                _logger.LogError(ex, errorMessage);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsJsonAsync(new { Message = errorMessage, StatusCode = context.Response.StatusCode });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                
                //await context.Response.WriteAsync(response);
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Home/Error"); // 🔁 Safe redirect
                }
                else
                {
                    // Fallback if response already started
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    string response = new { Message = "An unexpected exception couured.Please try later.", StatusCode = context.Response.StatusCode }.ToString();
                    await context.Response.WriteAsync(response);
                }
            
            }
        }
    }
}
