//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//namespace MFramework.Core.Editor
//{
//    [CustomPropertyDrawer(typeof(EnumLabelAttribute))]
//    public sealed class EnumLabelDrawer : PropertyDrawer
//    {
//        private readonly List<string> m_displayNames = new List<string>();

//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            var att = (EnumLabelAttribute)attribute;
//            if (m_displayNames.Count == 0)
//            {
//                var type = property.serializedObject.targetObject.GetType();
//                var field = type.GetField(property.name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//                var enumtype = field.FieldType;
//                foreach (var enumName in property.enumNames)
//                {
//                    var enumfield = enumtype.GetField(enumName);
//                    var hds = enumfield.GetCustomAttributes(typeof(HeaderAttribute), false);
//                    m_displayNames.Add(hds.Length <= 0 ? enumName : ((HeaderAttribute)hds[0]).header);
//                }
//            }
//            EditorGUI.BeginChangeCheck();
//            var value = EditorGUI.Popup(position, string.IsNullOrWhiteSpace(att.header) ? property.displayName : att.header, property.enumValueIndex, m_displayNames.ToArray());
//            if (EditorGUI.EndChangeCheck())
//            {
//                property.enumValueIndex = value;
//            }
//        }
//    }
//}
