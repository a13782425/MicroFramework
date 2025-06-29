using MFramework.Core;
using System;
using TMPro;

namespace MFramework.Runtime
{
    [Ignore]
    public class TMP_UIInputField : UIComponent<TMP_InputField>
    {
        public TMP_UIInputField(UIView view, TMP_InputField element) : base(view, element)
        {
            element.onValueChanged.AddListener(m_onValueChanged);
            element.onEndEdit.AddListener(m_onEndEdit);
        }
        private void m_onEndEdit(string value)
        {
            if (trigger == ComponentTriggerRule.EndChanged)
            {
                string oldValue = _text;
                _text = value;
                this.Publish("text", oldValue, value);
            }
        }

        private void m_onValueChanged(string value)
        {
            _onValueChanged?.Invoke(value);
            if (trigger == ComponentTriggerRule.Default)
            {
                string oldValue = _text;
                _text = value;
                this.Publish("text", oldValue, value);
            }
        }

        /// <summary>
        /// input值发生改变
        /// </summary>
        private Action<string> _onValueChanged;
        /// <summary>
        /// input值发生改变
        /// </summary>
        public event Action<string> onValueChanged { add { _onValueChanged += value; } remove { _onValueChanged -= value; } }

        private string _text;
        /// <summary>
        /// text
        /// </summary>
        public string text
        {
            get => target.text;
            set
            {
                string oldValue = target.text;
                target.text = value;
                _text = value;
                this.Publish("text", oldValue, value);
            }
        }
    }
}
