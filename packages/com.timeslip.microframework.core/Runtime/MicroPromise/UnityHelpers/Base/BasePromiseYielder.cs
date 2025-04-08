#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0074 // Use compound assignment
#pragma warning disable IDE0090 // Use 'new(...)'
using MFramework.Core.CompilerServices;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static MFramework.Core.Promise;

namespace MFramework.Core
{
    /// <summary>
    /// Promise的接口类，方便使用
    /// </summary>
    static partial class PromiseYielder
    {
        static partial void Assert(bool condition, string argName, int skipFrames);

#if PROMISE_DEBUG
        static partial void Assert(bool condition, string argName, int skipFrames)
        {
            if (!condition)
            {
                throw new PromiseInvalidArgumentException(argName, "Assert defeat", Internal.GetFormattedStacktrace(skipFrames + 1));
            }
        }
#endif
        /// <summary>
        /// 下一个Update执行
        /// </summary>
        /// <param name="action"></param>
        [MethodImpl(Internal.InlineOption)]
        public static void ExecuteForNextUpdate(Action action)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            InternalHelper.PromiseBehaviour.s_updateProcessor.WaitForNext(action);
        }
        /// <summary>
        /// 下一个LateUpdate执行
        /// </summary>
        /// <param name="action"></param>
        [MethodImpl(Internal.InlineOption)]
        public static void ExecuteForNextLateUpdate(Action action)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            InternalHelper.PromiseBehaviour.s_lateUpdateProcessor.WaitForNext(action);
        }
        /// <summary>
        /// 下一个FixedUpdate执行
        /// </summary>
        /// <param name="action"></param>
        [MethodImpl(Internal.InlineOption)]
        public static void ExecuteForNextFixedUpdate(Action action)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            InternalHelper.PromiseBehaviour.s_fixedUpdateProcessor.WaitForNext(action);
        }
        /// <summary>
        /// 下一个EndOfFrame执行
        /// </summary>
        /// <param name="action"></param>
        [MethodImpl(Internal.InlineOption)]
        public static void ExecuteForNextEndOfFrame(Action action)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            InternalHelper.PromiseBehaviour.s_endOfFrameProcessor.WaitForNext(action);
        }
        /// <summary>
        /// Returns a <see cref="PromiseSwitchToContextAwaiter"/> that will complete after switch to the background context.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public static PromiseSwitchToContextAwaiter WaitForThread() => new PromiseSwitchToContextAwaiter(Config.BackgroundContext, false);
        /// <summary>
        /// Returns a <see cref="PromiseSwitchToContextAwaiter"/> that will complete after switch to the foreground context.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public static PromiseSwitchToContextAwaiter WaitForUnity() => new PromiseSwitchToContextAwaiter(Config.ForegroundContext, false);

        /// <summary>
        /// Returns a <see cref="Instructions.WaitTimeInstruction"/> that will complete after the specified timespan has passed, using scaled time.
        /// </summary>
        /// <param name="second">How much second to wait for.</param>
        [MethodImpl(Internal.InlineOption)]
        public static Instructions.WaitTimeInstruction WaitForTime(float second)
        {
            Assert(second > 0, "second", 1);
            int s = (int)Math.Floor(second);
            int ms = (int)Math.Floor(second * 1000) - s * 1000;
            return new Instructions.WaitTimeInstruction(new TimeSpan(0, 0, 0, s, ms));
        }
        /// <summary>
        /// Returns a <see cref="Instructions.WaitTimeInstruction"/> that will complete after the specified timespan has passed, using scaled time.
        /// </summary>
        /// <param name="milliseconds">How milliseconds time to wait for.</param>
        [MethodImpl(Internal.InlineOption)]
        public static Instructions.WaitTimeInstruction WaitForTime(uint milliseconds)
        {
            return new Instructions.WaitTimeInstruction(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        /// Returns a <see cref="Instructions.WaitRealTimeInstruction"/> that will complete after the specified timespan has passed, using unscaled, real time.
        /// </summary>
        /// <param name="second">How much second to wait for.</param>
        [MethodImpl(Internal.InlineOption)]
        public static Instructions.WaitRealTimeInstruction WaitForRealTime(float second)
        {
            Assert(second > 0, "second", 1);
            int s = (int)Math.Floor(second);
            int ms = (int)Math.Floor(second * 1000) - s * 1000;
            return new Instructions.WaitRealTimeInstruction(new TimeSpan(0, 0, 0, s, ms));
        }

        /// <summary>
        /// Returns a <see cref="Instructions.WaitRealTimeInstruction"/> that will complete after the specified timespan has passed, using unscaled, real time.
        /// </summary>
        /// <param name="milliseconds">How much milliseconds to wait for.</param>
        [MethodImpl(Internal.InlineOption)]
        public static Instructions.WaitRealTimeInstruction WaitForRealTime(uint milliseconds)
        {
            return new Instructions.WaitRealTimeInstruction(TimeSpan.FromMilliseconds(milliseconds));
        }
    }
}
