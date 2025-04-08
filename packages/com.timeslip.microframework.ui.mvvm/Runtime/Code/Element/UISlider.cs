using MFramework.Core;
using System;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    [Ignore]
    public class UISlider : UIComponent<Slider>
    {
        public UISlider(UIView view, Slider element) : base(view, element)
        {
            target.onValueChanged.AddListener(m_onValueChanged);
        }
        public event Action<float> onValueChanged = null;
        private float _value;
        public float maxValue
        {
            get { return target.maxValue; }
            set
            {
                float oldValue = target.value;
                target.value = value;
                this.Publish("maxValue", oldValue, value);
            }
        }
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
