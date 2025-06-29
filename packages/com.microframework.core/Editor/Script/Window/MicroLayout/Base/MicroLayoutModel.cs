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

        private string _title;

        public string Title
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SubTitle))
                {
                    return _title;
                }
                return $"{_title}<size=10><color=#999999><i>{SubTitle}</i></color></size>";
            }
            private set
            {
                _title = value;
            }
        }

        /// <summary>
        /// 子标题
        /// 如果标题相同,则使用程序集名作为子标题
        /// </summary>
        public string SubTitle { get; private set; }

        public int Priority { get; private set; }

        //public bool IsDefault { get; private set; }

        /// <summary>
        /// 标题层级
        /// </summary>
        public string[] TitleLayers { get; private set; }

        /// <summary>
        /// 原始的标题层级
        /// </summary>
        private string[] originTitleLayers;

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
            //<size=10><color=#999999><i>dada</i></color></size>
            this.MicroLayout = microLayout;
            this.TitleLayers = this.MicroLayout.Title.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            this.originTitleLayers = this.TitleLayers.ToArray();
            this.Title = this.TitleLayers[this.TitleLayers.Length - 1];
            Priority = this.MicroLayout.Priority;
        }

        /// <summary>
        /// 检查是否重复
        /// </summary>
        /// <param name="models"></param>
        internal void CheckDuplicate(in List<MicroLayoutModel> models)
        {
            bool hasDuplicate = false;
            foreach (var item in models)
            {
                if (this.CustomEqual(item))
                {
                    hasDuplicate = true;
                    item.SubTitle = item.MicroLayout.GetType().FullName;
                    item.TitleLayers[item.TitleLayers.Length - 1] = item.Title;
                    break;
                }
            }
            if (hasDuplicate)
            {
                this.SubTitle = this.MicroLayout.GetType().FullName;
                this.TitleLayers[this.TitleLayers.Length - 1] = this.Title;
            }
        }

        private bool CustomEqual(MicroLayoutModel model)
        {
            if (this.originTitleLayers.Length != model.originTitleLayers.Length)
            {
                return false;
            }
            bool isEqual = true;
            for (int i = 0; i < this.originTitleLayers.Length; i++)
            {
                if (this.originTitleLayers[i] != model.originTitleLayers[i])
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }
    }
}
