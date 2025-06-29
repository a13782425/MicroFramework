using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace MFramework.AssetMonitor
{
    internal class SettingAssetMonitorTab : BaseAssetMonitorTab
    {
        protected internal override string title => "设置";

        protected internal override string icon => "_Popup";
        protected internal override int priority => int.MaxValue;

        public override void Init(AssetMonitorPanel panel)
        {
            base.Init(panel);

            Button button = new Button();
            button.text = "Refresh Asset Monitor";
            button.clicked += () => RefreshAssetMonitor();

            this.Add(button);

            button = new Button();
            button.text = "强制刷新";
            button.clicked +=  RefreshAssetMonitor1;
            this.Add(button);

            button = new Button();
            button.text = "测试";
            button.clicked += Test;
            this.Add(button);
        }

        private void Test()
        {
            string guid = AssetDatabase.AssetPathToGUID("Packages");
            UnityEngine.Debug.LogError(Path.GetDirectoryName("Assets"));
        }

        private void RefreshAssetMonitor1()
        {
            //AssetMonitorConfig.Instance.AssetInfoDict.Clear();
            AssetMonitorTools.InitFiles(AssetDatabase.GetAllAssetPaths());
        }

        private void RefreshAssetMonitor()
        {
            AssetMonitorTools.InitFiles(AssetDatabase.GetAllAssetPaths());
        }
    }
}
