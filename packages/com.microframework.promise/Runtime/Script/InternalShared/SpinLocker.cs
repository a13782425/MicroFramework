﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MFramework.Promise
{
    partial class Internal
    {
        /// <summary>
        /// Use instead of Monitor.Enter(object).
        /// Must not be readonly.
        /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal struct SpinLocker
        {
            volatile private int _locker;

            internal void Enter()
            {
                if (!TryEnter())
                {
                    EnterCore();
                }
            }

            [MethodImpl(InlineOption)]
            internal bool TryEnter()
                => Interlocked.Exchange(ref _locker, 1) == 0;

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void EnterCore()
            {
                // Spin until we successfully get lock.
                var spinner = new SpinWait();
                do
                {
                    spinner.SpinOnce();
                }
                while (!TryEnter());
            }

            [MethodImpl(InlineOption)]
            internal void Exit()
                => _locker = 0; // Release lock.
        }
    } // class Internal
} // namespace Proto.Promises