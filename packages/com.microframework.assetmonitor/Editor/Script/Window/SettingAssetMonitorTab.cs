using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    internal class SettingAssetMonitorTab : BaseAssetMonitorTab
    {
        protected internal override string title => "设置";

        protected internal override string icon => "_Popup";
        protected internal override int priority => int.MaxValue;

        #region 基础设置

        private Toggle _basicUseUnitySize;
        private Toggle _basicShowInProject;
        private Toggle _basicShowEmptyReference;
        private Toggle _basicSelectInProject;
        private Toggle _basicAutoExpandedTree;

        #endregion

        private ScrollView _container;
        public override void Init(AssetMonitorWindow window)
        {
            base.Init(window);
            _container = new ScrollView();
            _container.style.flexGrow = 1;
            _container.style.marginBottom = 6;
            this.Add(_container);

            m_createBasicSetting();
            m_createCommonSetting();
            m_createSearchSetting();
            m_createWatcherSetting();
            m_createVerifySetting();

            VisualElement btnContainer = new VisualElement();
            btnContainer.style.flexDirection = FlexDirection.Row;
            btnContainer.style.height = 36;
            btnContainer.style.minHeight = 36;
            btnContainer.style.marginBottom = 6;
            this.Add(btnContainer);
            Button button = new Button();
            button.text = "强制刷新";
            button.AddToClassList(USS_SETTING_BOTTOM_BTN_CLASS);
            button.clicked += m_refreshAssetMonitor;
            btnContainer.Add(button);

            button = new Button(m_saveSetting);
            button.text = "保存设置";
            button.AddToClassList(USS_SETTING_BOTTOM_BTN_CLASS);
            btnContainer.Add(button);
        }



        public override void Show()
        {
            m_showBasicSetting();
        }

        public override void Hide()
        {
            m_saveBasicSetting();
            AssetMonitorConfig.Save();
        }

        public override void Exit()
        {
            m_saveBasicSetting();
            AssetMonitorConfig.Save();
        }

        private void m_createBasicSetting()
        {
            CollapsibleBox box = new CollapsibleBox("基础配置");
            _container.Add(box);
            _basicUseUnitySize = new Toggle("使用Unity大小");
            _basicUseUnitySize.value = AssetMonitorConfig.Instance.UseUnitySize;
            box.AddElement(_basicUseUnitySize);

            _basicShowInProject = new Toggle("在Project显示");
            _basicShowInProject.RegisterValueChangedCallback(m_showInProjectChanged);
            _basicShowInProject.value = AssetMonitorConfig.Instance.ShowInProject;
            box.AddElement(_basicShowInProject);

            _basicShowEmptyReference = new Toggle("显示0引用");
            _basicShowEmptyReference.value = AssetMonitorConfig.Instance.ShowEmptyReference;
            box.AddElement(_basicShowEmptyReference);

            _basicSelectInProject = new Toggle("在Project选中");
            _basicSelectInProject.value = AssetMonitorConfig.Instance.SelectInProject;
            box.AddElement(_basicSelectInProject);

            _basicAutoExpandedTree = new Toggle("自动展开树");
            _basicAutoExpandedTree.value = AssetMonitorConfig.Instance.AutoExpandedTree;
            box.AddElement(_basicAutoExpandedTree);
        }
        private void m_createCommonSetting()
        {
            CollapsibleBox box = new CollapsibleBox("右键菜单");
            _container.Add(box);
            foreach (var item in AssetMonitorConfig.Instance.CommandInfoDict)
            {
                ConfigExtensionItemView searcherItem = new ConfigExtensionItemView(this, item.Value);
                box.AddElement(searcherItem);
            }
        }
        private void m_createSearchSetting()
        {
            CollapsibleBox box = new CollapsibleBox("搜索器");
            _container.Add(box);

            foreach (var item in AssetMonitorConfig.Instance.SearcherInfoDict)
            {
                ConfigExtensionItemView searcherItem = new ConfigExtensionItemView(this, item.Value);
                box.AddElement(searcherItem);
            }

        }
        private void m_createWatcherSetting()
        {
            CollapsibleBox box = new CollapsibleBox("资源观察");
            _container.Add(box);
            foreach (var item in AssetMonitorConfig.Instance.WatcherInfoDict)
            {
                RefreshExtensionItemView searcherItem = new RefreshExtensionItemView(this, item.Value);
                box.AddElement(searcherItem);
            }
        }

        private void m_createVerifySetting()
        {
            CollapsibleBox box = new CollapsibleBox("资源校验");
            _container.Add(box);
            foreach (var item in AssetMonitorConfig.Instance.VerifierInfoDict)
            {
                RefreshExtensionItemView searcherItem = new RefreshExtensionItemView(this, item.Value);
                box.AddElement(searcherItem);
            }
        }
        private void m_showInProjectChanged(ChangeEvent<bool> evt)
        {
            _basicShowEmptyReference.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void m_showBasicSetting()
        {
            _basicUseUnitySize.value = AssetMonitorConfig.Instance.UseUnitySize;
            _basicShowInProject.value = AssetMonitorConfig.Instance.ShowInProject;
            _basicShowEmptyReference.value = AssetMonitorConfig.Instance.ShowEmptyReference;
            _basicSelectInProject.value = AssetMonitorConfig.Instance.SelectInProject;
            _basicAutoExpandedTree.value = AssetMonitorConfig.Instance.AutoExpandedTree;
        }
        private void m_saveSetting()
        {
            m_saveBasicSetting();
            AssetMonitorConfig.Save();
            this.window.ShowNotification(new UnityEngine.GUIContent("配置保存成功"));
        }

        private void m_saveBasicSetting()
        {
            AssetMonitorConfig.Instance.UseUnitySize = _basicUseUnitySize.value;
            AssetMonitorConfig.Instance.ShowInProject = _basicShowInProject.value;
            AssetMonitorConfig.Instance.ShowEmptyReference = _basicShowEmptyReference.value;
            AssetMonitorConfig.Instance.SelectInProject = _basicSelectInProject.value;
            AssetMonitorConfig.Instance.AutoExpandedTree = _basicAutoExpandedTree.value;
        }
        private void m_refreshAssetMonitor()
        {
            AssetMonitorConfig.Instance.ClearAllRecords();
            AssetMonitorTools.InitFiles();
        }


        /// <summary>
        /// 格式化是否启用的信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        private string m_formatEnableInfo(string info, bool isEnable)
        {
            return (isEnable ? "<color=#00ff00>[启用]</color> " : "<color=#ff0000>[禁用]</color> ") + info;
        }

        private class ConfigExtensionItemView : VisualElement
        {
            private readonly SettingAssetMonitorTab _tab;
            private readonly IConfigExtension _configExtension;
            internal SettingAssetMonitorTab Tab => _tab;
            internal IConfigExtension ConfigExtension => _configExtension;

            private Toggle _collapsibleToggle;
            private Toggle _enableToggle;
            private Label _descLabel;

            protected readonly VisualElement headerContainer;
            protected readonly VisualElement _container;

            public ConfigExtensionItemView(SettingAssetMonitorTab tab, IConfigExtension configExtension)
            {
                _tab = tab;
                _configExtension = configExtension;
                this.AddToClassList(USS_SETTING_ITEM_CLASS);
                headerContainer = new VisualElement();
                headerContainer.AddToClassList(USS_SETTING_ITEM_HEADER_CLASS);
                this.Add(headerContainer);

                _container = new VisualElement();
                _container.AddToClassList(USS_SETTING_ITEM_CONTAINER_CLASS);
                _container.style.display = DisplayStyle.None;
                this.Add(_container);


                _collapsibleToggle = new Toggle();
                _collapsibleToggle.RegisterValueChangedCallback(m_onDetailsClick);
                _collapsibleToggle.AddToClassList(Foldout.toggleUssClassName);
                _collapsibleToggle.text = _tab.m_formatEnableInfo(_configExtension.GetDisplayName(), _configExtension.IsEnabled);
                headerContainer.Add(_collapsibleToggle);

                _enableToggle = new Toggle();
                _enableToggle.text = "是否启用";
                _enableToggle.value = _configExtension.IsEnabled;
                _enableToggle.RegisterValueChangedCallback(m_onEnableClick);

                _container.Add(_enableToggle);
                _descLabel = new Label();
                string desc = _configExtension.GetDescription();
                if (string.IsNullOrWhiteSpace(desc))
                {
                    desc = "暂无描述信息, 描述信息可使用富文本\r\n<color=#ffff00>示例</color>";
                }
                _descLabel.text = desc;
                _container.Add(_descLabel);
            }

            private void m_onEnableClick(ChangeEvent<bool> evt)
            {
                _configExtension.IsEnabled = evt.newValue;
                _collapsibleToggle.text = _tab.m_formatEnableInfo(_configExtension.GetDisplayName(), _configExtension.IsEnabled);
                onEnableChanged();
            }

            protected virtual void onEnableChanged()
            {

            }

            private void m_onDetailsClick(ChangeEvent<bool> evt)
            {
                if (!evt.newValue)
                {
                    _container.style.display = DisplayStyle.None;
                }
                else
                {
                    _container.style.display = DisplayStyle.Flex;
                }
            }
        }


        private class RefreshExtensionItemView : ConfigExtensionItemView
        {
            private Button _refreshButton;

            public RefreshExtensionItemView(SettingAssetMonitorTab tab, IConfigExtension configExtension) : base(tab, configExtension)
            {
                _refreshButton = new Button(m_refreshClick);
                //Refresh
                _refreshButton.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("RotateTool").image);
                this.headerContainer.Add(this._refreshButton);
            }

            protected override void onEnableChanged()
            {
                m_refreshClick();
            }
            private void m_refreshClick()
            {
                switch (ConfigExtension)
                {
                    case VerifierInfo verifierInfo:
                        foreach (var item in AssetMonitorConfig.Instance.GuidAssetRecordDict)
                        {
                            item.Value.RemoveVerifyResult(verifierInfo);
                            if (AssetMonitorTools.IsMatchVerifierInfo(verifierInfo, item.Value.AssetPath))
                                item.Value.RefreshVerifyResult(verifierInfo);
                        }
                        break;
                    case WatcherInfo watcherInfo:
                        foreach (var item in AssetMonitorConfig.Instance.GuidAssetRecordDict)
                        {
                            item.Value.RemoveWatcherRelation(watcherInfo);
                            if (AssetMonitorTools.IsMatchWatcherInfo(watcherInfo, item.Value.AssetPath))
                                item.Value.RefreshWatcherRelation(watcherInfo);
                        }
                        break;
                    default:
                        break;
                }

                Tab.window.ShowNotification(new GUIContent($"刷新 {ConfigExtension.GetDisplayName()} 完成"));
            }
        }
    }

}
