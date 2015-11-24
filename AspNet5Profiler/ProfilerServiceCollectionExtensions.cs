using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using AspNet5Profiler.DependencyInjection;
using AspNet5Profiler.Internal;

namespace AspNet5Profiler
{
    public static class ProfilerServiceCollectionExtensions
    {
        public static IProfilerBuilder AddProfiler(this IServiceCollection services)
        {
            return new ProfilerBuilder(services);
        }
    }
}
