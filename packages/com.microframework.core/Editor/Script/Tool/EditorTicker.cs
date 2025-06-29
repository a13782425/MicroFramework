using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 编辑器更新
    /// </summary>
    public static class EditorTicker
    {
        /// <summary>
        /// 编辑器启动的时间
        /// </summary>
        public static double timeSinceStartup => EditorApplication.timeSinceStartup;
        /// <summary>
        /// Update间隔
        /// </summary>
        public static float TickInterval => s_tickInterval;

        /// <summary>
        /// tick间隔
        /// </summary>
        private static float s_tickInterval = 1f / 30f;
        /// <summary>
        /// 计数时间
        /// </summary>
        private static float s_elapsedTime;
        /// <summary>
        /// 最后一次Tick时间
        /// </summary>
        private static double s_lastTickTime;

        private static DelayDictionary<int, EditorTick> s_allTick = new DelayDictionary<int, EditorTick>();

        internal static void Init()
        {
            s_elapsedTime = 0;
            s_lastTickTime = timeSinceStartup;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        internal static void Free()
        {
            EditorApplication.update -= OnUpdate;
        }

        /// <summary>
        /// 添加一个定时器
        /// </summary>
        /// <returns></returns>
        public static EditorTick GetTick() => GetTick(null, 0, -1, 0, null);
        /// <summary>
        /// 添加一个定时器
        /// </summary>
        /// <param name="tickAction">回调</param>
        /// <param name="interval">间隔</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static EditorTick GetTick(Action<object> tickAction, float interval, object param = null) => GetTick(tickAction, interval, -1, 0, param);
        /// <summary>
        /// 添加一个定时器
        /// </summary>
        /// <param name="tickAction">回调</param>
        /// <param name="interval">间隔</param>
        /// <param name="loop">循环次数,-1是一直循环</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static EditorTick GetTick(Action<object> tickAction, float interval, int loop, object param = null) => GetTick(tickAction, interval, loop, 0, param);
        /// <summary>
        /// 添加一个定时器
        /// </summary>
        /// <param name="tickAction">回调</param>
        /// <param name="interval">间隔</param>
        /// <param name="loop">循环次数,-1是一直循环</param>
        /// <param name="delay">第一次调用延迟,默认是0</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static EditorTick GetTick(Action<object> tickAction, float interval, int loop, float delay, object param = null)
        {
            EditorTick tick = new EditorTick(tickAction, interval, loop, delay, param);
            return tick;
        }

        public static bool StopTick(EditorTick tick)
        {
            return true;
        }


        private static void OnUpdate()
        {
            s_elapsedTime += (float)(timeSinceStartup - s_lastTickTime);
            s_lastTickTime = timeSinceStartup;
            if (s_elapsedTime < TickInterval)
                return;
            s_elapsedTime -= TickInterval;

            s_allTick.Push();
            foreach (var item in s_allTick)
            {
                try
                {
                    item.Value.Tick();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

        }

        /// <summary>
        /// Tick类
        /// </summary>
        public sealed class EditorTick
        {
            public Action<object> TickAction { get; private set; }
            public float Interval { get; private set; }
            public int Loop { get; private set; }
            public float Delay { get; private set; }
            public object Param { get; private set; }

            private float _elapsedTime = 0;
            private int _curLoop = 0;
            private bool _isFirst = false;
            private EditorTick() { }
            internal EditorTick(Action<object> tickAction, float interval, int loop, float delay, object param)
            {
                TickAction = tickAction;
                Interval = interval;
                Loop = loop;
                Delay = delay;
                Param = param;
            }

            /// <summary>
            /// 设置方法
            /// </summary>
            /// <param name="tickAction"></param>
            /// <returns></returns>
            public EditorTick SetAction(Action<object> tickAction) { TickAction = tickAction; return this; }
            /// <summary>
            /// 设置间隔
            /// </summary>
            /// <param name="interval"></param>
            /// <returns></returns>
            public EditorTick SetInterval(float interval) { Interval = interval; return this; }
            /// <summary>
            /// 设置循环
            /// </summary>
            /// <param name="loop"></param>
            /// <returns></returns>
            public EditorTick SetLoop(int loop) { Loop = loop; return this; }
            /// <summary>
            /// 设置初次运行延时
            /// </summary>
            /// <param name="delay"></param>
            /// <returns></returns>
            public EditorTick SetDelay(float delay) { Delay = delay; return this; }
            /// <summary>
            /// 设置参数
            /// </summary>
            /// <param name="param"></param>
            /// <returns></returns>
            public EditorTick SetParam(object param) { Param = param; return this; }

            /// <summary>
            /// 启动
            /// </summary>
            public void Run()
            {
                EditorTicker.s_allTick.Add(this.GetHashCode(), this);
                _isFirst = true;
                if (Delay > 0)
                    _elapsedTime = 0;
                else
                    _elapsedTime = Interval + 1;
            }

            internal void Tick()
            {
                if (Delay > 0 && _isFirst)
                {
                    if (_elapsedTime > Delay)
                    {
                        TickAction?.Invoke(this.Param);
                        _curLoop++;
                        _isFirst = false;
                        _elapsedTime = 0;
                    }
                    else
                    {
                        _elapsedTime += TickInterval;
                    }
                }
                else
                {
                    if (_elapsedTime > Interval)
                    {
                        TickAction?.Invoke(this.Param);
                        _curLoop++;
                        _elapsedTime = 0;
                    }
                    else
                    {
                        _elapsedTime += TickInterval;
                    }
                }

                if (Loop > -1 && _curLoop >= Loop)
                {
                    Stop();
                }

            }

            /// <summary>
            /// 启动
            /// </summary>
            public void Stop()
            {
                EditorTicker.s_allTick.Remove(this.GetHashCode());
            }
        }
    }
}
