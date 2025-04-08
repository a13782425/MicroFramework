using MFramework.Core;
using System;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    [Ignore]
    public class UIScrollbar : UIComponent<Scrollbar>
    {
        public UIScrollbar(UIView view, Scrollbar element) : base(view, element)
        {
            target.onValueChanged.AddListener(m_onValueChanged);
        }

        private float _value;
        /// <summary>
        /// Scrollbar的值改变
        /// </summary>
        public event Action<float> onValueChanged = null;
        public float value
        {
            get { return target.value; }
            set
            {
                float oldValue = _value;
                target.value = value;
                _value = value;
                this.Publish("value", oldValue, value);
            }
        }
        private void m_onValueChanged(float value)
        {
            onValueChanged?.Invoke(value);
            float oldValue = _value;
            _value = value;
            this.Publish("value", oldValue, value);
        }
    }
}
