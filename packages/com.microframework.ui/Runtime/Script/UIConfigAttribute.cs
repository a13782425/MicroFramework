using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.UI
{
    /// <summary>
    /// UI配置属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIConfigAttribute : Attribute
    {
        public string UIPath;
        public UIConfigAttribute(string uipath) => UIPath = uipath;
    }
}
