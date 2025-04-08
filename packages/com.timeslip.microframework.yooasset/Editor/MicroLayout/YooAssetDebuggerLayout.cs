using MFramework.Core.Editor;
using UnityEngine;
using YooAsset.Editor;

namespace MFramework.Editor
{
    internal class YooAssetDebuggerLayout : BaseMicroLayout
    {
        public override string Title => "资源管理/YooAsset/AB 调试器";
        public override int Priority => 3;

        private AssetBundleDebuggerWindow debuggerWindow;
        public override bool Init()
        {
            debuggerWindow = ScriptableObject.CreateInstance<AssetBundleDebuggerWindow>();
            debuggerWindow.CreateGUI();
            this.panel.Add(debuggerWindow.rootVisualElement);
            debuggerWindow.rootVisualElement.style.flexGrow = 1;
            return true;
        }
        public override void HideUI()
        {
        }
        public void OnDestroy()
        {
            Object.DestroyImmediate(debuggerWindow);
        }

    }
}
