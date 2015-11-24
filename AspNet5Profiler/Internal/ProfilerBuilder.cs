using AspNet5Profiler.DependencyInjection;
using System;
using Microsoft.Framework.DependencyInjection;

namespace AspNet5Profiler.Internal
{
    public class ProfilerBuilder : IProfilerBuilder
    {
        public ProfilerBuilder(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Services = services;

            Services.AddScoped<Profiler>();
        }

        public IServiceCollection Services { get; }
    }
}
