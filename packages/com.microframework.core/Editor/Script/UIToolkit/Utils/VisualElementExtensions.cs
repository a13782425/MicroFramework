using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    [Flags]
    internal enum CustomPseudoStates
    {
        Active = 1,
        Hover = 2,
        Checked = 8,
        Disabled = 0x20,
        Focus = 0x40,
        Root = 0x80
    }
    public static partial class VisualElementExtensions
    {
        private static PropertyInfo s_pseudoStateProp;
        private static Type s_pseudoStateType;

        private static MethodInfo s_setPropertyMethod;
        private static MethodInfo s_getPropertyMethod;
        private static MethodInfo s_hasPropertyMethod;

        static VisualElementExtensions()
        {
            s_pseudoStateProp = typeof(VisualElement).GetProperty("pseudoStates", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_getPropertyMethod = typeof(VisualElement).GetMethod("GetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_setPropertyMethod = typeof(VisualElement).GetMethod("SetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_hasPropertyMethod = typeof(VisualElement).GetMethod("HasProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_pseudoStateType = s_pseudoStateProp.PropertyType;
        }
        internal static void AddStyleSheet<T>(this T target, string styleSheetPath) where T : VisualElement
        {
            target.styleSheets.Add(MicroContextEditor.LoadRes<StyleSheet>(styleSheetPath));
        }
        internal static void AddToClassList<T>(this T target, params string[] strs) where T : VisualElement
        {
            if (strs != null)
            {
                foreach (var item in strs)
                {
                    target.AddToClassList(item);
                }
            }
        }
        /// <summary>
        /// 获取伪状态
        /// </summary>
        /// <param name="ve"></param>
        /// <returns></returns>
        internal static CustomPseudoStates GetPseudoStates(this VisualElement ve)
        {
            object value = s_pseudoStateProp.GetValue(ve);
            int result = (int)Convert.ChangeType(value, typeof(int));
            return (CustomPseudoStates)result;
        }

        /// <summary>
        /// 设置伪状态
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="pseudoState"></param>
        internal static void SetPseudoStates(this VisualElement ve, int pseudoState)
        {
            object result = Enum.ToObject(s_pseudoStateType, pseudoState);
            s_pseudoStateProp.SetValue(ve, result);
        }
        /// <summary>
        /// 设置伪状态
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="pseudoState"></param>
        internal static void SetPseudoStates(this VisualElement ve, CustomPseudoStates pseudoState)
        {
            object result = Enum.ToObject(s_pseudoStateType, (int)pseudoState);
            s_pseudoStateProp.SetValue(ve, result);
        }

        /// <summary>
        /// 获取VisualElement的属性
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static object GetPropertyEx(this VisualElement ve, PropertyName key)
        {
            return s_getPropertyMethod.Invoke(ve, new object[] { key });
        }
        /// <summary>
        /// 设置VisualElement的属性
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal static void SetPropertyEx(this VisualElement ve, PropertyName key, object value)
        {
            s_setPropertyMethod.Invoke(ve, new object[] { key, value });
        }
        /// <summary>
        /// 判断VisualElement是否有指定的属性
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool HasPropertyEx(this VisualElement ve, PropertyName key)
        {
            return (bool)s_hasPropertyMethod.Invoke(ve, new object[] { key });
        }

        //internal static SerializedProperty GetSerializedProperty(this IMicroEditorConfig config)
        //{
        //    if (config == null)
        //        return null;
        //    SerializedProperty configsProp = MicroContextEditor.EditorSerializedObject.FindProperty("Configs");
        //    foreach (SerializedProperty configProp in configsProp)
        //    {
        //        if (configProp.managedReferenceValue == config)
        //        {
        //            return configProp;
        //        }
        //    }
        //    return null;
        //}
        //internal static SerializedProperty GetSerializedProperty(this IMicroRuntimeConfig config)
        //{
        //    if (config == null)
        //        return null;
        //    SerializedProperty configsProp = MicroContextEditor.RuntimeSerializedObject.FindProperty("Configs");
        //    foreach (SerializedProperty configProp in configsProp)
        //    {
        //        if (configProp.managedReferenceValue == config)
        //        {
        //            return configProp;
        //        }
        //    }
        //    return null;
        //}
    }
}
