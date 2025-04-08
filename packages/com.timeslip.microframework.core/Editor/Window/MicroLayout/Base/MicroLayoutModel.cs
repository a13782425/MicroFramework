using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 微架构布局模型
    /// </summary>
    internal class MicroLayoutModel
    {
        public BaseMicroLayout MicroLayout { get; private set; }

        public string Title { get; private set; }

        public int Priority { get; private set; }

        public bool IsDefault { get; private set; }

        public string[] TitleLayers { get; private set; }

        public List<MicroLayoutModel> Children { get; private set; }

        public MicroLayoutModel() => Children = new List<MicroLayoutModel>();
        public MicroLayoutModel Parent { get; private set; }

        public MicroLayoutModel(BaseMicroLayout microLayout) : this()
        {
            SetMicroLayout(microLayout);
        }

        /// <summary>
        /// 设置布局 
        /// </summary>
        public void SetMicroLayout(BaseMicroLayout microLayout)
        {

            this.MicroLayout = microLayout;
            this.TitleLayers = this.MicroLayout.Title.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            this.Title = this.TitleLayers[this.TitleLayers.Length - 1];
            Priority = this.MicroLayout.Priority;
        }
    }
}
