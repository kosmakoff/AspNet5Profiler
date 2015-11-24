using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using System;

namespace AspNet5Profiler
{
    public static class ProfilerExtensions
    {
        public static Profiler GetProfiler(this HttpContext context)
        {
            return context.RequestServices?.GetService<Profiler>();
        }

        public static IDisposable Step(this Profiler profiler, string name)
        {
            return profiler == null ? null : profiler.StepImpl(name);
        }
    }
}
