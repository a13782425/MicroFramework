using MFramework.Core.Editor;
using UnityEngine;
using YooAsset.Editor;

namespace MFramework.Editor
{
    internal class YooAssetCollectorLayout : BaseMicroLayout
    {
        public override string Title => "资源管理/YooAsset/AB 收集器";

        private AssetBundleCollectorWindow collectorWindow;
        public override bool Init()
        {
            collectorWindow = ScriptableObject.CreateInstance<AssetBundleCollectorWindow>();
            collectorWindow.CreateGUI();
            this.panel.Add(collectorWindow.rootVisualElement);
            collectorWindow.rootVisualElement.style.flexGrow = 1;
            return true;
        }
        public override void ShowUI()
        {
            collectorWindow.OnEnable();
        }

        public override void HideUI()
        {
            collectorWindow.OnDisable();
        }
        public override void Exit()
        {
            Object.DestroyImmediate(collectorWindow);
        }
        public override void OnUpdate()
        {
            collectorWindow.Update();
        }
    }
}
