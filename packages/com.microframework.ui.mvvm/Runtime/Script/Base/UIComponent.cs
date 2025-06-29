using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFramework.Runtime
{
    /// <summary>
    /// 所有UI的根入口
    /// </summary>    
    [Ignore]
    public abstract class UIComponent : IBindable
    {
        private GameObject _gameObject;
        public GameObject gameObject
        {
            get => _gameObject;
            internal set
            {
                _gameObject = value;
                this.rectTransform = _gameObject.transform as RectTransform;
                this.transform = _gameObject.transform;
            }
        }
        private IMicroLogger _logger;
        protected IMicroLogger logger => _logger;
        protected UIComponent()
        {
            _instanceId = UIModuleUtils.GetInstanceId();
            _logger = MicroLogger.GetMicroLogger(this.GetType().Name);
        }
        public Transform transform { get; private set; }
        public RectTransform rectTransform { get; private set; }
        /// <summary>
        /// 所有订阅
        /// key:组件ID,Value:当前组件上的所有订阅
        /// </summary>
        private List<IUIObserver> _observers = new List<IUIObserver>();

        private int _instanceId = 0;

        #region Unity属性

        public string name { get => gameObject.name; set => gameObject.name = value; }
        public HideFlags hideFlags { get => gameObject.hideFlags; set => gameObject.hideFlags = value; }
        public bool activeSelf
        {
            get => gameObject.activeSelf;
            set
            {
                bool oldValue = gameObject.activeSelf;
                gameObject.SetActive(value);
                this.Publish("activeSelf", oldValue, value);
            }
        }
        public Vector2 sizeDelta
        {
            get => rectTransform.sizeDelta;
            set
            {
                Vector2 oldValue = rectTransform.sizeDelta;
                rectTransform.sizeDelta = value;
                this.Publish("sizeDelta", oldValue, value);
            }
        }
        public Vector2 anchoredPosition
        {
            get => rectTransform.anchoredPosition;
            set
            {
                Vector2 oldValue = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = value;
                this.Publish("anchoredPosition", oldValue, value);
            }
        }
        public Vector3 position
        {
            get => rectTransform.position;
            set
            {
                Vector3 oldValue = rectTransform.position;
                rectTransform.position = value;
                this.Publish("position", oldValue, value);
            }
        }
        public Vector3 localPosition
        {
            get => rectTransform.localPosition;
            set
            {
                Vector3 oldValue = rectTransform.localPosition;
                rectTransform.localPosition = value;
                this.Publish("localPosition", oldValue, value);
            }
        }
        public Vector3 localScale
        {
            get => rectTransform.localScale;
            set
            {
                Vector3 oldValue = rectTransform.localScale;
                rectTransform.localScale = value;
                this.Publish("localScale", oldValue, value);
            }
        }
        public Vector3 eulerAngles
        {
            get => transform.eulerAngles;
            set
            {
                Vector3 oldValue = transform.eulerAngles;
                transform.eulerAngles = value;
                this.Publish("eulerAngles", oldValue, value);
            }
        }
        public Vector3 localEulerAngles
        {
            get => transform.localEulerAngles;
            set
            {
                Vector3 oldValue = transform.localEulerAngles;
                transform.localEulerAngles = value;
                this.Publish("localEulerAngles", oldValue, value);
            }
        }
        public Quaternion localRotation
        {
            get => transform.localRotation;
            set
            {
                Quaternion oldValue = transform.localRotation;
                transform.localRotation = value;
                this.Publish("localRotation", oldValue, value);
            }
        }
        public Quaternion rotation
        {
            get => transform.rotation;
            set
            {
                Quaternion oldValue = transform.rotation;
                transform.rotation = value;
                this.Publish("rotation", oldValue, value);
            }
        }

        #endregion

        public override int GetHashCode()
        {
            return _instanceId;
        }

        internal virtual void Release()
        {
            UIModuleUtils.RecoverInstanceId(this.GetHashCode());
        }

        #region IBindableHandle
        private Dictionary<string, BindableHandle> _bindableDic = new();
        BindableHandle IBindable.GetHandle(object subscribeKey = null)
        { 
            string key = subscribeKey == null? BindableUtils.BINDABLE_ALL_KEY : subscribeKey.ToString();
            if (_bindableDic.TryGetValue(key, out BindableHandle bindable))
            {
                return bindable;
            }
            if (key == BindableUtils.BINDABLE_ALL_KEY)
            {
                bindable = new BindableHandle();
                bindable.Init(this, subscribeKey, null);
                _bindableDic.Add(key, bindable);
            }
            else
            {
                PropertyInfo property = this.GetType().GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                {
                    _logger.LogError($"没找找到：{key} 订阅属性");
                    return ((IBindable)this).GetHandle(BindableUtils.BINDABLE_ALL_KEY);
                }
                bindable = (BindableHandle)Activator.CreateInstance(typeof(BindableHandle<>).MakeGenericType(property.PropertyType));
                bindable.Init(this, key, ((IBindable)this).GetHandle(BindableUtils.BINDABLE_ALL_KEY));
                _bindableDic.Add(key, bindable);
            }
            return bindable;
        }

        #endregion
    }

    /// <summary>
    /// UI控件基类
    /// </summary>
    [Ignore]
    public abstract class UIComponent<T> : UIComponent where T : UIBehaviour
    {
        /// <summary>
        /// 控件
        /// </summary>
        public readonly T target;
        protected readonly UIView view;
        public UIComponent(UIView view, T element)
        {
            target = element;
            this.gameObject = element.gameObject;
            this.view = view;
            view.RegisterComponent(this);
        }
        private ComponentTriggerRule _trigger = ComponentTriggerRule.Default;
        /// <summary>
        /// 绑定组件触发规则
        /// TwoWay和OneWayToSource时候生效
        /// </summary>
        public ComponentTriggerRule trigger
        {
            get => _trigger;
            set => _trigger = value;
        }

        public bool enabled
        {
            get => target.enabled;
            set
            {
                bool oldValue = target.enabled;
                target.enabled = value;
                this.Publish("enabled", oldValue, value);
            }
        }
    }
}
