using MFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    [Ignore]
    public class UIRawImage : UIComponent<RawImage>
    {
        public UIRawImage(UIView view, RawImage element) : base(view, element)
        {
        }
        /// <summary>
        /// texture
        /// </summary>
        public Texture texture
        {
            get { return target.texture; }
            set
            {
                Texture oldValue = target.texture;
                target.texture = value;
                this.Publish("texture", oldValue, value);
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
