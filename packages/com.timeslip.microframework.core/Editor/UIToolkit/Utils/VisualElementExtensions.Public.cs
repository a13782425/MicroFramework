using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using ElementHierarchy = UnityEngine.UIElements.VisualElement.Hierarchy;
using System.Reflection;

namespace MFramework.Core.Editor
{
    partial class VisualElementExtensions
    {
        //private static VisualElement DrawUI(FieldInfo fieldInfo, Func<object> getValue, Action<object> setValue)
        //{
        //    Type fieldType = fieldInfo.FieldType;
        //    if (fieldType.IsAbstract|| fieldType.IsInterface)
        //    {
        //        return null;
        //    }
        //    return null;
        //}
        
        ///// <summary>
        ///// 绘制一个对象
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public static VisualElement DrawUI(this object obj, bool showBorder = true)
        //{
        //    VisualElement container = new VisualElement();
        //    if (showBorder)
        //    {
        //        container.style.borderLeftWidth = 2;
        //        container.style.borderRightWidth = 2;
        //        container.style.borderTopWidth = 2;
        //        container.style.borderBottomWidth = 2;
        //        float color = 39 / 255f;
        //        container.style.borderRightColor = new Color(color, color, color, 1);
        //        container.style.borderLeftColor = new Color(color, color, color, 1);
        //        container.style.borderTopColor = new Color(color, color, color, 1);
        //        container.style.borderBottomColor = new Color(color, color, color, 1);
        //        container.style.borderTopLeftRadius = 4;
        //        container.style.borderTopRightRadius = 4;
        //        container.style.borderBottomLeftRadius = 4;
        //        container.style.borderBottomRightRadius = 4;
        //        container.style.marginLeft = 2;
        //        container.style.marginRight = 2;
        //        container.style.marginTop = 2;
        //        container.style.marginBottom = 2;
        //    }
            
