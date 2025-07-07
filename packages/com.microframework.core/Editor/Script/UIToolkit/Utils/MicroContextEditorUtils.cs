using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// MicroContextEditor 的工具类
    /// </summary>
    public static class MicroContextEditorUtils
    {
        private static readonly List<ICustomDrawer> FilterDrawers = new List<ICustomDrawer>();
        public static string GetDisplayName(object obj)
        {
            if (obj == null)
                return "Null";
            var attr = obj.GetType().GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
                return attr.DisplayName;
            else
            {
                Type type = obj.GetType();
                if (type.IsArray)
                    return $"{type.GetElementType().Name}[]";

                if (type.IsGenericType)
                {
                    var args = type.GetGenericArguments();
                    return type.Name.Split('`')[0] +
                           $"<{string.Join(",", args.Select(t => t.Name))}>";
                }
                return obj.GetType().Name;
            }
        }
        public static string GetDisplayName(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return "Null";
            var attr = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
                return attr.DisplayName;
            else
                return fieldInfo.Name;

        }

        /// <summary>
        /// 获取一个字段的所有绘制器
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static List<ICustomDrawer> GetDrawers(FieldInfo fieldInfo)
        {
            FilterDrawers.Clear();
            var fieldType = fieldInfo.FieldType;
            ICustomDrawer basicDrawer = null;
            foreach (var item in fieldInfo.GetCustomAttributes())
            {
                if (!MicroContextEditor.CustomDrawers.TryGetValue(item.GetType(), out ICustomDrawer drawer))
                    continue;
                if (drawer.DrawerType == CustomDrawerType.Basics)
                    basicDrawer = drawer;
                else
                    FilterDrawers.Add(drawer);
            }

            if (basicDrawer == null)
            {
                if (MicroContextEditor.CustomDrawers.TryGetValue(fieldType, out basicDrawer))
                    goto End;
                Type tempType = fieldType.BaseType;
                if (fieldType.IsEnum)
                    tempType = typeof(Enum);
                else if (fieldType.IsArray)
                    tempType = typeof(Array);
                else if (fieldType.IsGenericType)
                    tempType = fieldType.GetGenericTypeDefinition();

                while (tempType != null)
                {
                    if (MicroContextEditor.CustomDrawers.TryGetValue(fieldType, out basicDrawer))
                        goto End;
                    tempType = tempType.BaseType;
                }
            }
        End: if (basicDrawer != null)
                FilterDrawers.Add(basicDrawer);
            return FilterDrawers;
        }
    }
}
