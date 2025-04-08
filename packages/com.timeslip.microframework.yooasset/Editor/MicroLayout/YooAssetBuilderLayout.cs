using MFramework.Core.Editor;
using UnityEngine;
using YooAsset.Editor;

namespace MFramework.Editor
{
    internal class YooAssetBuilderLayout : BaseMicroLayout
    {
        public override string Title => "资源管理/YooAsset/AB 构建器";
        private AssetBundleBuilderWindow builderWindow;

        public override bool Init()
        {
            builderWindow = ScriptableObject.CreateInstance<AssetBundleBuilderWindow>();
            builderWindow.CreateGUI();
            this.panel.Add(builderWindow.rootVisualElement);
            builderWindow.rootVisualElement.style.flexGrow = 1;
            return true;
        }
        public override void Exit()
        {
            base.Exit();
            Object.DestroyImmediate(builderWindow);
        }
    }
}