        //    if (obj == null)
        //        return container;
        //    ScrollView scrollView = new ScrollView();
        //    scrollView.style.marginLeft = 16;
        //    if (showBorder)
        //    {
        //        float color = 39 / 255f;
        //        Foldout foldout = new Foldout();
        //        foldout.style.borderBottomWidth = 2;
        //        foldout.style.borderBottomColor = new Color(color, color, color, 1);
        //        color = 53 / 255f;
        //        foldout.style.backgroundColor = new Color(color, color, color, 1);
        //        foldout.text = obj.GetDisplayName();
        //        foldout.RegisterValueChangedCallback(e => scrollView.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None);
        //        container.Add(foldout);
        //    }
        //    container.Add(scrollView);
        //    List<FieldInfo> fields = MicroTypeCache.GetSerializedFields(obj);
        //    foreach (var item in fields)
        //    {
        //        VisualElement element = DrawUI(obj, item);
        //        if (element != null)
        //            scrollView.Add(element);
        //    }
        //    return container;
        //}
        ///// <summary>
        ///// 绘制一个对象中的字段
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="fieldName"></param>
        ///// <returns></returns>
        //public static VisualElement DrawUI(this object obj, string fieldName)
        //{
        //    if (obj == null)
        //        return null;
        //    var fieldInfo = MicroTypeCache.GetSerializedFields(obj).FirstOrDefault(item => item.Name == fieldName);
        //    if (fieldInfo == null)
        //        return null;
        //    return DrawUI(obj, fieldInfo);
        //}
        ///// <summary>
        ///// 绘制一个对象中的字段
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="fieldInfo"></param>
        ///// <returns></returns>
        //public static VisualElement DrawUI(this object obj, FieldInfo fieldInfo)
        //{
        //    if (obj == null || fieldInfo == null)
        //        return null;
        //    var attrs = fieldInfo.GetCustomAttributes<Attribute>();
        //    List<ICustomDrawer> resutls = new List<ICustomDrawer>();
        //    Type fieldType = fieldInfo.FieldType;
        //    if (fieldType.IsEnum)
        //        fieldType = typeof(Enum);
        //    else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        //        fieldType = typeof(List<>);
        //    if (MicroContextEditor.CustomDrawers.TryGetValue(fieldType, out ICustomDrawer drawer))
        //        resutls.Add(drawer);
        //    else if (fieldInfo.FieldType.IsClass && !fieldInfo.FieldType.IsAbstract)
        //    {
        //        object fieldValue = fieldInfo.GetValue(obj);
        //        if (fieldValue == null)
        //        {
        //            fieldValue = Activator.CreateInstance(fieldInfo.FieldType);
        //            fieldInfo.SetValue(obj, fieldValue);
        //        }
        //        return DrawUI(fieldValue);
        //    }
        //    foreach (var item in attrs)
        //    {
        //        if (MicroContextEditor.CustomDrawers.TryGetValue(item.GetType(), out drawer))
        //            resutls.Add(drawer);
        //    }
        //    if (resutls.Count == 0)
        //        return null;
        //    bool hasBaisc = false;
        //    for (int i = resutls.Count - 1; i >= 0; i--)
        //    {
        //        drawer = resutls[i];
        //        if (drawer.DrawerType == CustomDrawerType.Basics)
        //        {
        //            if (hasBaisc)
        //                resutls.RemoveAt(i);
        //            else
        //                hasBaisc = true;
        //        }
        //    }
        //    resutls.Sort((x, y) => x.DrawerType.CompareTo(y.DrawerType));
        //    VisualElement container = new VisualElement();
        //    VisualElement element = null;
        //    //foreach (var item in resutls)
        //    //{
        //    //    switch (item.DrawerType)
        //    //    {
        //    //        case CustomDrawerType.Basics:
        //    //            element = item.DrawUI(obj, fieldInfo);
        //    //            if (element != null)
        //    //                container.Add(element);
        //    //            break;
        //    //        case CustomDrawerType.Modify:
        //    //            if (element != null)
        //    //                element = item.DrawUI(obj, fieldInfo, element);
        //    //            if (element != null)
        //    //            {
        //    //                container.RemoveAt(container.childCount - 1);
        //    //                container.Add(element);
        //    //            }
        //    //            break;
        //    //        case CustomDrawerType.PreDecorate:
        //    //            container.Add(item.DrawUI(obj, fieldInfo));
        //    //            break;
        //    //        case CustomDrawerType.NextDecorate:
        //    //            container.Add(item.DrawUI(obj, fieldInfo));
        //    //            break;
        //    //    }
        //    //}
        //    if (resutls.Count > 0)
        //        return container;
        //    return element;
        //}
        public static string GetDisplayName(this object obj)
        {
            if (obj == null)
                return "";
            var attr = obj.GetType().GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
                return attr.DisplayName;
            else
                return obj.GetType().Name;
        }
        public static string GetDisplayName(this FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return "";
            var attr = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
                return attr.DisplayName;
            else
                return fieldInfo.Name;
            
        }
        
        public static VisualElement DrawUI(this ICustomMicroEditorConfig config)
        {
            if (config == null)
                return null;
            SerializedProperty property = config.GetSerializedProperty();
            if (property == null)
                return null;
            string displayName = config.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            PropertyField field = new PropertyField(property, displayName ?? config.GetType().Name);
            field.Bind(property.serializedObject);
            return field;
        }
        
        //public static VisualElement DrawUI(this ICustomMicroRuntimeConfig config)
        //{
        //    if (config == null)
        //        return null;
        //    SerializedProperty property = config.GetSerializedProperty();
        //    if (property == null)
        //        return null;
        //    string displayName = config.GetType().GetCustomAttribute<DisplayNameAttribute>()?.showName;
        //    PropertyField field = new PropertyField(property, displayName ?? config.GetType().Name);
        //    field.Bind(property.serializedObject);
        //    return field;
        //}
        
        /// <summary> Get an element's layout rect in local space.</summary>
        /// <param name="ve">The VisualElement.</param>
        /// <returns>The local layout rect.</returns>
        public static Rect GetLocalRect(this VisualElement ve)
        {
            var layout = ve.layout;
            return new Rect(0, 0, layout.width, layout.height);
        }
        
