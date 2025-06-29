using MFramework.Core;
using System;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    /// <summary>
    /// 下拉菜单
    /// </summary>
    [Ignore]
    public class UIDropdown : UIComponent<Dropdown>
    {
        public UIDropdown(UIView view, Dropdown element) : base(view, element)
        {
            target.onValueChanged.AddListener(m_onValueChanged);
        }
        private int _value;
        /// <summary>
        /// 下拉菜单的值改变
        /// </summary>
        public event Action<int> onValueChanged = null;
        public int value
        {
            get { return target.value; }
            set
            {
                int oldValue = _value;
                target.value = value;
                _value = value;
                this.Publish("value", oldValue, value);
            }
        }
        private void m_onValueChanged(int value)
        {
            onValueChanged?.Invoke(value);
            int oldValue = _value;
            _value = value;
            this.Publish("value", oldValue, value);
        }
    }
}
