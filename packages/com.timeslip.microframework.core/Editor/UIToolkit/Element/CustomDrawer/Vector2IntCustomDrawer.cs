using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    [CustomDrawer(typeof(Vector2Int))]
    internal class Vector2IntCustomDrawer : ICustomDrawer
    {
        public CustomDrawerType DrawerType => CustomDrawerType.Basics;

        public VisualElement DrawUI(object target, FieldInfo fieldInfo, VisualElement originalElement = null)
        {
            Vector2IntField field = new Vector2IntField(fieldInfo.GetDisplayName());
            field.value = (Vector2Int)fieldInfo.GetValue(target);
            field.RegisterValueChangedCallback(e =>
            {
                fieldInfo.SetValue(target, e.newValue);
            });
            return field;
        }
    }
}
