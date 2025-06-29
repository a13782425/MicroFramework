//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UIElements;

//namespace MFramework.Core.Editor
//{
//    [CustomDrawer(typeof(RangeAttribute))]
//    internal class RangeAttributeCustomDrawer : ICustomDrawer
//    {
//        public CustomDrawerType DrawerType => CustomDrawerType.Basics;

//        public VisualElement DrawUI(object target, FieldInfo fieldInfo, VisualElement originalElement = null)
//        {
//            RangeAttribute rangeAttribute = fieldInfo.GetCustomAttribute<RangeAttribute>();
//            Slider field = new Slider(fieldInfo.GetDisplayName(), rangeAttribute.min, rangeAttribute.max);
//            field.showInputField = true;
//            field.showMixedValue = true;
//            field.value = (float)fieldInfo.GetValue(target);
//            field.RegisterValueChangedCallback((e) =>
//            {
//                fieldInfo.SetValue(target, e.newValue);
//            });
//            return field;
//        }
//    }
//}
