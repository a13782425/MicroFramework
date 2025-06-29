using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static PlasticGui.Configuration.CloudEdition.GetFirstCloudServer;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控窗口
    /// </summary>
    internal class AssetMonitorWindow : EditorWindow
    {
        [MenuItem("Tools/资源监控")]
        static void ShowWindow()
        {
            var window = GetWindow<AssetMonitorWindow>();
            window.titleContent = new GUIContent("资源监控");
            window.minSize = new Vector2(640, 480);
            window.Show();
        }

        private AssetMonitorPanel _panel;

        private void CreateGUI()
        {
            VisualElement root = this.rootVisualElement;
            root.style.flexGrow = 1;
            _panel = new AssetMonitorPanel();
            root.Add(_panel);
        }
        private void OnEnable()
        {
            
        }

        private void Update()
        {
            _panel?.OnUpdate();
        }

        private void OnDisable()
        {
            _panel.OnDisable();
        }
    }
}
