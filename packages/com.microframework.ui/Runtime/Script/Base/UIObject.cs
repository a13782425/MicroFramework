using MFramework.Core;
using MFramework.Runtime;
using UnityEngine;

namespace MFramework.UI
{
    /// <summary>
    /// UI 基类
    /// </summary>
    public abstract partial class UIObject : IBindable
    {
        private GameObject _gameObject;
        public GameObject gameObject
        {
            get => _gameObject;
            internal set
            {
                _gameObject = value;
                this._rectTransform = _gameObject.transform as RectTransform;
                this._transform = _gameObject.transform;
            }
        }
        private IMicroLogger _logger;
        /// <summary>
        /// 日志对象
        /// </summary>
        protected IMicroLogger logger => _logger;
        protected UIObject()
        {
            _instanceId = UIModuleUtils.GetInstanceId();
            _logger = MicroLogger.GetMicroLogger(this.GetType().Name);
        }
        private int _instanceId = 0;
        private Transform _transform;
        public Transform transform => _transform;
        private RectTransform _rectTransform;
        public RectTransform rectTransform => _rectTransform;

        ~UIObject()
        {
            UIModuleUtils.RecoverInstanceId(_instanceId);
        }

        #region 可绑定类型

        [BindableField("可绑定属性")]
        private string _name;
        [BindableField("可绑定属性")]
        private HideFlags _hideFlags;
        [BindableField("可绑定属性")]
        private bool _activeSelf;
        [BindableField("可绑定属性")]
        private Vector2 _sizeDelta;
        [BindableField("可绑定属性")]
        private Vector2 _anchoredPosition;
        [BindableField("可绑定属性")]
        private Vector3 _anchoredPosition3D;
        [BindableField("可绑定属性")]
        private Vector3 _position;
        [BindableField("可绑定属性")]
        private Vector3 _localPosition;
        [BindableField("可绑定属性")]
        private Vector3 _localScale;
        [BindableField("可绑定属性")]
        private Vector3 _eulerAngles;
        [BindableField("可绑定属性")]
        private Vector3 _localEulerAngles;
        [BindableField("可绑定属性")]
        private Quaternion _rotation;
        [BindableField("可绑定属性")]
        private Quaternion _localRotation;

        #endregion

        #region 部分方法
        partial void OnNamePreGet() => _name = this._gameObject.name;
        partial void OnNamePrePublish(string oldValue, string newValue) => this._gameObject.name = newValue;
        partial void OnHideFlagsPreGet() => _hideFlags = this._gameObject.hideFlags;
        partial void OnHideFlagsPrePublish(HideFlags oldValue, HideFlags newValue) => this._gameObject.hideFlags = newValue;
        partial void OnActiveSelfPreGet() => _activeSelf = this._gameObject.activeSelf;
        partial void OnActiveSelfPrePublish(bool oldValue, bool newValue) => _gameObject.SetActive(newValue);
        partial void OnSizeDeltaPreGet() => _sizeDelta = this._rectTransform.sizeDelta;
        partial void OnSizeDeltaPrePublish(Vector2 oldValue, Vector2 newValue) => this._rectTransform.sizeDelta = newValue;
        partial void OnAnchoredPositionPreGet() => _anchoredPosition = this._rectTransform.anchoredPosition;
        partial void OnAnchoredPositionPrePublish(Vector2 oldValue, Vector2 newValue) => this._rectTransform.anchoredPosition = newValue;
        partial void OnAnchoredPosition3DPreGet() => _anchoredPosition3D = this._rectTransform.anchoredPosition3D;
        partial void OnAnchoredPosition3DPrePublish(Vector3 oldValue, Vector3 newValue) => this._rectTransform.anchoredPosition3D = newValue;
        partial void OnPositionPreGet() => _position = this._rectTransform.position;
        partial void OnPositionPrePublish(Vector3 oldValue, Vector3 newValue) => this._rectTransform.position = newValue;
        partial void OnLocalPositionPreGet() => _localPosition = this._rectTransform.localPosition;
        partial void OnLocalPositionPrePublish(Vector3 oldValue, Vector3 newValue) => this._rectTransform.localPosition = newValue;
        partial void OnLocalScalePreGet() => _localScale = this._rectTransform.localScale;
        partial void OnLocalScalePrePublish(Vector3 oldValue, Vector3 newValue) => this._rectTransform.localScale = newValue;
        partial void OnEulerAnglesPreGet() => _eulerAngles = this._rectTransform.eulerAngles;
        partial void OnEulerAnglesPrePublish(Vector3 oldValue, Vector3 newValue) => this._rectTransform.eulerAngles = newValue;
        partial void OnLocalEulerAnglesPreGet() => _localEulerAngles = this._rectTransform.localEulerAngles;
        partial void OnLocalEulerAnglesPrePublish(Vector3 oldValue, Vector3 newValue) => this._rectTransform.localEulerAngles = newValue;
        partial void OnRotationPreGet() => _rotation = this._rectTransform.rotation;
        partial void OnRotationPrePublish(Quaternion oldValue, Quaternion newValue) => this._rectTransform.rotation = newValue;
        partial void OnLocalRotationPreGet() => _localRotation = this._rectTransform.localRotation;
        partial void OnLocalRotationPrePublish(Quaternion oldValue, Quaternion newValue) => this._rectTransform.localRotation = newValue;

        #endregion

        public override int GetHashCode()
        {
            return _instanceId;
        }


    }
}
