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
            private bool _isEnabled = true;
            public bool IsDebugEnabled { get => _isDebugEnabled ? _isEnabled : _isDebugEnabled; set => _isEnabled = value; }
            public UnityLogger(string name)
            {
                _name = name;
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

        private static bool _isDebugEnabled = true;
        public static bool IsDebugEnabled { get => _isDebugEnabled; set => _isDebugEnabled = value; }

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
            logger = new UnityLogger(loggerName);
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
