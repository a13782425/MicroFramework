using MFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    /// <summary>
    /// Image中间件
    /// </summary>
    [Ignore]
    public class UIImage : UIComponent<Image>
    {
        public UIImage(UIView view, Image element) : base(view, element)
        {
        }
        /// <summary>
        /// sprite
        /// </summary>
        public Sprite sprite
        {
            get => target.sprite;
            set
            {
                Sprite oldValue = target.sprite;
                target.sprite = value;
                this.Publish("sprite", oldValue, value);
            }
        }
        /// <summary>
        /// fillAmount
        /// </summary>
        public float fillAmount
        {
            get => target.fillAmount;
            set
            {
                float oldValue = target.fillAmount;
                target.fillAmount = value;
                this.Publish("fillAmount", oldValue, value);
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
