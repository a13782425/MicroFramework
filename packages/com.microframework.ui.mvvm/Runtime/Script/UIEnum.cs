using System;

namespace MFramework.Runtime
{
    /// <summary>
    /// UI层级
    /// </summary>
    [Flags]
    public enum UILayer
    {
        /// <summary>
        /// 初始层
        /// </summary>
        None = 1,
        /// <summary>
        /// 底部层级
        /// </summary>
        Bottom,
        /// <summary>
        /// 固定层级
        /// </summary>
        Fixed,
        /// <summary>
        /// 普通层级
        /// </summary>
        Normal,
        /// <summary>
        /// 提示层级
        /// </summary>
        Tooltip,
        /// <summary>
        /// 弹窗层级
        /// </summary>
        Dialog,
        /// <summary>
        /// 公告层级
        /// </summary>
        Notice,
        /// <summary>
        /// (最高层级)
        /// </summary>
        Max,
    }

    /// <summary>
    /// UI状态枚举
    /// </summary>
    [Flags]
    public enum UIState
    {
        None = 0,
        /// <summary>
        /// 加载中
        /// </summary>
        Load = 1,
        /// <summary>
        /// 加载完毕
        /// </summary>
        Loaded = 2,
        /// <summary>
        /// 处于显示状态
        /// </summary>
        Show = 4,
        /// <summary>
        /// 处于隐藏状态(预先隐藏, 不会走OnEnable, 但会走OnDisable)
        /// </summary>
        Hide = 8,
        /// <summary>
        /// 处于关闭状态(需要销毁)
        /// </summary>
        Close = 16,
        /// <summary>
        /// 处于错误状态
        /// </summary>
        Error = 32,
    }
}
