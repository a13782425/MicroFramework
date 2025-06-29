using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #region uss
        internal const string USS_BASE_CLASS = "assetmonitor-panel";

        internal const string USS_TAB_CLASS = USS_BASE_CLASS + "__tab";
        internal const string USS_TAB_BUTTON_CLASS = USS_TAB_CLASS + "-button";
        internal const string USS_TAB_ICON_CLASS = USS_TAB_CLASS + "-icon";

        internal const string USS_PAGE_CLASS = USS_BASE_CLASS + "__page";
        internal const string USS_PAGE_CONTAINER_CLASS = USS_PAGE_CLASS + "-container";

        internal const string USS_PROJECT_CLASS = USS_BASE_CLASS + "__project";

        internal const string USS_PROJECT_INIT_CLASS = USS_PROJECT_CLASS + "__init";
        internal const string USS_PROJECT_INIT_BTN_CLASS = USS_PROJECT_INIT_CLASS + "-btn";


        internal const string USS_PROJECT_FLODER_CLASS = USS_PROJECT_CLASS + "__folder";
        internal const string USS_PROJECT_FLODER_ITEM_CLASS = USS_PROJECT_FLODER_CLASS + "-item";
        internal const string USS_PROJECT_FLODER_ITEM_ICON_CLASS = USS_PROJECT_FLODER_ITEM_CLASS + "-icon-image";
        internal const string USS_PROJECT_FLODER_ITEM_NAME_CLASS = USS_PROJECT_FLODER_ITEM_CLASS + "-name-label";
        internal const string USS_PROJECT_FLODER_ITEM_SIZE_CLASS = USS_PROJECT_FLODER_ITEM_CLASS + "-size-label";
        internal const string USS_PROJECT_FLODER_ITEM_COUNT_CLASS = USS_PROJECT_FLODER_ITEM_CLASS + "-count-label";
        internal const string USS_PROJECT_FLODER_ITEM_REF_BTN_CLASS = USS_PROJECT_FLODER_ITEM_CLASS + "-ref-btn";
        internal const string USS_PROJECT_FLODER_ITEM_DEP_BTN_CLASS = USS_PROJECT_FLODER_ITEM_CLASS + "-dep-btn";

        internal const string USS_PROJECT_REFERENCE_CLASS = USS_PROJECT_CLASS + "__ref";
        internal const string USS_PROJECT_REFERENCE_TITLE_CLASS = USS_PROJECT_REFERENCE_CLASS + "-title-label";

        #endregion

    }
}
