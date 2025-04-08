using System;
using System.Diagnostics;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// �ֶ���Inspector����ʾ����
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class DisplayNameAttribute : PropertyAttribute
    {
        /// <summary>
        /// ��Ҫ��ʾ������
        /// </summary>
        public readonly string DisplayName;

        public DisplayNameAttribute(string displayName)
        {
            this.order = int.MinValue;
            this.DisplayName = displayName;
        }
    }
}
