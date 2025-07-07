using MFramework.Core;
using MFramework.Runtime;
using MFramework.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MFramework.UI
{
    /// <summary>
    /// UI界面 基类
    /// </summary>
    public partial class UIView : UIObject
    {

        [BindableField("可绑定属性")]
        private int _siblingIndex;
        partial void OnSiblingIndexPreGet() => _siblingIndex = this.RectTransform.GetSiblingIndex();
        partial void OnSiblingIndexPrePublish(int oldValue, int newValue) => this.RectTransform.SetSiblingIndex(newValue);

        private object _data;
        /// <summary>
        /// 界面数据,每次打开界面时候传递的
        /// </summary>
        public object Data { get => _data; internal set => _data = value; }
        private UIState _state = UIState.Load;
        /// <summary>
        /// 界面状态
        /// </summary>
        public UIState State { get => _state; internal set => _state = value; }

        /// <summary>
        /// 下一个状态
        /// </summary>
        private UIState _nextState = UIState.None;
        /// <summary>
        /// 当前界面所在的画布
        /// </summary>
        public Canvas Canvas { get; private set; }
        /// <summary>
        /// 父级界面
        /// </summary>
        public UIView Parent { get; private set; }
        /// <summary>
        /// 当前界面的子界面列表
        /// </summary>
        private readonly Dictionary<int, UIView> _children = new Dictionary<int, UIView>();
        /// <summary>
        /// 资源句柄
        /// </summary>
        private Dictionary<string, ResHandler> _resHandler = new Dictionary<string, ResHandler>();

        /// <summary>
        /// 界面的mono
        /// </summary>
        private UIMono _mono = default;

    }
    //不可重写方法
    partial class UIView
    {

        /// <summary>
        /// 显示一个视图
        /// </summary>
        public void Show(object data = null)
        {
            Data = data;
            Internal_Show();
        }
        /// <summary>
        /// 隐藏一个视图
        /// </summary>
        public void Hide()
        {
            Internal_Hide();
        }
        /// <summary>
        /// 关闭一个视图
        /// </summary>
        public void Close()
        {
            Internal_Close();
        }

        /// <summary>
        /// 设置到当前层级的第一位
        /// </summary>
        public void SetAsFirstSibling()
        {
            this.RectTransform.SetAsFirstSibling();
        }
        /// <summary>
        /// 设置到当前层级的最后一位
        /// </summary>
        public void SetAsLastSibling()
        {
            this.RectTransform.SetAsLastSibling();
        }
        /// <summary>
        /// 添加Widget
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddWidget<T>(Transform parent = null, object data = null) where T : UIWidget, new()
        {
            parent = parent ?? this.Transform;
            T view = UIModuleUtils.CreateView<T>(parent, this, data);
            _children.Add(view.GetHashCode(), view);
            return view;
        }
        /// <summary>
        /// 添加已经加载Widget到一个View中
        /// </summary>
        /// <param name="obj">当前Widget的GameObject</param>
        /// <returns></returns>
        public T AddWidget<T>(GameObject obj, object data = null) where T : UIWidget, new()
        {
            if (obj == null)
                throw new Exception("Widget对应的GameObject是空");
            T view = new T();
            view.Internal_Create(obj, this, data);
            _children.Add(view.GetHashCode(), view);
            return view;
        }
        /// <summary>
        /// 根据类型获取一个Widget
        /// <para>如果不存在则返回空</para>
        /// <para>如果存在多个则返回第一个</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWidget<T>() where T : UIWidget, new()
        {
            return GetWidget(typeof(T)) as T;
        }
        /// <summary>
        /// 根据类型获取一个Widget
        /// <para>如果不存在则返回空</para>
        /// <para>如果存在多个则返回第一个</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UIWidget GetWidget(Type widgetType)
        {
            foreach (var widget in _children)
                if (widget.Value.GetType() == widgetType)
                    return widget.Value as UIWidget;
            return default(UIWidget);
        }
        /// <summary>
        /// 根据类型获取全部Widget
        /// <para>如果不存在则返回空</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetWidgets<T>() where T : UIWidget, new()
        {
            List<T> widgets = new List<T>();
            foreach (var item in _children)
                if (item.Value is T widget)
                    widgets.Add(widget);
            return widgets;
        }
        /// <summary>
        /// 根据类型获取一个Widget
        /// <para>如果不存在则返回空</para>
        /// <para>如果存在多个则返回第一个</para>
        /// </summary>
        /// <returns></returns>
        public List<UIWidget> GetWidgets(Type widgetType)
        {
            List<UIWidget> widgets = new List<UIWidget>();
            foreach (var item in _children)
                if (item.Value.GetType() == widgetType)
                    widgets.Add(item.Value as UIWidget);
            return widgets;
        }

        /// <summary>
        /// 移除类型为T 的widget
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>移除的数量</returns>
        public int RemoveWidget<T>() where T : UIWidget, new()
        {
            int removeCount = 0;
            foreach (var item in _children.Keys.ToList())
            {
                if (_children.TryGetValue(item, out UIView value))
                {
                    if (value is T)
                    {
                        removeCount++;
                        value.Internal_Close();
                    }
                }
            }
            return removeCount;
        }

        /// <summary>
        /// 内部创建方法
        /// </summary>
        internal void Internal_Create(GameObject uiObject, UIView parent = null, object data = null)
        {
            this.GameObject = uiObject;
            this.Data = data;
            this.Parent = parent;
            this.Canvas = this.Transform.GetComponentInParent<Canvas>();
            InitializeElement();
            _mono = uiObject.AddComponent<UIMono>();
            _mono.Init(this);
            this.State = UIState.Loaded;
            switch (_nextState)
            {
                case UIState.None:
                case UIState.Showed:
                    Internal_Show();
                    break;
                case UIState.Hidden:
                    Internal_Hide();
                    break;
                case UIState.Closed:
                    Internal_Close();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 内部显示方法
        /// </summary>
        internal void Internal_Show()
        {
            if (this.State == UIState.Closed)
            {
                Logger?.LogError($"界面：{this} 已经关闭，无法显示");
                return;
            }
            if (this.State == UIState.Load)
            {
                _nextState = UIState.Showed;
                return;
            }
            _nextState = UIState.None;
            this.State = UIState.Showed;
            this.ActiveSelf = true;
            if (this is IMicroLogicUpdate logicUpdate)
                MicroContext.AddLogicUpdate(logicUpdate);
            if (this is IMicroUpdate update)
                MicroContext.AddUpdate(update);
        }
        /// <summary>
        /// 内部隐藏方法
        /// </summary>
        internal void Internal_Hide()
        {
            if (this.State == UIState.Closed)
            {
                Logger?.LogError($"界面：{this} 已经关闭，无法显示");
                return;
            }
            if (this.State == UIState.Load)
            {
                _nextState = UIState.Hidden;
                return;
            }
            _nextState = UIState.None;
            this.State = UIState.Hidden;
            this.ActiveSelf = false;
            if (this is IMicroLogicUpdate logicUpdate)
                MicroContext.RemoveLogicUpdate(logicUpdate);
            if (this is IMicroUpdate update)
                MicroContext.RemoveUpdate(update);
        }
        /// <summary>
        /// 内部关闭方法
        /// </summary>
        internal void Internal_Close()
        {
            if (this.State == UIState.Closed)
            {
                Logger?.LogWarning($"界面：{this} 已经关闭，无需再次关闭");
                return;
            }
            if (this.State == UIState.Load)
            {
                _nextState = UIState.Closed;
                return;
            }
            _nextState = UIState.None;
            foreach (var item in _resHandler)
            {
                UIModuleUtils.UnloadAsset(item.Key);
            }
            _resHandler.Clear();
            _children.Clear();
            this.State = UIState.Closed;
            GameObject.DestroyImmediate(this._mono);
            if (this.Parent != null)
                this.Parent._children.Remove(this.GetHashCode());
            UIModuleUtils.ReleaseUIAsset(this);
        }
    }
    //资源类
    partial class UIView
    {
        /// <summary>
        /// 加载一个资源
        /// 会随着界面关闭而销毁
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
        public void Load<T>(string resPath, ResLoadDelegate<T> callback, params object[] args) where T : UnityObject
        {
            if (GameObject == null)
                throw new NullReferenceException("界面没有加载完成");

            if (_resHandler.TryGetValue(resPath, out ResHandler handler))
            {
                if (handler.IsDone)
                {
                    callback.Invoke(handler.Asset as T, args);
                }
                else
                {
                    m_waitLoad(handler, callback, args).ToDepend(this.GameObject);
                }
            }
            else
            {
                handler = UIModuleUtils.resourceModule.Load<T>(resPath, callback, args);
                _resHandler.Add(resPath, handler);
            }
            async MicroTask m_waitLoad(ResHandler handler, ResLoadDelegate<T> callback, object[] args)
            {
                await handler;
                if (handler.IsCancel)
                    return;
                callback.Invoke(handler.Asset as T, args);
            }
        }
        /// <summary>
        /// 加载一个资源
        /// 会随着界面关闭而销毁
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
        public ResHandler Load(string resPath, Type type = null)
        {
            if (_resHandler.TryGetValue(resPath, out ResHandler handler))
            {
                return handler;
            }
            else
            {
                handler = UIModuleUtils.resourceModule.Load(resPath, type, null);
                _resHandler.Add(resPath, handler);
                return handler;
            }
        }
    }

    //子类可以重写方法
    partial class UIView
    {
        /// <summary>
        /// 初始化组件
        /// </summary>
        protected virtual void InitializeElement()
        {

        }
        /// <summary>
        /// 获取到viewModel
        /// </summary>
        /// <returns></returns>
        protected virtual IViewModel GetViewModel() => null;

        /// <summary>
        /// 界面创建完成,一个界面只会执行一次
        /// </summary>
        protected virtual void OnCreate() { }
        /// <summary>
        /// 界面显示,每次调用Show且自身为隐藏的时候执行
        /// </summary>
        protected virtual void OnEnable() { }
        /// <summary>
        /// 界面隐藏,每次调用Hide且自身为显示的时候执行
        /// </summary>
        protected virtual void OnDisable() { }
        /// <summary>
        /// 界面销毁,一个界面只会执行一次
        /// </summary>
        protected virtual void OnDestroy() { }

    }
    //内部mono
    partial class UIView
    {
        private class UIMono : MonoBehaviour
        {
            private UIView _view;
            private bool _isInit = false;
            private bool _isPanel = false;
            void Awake()
            {
                this.hideFlags = HideFlags.HideAndDontSave;
                this.enabled = false;
            }
            public void Init(UIView view)
            {
                //onCreate
                _isInit = true;
                _view = view;
                _isPanel = view is UIPanel;
                this.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                this.gameObject.name = view.GetType().Name;
                _view.GetViewModel()?.OnCreate();
                _view.OnCreate();
                if (_isPanel)
                    (UIModuleUtils.uiModule as UIModule)?.Internal_Create(this._view);
                this.enabled = true;
            }

            private void OnEnable()
            {
                //onEnable
                if (!_isInit)
                    return;
                this._view.GetViewModel()?.OnEnable();
                this._view.OnEnable();
                if (_isPanel)
                    (UIModuleUtils.uiModule as UIModule)?.Internal_Show(this._view);
            }

            private void OnDisable()
            {
                //onDisable
                if (!_isInit)
                    return;
                this._view.OnDisable();
                this._view.GetViewModel()?.OnDisable();
                if (_isPanel)
                    (UIModuleUtils.uiModule as UIModule)?.Internal_Hide(this._view);
            }

            private void OnDestroy()
            {
                //onDestroy
                if (!_isInit)
                    return;
                _view.OnDestroy();
                _view.GetViewModel()?.OnDestroy();
                if (_isPanel)
                    (UIModuleUtils.uiModule as UIModule)?.Internal_Close(this._view);
            }

        }
    }
}
