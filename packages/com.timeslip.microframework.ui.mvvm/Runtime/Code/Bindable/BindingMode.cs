using System;

namespace MFramework.Runtime
{
    /// <summary>
    /// 绑定类型
    /// </summary>
    [Flags]
    public enum BindingMode
    {
        /// <summary>
        /// Model变更新UI
        /// </summary>
        OneWay = 1,
        /// <summary>
        /// Model更新UI
        /// 绑定时候更新一次
        /// </summary>
        [Obsolete("暂时用处不大,取消使用", true)]
        OneTime = 2,
        /// <summary>
        /// UI更新Model
        /// </summary>
        OneWayToSource = 4,
        /// <summary>
        /// UI和Model互相更新(慎用，容易变成递归操作)
        /// </summary>
        TwoWay = 8,
    }

    /// <summary>
    /// 绑定组件触发规则
    /// TwoWay和OneWayToSource时候生效
    /// </summary>
    public enum ComponentTriggerRule
    {
        /// <summary>
        /// 默认,组件变化实时变化数据
        /// </summary>
        Default,
        EndChanged
    }
}
