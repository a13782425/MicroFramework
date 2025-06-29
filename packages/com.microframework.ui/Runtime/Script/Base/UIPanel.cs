using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
