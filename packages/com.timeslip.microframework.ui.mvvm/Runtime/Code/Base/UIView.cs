using MFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Profiling.HierarchyFrameDataView;
using UnityObject = UnityEngine.Object;

namespace MFramework.Runtime
{
    /// <summary>
    /// 界面基类
    /// </summary>
    [Ignore]
    public partial class UIView : UIComponent
    {
        protected UIView() : base()
        {
            _cancelationSource = CancelationSource.New();
        }

        /// <summary>
        /// 设置UI层级
        /// </summary>
        public int SiblingIndex { get { return this.rectTransform.GetSiblingIndex(); } set { this.rectTransform.SetSiblingIndex(value); } }

        private UIState _state = UIState.Load;
        /// <summary>
        /// 界面状态
        /// </summary>
        public UIState state { get => _state; internal set => _state = value; }

        private object _data;
        /// <summary>
        /// 界面数据,每次打开界面时候传递的
        /// </summary>
        public object data { get => _data; internal set => _data = value; }

        /// <summary>
        /// 当前界面所在的画布
        /// </summary>
        public Canvas canvas { get; private set; }

        public UIView parent { get; private set; }

        private readonly Dictionary<int, UIComponent> _coms = new Dictionary<int, UIComponent>();
        private readonly Dictionary<int, UIView> _widgets = new Dictionary<int, UIView>();
        private CancelationSource _cancelationSource;
    }
    //子类资源加载
    partial class UIView
    {
        /// <summary>
        /// 资源句柄
        /// </summary>
        private Dictionary<string, ResHandler> _resHandler = new Dictionary<string, ResHandler>();
        /// <summary>
        /// 加载一个资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resPath"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        protected internal void Load<T>(string resPath, ResLoadDelegate<T> callback, params object[] args) where T : UnityObject
        {
            if (_resHandler.TryGetValue(resPath, out ResHandler handler))
            {
                if (handler.isDone)
                {
                    callback.Invoke(handler.asset as T, args);
                }
                else
                {
                    m_waitLoad(handler, callback, args).WaitAsync(_cancelationSource.Token);
                }
            }
            else
            {
                handler = UIModuleUtils.resourceModule.Load<T>(resPath, callback, args);
                _resHandler.Add(resPath, handler);
            }
            async Promise m_waitLoad(ResHandler handler, ResLoadDelegate<T> callback, object[] args)
            {
                await handler;
                if (handler.isCancel)
                    return;
                callback.Invoke(handler.asset as T, args);
            }
        }
        /// <summary>
        /// 加载一个资源
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
        protected internal ResHandler Load(string resPath, Type type = null)
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

    //不可重写方法
    partial class UIView
    {
        /// <summary>
        /// 设置到当前层级的第一位
        /// </summary>
        public void SetAsFirstSibling()
        {
            this.rectTransform.SetAsFirstSibling();
        }
        /// <summary>
        /// 设置到当前层级的最后一位
        /// </summary>
        public void SetAsLastSibling()
        {
            this.rectTransform.SetAsLastSibling();
        }

        /// <summary>
        /// 添加Widget
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddWidget<T>(UITransform parent = null, object data = null) where T : UIWidget, new()
        {
            return this.AddWidget<T>(parent?.transform, data);
        }
        /// <summary>
        /// 添加Widget
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddWidget<T>(Transform parent = null, object data = null) where T : UIWidget, new()
        {
            parent = parent ?? this.transform;
            T view = new T();
            view.data = data;
            view.parent = this;
            UIModuleUtils.CreateView(view, parent);
            _widgets.Add(view.GetHashCode(), view);
            return view;
        }
        /// <summary>
        /// 添加已经加载Widget到一个View中
        /// </summary>
        /// <param name="obj">当前Widget的GameObject</param>
        /// <returns></returns>
        public T AddWidget<T>(UIGameObject obj, object data = null) where T : UIWidget, new()
        {
            return this.AddWidget<T>(obj.gameObject, data);
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
            view.gameObject = obj;
            view.data = data;
            view.parent = this;
            view.Internal_Create();
            _widgets.Add(view.GetHashCode(), view);
            return view;
        }

