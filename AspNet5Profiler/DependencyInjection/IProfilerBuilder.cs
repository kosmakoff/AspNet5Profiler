using Microsoft.Framework.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNet5Profiler.DependencyInjection
{
    public interface IProfilerBuilder
    {
        IServiceCollection Services { get; }
    }
}
