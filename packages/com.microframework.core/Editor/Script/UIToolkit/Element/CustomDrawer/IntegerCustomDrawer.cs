using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace MFramework.Core.Editor
{
    [CustomDrawer(typeof(int))]
    internal class IntegerCustomDrawer : ICustomDrawer
    {
        public CustomDrawerType DrawerType => CustomDrawerType.Basics;

        public VisualElement DrawUI(MicroObjectField objectField, FieldInfo fieldInfo)
        {
            IntegerField field = new IntegerField(MicroContextEditorUtils.GetDisplayName(fieldInfo));
            field.value = (int)fieldInfo.GetValue(objectField.Value);
            field.RegisterValueChangedCallback(e =>
            {
                fieldInfo.SetValue(objectField.Value, e.newValue);
            });
            return field;
        }
    }
}
