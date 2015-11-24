using System;
using System.Diagnostics;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Http;
#if DNX451
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif

namespace AspNet5Profiler
{
    public class Profiler
    {
        private readonly Stopwatch _sw;
        private readonly ILogger _logger;

        // TODO: figure out how to pass regular parameters along with injected ones
        public Profiler(ILoggerFactory loggerFactory, IHttpContextAccessor contextAccessor)
        {
            _logger = loggerFactory.CreateLogger<Profiler>();

            Id = Guid.NewGuid();
            Started = DateTime.UtcNow;

            _sw = Stopwatch.StartNew();

            Root = new Timing(this, contextAccessor.HttpContext.Request.Path);
        }

        public long ElapsedMilliseconds => _sw.ElapsedMilliseconds;

        public Guid Id { get; }

        public Timing Root { get; set; }

        public DateTime Started { get; set; }

#if DNX451
        public Timing Head
        {
            get
            {
                var ret = CallContext.LogicalGetData($"Profiler.{Id}") as Timing;
                return ret;
            }
            set
            {
                CallContext.LogicalSetData($"Profiler.{Id}", value);
            }
        }
#else
        private readonly AsyncLocal<Timing> _headTimingAsyncLocal = new AsyncLocal<Timing>();

        public Timing Head
        {
            get { return _headTimingAsyncLocal.Value; }
            set { _headTimingAsyncLocal.Value = value; }
        }
#endif

        public void Start()
        {

        }

        public void Stop()
        {
            Root.Stop();
        }

        internal IDisposable StepImpl(string name)
        {
            return new Timing(this, name);
        }

        public void Printout()
        {
            _logger.LogInformation("Profiler data:");

            PrintoutTiming(Root);
        }

        private void PrintoutTiming(Timing timing, int level = 0)
        {
            if (timing == null)
                return;

            var shift = new string(' ', level);

            _logger.LogInformation($"{shift}{timing.Name} took {timing.DurationMilliseconds} ms in total ({timing.StartMilliseconds} -> {timing.StopMilliseconds}):");

            foreach (var childTiming in timing.Children)
            {
                PrintoutTiming(childTiming, level + 2);
            }
        }
    }
}
