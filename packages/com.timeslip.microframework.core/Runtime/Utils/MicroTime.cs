
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace MFramework.Core
//{
//    /// <summary>
//    /// 微框架的时间（只读）
//    /// <para>可以在子线程中访问</para>
//    /// </summary>
//    public static class MicroTime
//    {
//        /// <summary>
//        /// 时间缩放
//        /// </summary>
//        public static float timeScale { get; private set; }
//        /// <summary>
//        /// 游戏开始后的总帧数
//        /// </summary>
//        public static int frameCount { get; private set; }
//        /// <summary>
//        /// 从上一帧到当前帧的秒间隔
//        /// </summary>
//        public static float deltaTime { get; private set; }
//        /// <summary>
//        /// 从上一帧到当前帧的时间间隔(以秒为单位)
//        /// <para>不受时间缩放影响</para>
//        /// </summary>
//        public static float unscaledDeltaTime { get; private set; }

//        /// <summary>
//        /// 以秒为单位的游戏开始后的实时时间
//        /// </summary>
//        public static float realtimeSinceStartup { get; private set; }
//        /// <summary>
//        /// 以秒为单位的游戏开始后的实时时间(双精度版)
//        /// </summary>
//        public static double realtimeSinceStartupAsDouble { get; private set; }
//        public static float time { get; private set; }
//        public static double timeAsDouble { get; private set; }
//        public static float unscaledTime { get; private set; }
//        public static double unscaledTimeAsDouble { get; private set; }

//        /// <summary>
//        /// 以秒为单位的物理和其他固定帧速率更新的间隔
//        /// </summary>
//        public static float fixedDeltaTime { get; private set; }
//        /// <summary>
//        /// 以秒为单位的物理和其他固定帧速率更新的间隔
//        /// <para>不受时间缩放影响</para>
//        /// </summary>
//        public static float fixedUnscaledDeltaTime { get; private set; }
//        public static float fixedTime { get; private set; }
//        public static double fixedTimeAsDouble { get; private set; }
//        public static float fixedUnscaledTime { get; private set; }
//        public static double fixedUnscaledTimeAsDouble { get; private set; }

//        static MicroTime()
//        {
//            MicroContext.RunOnUnityThread(() =>
//            {
//                MicroContext.onUpdate += m_onUpdate;
//                m_onUpdate(Time.deltaTime);
//            });
//        }

//        private static void m_onUpdate(float deltaTime)
//        {
//            timeScale = Time.timeScale;
//            frameCount = Time.frameCount;

//            MicroTime.deltaTime = deltaTime;
//            unscaledDeltaTime = Time.unscaledDeltaTime;

//            realtimeSinceStartup = Time.realtimeSinceStartup;
//            realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
//            time = Time.time;
//            timeAsDouble = Time.timeAsDouble;
//            unscaledTime = Time.unscaledTime;
//            unscaledTimeAsDouble = Time.unscaledTimeAsDouble;

//            fixedDeltaTime = Time.fixedDeltaTime;
//            fixedUnscaledDeltaTime = Time.fixedUnscaledDeltaTime;
//            fixedTime = Time.fixedTime;
//            fixedTimeAsDouble = Time.fixedTimeAsDouble;
//            fixedUnscaledTime = Time.fixedUnscaledTime;
//            fixedUnscaledTimeAsDouble = Time.fixedUnscaledTimeAsDouble;
//        }
//    }
//}
