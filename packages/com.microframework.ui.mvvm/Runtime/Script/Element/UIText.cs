using MFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    /// <summary>
    /// Text中间层
    /// </summary>
    [Ignore]
    public class UIText : UIComponent<Text>
    {
        public UIText(UIView view, Text element) : base(view, element)
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
