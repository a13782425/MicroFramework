using System;
using System.Reflection;

namespace MFramework.Core
{
    /// <summary>
    /// 序列化类
    /// </summary>
    [Serializable]
    public sealed class MicroClassSerializer
    {
        /// <summary>
        /// 程序集名字
        /// </summary>
        public string AssemblyName;
        /// <summary>
        /// 模块类型名
        /// </summary>
        public string TypeName;

        /// <summary>
        /// 显示名称
        /// </summary>
        [NonSerialized]
        internal string DisplayName;
        [NonSerialized]
        private Type _type;

        /// <summary>
        /// 当前类型
        /// </summary>
        public Type CurrentType
        {
            get
            {
                if (_type != null)
                    return _type;
                if (string.IsNullOrEmpty(AssemblyName) || string.IsNullOrEmpty(TypeName))
                    return null;
                _type = MicroContext.microTypeMapper?.GetType(this);
                if (_type != null)
                    return _type;
                Assembly assembly = Assembly.Load(AssemblyName);
                if (assembly == null)
                    return null;
                _type = assembly.GetType(TypeName);
                return _type;
            }
            set
            {
                _type = value;
                if (_type != null)
                {
                    AssemblyName = _type.Assembly.FullName;
                    TypeName = _type.FullName;
                }
            }
        }

        public override string ToString()
        {
            return TypeName;
        }
    }
}
