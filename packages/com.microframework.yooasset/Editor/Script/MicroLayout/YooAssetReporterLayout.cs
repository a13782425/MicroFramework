using MFramework.Core.Editor;
using UnityEngine;
using YooAsset.Editor;

namespace MFramework.Editor
{
    internal class YooAssetReporterLayout : BaseMicroLayout
    {
        public override string Title => "资源管理/YooAsset/AB 构建记录";
        public override int Priority => 2;

        private AssetBundleReporterWindow reporterWindow;
        public override bool Init()
        {
            reporterWindow = ScriptableObject.CreateInstance<AssetBundleReporterWindow>();
            reporterWindow.CreateGUI();
            this.panel.Add(reporterWindow.rootVisualElement);
            reporterWindow.rootVisualElement.style.flexGrow = 1;
            return true;
        }
        public override void HideUI()
        {
        }
        public override void Exit()
        {
           Object.DestroyImmediate(reporterWindow);
        }
    }
}
