﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MFramework.Promise
{
    partial class Internal
    {
        partial class PromiseRefBase : HandleablePromiseBase, ITraceable
        {
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class ConfiguredPromise<TResult> : PromiseSingleAwait<TResult>
            {
                private ConfiguredPromise() { }

                internal override void MaybeDispose()
                {
                    Dispose();
                    _synchronizationContext = null;
                    ObjectPool.MaybeRepool(this);
                }

                [MethodImpl(InlineOption)]
                private static ConfiguredPromise<TResult> GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<ConfiguredPromise<TResult>>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new ConfiguredPromise<TResult>()
                        : obj.UnsafeAs<ConfiguredPromise<TResult>>();
                }

                private static ConfiguredPromise<TResult> GetOrCreateBase(SynchronizationContext synchronizationContext, CompletedContinuationBehavior completedBehavior)
                {
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                    if (synchronizationContext == null)
                    {
#pragma warning disable IDE0016 // Use 'throw' expression
                        throw new PromiseInvalidOperationException("synchronizationContext cannot be null");
#pragma warning restore IDE0016 // Use 'throw' expression
                    }
#endif
                    var promise = GetOrCreate();
                    promise.Reset();
#if UNITY_2021_2_OR_NEWER || !UNITY_2018_3_OR_NEWER
                    // If the promise is converted to ValueTask, we ignore the context scheduling,
                    // since this is already configured to continue on a specified context.
                    promise._ignoreValueTaskContextScheduling = true;
#endif
                    promise._synchronizationContext = synchronizationContext;
                    promise._completedBehavior = completedBehavior;
                    return promise;
                }

                internal static ConfiguredPromise<TResult> GetOrCreate(SynchronizationContext synchronizationContext, CompletedContinuationBehavior completedBehavior)
                {
                    var promise = GetOrCreateBase(synchronizationContext, completedBehavior);
                    return promise;
                }

                internal static ConfiguredPromise<TResult> GetOrCreateFromResolved(SynchronizationContext synchronizationContext, in TResult result, CompletedContinuationBehavior completedBehavior)
                {
                    var promise = GetOrCreateBase(synchronizationContext, completedBehavior);
                    promise._result = result;
                    promise._tempState = Promise.State.Resolved;
                    promise._next = PromiseCompletionSentinel.s_instance;
                    return promise;
                }

                internal override void Handle(PromiseRefBase handler, Promise.State state)
                {
                    ThrowIfInPool(this);

                    handler.SetCompletionState(state);
                    _rejectContainer = handler._rejectContainer;
                    handler.SuppressRejection = true;
                    _result = handler.GetResult<TResult>();
                    _tempState = state;
                    handler.MaybeDispose();

                    // Leave pending until this is awaited.
                    if (ReadNextWaiterAndMaybeSetCompleted() == PendingAwaitSentinel.s_instance)
                    {
                        return;
                    }

                    ScheduleContinuationOnContext();
                }

                private void ScheduleContinuationOnContext()
                    => ScheduleContextCallback(_synchronizationContext, this,
                        obj => obj.UnsafeAs<ConfiguredPromise<TResult>>().HandleFromContext(),
                        obj => obj.UnsafeAs<ConfiguredPromise<TResult>>().HandleFromContext()
                    );

                [MethodImpl(InlineOption)]
                private void HandleFromContext()
                    => HandleNextInternal(_tempState);

                internal override PromiseRefBase AddWaiter(short promiseId, HandleablePromiseBase waiter, out HandleablePromiseBase previousWaiter)
                {
                    if (promiseId != Id)
                    {
                        previousWaiter = InvalidAwaitSentinel.s_instance;
                        return InvalidAwaitSentinel.s_instance;
                    }
                    ThrowIfInPool(this);
                    WasAwaitedOrForgotten = true;

                    var previous = CompareExchangeWaiter(waiter, PendingAwaitSentinel.s_instance);
                    if (previous != PendingAwaitSentinel.s_instance)
                    {
                        return VerifyAndHandleWaiter(waiter, out previousWaiter);
                    }
                    previousWaiter = PendingAwaitSentinel.s_instance;
                    return null; // It doesn't matter what we return since previousWaiter is set to PendingAwaitSentinel.s_instance.
                }

                [MethodImpl(InlineOption)]
                private bool GetShouldContinueImmediately()
                    => _completedBehavior == CompletedContinuationBehavior.Synchronous
                    || (_completedBehavior == CompletedContinuationBehavior.AllowSynchronous
                        && _synchronizationContext == Promise.Manager.ThreadStaticSynchronizationContext);

                // This is unusual, only happens when the promise already completed, or if a promise is incorrectly awaited twice.
                [MethodImpl(MethodImplOptions.NoInlining)]
                private PromiseRefBase VerifyAndHandleWaiter(HandleablePromiseBase waiter, out HandleablePromiseBase previousWaiter)
                {
                    // We do the verification process here instead of in the caller, because we need to handle continuations on the synchronization context.
                    bool shouldContinueImmediately = GetShouldContinueImmediately();
                    var setWaiter = shouldContinueImmediately ? InvalidAwaitSentinel.s_instance : waiter;
                    if (CompareExchangeWaiter(setWaiter, PromiseCompletionSentinel.s_instance) != PromiseCompletionSentinel.s_instance)
                    {
                        previousWaiter = InvalidAwaitSentinel.s_instance;
                        return InvalidAwaitSentinel.s_instance;
                    }

                    if (shouldContinueImmediately)
                    {
                        SetCompletionState(_tempState);
                        previousWaiter = waiter;
                        return null;
                    }

                    previousWaiter = PendingAwaitSentinel.s_instance;
                    ScheduleContinuationOnContext();
                    return null; // It doesn't matter what we return since previousWaiter is set to PendingAwaitSentinel.s_instance.
                }

                internal override bool GetIsCompleted(short promiseId)
                {
                    ValidateId(promiseId, this, 2);
                    ThrowIfInPool(this);
                    // Make sure the continuation executes on the synchronization context.
                    if (GetShouldContinueImmediately()
                        && CompareExchangeWaiter(InvalidAwaitSentinel.s_instance, PromiseCompletionSentinel.s_instance) == PromiseCompletionSentinel.s_instance)
                    {
                        WasAwaitedOrForgotten = true;
                        SetCompletionState(_tempState);
                        return true;
                    }
                    return false;
                }
            } // class ConfiguredPromise

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            private sealed class ConfiguredAsyncGenericContinuer : HandleablePromiseBase, ITraceable
            {
#if PROMISE_DEBUG
                CausalityTrace ITraceable.Trace { get; set; }
#endif
                private Action _continuation;
                private SynchronizationContext _context;

                private ConfiguredAsyncGenericContinuer() { }

                [MethodImpl(InlineOption)]
                private static ConfiguredAsyncGenericContinuer GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<ConfiguredAsyncGenericContinuer>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new ConfiguredAsyncGenericContinuer()
                        : obj.UnsafeAs<ConfiguredAsyncGenericContinuer>();
                }

                internal static ConfiguredAsyncGenericContinuer GetOrCreate(Action continuation, SynchronizationContext context)
                {
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                    if (context == null)
                    {
#pragma warning disable IDE0016 // Use 'throw' expression
                        throw new PromiseInvalidOperationException("context cannot be null");
#pragma warning restore IDE0016 // Use 'throw' expression
                    }
#endif

                    var continuer = GetOrCreate();
                    continuer._next = null;
                    continuer._continuation = continuation;
                    continuer._context = context;
                    SetCreatedStacktrace(continuer, 3);
                    return continuer;
                }

                [MethodImpl(InlineOption)]
                private void Dispose()
                {
                    _continuation = null;
                    _context = null;
                    ObjectPool.MaybeRepool(this);
                }

                internal override void Handle(PromiseRefBase handler, Promise.State state)
                {
                    ThrowIfInPool(this);
                    handler.SetCompletionState(state);

                    ScheduleContextCallback(_context, this,
                        obj => obj.UnsafeAs<ConfiguredAsyncGenericContinuer>().HandleFromContext(),
                        obj => obj.UnsafeAs<ConfiguredAsyncGenericContinuer>().HandleFromContext()
                    );
                }

                private void HandleFromContext()
                {
                    ThrowIfInPool(this);
                    var callback = _continuation;
#if PROMISE_DEBUG
                    SetCurrentInvoker(this);
#else
                    Dispose();
#endif
                    try
                    {
                        callback.Invoke();
                    }
                    catch (Exception e)
                    {
                        // This should never hit if the `await` keyword is used, but a user manually subscribing to OnCompleted could throw.
                        ReportRejection(e, this);
                    }
#if PROMISE_DEBUG
                    ClearCurrentInvoker();
                    Dispose();
#endif
                }
            } // class ConfiguredAsyncGenericContinuer

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class ConfiguredAsyncPromiseContinuer : HandleablePromiseBase
            {
                private SynchronizationContext _context;

                private ConfiguredAsyncPromiseContinuer() { }

                [MethodImpl(InlineOption)]
                private static ConfiguredAsyncPromiseContinuer GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<ConfiguredAsyncPromiseContinuer>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new ConfiguredAsyncPromiseContinuer()
                        : obj.UnsafeAs<ConfiguredAsyncPromiseContinuer>();
                }

                internal static void ConfigureAsyncContinuation(PromiseRefBase asyncPromiseRef, SynchronizationContext context, PromiseRefBase awaiter, short promiseId)
                {
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                    if (context == null)
                    {
                        throw new PromiseInvalidOperationException("context cannot be null");
                    }
#endif

                    asyncPromiseRef.ValidateAwait(awaiter, promiseId);
                    asyncPromiseRef.SetPrevious(awaiter);

                    if (awaiter == null)
                    {
                        ScheduleContextCallback(context, asyncPromiseRef,
                            obj => obj.UnsafeAs<PromiseRefBase>().ContinueAsyncFunction(),
                            obj => obj.UnsafeAs<PromiseRefBase>().ContinueAsyncFunction()
                        );
                        return;
                    }

                    var continuer = GetOrCreate();
                    continuer._next = asyncPromiseRef;
                    continuer._context = context;

                    awaiter.HookupNewWaiter(promiseId, continuer);
                }

                internal override void Handle(PromiseRefBase handler, Promise.State state)
                {
                    ThrowIfInPool(this);
                    handler.SetCompletionState(state);

                    var asyncPromiseRef = _next;
                    var context = _context;
                    _context = null;
                    ObjectPool.MaybeRepool(this);

                    ScheduleContextCallback(context, asyncPromiseRef,
                        obj => obj.UnsafeAs<PromiseRefBase>().ContinueAsyncFunction(),
                        obj => obj.UnsafeAs<PromiseRefBase>().ContinueAsyncFunction()
                    );
                }
            } // class ConfiguredAsyncPromiseContinuer
        } // class PromiseRefBase
    } // class Internal
}