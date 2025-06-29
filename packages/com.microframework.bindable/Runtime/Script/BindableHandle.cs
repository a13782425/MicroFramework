using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 数据驱动的类
    /// </summary>
    public class BindableHandle
    {
        protected class BindableEvent : IBindableObserver
        {
            /// <summary>
            /// 总是通知
            /// </summary>
            public bool notifyAnyway = false;
            public object callback;
#if UNITY_EDITOR
            public string debugName = "";
#endif
            public virtual void Execute(object args)
            {
                ((Delegate)callback)?.DynamicInvoke(args);
            }
            public virtual void Cancel()
            {

            }
            public virtual void Throttle(ThrottleType type, uint count)
            {

            }
        }

        /// <summary>
        /// 父节点
        /// </summary>
        protected BindableHandle parent = null;
        /// <summary>
        /// 对象
        /// </summary>
        protected object sender = default;
        /// <summary>
        /// 对象
        /// </summary>
        public object Sender => sender;
        /// <summary>
        /// 数据驱动的Key
        /// </summary>
        protected object subscribeKey = default;
        /// <summary>
        /// 数据驱动的Key
        /// </summary>
        public object SubscribeKey => subscribeKey;
        /// <summary>
        /// 是否可以通知
        /// </summary>
        protected bool notify = true;
        /// <summary>
        /// 通知进行中
        /// </summary>
        protected bool notifyInProgress = false;
        /// <summary>
        /// 是否存在 新委托
        /// </summary>
        protected bool hasNewDelegate = false;
        /// <summary>
        /// 当前有的委托数量
        /// </summary>
        protected int eventCount = 0;
        protected Dictionary<object, IBindableObserver> callbackDic = new Dictionary<object, IBindableObserver>();
        protected HashSet<IBindableObserver> eventSet = new HashSet<IBindableObserver>();
        protected IBindableObserver[] eventArray = new IBindableObserver[16];
        public virtual void Init(object sender, object subscribeKey, BindableHandle parent)
        {
            this.sender = sender;
            this.subscribeKey = subscribeKey;
            this.parent = parent;
        }
        public void SetParent(BindableHandle parent) => this.parent = parent;
        /// <summary>
        /// 订阅
        /// </summary>
        public IBindableObserver Subscribe(BindableDelegate action, bool firstNotify = false, bool notifyAnyway = false)
        {
            if (action == null)
                return null;
            IBindableObserver observer = internalSubscribe(action, notifyAnyway);
            if (firstNotify)
            {
                try
                {
                    using BindableEventArgs args = BindableEventArgs.GetEventArgs(sender, subscribeKey);
                    action.Invoke(args);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    if (action is Delegate @delegate)
                    {
                        string debugName = (@delegate.Target != null ? @delegate.Target.ToString() : "") + "." + @delegate.Method.Name;
                        MicroLogger.LogError($"{debugName} 发生异常，异常信息：{ex.Message}");
                    }
#else
                    MicroLogger.LogError(ex.ToString());
#endif
                }
            }
            return observer;
        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe(BindableDelegate action)
        {
            if (action == null)
                return;
            internalUnsubscribe(action);
        }

        /// <summary>
        /// 发布
        /// </summary>
        public virtual void Publish()
        {
            if (!notify || notifyInProgress)
                return;
            using BindableEventArgs args = BindableEventArgs.GetEventArgs(sender, subscribeKey);
            m_publish(args, true);
        }
        public void Publish(BindableEventArgs args, bool isChanged)
        {
            if (!notify || notifyInProgress)
                return;
            m_publish(args, isChanged);
        }
        public virtual void SetNotify(bool notify, bool isPublish = false)
        {
            if (this.notify == notify)
                return;
            this.notify = notify;
            if (notify && isPublish)
                Publish();
        }
        /// <summary>
        /// 获取是否可以通知
        /// </summary>
        /// <returns></returns>
        public virtual bool GetNotify()
        {
            return notify;
        }
        public virtual void Clear()
        {
            eventSet.Clear();
            callbackDic.Clear();
            hasNewDelegate = true;
        }
        private void m_publish(BindableEventArgs args, bool isChanged)
        {
            notifyInProgress = true;
            if (hasNewDelegate)
                swapAction();
            for (int i = 0; i < eventCount; i++)
            {
                BindableEvent item = (BindableEvent)eventArray[i];
                try
                {
                    if (item.notifyAnyway || isChanged)
                        item.Execute(args);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    MicroLogger.LogError($"{item.debugName} 发生异常，异常信息：{ex.Message}");
#else
                    MicroLogger.LogError(ex.Message);
#endif
                }
            }
            if (args.IsBubble && parent != null)
                parent.Publish(args, isChanged);
            notifyInProgress = false;
        }
        protected virtual BindableEvent GetBindableEvent()
        {
            return new BindableEvent();
        }
        protected IBindableObserver internalSubscribe(object action, bool notifyAnyway)
        {
            if (callbackDic.ContainsKey(action))
            {
                MicroLogger.LogWarning("重复订阅");
                return null;
            }
            BindableEvent custom = GetBindableEvent();
            custom.callback = action;
            custom.notifyAnyway = notifyAnyway;
#if UNITY_EDITOR
            if (action is Delegate @delegate)
            {
                custom.debugName = (@delegate.Target != null ? @delegate.Target.ToString() : "") + "." + @delegate.Method.Name;
            }
#endif
            callbackDic.Add(action, custom);
            eventSet.Add(custom);
            hasNewDelegate = true;
            return custom;
        }
        protected void internalUnsubscribe(object action)
        {
            if (!callbackDic.TryGetValue(action, out IBindableObserver bindableEvent))
                return;
            callbackDic.Remove(action);
            eventSet.Remove(bindableEvent);
            hasNewDelegate = true;
        }
        protected void swapAction()
        {
            int allCount = eventSet.Count;
            if (allCount > eventArray.Length)
                Array.Resize(ref eventArray, eventArray.Length * 2);
            eventSet.CopyTo(eventArray);
            eventCount = allCount;
            hasNewDelegate = false;
        }
    }
    public class BindableHandle<T> : BindableHandle
    {
        private Func<T> _getFunc = default;
        private Action<T> _setFunc = default;
        private Func<T, T, bool> _equalFunc = default;
        public override void Init(object sender, object key, BindableHandle parent)
        {
            base.Init(sender, key, parent);
            m_genGetValueFunc();
            m_genSetValueFunc();
            _equalFunc = BindableUtils.GetEqualValue<T>();
        }
        public void Init(object sender, object key, BindableHandle parent, Func<T> getFunc, Action<T> setFunc)
        {
            base.Init(sender, key, parent);
            _getFunc = getFunc;
            _setFunc = setFunc;
            _equalFunc = BindableUtils.GetEqualValue<T>();
        }

        public T GetValue()
        {
            T t = default;
            if (_getFunc != null)
            {
                t = _getFunc();
            }
            return t;
        }
        public void SetValue(T value)
        {
            _setFunc?.Invoke(value);
        }
        public IBindableObserver Subscribe(BindableDelegate<T> action, bool firstNotify = false, bool notifyAnyway = false)
        {
            if (action == null)
                return null;
            IBindableObserver custom = internalSubscribe(action, notifyAnyway);
            if (firstNotify)
            {
                try
                {
                    T value = _getFunc();
                    using BindableEventArgs<T> args = BindableEventArgs<T>.GetEventArgs(sender, subscribeKey, value, value);
                    action.Invoke(args);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    if (action is Delegate @delegate)
                    {
                        string debugName = (@delegate.Target != null ? @delegate.Target.ToString() : "") + "." + @delegate.Method.Name;
                        MicroLogger.LogError($"{debugName} 发生异常，异常信息：{ex.Message}");
                    }
#else
                    MicroLogger.LogError(ex.ToString());
#endif
                }
            }
            return custom;
        }
        public void Unsubscribe(BindableDelegate<T> action)
        {
            if (action == null)
                return;
            internalUnsubscribe(action);
        }
        public override void Publish()
        {
            if (!notify || notifyInProgress)
                return;
            T value = _getFunc();
            using BindableEventArgs<T> args = BindableEventArgs<T>.GetEventArgs(sender, subscribeKey, value, value);
            m_publish(args, true);
        }
        public void Publish(T oldValue, T newValue)
        {
            if (!notify || notifyInProgress)
                return;
            using BindableEventArgs<T> args = BindableEventArgs<T>.GetEventArgs(sender, subscribeKey, oldValue, newValue);
            m_publish(args, true);
        }
        public override void SetNotify(bool notify, bool isPublish = false)
        {
            if (this.notify == notify)
            {
                return;
            }
            this.notify = notify;
            if (notify && isPublish)
            {
                Publish();
            }
        }
        public override void Clear()
        {
            callbackDic.Clear();
            base.Clear();
        }

        private void m_publish(BindableEventArgs args, bool isChanged)
        {
            notifyInProgress = true;
            if (hasNewDelegate)
                swapAction();
            for (int i = 0; i < eventCount; i++)
            {
                BindableEvent item = (BindableEvent)eventArray[i];
                try
                {
                    if (item.notifyAnyway || isChanged)
                    {
                        item.Execute(args);
                    }
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    MicroLogger.LogError($"{item.debugName} 发生异常，异常信息：{ex.Message}");
#else
                    MicroLogger.LogError(ex.Message);
#endif
                }
            }
            if (args.IsBubble && parent != null)
                parent.Publish(args, isChanged);
            notifyInProgress = false;
        }

        /// <summary>
        /// 生成获取函数
        /// </summary>
        private void m_genGetValueFunc()
        {
            Type dataType = sender.GetType();
            PropertyInfo property = dataType.GetProperty(subscribeKey.ToString(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var senderValue = Expression.Constant(sender, sender.GetType());
            if (property == null)
            {
                if (subscribeKey.ToString() == BindableUtils.BINDABLE_ALL_KEY)
                {
                    if (dataType.IsClass)
                    {
                        _getFunc = Expression.Lambda<Func<T>>(Expression.TypeAs(senderValue, dataType)).Compile();
                    }
                    else
                    {
                        _getFunc = Expression.Lambda<Func<T>>(Expression.Convert(senderValue, dataType)).Compile();
                    }
                }
                else
                {
                    _getFunc = Expression.Lambda<Func<T>>(Expression.Default(typeof(T))).Compile();
                }
            }
            else
            {
                if (dataType.IsClass)
                {
                    _getFunc = Expression.Lambda<Func<T>>(Expression.Property(Expression.TypeAs(senderValue, dataType), property)).Compile();
                }
                else
                {
                    _getFunc = Expression.Lambda<Func<T>>(Expression.Property(Expression.Convert(senderValue, dataType), property)).Compile();
                }
            }
        }

        private void m_genSetValueFunc()
        {
            Type dataType = sender.GetType();
            PropertyInfo property = dataType.GetProperty(subscribeKey.ToString(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ParameterExpression paramExpre = Expression.Parameter(typeof(T), "v");
            var senderValue = Expression.Constant(sender, sender.GetType());
            if (property == null)
            {
                _setFunc = Expression.Lambda<Action<T>>(Expression.Block(), paramExpre).Compile();
            }
            else
            {
                if (dataType.IsClass)
                {
                    _setFunc = Expression.Lambda<Action<T>>(Expression.Assign(Expression.Property(Expression.TypeAs(senderValue, dataType), property), paramExpre), paramExpre).Compile();
                }
                else
                {
                    _setFunc = Expression.Lambda<Action<T>>(Expression.Assign(Expression.Property(Expression.Convert(senderValue, dataType), property), paramExpre), paramExpre).Compile();
                }
            }
        }
        protected override BindableEvent GetBindableEvent() => new ThrottledBindableEvent(this);

        private class ThrottledBindableEvent : BindableEvent
        {
            private T oldValue;
            private T newValue;
            private bool isWait = false;
            private ThrottleType _throttleType = ThrottleType.None;
            private uint _throttleCount = 0;
            private bool isCancel = false;
            private BindableHandle<T> handle;

            public ThrottledBindableEvent(BindableHandle<T> handle) => this.handle = handle;

            public override void Execute(object args)
            {
                BindableEventArgs<T> temp = (BindableEventArgs<T>)args;
                switch (_throttleType)
                {
                    case ThrottleType.None:
                        ((BindableDelegate<T>)callback)?.Invoke(temp);
                        break;
                    default:
                        if (isWait)
                        {
                            newValue = temp.newValue;
                            return;
                        }
                        isWait = true;
                        oldValue = temp.oldValue;
                        newValue = temp.newValue;
                        m_throttle();
                        break;
                }
            }

            public override void Cancel()
            {
                base.Cancel();
                isCancel = true;
            }
            public override void Throttle(ThrottleType type, uint count)
            {
                _throttleType = type;
                _throttleCount = count;
            }
            private async void m_throttle()
            {
                await System.Threading.Tasks.Task.Delay(1999);
                //switch (_throttleType)
                //{
                //    case ThrottleType.Frame:
                //        await PromiseYielder.WaitForFrames(_throttleCount);
                //        break;
                //    case ThrottleType.Millisecond:
                //        await PromiseYielder.WaitForTime(TimeSpan.FromMilliseconds(_throttleCount));
                //        break;
                //}
                //if (handle.notifyInProgress)
                //    await PromiseYielder.WaitUntil(() => !handle.notifyInProgress);
                if (isCancel || !Application.isPlaying)
                    return;
                using BindableEventArgs<T> args = BindableEventArgs<T>.GetEventArgs(handle.sender, handle.subscribeKey, oldValue, newValue);
                handle.m_publish(args, true);
                isCancel = false;
                isWait = false;
            }
        }
    }
}
