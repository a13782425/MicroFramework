using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("MFramework.Core.Editor")]
#endif

namespace MFramework.Core
{
    /// <summary>
    /// 微框架的所有配置项
    /// </summary>
    public static class MicroSetting
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public readonly static Version version = new Version(0, 1, 0);
        private static int _logicFrameRate = 30;
        /// <summary>
        /// 设置逻辑帧率(默认30帧)
        /// </summary>
        public static int logicFrameRate
        {
            get => _logicFrameRate;
            set
            {
                if (value <= 0)
                {
                    _logicFrameRate = -1;
                    _logicDeltaTime = float.MaxValue;
                }
                else
                {
                    _logicFrameRate = value;
                    _logicDeltaTime = 1f / _logicFrameRate;
                }
            }
        }

        private static float _logicDeltaTime = 1f / _logicFrameRate;
        /// <summary>
        /// 逻辑帧每帧时长(Update)
        /// </summary>
        internal static float logicDeltaTime => _logicDeltaTime;

        /// <summary>
        /// 在游戏开始后是否自动注册模块
        /// <para>自动注册只会执行一次</para>
        /// </summary>
        internal static bool autoRegisterModule => false;
    }
}
