using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    [CustomDrawer(typeof(Enum))]
    internal class EnumCustomDrawer : ICustomDrawer
    {
        public CustomDrawerType DrawerType => CustomDrawerType.Basics;

        public VisualElement DrawUI(object target, FieldInfo fieldInfo, VisualElement originalElement = null)
        {
            Enum enumValue = (Enum)fieldInfo.GetValue(target);
            EnumField field = new EnumField(fieldInfo.GetDisplayName(), enumValue);
            field.RegisterValueChangedCallback(e =>
            {
                fieldInfo.SetValue(target, e.newValue);
            });
            return field;
        }
    }
}
