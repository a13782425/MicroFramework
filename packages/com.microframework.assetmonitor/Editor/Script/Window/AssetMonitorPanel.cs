//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEditor.UIElements;
//using UnityEngine;
//using UnityEngine.UIElements;
//using static MFramework.AssetMonitor.AssetMonitorConst;

//namespace MFramework.AssetMonitor
//{
//    public class AssetMonitorPanel : VisualElement
//    {


//        private VisualElement _tabElement;

//        private VisualElement _pageContainer;

//        private List<BaseAssetMonitorTab> _tabs = new List<BaseAssetMonitorTab>();
//        private BaseAssetMonitorTab _currentTab = default;

//        internal AssetMonitorWindow window { get; }
//        public AssetMonitorPanel(AssetMonitorWindow window)
//        {
//            this.window = window;
//            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_DIR, "Resources/AssetMonitorPanel.uss"));
//            this.styleSheets.Add(styleSheet);

//            this.AddToClassList(USS_BASE_CLASS);
//            _tabElement = new VisualElement();
//            _tabElement.AddToClassList(USS_TAB_CLASS);
//            this.Add(_tabElement);
//            _pageContainer = new VisualElement();
//            _pageContainer.AddToClassList(USS_PAGE_CLASS);
//            this.Add(_pageContainer);

//            this.RegisterCallback<AttachToPanelEvent>(m_attachPanel);


//        }

//        private void m_attachPanel(AttachToPanelEvent evt)
//        {
//            this.UnregisterCallback<AttachToPanelEvent>(m_attachPanel);
//            m_createTabs();
//        }

//        private void m_createTabs()
//        {
//            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<BaseAssetMonitorTab>();

//            foreach (var item in types)
//            {
//                if (item.IsInterface || item.IsAbstract || item.IsGenericType || item.IsNested || (item.IsAbstract && item.IsSealed))
//                    continue;
//                BaseAssetMonitorTab obj = Activator.CreateInstance(item) as BaseAssetMonitorTab;
//                if (obj != null)
//                {
//                    _tabs.Add(obj);
//                    obj.Init(this);
//                }
//            }
//            _tabs.Sort((x, y) => x.priority.CompareTo(y.priority));
//            _tabs.ForEach(x =>
//            {
//                this._tabElement.Add(new TabButton(this, x));
//            });
//            if (this._tabElement.childCount > 0)
//            {
//                // 获取第一个 ToolbarToggle 并设置为选中状态
//                this._tabElement.Q<TabButton>().value = true;
//            }
//        }
//        internal void OnUpdate()
//        {
//            _currentTab?.OnUpdate();
//        }
//        internal void OnDisable()
//        {
//            try
//            {
//                foreach (var item in _tabs)
//                {
//                    item.Exit();
//                }
//                AssetMonitorConfig.Save();
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex.ToString());
//            }
//        }
        
//    }
//}