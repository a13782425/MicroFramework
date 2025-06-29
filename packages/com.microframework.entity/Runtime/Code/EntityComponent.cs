using MFramework.Core;
using System;

namespace MFramework.Runtime
{
    /// <summary>
    /// 组件状态
    /// </summary>
    public enum ComponentState
    {
        /// <summary>
        /// 空状态
        /// </summary>
        None,
        /// <summary>
        /// 初始化
        /// </summary>
        Initializing,
        /// <summary>
        /// 运行
        /// </summary>
        Running,
        /// <summary>
        /// 中止
        /// </summary>
        Suspended,
        /// <summary>
        /// 释放
        /// </summary>
        Destory,
    }

    /// <summary>
    /// 实体组件用到的参数
    /// <para>仅做调试作用</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EntityArgsAttribute : Attribute
    {
        public string[] args = { };

        public EntityArgsAttribute(params string[] args)
        {
            this.args = args;
        }
    }

    /// <summary>
    /// 实体组件基类
    /// </summary>
    public abstract class EntityComponent
    {
        /// <summary>
        /// 组件状态
        /// </summary>
        internal protected ComponentState state { get; internal set; }
        /// <summary>
        /// 实体
        /// </summary>
        internal protected Entity entity { get; internal set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void OnInit() { }
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        public virtual bool IsInit => true;
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(float deltaTime) { }
        /// <summary>
        /// 逻辑更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnLogicUpdate(float deltaTime) { }
        /// <summary>
        /// 暂停使用
        /// <para>在初始化完成后删除了依赖,会调用此方法</para>
        /// </summary>
        public virtual void OnSuspend() { }
        /// <summary>
        /// 恢复使用
        /// <para>在初始化完成后恢复其依赖关系,会调用此方法</para>
        /// </summary>
        public virtual void OnResume() { }
        /// <summary>
        /// 释放
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// 直接调用Entity上GetArgs的语法糖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetArgs<T>(string argName, T defaultValue = default)
        {
            return entity.GetArgs(argName, defaultValue);
        }
        /// <summary>
        /// 直接调用Entity上SetArgs的语法糖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argName"></param>
        /// <param name="value"></param>
        protected void SetArgs<T>(string argName, T value)
        {
            entity.SetArgs(argName, value);
        }
        /// <summary>
        /// 直接调用Entity上Subscribe的语法糖
        /// </summary>
        /// <param name="argName"></param>
        /// <param name="action"></param>
        /// <param name="firstNotify"></param>
        /// <param name="notifyAnyway"></param>
        protected void Subscribe(string argName, BindableDelegate<object> action, bool firstNotify = false, bool notifyAnyway = false)
        {
            entity.Subscribe(argName, action, firstNotify, notifyAnyway);
        }
        /// <summary>
        /// 直接调用Entity上Unsubscribe的语法糖
        /// </summary>
        /// <param name="argName"></param>
        /// <param name="action"></param>
        protected void Unsubscribe(string argName, BindableDelegate<object> action)
        {
            entity.Unsubscribe(argName, action);
        }
        /// <summary>
        /// 直接调用Entity上Publish的语法糖
        /// </summary>
        /// <param name="argName"></param>
        protected void Publish(string argName)
        {
            entity.Publish(argName);
        }
    }
}
