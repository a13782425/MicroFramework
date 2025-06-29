using MFramework.Core;
using System;
using UnityEngine.UI;


namespace MFramework.Runtime
{
    /// <summary>
    /// Toggle中间层
    /// </summary>
    [Ignore]
    public class UIToggle : UIComponent<Toggle>
    {
        public UIToggle(UIView view, Toggle element) : base(view, element)
        {
            element.onValueChanged.AddListener(m_onValueChanged);
        }

        private void m_onValueChanged(bool value)
        {
            this.Publish("isOn", !value, value);
            _onValueChanged?.Invoke(value);
        }
        /// <summary>
        /// input值发生改变
        /// </summary>
        private Action<bool> _onValueChanged;
        /// <summary>
        /// input值发生改变
        /// </summary>
        public event Action<bool> onValueChanged { add { _onValueChanged += value; } remove { _onValueChanged -= value; } }
        /// <summary>
        /// 是否按下
        /// </summary>
        public bool isOn
        {
            get => target.isOn;
            set
            {
                bool oldValue = target.isOn;
                target.isOn = value;
                this.Publish("isOn", oldValue, value);
            }
        }
    }
}
