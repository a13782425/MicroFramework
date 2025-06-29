using System;
using System.Runtime.CompilerServices;
using System.Security;


namespace MFramework.Task
{
    [AsyncMethodBuilder(typeof(MicroTaskMethodBuilder<>))]
    public readonly partial struct MicroTask<T>
    {
        internal readonly IMicroTaskSource<T> source;
        private readonly short token;

        public MicroTask(IMicroTaskSource<T> source, short token)
        {
            this.source = source;
            this.token = token;
        }

        internal short Token => token;
        public bool IsValid => source != null;
        public MicroTaskStatus Status
        {
            get
            {
                if (source == null) throw new InvalidOperationException("Cannot get status of default UTask<T>");
                return source.GetStatus(token);
            }
        }

        public bool IsCompleted
        {
            get
            {
                if (source == null) throw new InvalidOperationException("Cannot check completion of default UTask<T>");
                return source.GetStatus(token).IsCompleted();
            }
        }

        public T GetResult()
        {
            if (source == null) throw new InvalidOperationException("Cannot get result of default UTask<T>");
            return source.GetResult(token);
        }

        public MicroTaskAwaiter<T> GetAwaiter()
        {
            return new MicroTaskAwaiter<T>(this);
        }

        internal void OnCompleted(Action continuation)
        {
            if (source == null)
            {
                MicroTaskScheduler.Schedule(continuation);
                return;
            }
            source.OnCompleted(state => ((Action)state)(), continuation, Token);
        }
    }

    public readonly struct MicroTaskAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly MicroTask<T> task;

        public MicroTaskAwaiter(MicroTask<T> task)
        {
            this.task = task;
        }

        public bool IsCompleted => task.IsCompleted;

        public T GetResult() => task.GetResult();

        public void OnCompleted(Action continuation)
        {
            task.OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }
    }

    public struct MicroTaskMethodBuilder<T>
    {
        private MicroTaskCompletionSource<T> tcs;

        public static MicroTaskMethodBuilder<T> Create()
        {
            return new MicroTaskMethodBuilder<T> { tcs = new MicroTaskCompletionSource<T>() };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void SetResult(T result)
        {
            tcs.TrySetResult(result);
        }

        public void SetException(Exception exception)
        {
            tcs.TrySetException(exception);
        }

        public MicroTask<T> Task => tcs.Task;

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
    }
}