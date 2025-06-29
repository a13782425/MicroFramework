//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEditor.UIElements;
//using UnityEngine;
//using UnityEngine.UIElements;

//namespace MFramework.Core.Editor
//{
//    [CustomPropertyDrawer(typeof(DisplayNameAttribute))]
//    public sealed class DisplayNameDrawer : PropertyDrawer
//    {
//        public override VisualElement CreatePropertyGUI(SerializedProperty property)
//        {
//            var att = (DisplayNameAttribute)attribute;
//            return new PropertyField(property, string.IsNullOrWhiteSpace(att.DisplayName) ? property.displayName : att.DisplayName);
//        }
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            var att = (DisplayNameAttribute)attribute;
//            EditorGUI.BeginChangeCheck();
//            EditorGUI.PropertyField(position, property, new GUIContent(string.IsNullOrWhiteSpace(att.DisplayName) ? property.displayName : att.DisplayName));
//            EditorGUI.EndChangeCheck();
//        }
//    }
//}