        /// <summary>
        /// 隐藏一个视图
        /// </summary>
        public void Show(object data = null)
        {
            this.data = data ?? this.data;
            switch (this.state)
            {
                case UIState.Loaded:
                case UIState.Hide:
                    this.activeSelf = true;
                    Internal_Show();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 隐藏一个视图
        /// </summary>
        public void Hide()
        {
            switch (this.state)
            {
                case UIState.Loaded:
                case UIState.Show:
                    this.activeSelf = false;
                    Internal_Hide();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 关闭一个视图
        /// </summary>
        public void Close()
        {
            if (this.state == UIState.Show)
            {
                Hide();
            }
            Internal_Close();
        }

        /// <summary>
        /// 内部创建方法
        /// </summary>
        internal void Internal_Create()
        {
            Canvas canv = null;
            Transform tran = this.transform;
            do
            {
                canv = tran.GetComponent<Canvas>();
                if (canv != null)
                    break;
                tran = tran.parent;
            } while (tran != null);
            this.canvas = canv;
            InitializeElement();
            UIMono uiMono = this.gameObject.AddComponent<UIMono>();
            uiMono.Init(this);
            if (state == UIState.Close)
            {
                //todo 关闭界面
            }
            this.state = UIState.Loaded;

        }
        /// <summary>
        /// 内部显示方法
        /// </summary>
        internal void Internal_Show()
        {
            this.state = UIState.Show;
            foreach (var item in _widgets)
            {
                if (item.Value.activeSelf)
                {
                    item.Value.Internal_Show();
                }
            }
            OnEnable();
        }
        /// <summary>
        /// 内部隐藏方法
        /// </summary>
        internal void Internal_Hide()
        {
            this.state = UIState.Hide;
            foreach (var item in _widgets)
            {
                if (item.Value.activeSelf)
                {
                    item.Value.Internal_Hide();
                }
            }
            OnDisable();
        }
        /// <summary>
        /// 内部关闭方法
        /// </summary>
        internal void Internal_Close()
        {
            this.state = UIState.Close;
            foreach (var item in _coms)
            {
                item.Value.Release();
            }

            foreach (var item in _widgets)
            {
                if (item.Value.state == UIState.Close)
                {
                    continue;
                }
                item.Value.Internal_Close();
            }
            this.Release();
            GameObject.Destroy(this.gameObject);
        }

        internal override void Release()
        {
            foreach (var item in _resHandler)
                UIModuleUtils.resourceModule.UnloadAsset(item.Key);
            _resHandler.Clear();
            _coms.Clear();
            _widgets.Clear();
            base.Release();
        }
        /// <summary>
        /// 注册Component
        /// </summary>
        internal protected void RegisterComponent(UIComponent component)
        {
            int id = component.GetHashCode();
            if (!_coms.ContainsKey(id))
            {
                _coms.Add(id, component);
            }
        }

        /// <summary>
        /// 取消注册Component
        /// </summary>
        internal protected void UnRegisterComponent(UIComponent component)
        {
            int id = component.GetHashCode();
            if (_coms.ContainsKey(id))
            {
                _coms.Remove(id);
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
        /// 【异步】打开动画，不会阻挡正常生命周期
        /// </summary>
        /// <returns></returns>
        protected virtual async Promise OnOpenAnim() { await 0; }
        /// <summary>
        /// 【异步】关闭动画，不会阻挡正常生命周期
        /// </summary>
        /// <returns></returns>
        protected virtual async Promise OnCloseAnim() { await 0; }

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

    partial class UIView
    {
        private class UIMono : MonoBehaviour
        {
            private UIView _view;
            void Awake()
            {
                this.hideFlags = HideFlags.HideAndDontSave;
            }
            public void Init(UIView view)
            {
                //onCreate
                _view = view;
                view.OnCreate();
            }

            private void OnEnable()
            {
                //onEnable
            }

            private void OnDisable()
            {
                //onDisable
            }

            private void OnDestroy()
            {
                //onDestroy
            }

        }
    }
}
