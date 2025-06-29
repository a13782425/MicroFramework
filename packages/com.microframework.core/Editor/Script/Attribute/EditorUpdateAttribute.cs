using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 编辑器下的Update
    /// <para>无参</para>
    /// <para>只对静态方法有效</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorUpdateAttribute : Attribute
    {
    }
}
