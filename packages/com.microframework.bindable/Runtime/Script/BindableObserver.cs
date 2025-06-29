namespace MFramework.Runtime
{
    /// <summary>
    /// 节流方案
    /// </summary>
    public enum ThrottleType
    {
        /// <summary>
        /// 不使用节流(实时刷新)
        /// </summary>
        None = 0,
        /// <summary>
        /// 按帧刷新
        /// </summary>
        Frame,
        /// <summary>
        /// 按毫秒刷新
        /// </summary>
        Millisecond,
    }

    /// <summary>
    /// 可绑定字段观察者接口
    /// </summary>
    public interface IBindableObserver
    {
        /// <summary>
        /// 节流方案
        /// 当数据发生变化时，按照此方法设置的节流方式进行通知
        /// </summary>
        void Throttle(ThrottleType type, uint count = 0);
        /// <summary>
        /// 执行变化通知
        /// </summary>
        /// <param name="args"></param>
        void Execute(object args);
        /// <summary>
        /// 取消当前通知
        /// </summary>
        void Cancel();
    }
}
