using MFramework.Runtime;
using UnityEngine.EventSystems;

namespace MFramework.UI
{
    /// <summary>
    /// UI组件基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class UIComponent<T> : UIObject where T : UIBehaviour
    {
        /// <summary>
        /// 控件
        /// </summary>
        public readonly T target;
        protected readonly UIView view;
        public UIComponent(UIView view, T element)
        {
            target = element;
            this.GameObject = element.gameObject;
            this.view = view;
        }
        [BindableField("可绑定属性")]
        private bool _enable;
        partial void OnEnablePreGet() => _enable = target.enabled;
        partial void OnEnablePrePublish(bool oldValue, bool newValue) => target.enabled = newValue;
    }
}
