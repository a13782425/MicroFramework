using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Task
{
    public readonly partial struct MicroTask
    {
        // Static Methods
        public static MicroTask CompletedTask => new MicroTask();

        public static MicroTask FromException(Exception exception)
        {
            var source = new MicroTaskCompletionSource();
            source.TrySetException(exception);
            return source.Task;
        }

        public static MicroTask Break()
        {
            var source = new MicroTaskCompletionSource();
            source.TrySetCanceled();
            return source.Task;
        }

        // Moved from UTaskUtility
        public static MicroTask Wait(float seconds) => Delay(seconds);

        public static MicroTask Next() => NextFrame();

        public static MicroTask Fixed() => WaitForFixedUpdate();

        public static MicroTask Until(Func<bool> predicate) => WaitUntil(predicate);

        public static MicroTask While(Func<bool> predicate) => WaitWhile(predicate);

        public static MicroTask All(params MicroTask[] tasks) => WhenAll(tasks);

        private static readonly MinHeap<DelayEntry> delayHeap = new MinHeap<DelayEntry>(128);
        private static bool isDelaySystemInitialized;

        private struct DelayEntry : IComparable<DelayEntry>
        {
            public float TargetTime;
            public MicroTaskCompletionSource Source;

            public int CompareTo(DelayEntry other)
            {
                return TargetTime.CompareTo(other.TargetTime);
            }
        }

        private static void InitializeDelaySystem()
        {
            if (isDelaySystemInitialized) return;
            isDelaySystemInitialized = true;

            MicroTaskScheduler.SchedulePerFrame(ProcessDelayQueue);
        }

        private static void ProcessDelayQueue()
        {
            float currentTime = Time.time;

            while (delayHeap.Count > 0 && delayHeap.Peek().TargetTime <= currentTime)
            {
                var entry = delayHeap.Pop();
                entry.Source.TrySetResult();
            }
        }

        public static MicroTask Delay(float seconds)
        {
            if (float.IsNaN(seconds))
                throw new ArgumentException("Delay duration cannot be NaN", nameof(seconds));

            if (seconds <= 0)
            {
                var immediateSource = new MicroTaskCompletionSource();
                immediateSource.TrySetResult();
                return immediateSource.Task;
            }

            if (!isDelaySystemInitialized)
                InitializeDelaySystem();

            var source = new MicroTaskCompletionSource();
            var entry = new DelayEntry
            {
                TargetTime = Time.time + seconds,
                Source = source
            };

            delayHeap.Push(entry);
            return source.Task;
        }

        public static MicroTask DelayRealtime(float seconds)
        {
            if (float.IsNaN(seconds))
                throw new ArgumentException("Delay duration cannot be NaN", nameof(seconds));

            if (seconds <= 0)
            {
                var immediateSource = new MicroTaskCompletionSource();
                immediateSource.TrySetResult();
                return immediateSource.Task;
            }

            var source = new MicroTaskCompletionSource();
            var targetTime = Time.realtimeSinceStartup + Mathf.Max(0, seconds);
            void CheckTimeAction()
            {
                if (Time.realtimeSinceStartup >= targetTime)
                {
                    source.TrySetResult();
                    MicroTaskScheduler.RemovePerFrame(CheckTimeAction);
                }
            }
            MicroTaskScheduler.SchedulePerFrame(CheckTimeAction);
            return source.Task;
        }

        public static async MicroTask WhenAll(params MicroTask[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
                return;

            // Count valid tasks
            int validTaskCount = 0;
            foreach (var task in tasks)
            {
                if (task.IsValid) validTaskCount++;
            }

            if (validTaskCount == 0)
                return;

            var remaining = validTaskCount;
            var tcs = new MicroTaskCompletionSource();
            var exceptions = new List<Exception>();
            var anyCanceled = false;

            foreach (var task in tasks)
            {
                if (!task.IsValid) continue;  // Skip invalid tasks
                RunTask(task);
            }

            async void RunTask(MicroTask task)
            {
                try
                {
                    await task;
                }
                catch (OperationCanceledException)
                {
                    anyCanceled = true;
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
                finally
                {
                    if (--remaining == 0)
                    {
                        if (exceptions.Count > 0)
                            tcs.TrySetException(exceptions.Count == 1 ? exceptions[0] : new AggregateException(exceptions));
                        else if (anyCanceled)
                            tcs.TrySetCanceled();
                        else
                            tcs.TrySetResult();
                    }
                }
            }

            await tcs.Task;
        }

        public static MicroTask NextFrame()
        {
            var source = new MicroTaskCompletionSource();
            MicroTaskScheduler.Schedule(() => { source.TrySetResult(); });
            return source.Task;
        }

        public static MicroTask WaitUntil(Func<bool> predicate)
        {
            var source = new MicroTaskCompletionSource();
            Action checkCondition = null;
            checkCondition = () =>
            {
                if (predicate())
                {
                    source.TrySetResult();
                    MicroTaskScheduler.RemovePerFrame(checkCondition);
                }
            };
            MicroTaskScheduler.SchedulePerFrame(checkCondition);
            return source.Task;
        }

        public static MicroTask WaitWhile(Func<bool> predicate)
        {
            return WaitUntil(() => !predicate());
        }

        public static MicroTask WaitForFixedUpdate()
        {
            var source = new MicroTaskCompletionSource();
            bool isFirstFrame = true;
            Action checkFixedUpdate = null;
            checkFixedUpdate = () =>
            {
                if (isFirstFrame)
                {
                    isFirstFrame = false;
                    return;
                }

                source.TrySetResult();
                MicroTaskScheduler.RemovePerFrame(checkFixedUpdate);
            };
            MicroTaskScheduler.SchedulePerFrame(checkFixedUpdate);
            return source.Task;
        }

        public static async MicroTask<MicroTask> WhenAny(params MicroTask[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
                throw new ArgumentException("At least one task is required", nameof(tasks));

            var tcs = new MicroTaskCompletionSource<MicroTask>();

            foreach (var task in tasks)
            {
                if (!task.IsValid) continue;

                RunTask(task);
            }

            async void RunTask(MicroTask task)
            {
                try
                {
                    await task;
                    tcs.TrySetResult(task);
                }
                catch (Exception)
                {
                    // Ignore exceptions, they will be thrown when awaiting the task
                }
            }

            return await tcs.Task;
        }

        public static MicroTaskCompletionSource CreateManualTask()
        {
            return new MicroTaskCompletionSource();
        }

        public static MicroTaskCompletionSource<T> CreateManualTask<T>()
        {
            return new MicroTaskCompletionSource<T>();
        }
    }

    public readonly partial struct MicroTask<T>
    {
        public static MicroTask<T> Break()
        {
            var source = new MicroTaskCompletionSource<T>();
            source.TrySetCanceled();
            return source.Task;
        }
    }

    internal class MinHeap<T> where T : IComparable<T>
    {
        private T[] items;
        private int count;

        public MinHeap(int capacity)
        {
            items = new T[capacity];
        }

        public int Count => count;

        public void Push(T item)
        {
            if (count == items.Length)
                Array.Resize(ref items, items.Length * 2);

            items[count] = item;
            SiftUp(count++);
        }

        public T Pop()
        {
            var result = items[0];
            items[0] = items[--count];
            if (count > 0)
                SiftDown(0);
            return result;
        }

        public T Peek() => items[0];

        private void SiftUp(int index)
        {
            var item = items[index];
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 1;
                if (items[parentIndex].CompareTo(item) <= 0)
                    break;
                items[index] = items[parentIndex];
                index = parentIndex;
            }
            items[index] = item;
        }

        private void SiftDown(int index)
        {
            var item = items[index];
            while (true)
            {
                int childIndex = (index << 1) + 1;
                if (childIndex >= count)
                    break;
                int rightChildIndex = childIndex + 1;
                if (rightChildIndex < count && items[rightChildIndex].CompareTo(items[childIndex]) < 0)
                    childIndex = rightChildIndex;
                if (item.CompareTo(items[childIndex]) <= 0)
                    break;
                items[index] = items[childIndex];
                index = childIndex;
            }
            items[index] = item;
        }
    }
}