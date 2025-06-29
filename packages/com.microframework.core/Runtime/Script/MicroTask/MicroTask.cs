using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MFramework.Task
{
    [AsyncMethodBuilder(typeof(MicroTaskMethodBuilder))]
    public readonly partial struct MicroTask : IEquatable<MicroTask>
    {
        internal readonly IMicroTaskSource source;
        private readonly short token;

        public MicroTask(IMicroTaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }

        internal short Token => token;

        public bool IsValid => source != null;

        public bool IsCompleted => source?.GetStatus(token).IsCompleted() ?? false;

        public bool IsFaulted => source?.GetStatus(token) == MicroTaskStatus.Faulted;

        public bool IsCanceled => source?.GetStatus(token) == MicroTaskStatus.Canceled;

        public MicroTaskStatus Status
        {
            get
            {
                if (source == null) return MicroTaskStatus.Canceled;
                return source.GetStatus(token);
            }
        }

        // Instance methods
        public bool Equals(MicroTask other)
        {
            return source == other.source && token == other.token;
        }

        public override bool Equals(object obj)
        {
            return obj is MicroTask other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((source != null ? source.GetHashCode() : 0) * 397) ^ token.GetHashCode();
            }
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

        public void GetResult()
        {
            if (source == null) return;
            source.GetResult(token);
        }

        public MicroTaskAwaiter GetAwaiter()
        {
            return new MicroTaskAwaiter(this);
        }

        public static bool operator ==(MicroTask left, MicroTask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MicroTask left, MicroTask right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct MicroTaskAwaiter : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly MicroTask task;

        public MicroTaskAwaiter(MicroTask task)
        {
            this.task = task;
        }

        public bool IsCompleted => task.IsCompleted;

        public void GetResult() => task.GetResult();

        public void OnCompleted(Action continuation)
        {
            task.OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }
    }

    public enum MicroTaskStatus
    {
        Pending,
        Succeeded,
        Faulted,
        Canceled
    }

    public static class MicroTaskStatusExtensions
    {
        public static bool IsCompleted(this MicroTaskStatus status)
        {
            return status != MicroTaskStatus.Pending;
        }
    }
}