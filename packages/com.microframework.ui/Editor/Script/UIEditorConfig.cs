using MFramework.Core;
using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.UI.Editor
{
    /// <summary>
    /// UI编辑器参数
    /// </summary>
    [Serializable]
    [DisplayName("界面编辑器配置")]
    public class UIEditorConfig : IMicroEditorConfig
    {
        /// <summary>
        /// 需要到处的预制体名
        /// </summary>
        public const string TAG_NAME = "Export";
        /// <summary>
        /// widget开头
        /// </summary>
        public const string WIDGET_HEAD = "widget_";

        /// <summary>
        /// 生成的命名空间
        /// </summary>
        [SerializeField]
        [DisplayName("脚本命名空间")]
        internal string RootNamespace = "";
        /// <summary>
        /// 使用使用文件夹名字来完善命名空间
        /// </summary>
        [SerializeField]
        [DisplayName("是否使用文件夹名字来完善命名空间")]
        internal bool UseFolderNameSpace = false;
        /// <summary>
        /// 预制体目录
        /// </summary>
        [SerializeField]
        [DisplayName("预制体目录")]
        internal string PrefabRootPath = "Assets/Resources/Prefab/UI";
        /// <summary>
        /// 脚本路径
        /// </summary>
        [SerializeField]
        [DisplayName("代码目录")]
        internal string CodeRootPath = "Assets/Script/UI";
        /// <summary>
        /// 界面生成脚本文件
        /// </summary>
        [SerializeField]
        [DisplayName("生成代码目录")]
        internal string CodeGenRootPath = "Assets/Script/Gen/UI";

        [SerializeReference]
        [DisplayName("导出组件配置")]
        internal List<UIExportConfig> Exports = new List<UIExportConfig>()
        {
            new UIExportConfig() { UIPrefix = "go_", UIType = new MicroClassSerializer (){ CurrentType = typeof(GameObject) }},
            new UIExportConfig() { UIPrefix = "can_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Canvas) }},
            new UIExportConfig() { UIPrefix = "tran_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Transform) }},
            new UIExportConfig() { UIPrefix = "rtran_", UIType = new MicroClassSerializer(){ CurrentType = typeof(RectTransform) }},
            new UIExportConfig() { UIPrefix = "btn_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Button) }},
            new UIExportConfig() { UIPrefix = "img_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Image) }},
            new UIExportConfig() { UIPrefix = "txt_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Text) }},
            new UIExportConfig() { UIPrefix = "inp_", UIType = new MicroClassSerializer(){ CurrentType = typeof(InputField) }},
            new UIExportConfig() { UIPrefix = "srect_", UIType = new MicroClassSerializer(){ CurrentType = typeof(ScrollRect) }},
            new UIExportConfig() { UIPrefix = "sbar_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Scrollbar) }},
            new UIExportConfig() { UIPrefix = "tog_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Toggle) }},
            new UIExportConfig() { UIPrefix = "sli_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Slider) }},
            new UIExportConfig() { UIPrefix = "drop_", UIType = new MicroClassSerializer(){ CurrentType = typeof(Dropdown) }},
        };

        public void Save()
        {
            ((IMicroEditorConfig)this).Save();
        }
    }

    [Serializable]
    internal class UIExportConfig
    {
        /// <summary>
        /// 组件类型名简称
        /// </summary>
        public string UIPrefix;
        /// <summary>
        /// 组件类型全名
        /// </summary>
        public MicroClassSerializer UIType = new MicroClassSerializer();
    }
}
