using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控窗口
    /// </summary>
    public sealed class AssetMonitorWindow : EditorWindow
    {

        private VisualElement _tabElement;

        private VisualElement _pageContainer;

        private BaseAssetMonitorTab _currentTab = default;

        private List<TabButton> _tabButtonElements = new List<TabButton>();

        private void CreateGUI()
        {
            VisualElement root = this.rootVisualElement;
            root.style.flexGrow = 1;
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_DIR, "Resources/AssetMonitorWindow.uss")));
            root.AddToClassList(USS_BASE_CLASS);
            _tabElement = new VisualElement();
            _tabElement.AddToClassList(USS_TAB_CLASS);
            root.Add(_tabElement);
            _pageContainer = new VisualElement();
            _pageContainer.AddToClassList(USS_PAGE_CLASS);
            root.Add(_pageContainer);
            m_createTabs();
            root.AddManipulator(new ContextualMenuManipulator(m_buildContextMenu));

            _tabElement.SetEnabled(AssetMonitorConfig.Instance.IsInitialized);
        }

        /// <summary>
        /// 关系初始化完成
        /// </summary>
        internal void RelationInitialize()
        {
            _tabElement.SetEnabled(AssetMonitorConfig.Instance.IsInitialized);
        }

        private void m_buildContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopImmediatePropagation();
        }

        /// <summary>
        /// 选中资源
        /// </summary>
        /// <param name="assetGuid"></param>
        /// <param name="showReference"></param>
        internal void SelectAsset(string assetGuid, bool showReference)
        {
            if (!AssetMonitorConfig.Instance.IsInitialized)
                return;

            TabButton projectTabBtn = _tabButtonElements.FirstOrDefault(a => a.Tab is RelationAssetMonitorTab);

            if (projectTabBtn == null)
                return;

            if (_currentTab != projectTabBtn.Tab)
                projectTabBtn.value = true;
            var projectTab = projectTabBtn.Tab as RelationAssetMonitorTab;
            projectTab.ShowReferenceInfoByGuid(assetGuid, showReference, AssetMonitorConfig.Instance.AutoExpandedTree);
        }

        private void OnEnable()
        {

        }

        private void Update()
        {
            _currentTab?.OnUpdate();
        }

        private void OnDisable()
        {
            try
            {
                foreach (var item in _tabButtonElements)
                {
                    item.Tab.Exit();
                }
                AssetMonitorConfig.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        private void m_createTabs()
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<BaseAssetMonitorTab>();

            List<BaseAssetMonitorTab> tabs = new List<BaseAssetMonitorTab>();

            foreach (var item in types)
            {
                if (item.IsInterface || item.IsAbstract || item.IsGenericType || item.IsNested || (item.IsAbstract && item.IsSealed))
                    continue;
                BaseAssetMonitorTab obj = Activator.CreateInstance(item) as BaseAssetMonitorTab;
                if (obj != null)
                {
                    tabs.Add(obj);
                    obj.Init(this);
                }
            }
            tabs.Sort((x, y) => x.priority.CompareTo(y.priority));
            tabs.ForEach(x =>
            {
                TabButton tabButton = new TabButton(this, x);
                _tabButtonElements.Add(tabButton);
                this._tabElement.Add(tabButton);
            });
            if (_tabButtonElements.Count > 0)
            {
                // 获取第一个 ToolbarToggle 并设置为选中状态
                _tabButtonElements[0].value = true;
            }
        }

        private class TabButton : ToolbarToggle
        {
            private BaseAssetMonitorTab _tab;

            public BaseAssetMonitorTab Tab => _tab;

            private Image _icon;
            private AssetMonitorWindow _window;

            public TabButton(AssetMonitorWindow window, BaseAssetMonitorTab tab)
            {
                this._tab = tab;
                this._window = window;
                text = tab.title;
                tooltip = tab.tooltip;
                this.AddToClassList(USS_TAB_BUTTON_CLASS);
                if (!string.IsNullOrWhiteSpace(tab.icon))
                {
                    _icon = new Image();
                    _icon.AddToClassList(USS_TAB_ICON_CLASS);
                    this.Insert(0, _icon);
                    GUIContent iconContent = EditorGUIUtility.IconContent(tab.icon);
                    _icon.image = iconContent.image;
                }
                value = false;
                this.RegisterValueChangedCallback(m_onValueChanged);
            }

            private void m_onValueChanged(ChangeEvent<bool> evt)
            {
                // 当 Toggle 被选中时 (evt.newValue is true)
                if (evt.newValue)
                {
                    m_switchContent();
                    _window._pageContainer.Add(_tab.container);
                    _tab.Show();
                    _window._currentTab = _tab;
                }
                else
                {
                    _tab.container.RemoveFromHierarchy();
                    _tab.Hide();
                }
            }

            protected override void ToggleValue()
            {
                if (value)
                    return;
                base.ToggleValue();
            }

            private void m_switchContent()
            {
                foreach (var item in this._window._tabElement.Children())
                {
                    TabButton button = item as TabButton;
                    if (button == null)
                        continue;
                    if (button != this)
                    {
                        button.value = false;
                    }
                }
            }
        }
    }
}
