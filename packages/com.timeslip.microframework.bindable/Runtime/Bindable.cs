using System.Collections.Generic;

namespace MFramework.Runtime
{
    /// <summary>
    /// 绑定数据的接口
    /// <para>默认Key是字符串</para>
    /// </summary>
    public interface IBindable
    {
        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="parent"></param>
        void SetParent(IBindable parent)
        {
            if (parent == null)
                GetHandle()?.SetParent(null);
            else
                GetHandle()?.SetParent(parent.GetHandle());
        }
        /// <summary>
        /// 订阅类的消息
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="firstNotify"></param>
        /// <param name="notifyAnyway"></param>
        IBindableObserver Subscribe(BindableDelegate callback, bool firstNotify = false, bool notifyAnyway = false)
        {
            return GetHandle()?.Subscribe(callback, firstNotify, notifyAnyway);
        }
        /// <summary>
        /// 取消订阅类的消息
        /// </summary>
        /// <param name="callback"></param>
        void Unsubscribe(BindableDelegate callback)
        {
            GetHandle()?.Unsubscribe(callback);
        }
        /// <summary>
        /// 发布类的消息
        /// </summary>
        void Publish()
        {
            GetHandle()?.Publish();
        }
        /// <summary>
        /// 取消全部订阅
        /// </summary>
        void UnsubscribeAll()
        {
            foreach (var handle in GetHandles())
                handle.Clear();
        }
        /// <summary>
        /// 发布除类之外的全部注册
        /// </summary>
        void PublishAll()
        {
            foreach (var handle in GetHandles())
            {
                if (handle.SubscribeKey.ToString() == BindableUtils.BINDABLE_ALL_KEY)
                    continue;
                handle.Publish();
            }
        }
        /// <summary>
        /// 设置当前类是否可以被通知
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="isPublish"></param>
        void SetNotify(bool notify, bool isPublish = false)
        {
            foreach (var handle in GetHandles())
            {
                if (handle.SubscribeKey.ToString() == BindableUtils.BINDABLE_ALL_KEY)
                    continue;
                handle.SetNotify(notify, isPublish);
            }
        }
        /// <summary>
        /// 获取Handle
        /// </summary>
        /// <param name="memberName"></param>
        BindableHandle GetHandle(object memberName = null) => null;
        /// <summary>
        /// 获取所有Handle
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        List<BindableHandle> GetHandles() => null;
    }
}
