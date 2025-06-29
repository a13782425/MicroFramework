//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine.UIElements;

//namespace MFramework.Core.Editor
//{
//    [CustomDrawer(typeof(int))]
//    internal class IntegerCustomDrawer : ICustomDrawer
//    {
//        public CustomDrawerType DrawerType => CustomDrawerType.Basics;

//        public VisualElement DrawUI(object target, FieldInfo fieldInfo, VisualElement originalElement = null)
//        {
//            IntegerField field = new IntegerField(fieldInfo.GetDisplayName());
//            field.value = (int)fieldInfo.GetValue(target);
//            field.RegisterValueChangedCallback(e =>
//            {
//                fieldInfo.SetValue(target, e.newValue);
//            });
//            return field;
//        }
//    }
//}
