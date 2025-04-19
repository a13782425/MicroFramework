#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Diagnostics;

namespace MFramework.Core.Promise
{
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseInvalidOperationException : System.InvalidOperationException
    {
        public PromiseInvalidOperationException(string message, string stackTrace = null) : base(message)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseInvalidArgumentException : ArgumentException
    {
        public PromiseInvalidArgumentException(string paramName, string message, string stackTrace = null) : base(message, paramName)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseEmptyArgumentException : ArgumentException
    {
        public PromiseEmptyArgumentException(string paramName, string message, string stackTrace = null) : base(message, paramName)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseArgumentNullException : System.ArgumentNullException
    {
        public PromiseArgumentNullException(string paramName, string message, string stackTrace = null) : base(paramName, message)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseArgumentOutOfRangeException : System.ArgumentOutOfRangeException
    {
        public PromiseArgumentOutOfRangeException(string paramName, string message, string stackTrace = null) : base(paramName, message)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseInvalidElementException : PromiseInvalidArgumentException
    {
        public PromiseInvalidElementException(string paramName, string message, string stackTrace = null) : base(paramName, message)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseInvalidReturnException : System.InvalidOperationException
    {
        public PromiseInvalidReturnException(string message, string stackTrace = null, Exception innerException = null) : base(message, innerException)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseUnhandledDeferredException : Exception
    {
        public static readonly PromiseUnhandledDeferredException instance =
            new PromiseUnhandledDeferredException("A Deferred object was garbage collected that was not handled. You must Resolve, Reject, or Cancel all Deferred objects.");

        private PromiseUnhandledDeferredException(string message) : base(message) { }
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseUnreleasedObjectException : Exception
    {
        public PromiseUnreleasedObjectException(string message, string stackTrace = null, Exception innerException = null) : base(message, innerException)
        {
            _stackTrace = stackTrace;
        }

        private readonly string _stackTrace;
        public override string StackTrace => _stackTrace ?? base.StackTrace;
    }

#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseUnobservedPromiseException : Exception
    {
        public PromiseUnobservedPromiseException(string message) : base(message) { }
    }


    /// <summary>
    /// Exception that is thrown if a promise is rejected and that rejection is never handled.
    /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public abstract class PromiseUnhandledException : Exception
    {
        private readonly object _value;
        private readonly string _stackTrace;

        internal PromiseUnhandledException(object value, string message, string stackTrace, Exception innerException) : base(message, innerException)
        {
            _value = value;
            _stackTrace = stackTrace;
        }

        public override string StackTrace => _stackTrace;

        public object Value => _value;
    }

    /// <summary>
    /// Exception that is used to propagate cancelation of an operation.
    /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public abstract class PromiseCanceledException : OperationCanceledException
    {
        internal PromiseCanceledException(string message) : base(message) { }
    }


    /// <summary>
    /// Special Exception that is used to rethrow a rejection from a Promise onRejected callback.
    /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public class PromiseRethrowException : Exception, Internal.IRejectionToContainer
    {
        // Old Unity runtime has a bug where stack traces are continually appended to the exception, causing a memory leak and runtime slowdowns.
        // To avoid the issue, we only use a singleton in runtimes where the bug is not present.
#if PROMISE_DEBUG || NETSTANDARD2_0 || (UNITY_2018_3_OR_NEWER && !UNITY_2021_2_OR_NEWER)
        // Don't re-use instance in DEBUG mode so that we can read its stacktrace on any thread.
        internal static PromiseRethrowException GetOrCreate() => new PromiseRethrowException();
#else
        private static readonly PromiseRethrowException s_instance = new PromiseRethrowException();

        internal static PromiseRethrowException GetOrCreate() => s_instance;
#endif

        protected PromiseRethrowException() { }

        Internal.IRejectContainer Internal.IRejectionToContainer.ToContainer(Internal.ITraceable traceable)
        {
#if PROMISE_DEBUG
            string stacktrace = Internal.FormatStackTrace(new StackTrace[1] { new StackTrace(this, true) });
#else
            string stacktrace = new StackTrace(this, true).ToString();
#endif
            Exception exception = new PromiseInvalidOperationException("RethrowException is only valid in promise onRejected callbacks.", stacktrace);
            return Internal.RejectionContainerException.Create(exception, int.MinValue, null, traceable);
        }
    }

    /// <summary>
    /// Special Exception that is used to reject a Promise from an onResolved or onRejected callback.
    /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode]
#endif
    public abstract class PromiseRejectException : Exception
    {
        internal PromiseRejectException() { }

        public override string Message => "This is used to reject a Promise from an onResolved or onRejected handler.";
    }
}