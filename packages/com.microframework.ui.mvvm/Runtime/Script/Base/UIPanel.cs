using MFramework.Core;

namespace MFramework.Runtime
{
    /// <summary>
    /// 界面不能同时存在多个实例
    /// </summary>
    [Ignore]
    public class UIPanel : UIView
    {
        /// <summary>
        /// UI层级
        /// </summary>
        public virtual UILayer LayerEnum => UILayer.Normal;
    }
}
