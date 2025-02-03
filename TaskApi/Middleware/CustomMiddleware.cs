using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// Logs request body, logs response body, ensures stream positions reset so pipeline cont normally, also tracks time taken for tasks to execute

namespace TaskApi.Middleware
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomMiddleware> _logger;

        public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

            try
            {
                // Read and log the request body
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset stream position
                _logger.LogInformation($"Request Body: {requestBody}");

                // Capture response body
                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context); // Continue the pipeline

                    // Read and log the response body
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);

                    _logger.LogInformation($"Outgoing response: {context.Response.StatusCode}, Body: {responseText}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An unexpected error occurred.");
            }

            stopwatch.Stop();
            _logger.LogInformation($"Request completed in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
