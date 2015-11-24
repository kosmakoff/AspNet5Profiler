using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using System.Threading.Tasks;
using System;

namespace AspNet5Profiler
{
    public class ProfilerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ProfilerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ProfilerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            // resolve Profiler right away
            var profiler = context.GetProfiler();

            await _next.Invoke(context);

            profiler.Stop();

            context.Response.Headers.Add("X-Profiler-Id", profiler.Id.ToString());

            profiler.Printout();
        }
    }
}