        /// <summary>
        /// Get the direct children of a VisualElement filtered by type and an optional filter delegate. It stores them in a List parameter
        /// to avoid memory allocations.
        /// </summary>
        /// <typeparam name="TElement">The type of the child elements</typeparam>
        /// <param name="ve">The parent element</param>
        /// <param name="results">A list to store the results.</param>
        /// <param name="filter">An optional filter callback.</param>
        public static void GetChildren<TElement>(this VisualElement ve, List<TElement> results, Func<TElement, bool> filter = null) where TElement : VisualElement
        {
            if (ve.contentContainer == ve)
                ve.hierarchy.GetChildren(results, filter);
            else
                ve.contentContainer.GetChildren(results, filter);
        }
        
        /// <summary>
        /// Get the direct children of a <see cref="Hierarchy"/> filtered by type and an optional filter delegate. It stores them in a List parameter
        /// to avoid memory allocations.
        /// </summary>
        /// <typeparam name="TElement">The type of the child elements</typeparam>
        /// <param name="hierarchy">The parent hierarchy</param>
        /// <param name="results">A List to store the results</param>
        /// <param name="filter">An optional filter callback</param>
        public static void GetChildren<TElement>(this ElementHierarchy hierarchy, List<TElement> results, Func<TElement, bool> filter = null) where TElement : VisualElement
        {
            for (int i = 0; i < hierarchy.childCount; i++)
                if (hierarchy[i] is TElement element && (filter == null || filter(element)))
                    results.Add(element);
        }
        
        /// <summary>
        /// Get the first direct child with a certain type that passes an optional filter delegate.
        /// </summary>
        /// <typeparam name="TElement">The type of the child</typeparam>
        /// <param name="ve">The parent element</param>
        /// <param name="filter">An optional filter callback</param>
        /// <returns>A child that satisfies conditions or null.</returns>
        public static TElement GetFirstChild<TElement>(this VisualElement ve, Func<TElement, bool> filter = null) where TElement : VisualElement
        {
            if (ve.contentContainer == ve)
                return ve.hierarchy.GetFirstChild(filter);
            else
                return ve.contentContainer.GetFirstChild(filter);
        }
        
        /// <summary>
        /// Get the first direct child with a certain type that passes an optional filter delegate.
        /// </summary>
        /// <typeparam name="TElement">The type of the child</typeparam>
        /// <param name="hierarchy">The parent element</param>
        /// <param name="filter">An optional filter callback</param>
        /// <returns>A child that satisfies conditions or null.</returns>
        public static TElement GetFirstChild<TElement>(this ElementHierarchy hierarchy, Func<TElement, bool> filter = null) where TElement : VisualElement
        {
            for (int i = 0; i < hierarchy.childCount; i++)
                if (hierarchy[i] is TElement element && (filter == null || filter(element)))
                    return element;
            
            return null;
        }
        
        /// <summary>
        /// Execute an action on all direct children with a certain type.
        /// </summary>
        /// <typeparam name="TElement">The type of the child elements.</typeparam>
        /// <param name="ve">The parent element.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEachChild<TElement>(this VisualElement ve, Action<TElement> action) where TElement : VisualElement
        {
            if (ve.contentContainer == ve)
                ve.hierarchy.ForEachChild<TElement>(action);
            else
                ve.contentContainer.ForEachChild<TElement>(action);
        }
        
        /// <summary>
        /// Execute an action on all direct children with a certain type.
        /// </summary>
        /// <typeparam name="TElement">The type of the child elements.</typeparam>
        /// <param name="hierarchy">The parent hierarchy.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEachChild<TElement>(this ElementHierarchy hierarchy, Action<TElement> action) where TElement : VisualElement
        {
            for (int i = 0; i < hierarchy.childCount; i++)
                if (hierarchy[i] is TElement element)
                    action(element);
        }
    }
}