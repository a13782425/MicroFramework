using System;
using System.Runtime.CompilerServices;

namespace MFramework.Runtime
{
    public delegate void BindableDelegate(BindableEventArgs args);
    public delegate void BindableDelegate<T>(BindableEventArgs<T> args);

    internal static class BindableEventArgsPool<T> where T : BindableEventArgs, new()
    {
        private const int CAPACITY = 16;
        private static int _useIndex = -1;
        private static readonly T[] _argsList = new T[CAPACITY];
        static BindableEventArgsPool()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                _argsList[i] = new T();
            }
        }
        public static T GetEventArgs()
        {
            T args = null;
            _useIndex++;
            if (_useIndex == CAPACITY)
                throw new StackOverflowException("可绑定通知嵌套执行过审，目前仅支持嵌套16层");
            else
                args = _argsList[_useIndex];
            return args;
        }
        internal static void RetEventArgs(T args)
        {
            _argsList[_useIndex] = args;
            _useIndex--;
        }
    }
    public class BindableEventArgs : EventArgs, IDisposable
    {

        protected object sender;
        public object Sender => sender;

        protected object subscribeKey;

        /// <summary>
        /// 哪个属性变化了(不一定是字符串)
        /// </summary>
        public object SubscribeKey => subscribeKey;

        protected bool isBubble = true;

        /// <summary>
        /// 是否冒泡
        /// </summary>
        public bool IsBubble => isBubble;

        public BindableEventArgs() { }

        public BindableEventArgs(object sender, object keyName)
        {
            this.sender = sender;
            this.subscribeKey = keyName;
            this.isBubble = true;
        }

        public virtual object GetOldValue()
        {
            return default;
        }
        public virtual object GetNewValue()
        {
            return default;
        }
        /// <summary>
        /// 停止冒泡(默认冒泡)
        /// <para>在同一层级全部执行完毕才会确定是否冒泡</para>
        /// <para>所以请不要滥用该方法</para>
        /// </summary>
        public void StopPropagation()
        {
            this.isBubble = false;
        }
        internal static BindableEventArgs GetEventArgs(object sender, object keyName)
        {
            BindableEventArgs args = BindableEventArgsPool<BindableEventArgs>.GetEventArgs();
            args.sender = sender;
            args.subscribeKey = keyName;
            args.isBubble = true;
            return args;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose()
        {
            BindableEventArgsPool<BindableEventArgs>.RetEventArgs(this);
        }
    }

    public class BindableEventArgs<T> : BindableEventArgs
    {
        protected internal T oldValue = default;
        protected internal T newValue = default;
        public T OldValue => oldValue;
        public T NewValue => newValue;
        public BindableEventArgs() { }
        public BindableEventArgs(object sender, object propName, T old, T @new) : base(sender, propName)
        {
            this.oldValue = old;
            this.newValue = @new;
        }
        public override object GetOldValue()
        {
            return OldValue;
        }
        public override object GetNewValue()
        {
            return NewValue;
        }
        internal static BindableEventArgs<T> GetEventArgs(object sender, object keyName, T old, T @new)
        {
            BindableEventArgs<T> args = BindableEventArgsPool<BindableEventArgs<T>>.GetEventArgs();
            args.sender = sender;
            args.subscribeKey = keyName;
            args.isBubble = true;
            args.oldValue = old;
            args.newValue = @new;
            return args;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Dispose()
        {
            BindableEventArgsPool<BindableEventArgs<T>>.RetEventArgs(this);
        }
    }
    /// <summary>
    /// 容器数据驱动状态
    /// </summary>
    public enum ContainerBindableState
    {
        /// <summary>
        /// 仅通知
        /// </summary>
        Publish,
        /// <summary>
        /// 增加
        /// </summary>
        Add,
        /// <summary>
        /// 删除
        /// </summary>
        Remove,
        /// <summary>
        /// 删除范围
        /// </summary>
        RemoveRange,
        /// <summary>
        /// 替换
        /// </summary>
        Replace,
        /// <summary>
        /// 清空
        /// </summary>
        Clear,
        /// <summary>
        /// 添集合数据
        /// </summary>
        AddRange
    }
    public interface IContainerBindableEventArgs
    {
        ContainerBindableState state { get; }
    }
    public class ContainerBindableEventArgs : BindableEventArgs, IContainerBindableEventArgs
    {
        private ContainerBindableState _state = ContainerBindableState.Publish;

        public ContainerBindableState state => _state;
        public ContainerBindableEventArgs() { }
        public ContainerBindableEventArgs(object sender, object propName, ContainerBindableState state) : base(sender, propName)
        {
            this._state = state;
        }
        internal static ContainerBindableEventArgs GetEventArgs(object sender, object keyName, ContainerBindableState state)
        {
            ContainerBindableEventArgs args = BindableEventArgsPool<ContainerBindableEventArgs>.GetEventArgs();
            args.sender = sender;
            args.subscribeKey = keyName;
            args.isBubble = true;
            args._state = state;
            return args;
        }
        public override void Dispose()
        {
            BindableEventArgsPool<ContainerBindableEventArgs>.RetEventArgs(this);
        }
    }
    public class ContainerBindableEventArgs<T> : BindableEventArgs<T>, IContainerBindableEventArgs
    {
        private ContainerBindableState _state = ContainerBindableState.Publish;

        public ContainerBindableState state => _state;
        public ContainerBindableEventArgs() { }
        public ContainerBindableEventArgs(object sender, object propName, T old, T @new, ContainerBindableState state) : base(sender, propName, old, @new)
        {
            this._state = state;
        }
        internal static ContainerBindableEventArgs<T> GetEventArgs(object sender, object keyName, T old, T @new, ContainerBindableState state)
        {
            ContainerBindableEventArgs<T> args = BindableEventArgsPool<ContainerBindableEventArgs<T>>.GetEventArgs();
            args.sender = sender;
            args.subscribeKey = keyName;
            args.isBubble = true;
            args.oldValue = old;
            args.newValue = @new;
            args._state = state;
            return args;
        }
        public override void Dispose()
        {
            BindableEventArgsPool<ContainerBindableEventArgs<T>>.RetEventArgs(this);
        }
    }
}

