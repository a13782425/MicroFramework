using System;
using System.Collections.Generic;

namespace MFramework.Task
{
    public readonly struct MicroTaskCancellationToken
    {
        private readonly MicroTaskCancellationTokenSource source;

        internal MicroTaskCancellationToken(MicroTaskCancellationTokenSource source)
        {
            this.source = source;
        }

        public bool IsCancellationRequested => source?.IsCancellationRequested ?? false;

        public bool CanBeCanceled => source != null;

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
                throw new OperationCanceledException();
        }

        public MicroTaskCancellationRegistration Register(Action callback)
        {
            if (source == null)
                return new MicroTaskCancellationRegistration();

            return source.Register(callback);
        }

        public static MicroTaskCancellationToken None => new MicroTaskCancellationToken(null);
    }

    public class MicroTaskCancellationTokenSource : IDisposable
    {
        private bool disposed;
        private bool cancellationRequested;
        private readonly List<Action> registeredCallbacks;
        private readonly object lockObject = new object();

        public MicroTaskCancellationTokenSource()
        {
            registeredCallbacks = new List<Action>();
        }

        public MicroTaskCancellationToken Token => new MicroTaskCancellationToken(this);

        public bool IsCancellationRequested
        {
            get
            {
                ThrowIfDisposed();
                return cancellationRequested;
            }
        }

        public void Cancel()
        {
            ThrowIfDisposed();

            bool shouldNotify = false;
            Action[] callbacks = null;

            lock (lockObject)
            {
                if (!cancellationRequested)
                {
                    cancellationRequested = true;
                    shouldNotify = true;
                    callbacks = registeredCallbacks.ToArray();
                    registeredCallbacks.Clear();
                }
            }

            if (shouldNotify && callbacks != null)
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback();
                    }
                    catch (Exception)
                    {
                        // Swallow exceptions from callbacks
                    }
                }
            }
        }

        internal MicroTaskCancellationRegistration Register(Action callback)
        {
            ThrowIfDisposed();

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            bool shouldInvokeImmediately = false;
            lock (lockObject)
            {
                if (cancellationRequested)
                {
                    shouldInvokeImmediately = true;
                }
                else
                {
                    registeredCallbacks.Add(callback);
                }
            }

            if (shouldInvokeImmediately)
            {
                callback();
            }

            return new MicroTaskCancellationRegistration(this, callback);
        }

        internal void Unregister(Action callback)
        {
            if (disposed || callback == null)
                return;

            lock (lockObject)
            {
                registeredCallbacks.Remove(callback);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MicroTaskCancellationTokenSource));
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            lock (lockObject)
            {
                registeredCallbacks.Clear();
            }
        }
    }

    public readonly struct MicroTaskCancellationRegistration : IDisposable
    {
        private readonly MicroTaskCancellationTokenSource source;
        private readonly Action callback;

        internal MicroTaskCancellationRegistration(MicroTaskCancellationTokenSource source, Action callback)
        {
            this.source = source;
            this.callback = callback;
        }

        public void Dispose()
        {
            source?.Unregister(callback);
        }
    }
}