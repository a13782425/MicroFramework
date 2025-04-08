using MFramework.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    [Ignore]
    public class UIScrollRect : UIComponent<ScrollRect>
    {
        public UIScrollRect(UIView view, ScrollRect element) : base(view, element)
        {
            target.onValueChanged.AddListener(m_onValueChanged);
        }
        /// <summary>
        /// ScrollRect的值改变
        /// </summary>
        public event Action<Vector2> onValueChanged = null;
        private void m_onValueChanged(Vector2 value)
        {
            onValueChanged?.Invoke(value);
        }
    }
}
