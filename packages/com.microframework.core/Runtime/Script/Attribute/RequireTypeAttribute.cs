using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core
{
    /// <summary>
    /// 需要关联的类型
    /// <para>请注意不要循环关联，如果循环关联会导致不可预知的问题</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RequireTypeAttribute : Attribute
    {
        public readonly Type[] RequireTypes;
        public RequireTypeAttribute(params Type[] types)
        {
            RequireTypes = types;
        }
    }

    /// <summary>
    /// 忽略特性,存在此特性的脚本都不会被框架收集
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class IgnoreAttribute : Attribute
    {
    }
}
