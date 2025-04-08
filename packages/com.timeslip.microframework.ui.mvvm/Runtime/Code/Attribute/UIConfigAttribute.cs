using System;

namespace MFramework.Runtime
{
    /// <summary>
    /// 该属性自动生成时候添加，请勿主动添加
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIConfigAttribute : Attribute
    {
        public string UIPath;
        public UIConfigAttribute(string uipath) => UIPath = uipath;
    }
}
