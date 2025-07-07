using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.UI
{
    /// <summary>
    /// UI层级
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 初始层
        /// </summary>
        None = 0,
        /// <summary>
        /// 底部层级
        /// </summary>
        Bottom = 1 << 0,
        /// <summary>
        /// 固定层级
        /// </summary>
        Fixed = 1 << 2,
        /// <summary>
        /// 普通层级
        /// </summary>
        Normal = 1 << 4,
        /// <summary>
        /// 提示层级
        /// </summary>
        Tooltip = 1 << 6,
        /// <summary>
        /// 弹窗层级
        /// </summary>
        Dialog = 1 << 8,
        /// <summary>
        /// 公告层级
        /// </summary>
        Notice = 1 << 10,
        /// <summary>
        /// (最高层级)
        /// </summary>
        Max = 1 << 12,
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
        Load = 1 << 0,
        /// <summary>
        /// 加载完毕
        /// </summary>
        Loaded = 1 << 1,
        /// <summary>
        /// 处于显示状态
        /// </summary>
        Showed = 1 << 3,
        /// <summary>
        /// 处于隐藏状态(预先隐藏, 不会走OnEnable, 但会走OnDisable)
        /// </summary>
        Hidden = 1 << 5,
        /// <summary>
        /// 处于关闭状态
        /// </summary>
        Closed = 1 << 7,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 1 << 10,
    }
}
