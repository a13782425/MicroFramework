namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控的静态常量
    /// </summary>
    internal static class AssetMonitorConst
    {
        /// <summary>
        /// 编辑器目录
        /// </summary>
        internal const string EDITOR_DIR = "Packages\\com.microframework.assetmonitor\\Editor";

        /// <summary>
        /// 缓存路径
        /// </summary>
        internal const string CONFIG_CACHE_PATH = "Library/AssetMonitor.db";

        /// <summary>
        /// Unity关系
        /// </summary>
        internal const string UNITY_RELATION = "Unity";

        /// <summary>
        /// GUID长度
        /// </summary>
        internal const int GUID_LENGTH = 32;

        /// <summary>
        /// 特殊的GUID
        /// </summary>
        internal const string SPECIAL_GUID = "--------------------------------";
        /// <summary>
        /// 特殊的文件夹
        /// </summary>
        internal const string SPECIAL_FOLDER = "Packages";

        /// <summary>
        /// library文件夹
        /// </summary>
        internal const string LIBRARY_FOLDER = "Library";

        /// <summary>
        /// 资源文件夹
        /// </summary>
        internal const string ASSET_FOLDER = "Assets";


        #region uss
        internal const string USS_BASE_CLASS = "assetmonitor-window";

        internal const string USS_TAB_CLASS = USS_BASE_CLASS + "__tab";
        internal const string USS_TAB_BUTTON_CLASS = USS_TAB_CLASS + "-button";
        internal const string USS_TAB_ICON_CLASS = USS_TAB_CLASS + "-icon";


        internal const string USS_PAGE_CLASS = USS_BASE_CLASS + "__page";
        internal const string USS_PAGE_CONTAINER_CLASS = USS_PAGE_CLASS + "-container";


        internal const string USS_BOX_CLASS = USS_BASE_CLASS + "__box";
        internal const string USS_BOX_TITLE_CLASS = USS_BOX_CLASS + "-title";


        internal const string USS_RELATION_CLASS = USS_BASE_CLASS + "__relation";
        internal const string USS_RELATION_INIT_CLASS = USS_RELATION_CLASS + "__init";
        internal const string USS_RELATION_INIT_BTN_CLASS = USS_RELATION_INIT_CLASS + "-btn";


        internal const string USS_RELATION_FLODER_CLASS = USS_RELATION_CLASS + "__folder";
        internal const string USS_RELATION_FLODER_TREE_CLASS = USS_RELATION_FLODER_CLASS + "-tree";
        internal const string USS_RELATION_FLODER_ITEM_CLASS = USS_RELATION_FLODER_CLASS + "-item";
        internal const string USS_RELATION_FLODER_ITEM_ICON_CLASS = USS_RELATION_FLODER_ITEM_CLASS + "-icon-image";
        internal const string USS_RELATION_FLODER_ITEM_NAME_CLASS = USS_RELATION_FLODER_ITEM_CLASS + "-name-label";
        internal const string USS_RELATION_FLODER_ITEM_SIZE_CLASS = USS_RELATION_FLODER_ITEM_CLASS + "-size-label";
        internal const string USS_RELATION_FLODER_ITEM_COUNT_CLASS = USS_RELATION_FLODER_ITEM_CLASS + "-count-label";
        internal const string USS_RELATION_FLODER_ITEM_REF_BTN_CLASS = USS_RELATION_FLODER_ITEM_CLASS + "-ref-btn";
        internal const string USS_RELATION_FLODER_ITEM_DEP_BTN_CLASS = USS_RELATION_FLODER_ITEM_CLASS + "-dep-btn";


        internal const string USS_RELATION_REFERENCE_CLASS = USS_RELATION_CLASS + "__ref";
        internal const string USS_RELATION_REFERENCE_TREE_CLASS = USS_RELATION_REFERENCE_CLASS + "-tree";
        internal const string USS_RELATION_REFERENCE_TREE_ITEM_CLASS = USS_RELATION_REFERENCE_TREE_CLASS + "-item";
        internal const string USS_RELATION_REFERENCE_TREE_MISSING_CLASS = USS_RELATION_REFERENCE_TREE_ITEM_CLASS + "-missing";
        internal const string USS_RELATION_REFERENCE_TITLE_CLASS = USS_RELATION_REFERENCE_CLASS + "-title-label";


        internal const string USS_SETTING_CLASS = USS_BASE_CLASS + "__setting";
        internal const string USS_SETTING_BOTTOM_BTN_CLASS = USS_SETTING_CLASS + "-bottom-btn";

        internal const string USS_SETTING_ITEM_CLASS = USS_SETTING_CLASS + "-item";
        internal const string USS_SETTING_ITEM_HEADER_CLASS = USS_SETTING_ITEM_CLASS + "-header-container";
        internal const string USS_SETTING_ITEM_CONTAINER_CLASS = USS_SETTING_ITEM_CLASS + "-container";


        internal const string USS_VERIFY_CLASS = USS_BASE_CLASS + "__verify";
        internal const string USS_VERIFY_RESULT_CLASS = USS_VERIFY_CLASS + "-result";

        #endregion

    }
}
