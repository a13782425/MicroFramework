using MFramework.Task;
using MFramework.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using System;

namespace MFramework.UI
{
    /// <summary>
    /// UI界面 基类
    /// </summary>
    public partial class UIView : UIObject
    {

        [BindableField("可绑定属性")]
        private int _siblingIndex;
        partial void OnSiblingIndexPreGet() => _siblingIndex = this.rectTransform.GetSiblingIndex();
        partial void OnSiblingIndexPrePublish(int oldValue, int newValue) => this.rectTransform.SetSiblingIndex(newValue);

        private object _data;
        /// <summary>
        /// 界面数据,每次打开界面时候传递的
        /// </summary>
        public object data { get => _data; internal set => _data = value; }
        /// <summary>
        /// 当前界面所在的画布
        /// </summary>
        public Canvas canvas { get; private set; }
        /// <summary>
        /// 父级界面
        /// </summary>
        public UIView parent { get; private set; }
        /// <summary>
        /// 当前界面的子界面列表
        /// </summary>
        private readonly Dictionary<int, UIWidget> _widgets = new Dictionary<int, UIWidget>();
        /// <summary>
        /// 资源句柄
        /// </summary>
        private Dictionary<string, ResHandler> _resHandler = new Dictionary<string, ResHandler>();

    }//不可重写方法
    partial class UIView
    {

        /// <summary>
        /// 显示一个视图
        /// </summary>
        public void Show(object data = null)
        {
        }
        /// <summary>
        /// 隐藏一个视图
        /// </summary>
        public void Hide()
        {
        }
        /// <summary>
        /// 关闭一个视图
        /// </summary>
        public void Close()
        {
        }

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
        public T AddWidget<T>(Transform parent = null, object data = null) where T : UIWidget, new()
        {
            parent = parent ?? this.transform;
            T view = new T();
            view.data = data;
            view.parent = this;
            //UIModuleUtils.CreateView(view, parent);
            _widgets.Add(view.GetHashCode(), view);
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
            view.gameObject = obj;
            view.data = data;
            view.parent = this;
            //view.Internal_Create();
            _widgets.Add(view.GetHashCode(), view);
            return view;
        }
        /// <summary>
        /// 根据类型获取一个Widget
        /// <para>如果不存在则返回空</para>
        /// <para>如果存在多个则返回第一个</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWidget<T>() where T : UIView
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
            foreach (var widget in _widgets)
                if (widget.Value.GetType() == widgetType)
                    return widget.Value;
            return default(UIWidget);
        }
        /// <summary>
        /// 根据类型获取全部Widget
        /// <para>如果不存在则返回空</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetWidgets<T>() where T : UIView
        {
            List<T> widgets = new List<T>();
            foreach (var item in _widgets)
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
            foreach (var item in _widgets)
                if (item.Value.GetType() == widgetType)
                    widgets.Add(item.Value);
            return widgets;
        }


        /// <summary>
        /// 内部创建方法
        /// </summary>
        internal void Internal_Create()
        {
        }
        /// <summary>
        /// 内部显示方法
        /// </summary>
        internal void Internal_Show()
        {
        }
        /// <summary>
        /// 内部隐藏方法
        /// </summary>
        internal void Internal_Hide()
        {
        }
        /// <summary>
        /// 内部关闭方法
        /// </summary>
        internal void Internal_Close()
        {
        }
    }
    //资源类
    partial class UIView
    {
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
                    m_waitLoad(handler, callback, args);
                }
            }
            else
            {
                handler = UIModuleUtils.resourceModule.Load<T>(resPath, callback, args);
                _resHandler.Add(resPath, handler);
            }
            async void m_waitLoad(ResHandler handler, ResLoadDelegate<T> callback, object[] args)
            {
                await handler.ToMicroTask().ToDepend(this.gameObject);
                if (handler.isCancel)
                    return;
                callback.Invoke(handler.asset as T, args);
            }
        }
        /// <summary>
        /// 加载一个资源
        /// 会随着界面关闭而销毁
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
            void Awake()
            {
                this.hideFlags = HideFlags.HideAndDontSave;
            }
            public void Init(UIView view)
            {
                //onCreate
                _view = view;
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
