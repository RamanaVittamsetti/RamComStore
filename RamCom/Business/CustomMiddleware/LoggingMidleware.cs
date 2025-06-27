using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Business.CustomMiddleware
{
    public class LoggingMidleware
    {
        private readonly ILogger<LoggingMidleware> _logger;
        private readonly RequestDelegate _next;
        public LoggingMidleware(ILogger<LoggingMidleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Middleware execution started for the URL {context.Request.GetDisplayUrl}");
            await _next(context);
            _logger.LogInformation($"Middleware execution Ended for the URL {context.Request.GetDisplayUrl}, Status Code : {context.Response.StatusCode}");
        }

    }
}
