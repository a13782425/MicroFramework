using System;
using System.Diagnostics;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// 对象的显示名字
    /// 不支持gui, 如果需要支持请自行实现Drawer
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class DisplayNameAttribute : PropertyAttribute
    {
        /// <summary>
        /// 需要显示的名字
        /// </summary>
        public readonly string DisplayName;

        public DisplayNameAttribute(string displayName)
        {
            this.order = int.MinValue;
            this.DisplayName = displayName;
        }
    }
}
