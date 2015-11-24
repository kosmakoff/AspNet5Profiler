using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;

namespace AspNet5Profiler
{
    public static class ProfilerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseProfiler(this IApplicationBuilder app)
        {
            var routes = new RouteBuilder();

            // add profiler specific routes
            // app.UseRouter(routes.Build());

            // add profiler middleware
            app.UseMiddleware<ProfilerMiddleware>();

            return app;
        }
    }
}
