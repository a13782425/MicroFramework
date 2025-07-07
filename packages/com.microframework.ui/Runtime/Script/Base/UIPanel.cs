using UnityEngine;

namespace MFramework.UI
{
    /// <summary>
    /// 界面基类
    /// </summary>
    public partial class UIPanel : UIView
    {
        /// <summary>
        /// UI层级
        /// </summary>
        public virtual UILayer LayerEnum => UILayer.Normal;
        /// <summary>
        /// 是否是堆栈UI，默认UILayer.Normal都为堆栈UI
        /// </summary>
        public virtual bool IsStackUI => LayerEnum == UILayer.Normal;

        /// <summary>
        /// 压入界面堆栈(不会执行OnDestory，但会执行OnDisable)
        /// </summary>
        /// <returns>快照数据</returns>
        public virtual object PushStack() { return null; }
        /// <summary>
        /// 将界面弹出堆栈(不会执行OnCreate，但会执行OnEnable)
        /// </summary>
        public virtual void PopStack(object snapshootData) { }
    }
}
