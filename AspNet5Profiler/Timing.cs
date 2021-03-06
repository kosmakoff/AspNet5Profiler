﻿using System;
using System.Collections.Generic;

namespace AspNet5Profiler
{
    public class Timing : IDisposable
    {
        public Timing(Profiler profiler, string name)
        {
            Id = Guid.NewGuid();
            Profiler = profiler;

            ParentTiming = Profiler.Head;

            if (ParentTiming != null)
            {
                ParentTiming.AddChild(this);
            }

            Profiler.Head = this;

            Name = name;
            StartMilliseconds = profiler.ElapsedMilliseconds;
        }

        #region Public Properties

        public Guid Id { get; }

        public string Name { get; }

        public List<Timing> Children { get; set; } = new List<Timing>();

        public Timing ParentTiming { get; set; }

        public long StartMilliseconds { get; set; }

        public long? StopMilliseconds { get; set; }

        public long DurationMilliseconds => StopMilliseconds.HasValue ? StopMilliseconds.Value - StartMilliseconds : 0;

        #endregion

        internal Profiler Profiler { get; set; }

        #region Public Methods

        public void AddChild(Timing timing)
        {
            lock(Children)
            {
                Children.Add(timing);
            }

            timing.Profiler = Profiler;
            timing.ParentTiming = this;
        }

        public void Stop()
        {
            if (StopMilliseconds != null)
                return;

            StopMilliseconds = Profiler.ElapsedMilliseconds;
            Profiler.Head = ParentTiming;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                Stop();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Timing() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
