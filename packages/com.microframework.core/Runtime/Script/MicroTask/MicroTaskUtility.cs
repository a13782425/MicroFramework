using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Task
{
    public static class MicroTaskUtility
    {

        // Extension methods for IEnumerator
        public static MicroTask ToMicroTask(this IEnumerator enumerator)
        {
            if (enumerator == null)
                throw new ArgumentNullException(nameof(enumerator));

            var source = new MicroTaskCompletionSource();

            void HandleCoroutine()
            {
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        source.TrySetResult();
                        return;
                    }

                    var current = enumerator.Current;

                    // Handle null yield return
                    if (current == null)
                    {
                        MicroTaskScheduler.Schedule(HandleCoroutine);
                        return;
                    }

                    Action<Action> scheduleAction = next => MicroTaskScheduler.Schedule(next);

                    if (current is IEnumerator nestedCoroutine)
                    {
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await nestedCoroutine.ToMicroTask();
                            next();
                        });
                    }
                    else if (current is WaitForSeconds waitForSeconds)
                    {
                        float seconds = waitForSeconds.GetFieldValue<float>("m_Seconds", 0f);
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await MicroTask.Delay(seconds);
                            next();
                        });
                    }
                    else if (current is WaitForSecondsRealtime waitRealtime)
                    {
                        float seconds = waitRealtime.waitTime;
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await MicroTask.DelayRealtime(seconds);
                            next();
                        });
                    }
                    else if (current is WaitForFixedUpdate)
                    {
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await MicroTask.WaitForFixedUpdate();
                            next();
                        });
                    }
                    else if (current is WaitForEndOfFrame)
                    {
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await MicroTask.NextFrame();
                            next();
                        });
                    }
                    else if (current is WaitUntil waitUntil)
                    {
                        var predicate = waitUntil.GetFieldValue<Func<bool>>("m_Predicate", null);
                        if (predicate != null)
                        {
                            scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                            {
                                await MicroTask.WaitUntil(predicate);
                                next();
                            });
                        }
                    }
                    else if (current is WaitWhile waitWhile)
                    {
                        var predicate = waitWhile.GetFieldValue<Func<bool>>("m_Predicate", null);
                        if (predicate != null)
                        {
                            scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                            {
                                await MicroTask.WaitWhile(predicate);
                                next();
                            });
                        }
                    }
                    else if (current is AsyncOperation asyncOp)
                    {
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await asyncOp.ToMicroTask();
                            next();
                        });
                    }
                    else if (current is CustomYieldInstruction customYield)
                    {
                        scheduleAction = next => MicroTaskScheduler.Schedule(async () =>
                        {
                            await MicroTask.WaitWhile(() => customYield.keepWaiting);
                            next();
                        });
                    }

                    scheduleAction(HandleCoroutine);
                }
                catch (Exception ex)
                {
                    source.TrySetException(ex);
                }
            }

            MicroTaskScheduler.Schedule(HandleCoroutine);
            return source.Task;
        }
        public static MicroTaskAwaiter GetAwaiter(this IEnumerator enumerator)
        {
            return ToMicroTask(enumerator).GetAwaiter();
        }
        private static T GetFieldValue<T>(this object obj, string fieldName, T defaultValue)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field != null ? (T)field.GetValue(obj) : defaultValue;
        }

        // Extension methods for YieldInstruction and CustomYieldInstruction
        public static MicroTask ToMicroTask(this YieldInstruction yieldInstruction)
        {
            IEnumerator WrapYieldInstruction()
            {
                yield return yieldInstruction;
            }

            return WrapYieldInstruction().ToMicroTask();
        }

        public static MicroTask ToMicroTask(this CustomYieldInstruction yieldInstruction)
        {
            return MicroTask.WaitWhile(() => yieldInstruction.keepWaiting);
        }
        public static MicroTaskAwaiter GetAwaiter(this YieldInstruction yieldInstruction)
        {
            return ToMicroTask(yieldInstruction).GetAwaiter();
        }
        public static MicroTaskAwaiter GetAwaiter(this CustomYieldInstruction yieldInstruction)
        {
            return ToMicroTask(yieldInstruction).GetAwaiter();
        }

        // Extension method for AsyncOperation
        public static MicroTask ToMicroTask(this AsyncOperation operation)
        {
            var source = new MicroTaskCompletionSource();
            operation.completed += _ => { source.TrySetResult(); };
            return source.Task;
        }

        public static MicroTaskAwaiter GetAwaiter(this AsyncOperation operation)
        {
            return ToMicroTask(operation).GetAwaiter();
        }

        // Extension method for IMicroTaskInstruction

        public static MicroTask ToMicroTask(this IMicroTaskInstruction instruction)
        {
            return MicroTask.WaitUntil(() => instruction.IsCompleted());
        }
        public static MicroTaskAwaiter GetAwaiter(this IMicroTaskInstruction instruction)
        {
            return ToMicroTask(instruction).GetAwaiter();
        }
        public static MicroTask<T> ToMicroTask<T>(this IMicroTaskInstruction<T> instruction)
        {
            var tcs = new MicroTaskCompletionSource<T>();

            MicroTaskScheduler.Schedule(async () =>
            {
                try
                {
                    await MicroTask.WaitUntil(() => instruction.IsCompleted());
                    tcs.TrySetResult(instruction.GetResult());
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
        public static MicroTaskAwaiter<T> GetAwaiter<T>(this IMicroTaskInstruction<T> instruction)
        {
            return ToMicroTask(instruction).GetAwaiter();
        }

        // Extension method for UnityEngine.Object dependency
        public static MicroTask ToDepend(this MicroTask task, UnityEngine.Object dependency)
        {
            if (dependency == null)
                throw new ArgumentNullException(nameof(dependency), "Dependency object cannot be null");

            var tcs = new MicroTaskCompletionSource();

            MicroTaskScheduler.Schedule(async () =>
            {
                try
                {
                    await task;
                    if (dependency == null)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    tcs.TrySetResult();
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public static MicroTask<T> ToDepend<T>(this MicroTask<T> task, UnityEngine.Object dependency)
        {
            if (dependency == null)
                throw new ArgumentNullException(nameof(dependency), "Dependency object cannot be null");

            var tcs = new MicroTaskCompletionSource<T>();

            MicroTaskScheduler.Schedule(async () =>
            {
                try
                {
                    var result = await task;
                    if (dependency == null)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    tcs.TrySetResult(result);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        // Extension method for timeout
        public static async MicroTask<bool> WithTimeout(this MicroTask task, float timeoutSeconds)
        {
            if (!task.IsValid)
                throw new ArgumentException("Task is not valid", nameof(task));

            var timeoutTask = MicroTask.Delay(timeoutSeconds);
            var completedTask = await MicroTask.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
                return false;

            await task; // Propagate any exceptions from the original task
            return true;
        }

        // Extension method for retry
        public static async MicroTask WithRetry(this Func<MicroTask> taskFactory, int maxAttempts = 3, float delayBetweenAttempts = 1f)
        {
            if (taskFactory == null)
                throw new ArgumentNullException(nameof(taskFactory));

            Exception lastException = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    if (attempt > 0)
                        await MicroTask.Delay(delayBetweenAttempts);

                    await taskFactory();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new AggregateException($"Task failed after {maxAttempts} attempts", lastException);
        }

        // Extension method for timeout and retry
        public static async MicroTask WithTimeoutAndRetry(this Func<MicroTask> taskFactory, float timeoutSeconds, int maxAttempts = 3, float delayBetweenAttempts = 1f)
        {
            await WithRetry(async () =>
            {
                var task = taskFactory();
                if (!await task.WithTimeout(timeoutSeconds))
                    throw new TimeoutException($"Task timed out after {timeoutSeconds} seconds");
            }, maxAttempts, delayBetweenAttempts);
        }

        // Extension method for running tasks in sequence
        public static async MicroTask InSequence(this IEnumerable<Func<MicroTask>> taskFactories)
        {
            if (taskFactories == null)
                return;

            foreach (var factory in taskFactories)
            {
                if (factory == null) continue;
                await factory();
            }
        }

        public static MicroTask WithCancellation(this MicroTask task, MicroTaskCancellationToken cancellationToken)
        {
            if (!task.IsValid)
                throw new ArgumentException("Task is not valid", nameof(task));

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return MicroTask.Break();

            var tcs = new MicroTaskCompletionSource();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

            MicroTaskScheduler.Schedule(async () =>
            {
                try
                {
                    await task;
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    registration.Dispose();
                }
            });

            return tcs.Task;
        }

        public static MicroTask<T> WithCancellation<T>(this MicroTask<T> task, MicroTaskCancellationToken cancellationToken)
        {
            if (!task.IsValid)
                throw new ArgumentException("Task is not valid", nameof(task));

            if (!cancellationToken.CanBeCanceled)
                return task;

            if (cancellationToken.IsCancellationRequested)
                return MicroTask<T>.Break();

            var tcs = new MicroTaskCompletionSource<T>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

            MicroTaskScheduler.Schedule(async () =>
            {
                try
                {
                    var result = await task;
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    registration.Dispose();
                }
            });

            return tcs.Task;
        }
    }
}