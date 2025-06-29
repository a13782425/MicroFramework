using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Core
{
    public interface IMicroLogger
    {
        string LogName { get; }
        bool IsDebugEnabled { get; set; }
        void Log(object message);
        void LogByColor(object message, Color color);
        void LogWarning(object message);
        void LogError(object message);
        void LogException(Exception exception);
    }
}
