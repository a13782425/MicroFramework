using MFramework.Core;
using TMPro;
using UnityEngine;

namespace MFramework.Runtime
{
    [Ignore]
    public class TMP_UIText : UIComponent<TextMeshProUGUI>
    {
        public TMP_UIText(UIView view, TextMeshProUGUI element) : base(view, element)
        {
        }
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
                this.Publish("text", oldValue, value);
            }
        }
        /// <summary>
        /// 组件颜色
        /// </summary>
        public Color color
        {
            get => target.color;
            set
            {
                Color oldValue = target.color;
                target.color = value;
                this.Publish("color", oldValue, value);
            }
        }
    }
}
