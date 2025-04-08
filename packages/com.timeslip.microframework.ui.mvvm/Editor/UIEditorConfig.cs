using MFramework.Core;
using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Editor
{
    /// <summary>
    /// UI编辑器参数
    /// </summary>
    [Serializable]
    [DisplayName("界面编辑器配置")]
    public class UIEditorConfig : ICustomMicroEditorConfig
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
        internal string Namespace = "";
        /// <summary>
        /// Panel预制体目录
        /// </summary>
        [SerializeField]
        [DisplayName("Panel预制体目录")]
        internal string PanelPrefabRoot = "Assets/Resources/Prefab/UI/Panel";
        /// <summary>
        /// Widget预制体目录
        /// </summary>
        [SerializeField]
        [DisplayName("Widget预制体目录")]
        internal string WidgetPrefabRoot = "Assets/Resources/Prefab/UI/Widget";
        /// <summary>
        /// Panel脚本路径
        /// </summary>
        [SerializeField]
        [DisplayName("Panel脚本目录")]
        internal string PanelCodeRoot = "Assets/Script/UI/Panel";
        /// <summary>
        /// Widget脚本路径
        /// </summary>
        [SerializeField]
        [DisplayName("Widget脚本目录")]
        internal string WidgetCodeRoot = "Assets/Script/UI/Widget";
        /// <summary>
        /// 界面生成脚本文件
        /// </summary>
        [SerializeField]
        [DisplayName("生成代码目录")]
        internal string CodeGenFileRoot = "Assets/Script/Gen/UI";

        public void Save()
        {
            ((ICustomMicroEditorConfig)this).Save();
        }

        [HideInInspector]
        [NonSerialized]
        internal static Dictionary<string, string> UIExportDic = new Dictionary<string, string>()
        {
            {"btn_", typeof(Button).FullName },
            {"img_", typeof(Image).FullName },
            {"txt_", typeof(Text).FullName },
            {"ptxt_", "TextMeshProUGUI" },
            {"inp_", typeof(InputField).FullName },
            {"pinp_", "TMP_InputField" },
            {"srect_", typeof(ScrollRect).FullName },
            {"sbar_", typeof(Scrollbar).FullName },
            {"tog_", typeof(Toggle).FullName },
            {"sli_", typeof(Slider).FullName },
            {"drop_", typeof(Dropdown).FullName },
            {"pdrop_", "TMP_Dropdown" },
            {"can_", typeof(Canvas).FullName },
            {"go_", typeof(GameObject).FullName },
            {"tran_", typeof(Transform).FullName },
            {"rtran_", typeof(RectTransform).FullName },
        };
        [HideInInspector]
        [NonSerialized]
        internal static Dictionary<string, string> MVVMTypeDic = new Dictionary<string, string>()
        {
            {typeof(Button).FullName, "UIButton" },
            {typeof(Image).FullName, "UIImage" },
            {typeof(Text).FullName, "UIText" },
            {"TextMeshProUGUI", "TMP_UIText" },
            {typeof(InputField).FullName, "UIInputField" },
            {"TMP_InputField", "TMP_UIInputField" },
            {typeof(Toggle).FullName, "UIToggle" },
            {typeof(Slider).FullName, "UISlider" },
            {typeof(Scrollbar).FullName, "UIScrollbar" },
            {typeof(ScrollRect).FullName, "UIScrollRect" },
            {typeof(Dropdown).FullName, "UIDropdown" },
            {"TMP_Dropdown", "TMP_UIDropdown" },
            {typeof(Transform).FullName, "UITransform" },
            {typeof(RectTransform).FullName, "UIRectTransform" },
            {typeof(GameObject).FullName, "UIGameObject" },
        };
    }
}
