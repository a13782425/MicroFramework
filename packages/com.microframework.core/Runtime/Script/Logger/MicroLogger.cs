using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class MicroLogger
    {
        private class UnityLogger : IMicroLogger
        {
            private readonly string _name;
            public string LogName => _name;
            public bool IsDebugEnabled { get; set; }
            public UnityLogger(string name)
            {
                _name = name;
                IsDebugEnabled = MicroLogger.IsDebugEnabled;
            }
            public void Log(object message)
            {
                if (IsDebugEnabled)
                    Debug.Log($"[{DateTime.Now.ToString("HH:mm:ss:fff")}]-[{LogName}]:{message}");
            }

            public void LogByColor(object message, Color color)
            {
                if (IsDebugEnabled)
                    Debug.Log($"[{DateTime.Now.ToString("HH:mm:ss:fff")}]-<color=#{ColorUtility.ToHtmlStringRGB(color)}>[{LogName}]:{message}</color>");
            }
            public void LogWarning(object message)
            {
                if (IsDebugEnabled)
                    Debug.LogWarning($"[{DateTime.Now.ToString("HH:mm:ss:fff")}]-[{LogName}]:{message}");
            }
            public void LogError(object message)
            {
                Debug.LogError($"[{DateTime.Now.ToString("HH:mm:ss:fff")}]-[{LogName}]:{message}");
            }

            public void LogException(Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static IMicroLogger _logger = new UnityLogger("MFramework");

        /// <summary>
        /// 创建日志对象
        /// string:名字
        /// </summary>
        public static event Func<string, IMicroLogger> onCreateLogger;


        private static bool _isDebugEnabled = true;
        /// <summary>
        /// 统一设置日志是否启用
        /// </summary>
        public static bool IsDebugEnabled
        {
            get => _isDebugEnabled;
            set
            {
                _isDebugEnabled = value;
                foreach (var item in _cacheLoggers)
                    item.Value.IsDebugEnabled = value;
            }
        }

        private static Dictionary<string, IMicroLogger> _cacheLoggers = new Dictionary<string, IMicroLogger>();
        /// <summary>
        /// 获取一个日志
        /// </summary>
        /// <param name="loggerName"></param>
        /// <returns></returns>
        public static IMicroLogger GetMicroLogger(string loggerName)
        {
            if (_cacheLoggers.TryGetValue(loggerName, out IMicroLogger logger))
            {
                return logger;
            }
            logger = onCreateLogger?.Invoke(loggerName) ?? new UnityLogger(loggerName);
            _cacheLoggers.Add(loggerName, logger);
            return logger;
        }
        public static void Log(object message) => _logger.Log(message);
        public static void LogByColor(object message, Color color) => _logger.LogByColor(message, color);
        public static void LogByColor(Color color, object message) => _logger.LogByColor(message, color);
        public static void LogWarning(object message) => _logger.LogWarning(message);
        public static void LogError(object message) => _logger.LogError(message);
        public static void LogException(Exception exception) => _logger.LogException(exception);
    }
}
