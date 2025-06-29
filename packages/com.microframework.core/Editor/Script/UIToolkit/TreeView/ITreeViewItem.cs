using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 树视图
    /// </summary>
    public interface ITreeViewItem
    {
        string name { get; set; }
        int index { get; set; }
        string tooltip { get; }
        object userData { get; set; }
        ITreeViewItem parent { get; set; }
        List<ITreeViewItem> children { get; }
        bool hasChildren { get; }
        void OnSelect();
        void OnClick();
        void AddChild(ITreeViewItem child);
        void RemoveChild(ITreeViewItem child);
    }
}
