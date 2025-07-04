﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0251 // Make member 'readonly'

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MFramework.Promise
{
    partial class Internal
    {
        internal interface IParallelEnumerator<T> : IDisposable
        {
            bool TryMoveNext(object lockObj, ref bool stopExecuting, out T value);
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal struct ForLoopEnumerator : IParallelEnumerator<int>
        {
            private int _current;
            private readonly int _toIndex;

            [MethodImpl(InlineOption)]
            internal ForLoopEnumerator(int fromIndex, int toIndex)
            {
                _current = fromIndex;
                _toIndex = toIndex;
            }

            [MethodImpl(InlineOption)]
            public bool TryMoveNext(object lockObj, ref bool stopExecuting, out int value)
            {
                int current = _current;
                while (true)
                {
                    // Interlocked.CompareExchange has an implicit memory barrier, so we can get away without volatile read of stopExecuting.
                    if (current >= _toIndex | stopExecuting)
                    {
                        value = 0;
                        return false;
                    }
                    int oldValue = Interlocked.CompareExchange(ref _current, current + 1, current);
                    if (oldValue == current)
                    {
                        value = current;
                        return true;
                    }
                    current = oldValue;
                }
            }

            [MethodImpl(InlineOption)]
            void IDisposable.Dispose() { }
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal struct GenericEnumerator<TEnumerator, T> : IParallelEnumerator<T>
            where TEnumerator : IEnumerator<T>
        {
            // This must not be readonly, in case the enumerator is a struct.
#pragma warning disable IDE0044 // Add readonly modifier
            private TEnumerator _enumerator;
#pragma warning restore IDE0044 // Add readonly modifier

            [MethodImpl(InlineOption)]
            internal GenericEnumerator(TEnumerator enumerator)
            {
                _enumerator = enumerator;
            }

            [MethodImpl(InlineOption)]
            public bool TryMoveNext(object lockObj, ref bool stopExecuting, out T value)
            {
                // Get the next element from the enumerator. This requires locking around MoveNext/Current.
                lock (lockObj)
                {
                    if (stopExecuting || !_enumerator.MoveNext())
                    {
                        stopExecuting = true;
                        // Exit the lock before writing the value.
                        goto ReturnFalse;
                    }

                    value = _enumerator.Current;
                }
                return true;

            ReturnFalse:
                value = default;
                return false;
            }

            [MethodImpl(InlineOption)]
            void IDisposable.Dispose()
            {
                _enumerator.Dispose();
            }
        }

        internal interface IParallelBody<TSource>
        {
            Promise Invoke(TSource source, CancelationToken cancelationToken);
        }

        internal readonly struct ParallelBody<TSource> : IParallelBody<TSource>
        {
            private readonly Func<TSource, CancelationToken, Promise> _body;

            [MethodImpl(InlineOption)]
            internal ParallelBody(Func<TSource, CancelationToken, Promise> body)
            {
                _body = body;
            }

            [MethodImpl(InlineOption)]
            Promise IParallelBody<TSource>.Invoke(TSource source, CancelationToken cancelationToken)
                => _body.Invoke(source, cancelationToken);
        }

        internal readonly struct ParallelCaptureBody<TSource, TCapture> : IParallelBody<TSource>
        {
            private readonly Func<TSource, TCapture, CancelationToken, Promise> _body;
            private readonly TCapture _capturedValue;

            [MethodImpl(InlineOption)]
            internal ParallelCaptureBody(TCapture capturedValue, Func<TSource, TCapture, CancelationToken, Promise> body)
            {
                _capturedValue = capturedValue;
                _body = body;
            }

            [MethodImpl(InlineOption)]
            Promise IParallelBody<TSource>.Invoke(TSource source, CancelationToken cancelationToken)
                => _body.Invoke(source, _capturedValue, cancelationToken);
        }

        internal static Promise ParallelFor<TParallelBody>(int fromIndex, int toIndex, TParallelBody body, CancelationToken cancelationToken, SynchronizationContext synchronizationContext, int maxDegreeOfParallelism)
            where TParallelBody : IParallelBody<int>
        {
            if (maxDegreeOfParallelism == -1)
            {
                maxDegreeOfParallelism = Environment.ProcessorCount;
            }
            else if (maxDegreeOfParallelism < 1)
            {
                throw new PromiseArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "maxDegreeOfParallelism must be positive, or -1 for default (Environment.ProcessorCount). Actual: " + maxDegreeOfParallelism, GetFormattedStacktrace(2));
            }

            // Just return immediately if from >= to.
            if (fromIndex >= toIndex)
            {
                return Promise.Resolved();
            }

            // One fast up-front check for cancelation before we start the whole operation.
            if (cancelationToken.IsCancelationRequested)
            {
                return Promise.Canceled();
            }

            var promise = PromiseRefBase.PromiseParallelForEach<ForLoopEnumerator, TParallelBody, int>.GetOrCreate(
                new ForLoopEnumerator(fromIndex, toIndex), body, cancelationToken, synchronizationContext, maxDegreeOfParallelism);
            promise.MaybeLaunchWorker(true);
            return new Promise(promise, promise.Id);
        }

        internal static Promise ParallelForEach<TEnumerator, TParallelBody, TSource>(TEnumerator enumerator, TParallelBody body, CancelationToken cancelationToken, SynchronizationContext synchronizationContext, int maxDegreeOfParallelism)
            where TEnumerator : IEnumerator<TSource>
            where TParallelBody : IParallelBody<TSource>
        {
            if (maxDegreeOfParallelism == -1)
            {
                maxDegreeOfParallelism = Environment.ProcessorCount;
            }
            else if (maxDegreeOfParallelism < 1)
            {
                enumerator.Dispose();
                throw new PromiseArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "maxDegreeOfParallelism must be positive, or -1 for default (Environment.ProcessorCount). Actual: " + maxDegreeOfParallelism, GetFormattedStacktrace(2));
            }

            // One fast up-front check for cancelation before we start the whole operation.
            if (cancelationToken.IsCancelationRequested)
            {
                enumerator.Dispose();
                return Promise.Canceled();
            }

            var promise = PromiseRefBase.PromiseParallelForEach<GenericEnumerator<TEnumerator, TSource>, TParallelBody, TSource>.GetOrCreate(
                new GenericEnumerator<TEnumerator, TSource>(enumerator), body, cancelationToken, synchronizationContext, maxDegreeOfParallelism);
            promise.MaybeLaunchWorker(true);
            return new Promise(promise, promise.Id);
        }

        partial class PromiseRefBase
        {
            // Inheriting PromiseSingleAwait<VoidResult> instead of PromiseRefBase so we can take advantage of the already implemented methods.
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class PromiseParallelForEach<TEnumerator, TParallelBody, TSource> : PromiseSingleAwait<VoidResult>, ICancelable
                where TEnumerator : IParallelEnumerator<TSource>
                where TParallelBody : IParallelBody<TSource>
            {
                private TParallelBody _body;
                private TEnumerator _enumerator;
                private CancelationRegistration _externalCancelationRegistration;
                // Use the CancelationRef directly instead of CancelationSource struct to save memory.
                private CancelationRef _cancelationRef;
                private SynchronizationContext _synchronizationContext;
                private ExecutionContext _executionContext;
                private int _remainingAvailableWorkers;
                private int _waitCounter;
                private List<Exception> _exceptions;
                private Promise.State _completionState;
                private bool _stopExecuting;

                private PromiseParallelForEach() { }

                [MethodImpl(InlineOption)]
                private static PromiseParallelForEach<TEnumerator, TParallelBody, TSource> GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new PromiseParallelForEach<TEnumerator, TParallelBody, TSource>()
                        : obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>();
                }

                internal static PromiseParallelForEach<TEnumerator, TParallelBody, TSource> GetOrCreate(TEnumerator enumerator, TParallelBody body, CancelationToken cancelationToken, SynchronizationContext synchronizationContext, int maxDegreeOfParallelism)
                {
                    var promise = GetOrCreate();
                    promise.Reset();
                    promise._enumerator = enumerator;
                    promise._body = body;
                    promise._synchronizationContext = synchronizationContext ?? BackgroundSynchronizationContextSentinel.s_instance;
                    promise._remainingAvailableWorkers = maxDegreeOfParallelism;
                    promise._completionState = Promise.State.Resolved;
                    promise._stopExecuting = false;
                    promise._cancelationRef = CancelationRef.GetOrCreate();
                    cancelationToken.TryRegister(promise, out promise._externalCancelationRegistration);
                    if (Promise.Config.AsyncFlowExecutionContextEnabled)
                    {
                        promise._executionContext = ExecutionContext.Capture();
                    }
                    return promise;
                }

                internal override void MaybeDispose()
                {
                    ValidateNoPending();
                    Dispose();
                    _body = default;
                    _synchronizationContext = null;
                    _executionContext = null;
                    ObjectPool.MaybeRepool(this);
                }

                public void Cancel()
                {
                    _completionState = Promise.State.Canceled;
                    CancelWorkers();
                }

                internal void MaybeLaunchWorker(bool launchWorker)
                {
                    if (launchWorker & _remainingAvailableWorkers > 0)
                    {
                        --_remainingAvailableWorkers;
                        // We add to the wait counter before we run the worker to resolve a race condition where the counter could hit zero prematurely.
                        InterlockedAddWithUnsignedOverflowCheck(ref _waitCounter, 1);

                        ScheduleContextCallback(_synchronizationContext, this,
                            obj => obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>().ExecuteWorkerAndLaunchNext(),
                            obj => obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>().ExecuteWorkerAndLaunchNext()
                        );
                    }
                }

                private void ExecuteWorkerAndLaunchNext()
                {
                    if (_executionContext == null)
                    {
                        ExecuteWorker(true);
                    }
                    else
                    {
                        // .Net Framework doesn't allow us to re-use a captured context, so we have to copy it for each invocation.
                        // .Net Core's implementation of CreateCopy returns itself, so this is always as efficient as it can be.
                        ExecutionContext.Run(_executionContext.CreateCopy(), obj => obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>().ExecuteWorker(true), this);
                    }
                }

                private void ExecuteWorkerWithoutLaunchNext()
                {
                    if (_executionContext == null)
                    {
                        ExecuteWorker(false);
                    }
                    else
                    {
                        // .Net Framework doesn't allow us to re-use a captured context, so we have to copy it for each invocation.
                        // .Net Core's implementation of CreateCopy returns itself, so this is always as efficient as it can be.
                        ExecutionContext.Run(_executionContext.CreateCopy(), obj => obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>().ExecuteWorker(false), this);
                    }
                }

                private void ExecuteWorker(bool launchNext)
                {
                    SetCurrentInvoker(this);
                    try
                    {
                        WorkerBody(launchNext);
                    }
                    catch (OperationCanceledException)
                    {
                        _completionState = Promise.State.Canceled;
                        CancelWorkersAndMaybeComplete();
                    }
                    catch (Exception e)
                    {
                        // Record the failure and then don't let the exception propagate. The last worker to complete
                        // will propagate exceptions as is appropriate to the top-level promise.
                        RecordException(e);
                        CancelWorkersAndMaybeComplete();
                    }
                    ClearCurrentInvoker();
                }

                private void WorkerBody(bool launchNext)
                {
                    // The worker body. Each worker will execute this same body.
                    while (true)
                    {
                        if (!_enumerator.TryMoveNext(this, ref _stopExecuting, out var element))
                        {
                            MaybeComplete();
                            return;
                        }

                        // If the available workers allows it and we've not yet queued the next worker, do so now.  We wait
                        // until after we've grabbed an item from the enumerator to a) avoid unnecessary contention on the
                        // serialized resource, and b) avoid queueing another work if there aren't any more items.  Each worker
                        // is responsible only for creating the next worker, which in turn means there can't be any contention
                        // on creating workers (though it's possible one worker could be executing while we're creating the next).
                        MaybeLaunchWorker(launchNext);

                        // Process the loop body.
                        var promise = _body.Invoke(element, new CancelationToken(_cancelationRef, _cancelationRef.TokenId));
                        ValidateReturn(promise);

                        if (promise._ref != null)
                        {
                            // The promise may still be pending, hook this up to rerun the loop when it completes.
                            AddPending(promise._ref);
                            promise._ref.HookupExistingWaiter(promise._id, this);
                            return;
                        }

                        // The promise was already complete successfully. Rerun the loop synchronously without launching another worker.
                        launchNext = false;
                    }
                }

                internal override void Handle(PromiseRefBase handler, Promise.State state)
                {
                    RemoveComplete(handler);
                    var rejectContainer = handler._rejectContainer;
                    handler.SuppressRejection = true;
                    handler.SetCompletionState(state);
                    handler.MaybeDispose();

                    if (state == Promise.State.Resolved)
                    {
                        // Schedule the worker body to run again on the context, but without launching another worker.
                        ScheduleContextCallback(_synchronizationContext, this,
                            obj => obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>().ExecuteWorkerWithoutLaunchNext(),
                            obj => obj.UnsafeAs<PromiseParallelForEach<TEnumerator, TParallelBody, TSource>>().ExecuteWorkerWithoutLaunchNext()
                        );
                    }
                    else if (state == Promise.State.Canceled)
                    {
                        _completionState = Promise.State.Canceled;
                        CancelWorkersAndMaybeComplete();
                    }
                    else
                    {
                        CancelWorkers();
                        // Record the failure. The last worker to complete will propagate exceptions as is appropriate to the top-level promise.
                        RecordException(rejectContainer.GetValueAsException());
                        MaybeComplete();
                    }
                }

                private void RecordException(Exception e)
                {
                    lock (this)
                    {
                        Internal.RecordException(e, ref _exceptions);
                    }
                }

                private void CancelWorkers()
                {
                    _stopExecuting = true;
                    // We cancel the source to notify the workers that they don't need to continue processing.
                    // This may be called multiple times. It's fine because it checks internally if it's already canceled.
                    try
                    {
                        _cancelationRef.Cancel();
                    }
                    catch (Exception e)
                    {
                        RecordException(e);
                    }
                }

                private void CancelWorkersAndMaybeComplete()
                {
                    CancelWorkers();
                    MaybeComplete();
                }

                private void MaybeComplete()
                {
                    // If we're the last worker to complete, clean up and complete the operation.
                    if (InterlockedAddWithUnsignedOverflowCheck(ref _waitCounter, -1) != 0)
                    {
                        return;
                    }

                    _externalCancelationRegistration.Dispose();
                    _externalCancelationRegistration = default;
                    _cancelationRef.TryDispose(_cancelationRef.SourceId);
                    _cancelationRef = null;

                    try
                    {
                        _enumerator.Dispose();
                    }
                    catch (Exception e)
                    {
                        RecordException(e);
                    }

                    // Finally, complete the promise returned to the ParallelForEach caller.
                    // This must be the very last thing done.
                    if (_exceptions != null)
                    {
                        _rejectContainer = CreateRejectContainer(new AggregateException(_exceptions), int.MinValue, null, this);
                        _exceptions = null;
                        HandleNextInternal(Promise.State.Rejected);
                    }
                    else
                    {
                        HandleNextInternal(_completionState);
                    }
                }

                partial void ValidateNoPending();
                partial void AddPending(PromiseRefBase pendingPromise);
                partial void RemoveComplete(PromiseRefBase completePromise);
            } // class PromiseParallelForEach
        } // class PromiseRefBase
    } // class Internal
}