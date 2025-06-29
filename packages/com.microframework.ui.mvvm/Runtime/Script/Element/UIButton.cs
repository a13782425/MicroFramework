using MFramework.Core;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    /// <summary>
    /// 自定义按钮
    /// </summary>
    [Ignore]
    public class UIButton : UIComponent<Button>
    {

        /// <summary>
        /// 双击间隔
        /// </summary>
        public float doubleClickInterval { get => _uIClickEvent.doubleClickInterval; set => _uIClickEvent.doubleClickInterval = value; }
        /// <summary>
        /// 长按时间
        /// </summary>
        public float pressTime { get => _uIClickEvent.pressTime; set => _uIClickEvent.pressTime = value; }

        /// <summary>
        /// 点击事件
        /// </summary>
        private Action<GameObject> _onClick;
        /// <summary>
        /// 双击
        /// </summary>
        private Action<GameObject> _onDoubleClick;
        /// <summary>
        /// 右键点击，在移动端无效
        /// </summary>
        private Action<GameObject> _onRightClick;
        /// <summary>
        /// 长按
        /// </summary>
        private Action<GameObject> _onPress;
        private UIClickEvent _uIClickEvent;
        public UIButton(UIView view, Button element) : base(view, element)
        {
            _uIClickEvent = UIClickEvent.Get(gameObject);
        }

        /// <summary>
        ///  点击事件
        /// </summary>
        public event Action<GameObject> onClick
        {
            add
            {
                if (_onClick == null)
                    _uIClickEvent.onClick += m_onClick;
                _onClick += value;

            }
            remove
            {
                _onClick -= value;
                if (_onClick == null)
                    _uIClickEvent.onClick -= m_onClick;
            }
        }
        /// <summary>
        /// 双击事件
        /// </summary>
        public event Action<GameObject> onDoubleClick
        {
            add
            {
                if (_onDoubleClick == null)
                    _uIClickEvent.onDoubleClick += m_onDoubleClick;
                _onDoubleClick += value;
            }
            remove
            {
                _onDoubleClick -= value;
                if (_onDoubleClick == null)
                    _uIClickEvent.onDoubleClick -= m_onDoubleClick;
            }
        }
        /// <summary>
        /// 右键事件（移动端不会触发）
        /// </summary>
        public event Action<GameObject> onRightClick
        {
            add
            {
                if (_onRightClick == null)
                    _uIClickEvent.onRightClick += m_onRightClick;
                _onRightClick += value;
            }
            remove
            {
                _onRightClick -= value;
                if (_onRightClick == null)
                    _uIClickEvent.onRightClick -= m_onRightClick;
            }
        }
        /// <summary>
        /// 长按
        /// </summary>
        public event Action<GameObject> onPress
        {
            add
            {
                if (_onPress == null)
                    _uIClickEvent.onPress += m_onPress;
                _onPress += value;
            }
            remove
            {
                _onPress -= value;
                if (_onPress == null)
                    _uIClickEvent.onPress -= m_onPress;
            }
        }
        private void m_onPress(PointerEventData obj) => _onPress.Invoke(gameObject);
        private void m_onRightClick(PointerEventData obj) => _onRightClick.Invoke(gameObject);

        private void m_onDoubleClick(PointerEventData obj) => _onDoubleClick.Invoke(gameObject);

        private void m_onClick(PointerEventData obj) => _onClick.Invoke(gameObject);
    }
}
