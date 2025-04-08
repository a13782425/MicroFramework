using System;
using System.Linq.Expressions;

namespace MFramework.Core
{
    public static class BindableExtensions
    {
        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="son"></param>
        /// <param name="parent"></param>
        public static void SetParent(this IBindable son, IBindable parent)
        {
            son.SetParent(parent);
        }
        /// <summary>
        /// 订阅整个类
        /// </summary>
        /// <param name="callback">值发生变化时候的回调</param>
        /// <param name="firstNotify">第一次是否通知，默认通知</param>
        public static void Subscribe(this IBindable ibindable, BindableDelegate callback, bool firstNotify = false, bool notifyAnyway = false)
        {
            ibindable.Subscribe(callback, firstNotify, notifyAnyway);
        }
        /// <summary>
        /// 订阅某个字段
        /// </summary>
        /// <typeparam name="TBind"></typeparam>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="ibindable"></param>
        /// <param name="expression">字段的lambda表达式</param>
        /// <param name="callback">值发生变化时候的回调</param>
        /// <param name="firstNotify">第一次是否通知，默认通知</param>
        /// <param name="notifyAnyway">是否永远通知，false：值发生变化时才会通知，true：赋值即通知</param>
        public static IBindableObserver Subscribe<TBind, TResult>(this TBind ibindable, Expression<Func<TBind, TResult>> expression, BindableDelegate<TResult> callback, bool firstNotify = false, bool notifyAnyway = false) where TBind : IBindable
        {
            string subscribeKey = BindableUtils.ParseMemberName(expression);
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                return handle.Subscribe(callback, firstNotify, notifyAnyway);
            return null;
        }
        /// <summary>
        /// 订阅某个字段
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="ibindable"></param>
        /// <param name="subscribeKey">属性名</param>
        /// <param name="callback">值发生变化时候的回调</param>
        /// <param name="firstNotify">第一次是否通知，默认通知</param>
        /// <param name="notifyAnyway">是否永远通知，false：值发生变化时才会通知，true：赋值即通知</param>
        public static IBindableObserver Subscribe<TKey, TResult>(this IBindable ibindable, TKey subscribeKey, BindableDelegate<TResult> callback, bool firstNotify = false, bool notifyAnyway = false)
        {
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                return handle.Subscribe(callback, firstNotify, notifyAnyway);
            return null;
        }
        /// <summary>
        /// 取消订阅整个类
        /// </summary>
        /// <param name="ibindable"></param>
        /// <param name="callback">值发生变化时候的回调</param>
        public static void Unsubscribe(this IBindable ibindable, BindableDelegate callback)
        {
            ibindable.Unsubscribe(callback);
        }
        /// <summary>
        /// 取消订阅某个字段
        /// </summary>
        /// <typeparam name="TBind"></typeparam>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="ibindable"></param>
        /// <param name="expression">字段的lambda表达式</param>
        /// <param name="callback">值发生变化时候的回调</param>
        public static void Unsubscribe<TBind, TResult>(this TBind ibindable, Expression<Func<TBind, TResult>> expression, BindableDelegate<TResult> callback) where TBind : IBindable
        {
            string subscribeKey = BindableUtils.ParseMemberName(expression);
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                handle.Unsubscribe(callback);
        }
        /// <summary>
        /// 取消订阅某个字段
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="ibindable">绑定类实例</param>
        /// <param name="subscribeKey">订阅Key</param>
        /// <param name="callback">值发生变化时候的回调</param>
        public static void Unsubscribe<TKey, TResult>(this IBindable ibindable, TKey subscribeKey, BindableDelegate<TResult> callback)
        {
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                handle.Unsubscribe(callback);
        }
        /// <summary>
        /// 取消所有订阅
        /// </summary>
        /// <param name="ibindable"></param>
        public static void UnsubscribeAll(this IBindable ibindable)
        {
            ibindable.UnsubscribeAll();
        }
        /// <summary>
        /// 发布类的注册
        /// </summary>
        /// <param name="ibindable"></param>
        public static void Publish(this IBindable ibindable)
        {
            ibindable.Publish();
        }
        /// <summary>
        /// 发布对应成员的注册
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="ibindable"></param>
        /// <param name="subscribeKey"></param>
        public static void Publish<TKey>(this IBindable ibindable, TKey subscribeKey)
        {
            ibindable.GetHandle(subscribeKey)?.Publish();
        }
        /// <summary>
        /// 发布对应成员的注册
        /// </summary>
        /// <typeparam name="TBind"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ibindable"></param>
        /// <param name="expression"></param>
        public static void Publish<TBind, TResult>(this TBind ibindable, Expression<Func<TBind, TResult>> expression) where TBind : IBindable
        {
            string subscribeKey = BindableUtils.ParseMemberName(expression);
            ibindable.GetHandle(subscribeKey)?.Publish();
        }
        /// <summary>
        /// 发布对应成员的注册
        /// </summary>
        /// <typeparam name="TBind"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ibindable"></param>
        /// <param name="expression"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public static void Publish<TBind, TResult>(this TBind ibindable, Expression<Func<TBind, TResult>> expression, TResult oldValue, TResult newValue) where TBind : IBindable
        {
            string subscribeKey = BindableUtils.ParseMemberName(expression);
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                handle.Publish(oldValue, newValue);
        }
        /// <summary>
        /// 发布对应成员的注册
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ibindable"></param>
        /// <param name="subscribeKey"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public static void Publish<TKey, TResult>(this IBindable ibindable, TKey subscribeKey, TResult oldValue, TResult newValue)
        {
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                handle.Publish(oldValue, newValue);
        }
        /// <summary>
        /// 发布全部注册
        /// </summary>
        /// <param name="ibindable"></param>
        public static void PublishAll(this IBindable ibindable)
        {
            ibindable.PublishAll();
        }
        /// <summary>
        /// 设置全部注册的通知
        /// </summary>
        /// <param name="ibindable"></param>
        /// <param name="notify">是否通知</param>
        /// <param name="isPublish">当绑定类型为不通知,且isNotify为True时,该参数控制是否发布在不通知期间的修改(没有oldValue)</param>
        public static void SetNotify(this IBindable ibindable, bool notify, bool isPublish = false)
        {
            ibindable.SetNotify(notify, isPublish);
        }
        /// <summary>
        /// 针对对应成员设置是否通知
        /// </summary>
        /// <typeparam name="TBind"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ibindable"></param>
        /// <param name="expression"></param>
        /// <param name="notify"></param>
        /// <param name="isPublish"></param>
        public static void SetNotify<TBind, TResult>(this TBind ibindable, Expression<Func<TBind, TResult>> expression, bool notify, bool isPublish = false) where TBind : IBindable
        {
            string subscribeKey = BindableUtils.ParseMemberName(expression);
            ibindable.GetHandle(subscribeKey)?.SetNotify(notify, isPublish);
        }

        /// <summary>
        /// 针对对应成员设置是否通知
        /// </summary>
        /// <typeparam name="TDD"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ibindable"></param>
        /// <param name="subscribeKey"></param>
        /// <param name="notify"></param>
        /// <param name="isPublish"></param>
        public static void SetNotify<TKey>(this IBindable ibindable, TKey subscribeKey, bool notify, bool isPublish = false)
        {
            ibindable.GetHandle(subscribeKey)?.SetNotify(notify, isPublish);
        }
    }
}
