using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    internal partial class RelationAssetMonitorTab : BaseAssetMonitorTab
    {
        protected internal override string title => "关系查询";

        protected internal override string icon => "d_account";
        protected internal override int priority => int.MinValue;

        private VisualElement _initPanel = null;

        private VisualElement _projectPanel = null;


        private TwoPaneSplitView _splitView;
        private ToolbarPopupSearchField _searchField;
        private string _currentSearchText = "";
        //文件树
        private MTreeView _folderTreeView;
        // 添加引用TreeView相关字段
        private MTreeView _referenceTreeView;

        private VisualElement _folderTreePanel;
        private VisualElement _referenceTreePanel;

        //引用类型
        private Label _refTitleLabel;

        private List<MTreeViewItemData> _allItems = new List<MTreeViewItemData>();
        private List<MTreeViewItemData> _relationItems = new List<MTreeViewItemData>();
        private List<RelationInfo> _relationCacheList = new List<RelationInfo>();

        public override void Init(AssetMonitorWindow window)
        {
            base.Init(window);
            _initPanel = new VisualElement();
            _initPanel.AddToClassList(USS_RELATION_INIT_CLASS);
            Button initBtn = new Button(m_initBtnClick);
            initBtn.AddToClassList(USS_RELATION_INIT_BTN_CLASS);
            initBtn.text = "初始化依赖";
            _initPanel.Add(initBtn);
            this.Add(_initPanel);

            _projectPanel = new VisualElement();
            _projectPanel.AddToClassList(USS_RELATION_CLASS);
            this.Add(_projectPanel);
            _searchField = new ToolbarPopupSearchField();
#if UNITY_2021
            _searchField.style.width = new Length(0, (LengthUnit)2);
#else
            _searchField.style.width = Length.Auto();
#endif
            // 添加搜索事件监听
            _searchField.RegisterCallback<KeyDownEvent>(OnSearchKeyDown);
            _projectPanel.Add(_searchField);
            _splitView = new TwoPaneSplitView(0, AssetMonitorConfig.Instance.ProjectFiexdSize, TwoPaneSplitViewOrientation.Vertical);
            _folderTreePanel = new VisualElement();
            _referenceTreePanel = new VisualElement();
            _folderTreePanel.AddToClassList(USS_RELATION_FLODER_CLASS);
            _referenceTreePanel.AddToClassList(USS_RELATION_REFERENCE_CLASS);
            _splitView.Add(_folderTreePanel);
            _splitView.Add(_referenceTreePanel);
            _projectPanel.Add(_splitView);
            _splitView.RegisterCallback<GeometryChangedEvent>(m_spliteViewGeometryChanged);
            m_initFolderTreeView(_folderTreePanel);
            m_initReferenceTreeView(_referenceTreePanel);
            m_refreshSearchMenu();
        }

        public override void Show()
        {
            if (AssetMonitorConfig.Instance.IsInitialized)
            {
                _projectPanel.style.display = DisplayStyle.Flex;
                _initPanel.style.display = DisplayStyle.None;
                m_refreshFileTree();
            }
            else
            {
                _projectPanel.style.display = DisplayStyle.None;
                _initPanel.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary>
        /// 设置搜索文字
        /// </summary>
        /// <param name="str"></param>
        public void SetSearchText(string searchText = "")
        {
            if (_currentSearchText != searchText)
            {
                _currentSearchText = searchText;
                _searchField.SetValueWithoutNotify(searchText);
                m_searchFolderTree();
            }
        }
        /// <summary>
        /// 根据GUID显示引用信息
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="showReference"></param>
        public void ShowReferenceInfoByGuid(string guid, bool showReference = true, bool setSelection = false)
        {
            _relationItems.Clear();
            _relationCacheList.Clear();
            if (!AssetMonitorTools.CheckGuid(guid))
            {
                _refTitleLabel.text = "引用资源";
                _referenceTreeView.SetRootItems(_relationItems);
                _referenceTreeView.Rebuild();
                return;
            }
            var record = AssetMonitorTools.GetRecordByGuid(guid);
            if (record == null)
            {
                _refTitleLabel.text = "引用资源";
                _referenceTreeView.SetRootItems(_relationItems);
                _referenceTreeView.Rebuild();
                return;
            }
            if (showReference)
            {
                _refTitleLabel.text = "引用资源";
                _relationCacheList.AddRange(record.ReferenceRelations);
            }
            else
            {
                _refTitleLabel.text = "依赖资源";
                _relationCacheList.AddRange(record.DependencyRelations);
            }
            var rootRelation = new RelationInfo();
            rootRelation.Guid = record.Guid;
            var referencedByItem = new MTreeViewItemData(rootRelation);
            if (_relationCacheList.Count > 0)
            {
                foreach (var relationGroup in _relationCacheList.GroupBy(a => a.Relation).ToDictionary(a => a.Key, a => a.ToList()))
                {
                    var relationByChildren = new List<MTreeViewItemData>();
                    var childItem = new MTreeViewItemData(relationGroup.Key, relationByChildren);
                    referencedByItem.AddChild(childItem);
                    relationGroup.Value.Sort((a, b) =>
                    {
                        var aRecord = AssetMonitorTools.GetRecordByGuid(a.Guid);
                        var bRecord = AssetMonitorTools.GetRecordByGuid(b.Guid);
                        if (aRecord == null)
                            return -1;
                        if (bRecord == null)
                            return 1;
                        return aRecord.AssetType.CompareTo(bRecord.AssetType);
                    });
                    foreach (var relation in relationGroup.Value)
                    {
                        childItem.AddChild(new MTreeViewItemData(relation));
                    }
                }

                _relationItems.Add(referencedByItem);
            }
            else
            {
                referencedByItem.AddChild(new MTreeViewItemData(showReference ? "该资源没有引用" : "该资源没有被依赖"));
                _relationItems.Add(referencedByItem);
            }
            if (record.VerifyResults.Count > 0)
            {
                var verifyItem = new MTreeViewItemData("文件校验结果");
                foreach (var item in record.VerifyResults)
                    verifyItem.AddChild(new MTreeViewItemData(item));
                _relationItems.Add(verifyItem);
            }

            // 更新TreeView
            _referenceTreeView.SetRootItems(_relationItems);
            _referenceTreeView.Rebuild();

            // 展开所有节点
            _referenceTreeView.ExpandAll();

            if (!setSelection)
                return;
            foreach (var rootItem in _allItems)
            {
                var filteredItem = m_searchTreeItem(rootItem);
                if (filteredItem != null)
                {

                    m_selectTreeViewItem(filteredItem, withoutNotify: true);
                    break;
                }
            }
            MTreeViewItemData m_searchTreeItem(MTreeViewItemData item)
            {
                AssetInfoRecord record = item.GetData<AssetInfoRecord>();

                if (record == null)
                    return null;
                if (record.Guid == guid)
                    return item;
                // 递归过滤子项
                if (item.HasChildren)
                {
                    foreach (var child in item.Children)
                    {
                        var foundItem = m_searchTreeItem(child);
                        if (foundItem != null)
                            return foundItem;
                    }
                }
                return default;
            }
        }
        /// <summary>
        /// 根据路径显示引用信息
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="showReference"></param>
        public void ShowReferenceInfoByPath(string assetPath, bool showReference = true, bool setSelection = false) => ShowReferenceInfoByGuid(AssetMonitorTools.AssetPathToGuid(assetPath), showReference, setSelection);

        private void m_onPaneGeometryChanged(GeometryChangedEvent evt)
        {
            AssetMonitorConfig.Instance.ProjectFiexdSize = _splitView.fixedPane.style.height.value.value;
        }

        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void m_initBtnClick()
        {
            AssetMonitorTools.InitFiles();
            _projectPanel.style.display = DisplayStyle.Flex;
            _initPanel.style.display = DisplayStyle.None;
            m_refreshFileTree();
            window.RelationInitialize();
        }

        private void m_spliteViewGeometryChanged(GeometryChangedEvent evt)
        {
            _splitView.fixedPane.style.minHeight = 120;
            _splitView.fixedPane.RegisterCallback<GeometryChangedEvent>(m_onPaneGeometryChanged);
            _splitView.UnregisterCallback<GeometryChangedEvent>(m_spliteViewGeometryChanged);
        }

        #region 搜索

        // 修改为按键事件处理方法
        private void OnSearchKeyDown(KeyDownEvent evt)
        {
            // 只在按下回车键时执行搜索
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                SetSearchText(_searchField.value);
                // 阻止事件继续传播
                evt.StopPropagation();
                evt.PreventDefault();
            }
            // 可选：按ESC键清空搜索
            else if (evt.keyCode == KeyCode.Escape)
            {
                SetSearchText();

                evt.StopPropagation();
                evt.PreventDefault();
            }
        }

        //private void m_onSearchTextChanged(ChangeEvent<string> evt)
        //{
        //    _currentSearchText = evt.newValue;
        //    FilterFileTree();
        //}

        // 选中一个树并滚动到该节点
        private void m_selectTreeViewItem(MTreeViewItemData item, bool expandAll = false, bool withoutNotify = false)
        {
            if (expandAll)
                _folderTreeView.ExpandAll();
            else
                _folderTreeView.ExpandItem(item, true);
            var parentItem = item;
            var childItem = parentItem.Children.FirstOrDefault();
            while (childItem != null)
            {
                parentItem = childItem;
                childItem = childItem.Children.FirstOrDefault();
            }
            if (withoutNotify)
                _folderTreeView.SetSelectionWithoutNotify(parentItem, true);
            else
                _folderTreeView.SetSelection(parentItem, true);
        }

        #endregion

        #region 文件树
        private void m_refreshSearchMenu()
        {
            List<string> str = new List<string>();
#if UNITY_2022_3_OR_NEWER
            _searchField.menu.ClearItems();
#else
            _searchField.menu.MenuItems().Clear();
#endif
            foreach (var item in AssetMonitorConfig.Instance.SearcherInfoDict)
            {
                _searchField.menu.AppendAction(item.Value.GetDisplayName(), m_searchMenuClick, DropdownMenuAction.AlwaysEnabled, item.Value);
            }
        }

        private void m_searchMenuClick(DropdownMenuAction action)
        {
            var searcherInfo = action.userData as SearcherInfo;
            if (searcherInfo == null)
                return;
            string prefix = "";
            foreach (var item in searcherInfo.Searcher.SearchPrefixes)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;
                prefix = item;
                break;
            }
            if (string.IsNullOrWhiteSpace(prefix))
                return;
            var text = _searchField.value;
            if (text.Length > 0)
            {
                if (text[text.Length - 1] == ' ')
                {
                    text += $"{prefix}:";
                }
                else
                    text += $" {prefix}:";
            }
            else
            {
                text = $"{prefix}:";
            }
            _searchField.SetValueWithoutNotify(text);
        }

        private void m_initFolderTreeView(VisualElement container)
        {
            _folderTreeView = new MTreeView();
            _folderTreeView.AddToClassList(USS_RELATION_FLODER_TREE_CLASS);
            _folderTreeView.FixedItemHeight = 24;

            // 设置TreeView的回调函数
            _folderTreeView.onMakeItem = m_makeTreeItem;
            _folderTreeView.onBindItem = m_bindTreeItem;

            // 设置选择回调
            _folderTreeView.onSelectionChanged += m_onSelectionChanged;
            container.Add(_folderTreeView);
        }

        //生成文件树
        private VisualElement m_makeTreeItem()
        {
            return new FolderTreeItemView(this);
        }

        //绑定文件树
        private void m_bindTreeItem(VisualElement element, MTreeViewItemData item)
        {
            var record = item.Data as AssetInfoRecord;
            if (record == null) return;
            var treeItemView = element as FolderTreeItemView;
            if (treeItemView == null) return;
            treeItemView.Refresh(record);
        }
        private void m_refreshFileTree()
        {
            _allItems.Clear();

            // 扫描Assets文件夹
            AssetInfoRecord record = AssetMonitorTools.GetRecordByAssetPath("Assets");
            if (record != null)
            {
                var assetsChildren = m_scanDirectory(record);
                var assetsItem = new MTreeViewItemData(record, assetsChildren);
                _allItems.Add(assetsItem);
            }

            // 扫描Packages文件夹
            record = AssetMonitorTools.GetRecordByAssetPath("Packages");
            if (record != null)
            {
                var assetsChildren = m_scanDirectory(record);
                var assetsItem = new MTreeViewItemData(record, assetsChildren);
                _allItems.Add(assetsItem);
            }

            // 设置TreeView的数据源
            _folderTreeView.SetRootItems(_allItems);
            _folderTreeView.Rebuild();
        }

        private List<MTreeViewItemData> m_scanDirectory(AssetInfoRecord record)
        {
            var children = new List<MTreeViewItemData>();
            try
            {
                // 获取所有子目录
                foreach (var child in record.Childs)
                {
                    var dirChildren = m_scanDirectory(child);
                    var dirTreeItem = new MTreeViewItemData(child, dirChildren);
                    children.Add(dirTreeItem);
                }
                children.Sort((a, b) =>
                {
                    if (a.GetData<AssetInfoRecord>().IsFolder && !b.GetData<AssetInfoRecord>().IsFolder)
                        return -1;
                    else if (!a.GetData<AssetInfoRecord>().IsFolder && b.GetData<AssetInfoRecord>().IsFolder)
                        return 1;
                    else
                        return a.GetData<AssetInfoRecord>().AssetName.CompareTo(b.GetData<AssetInfoRecord>().AssetName);
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"扫描目录时出错 {record.ParentPath}: {e.Message}");
            }
            return children;
        }

        private void m_onSelectionChanged(IEnumerable<MTreeViewItemData> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                if (item.Data is not AssetInfoRecord treeData)
                    continue;
                ShowReferenceInfoByGuid(treeData.Guid, true);
                if (!AssetMonitorConfig.Instance.SelectInProject)
                    continue;
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(treeData.AssetPath);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }
        }

        #endregion

        #region 引用关系TreeView

        private void m_initReferenceTreeView(VisualElement container)
        {
            // 创建标题标签
            _refTitleLabel = new Label("引用资源");
            _refTitleLabel.AddToClassList(USS_RELATION_REFERENCE_TITLE_CLASS);
            container.Add(_refTitleLabel);

            // 创建引用TreeView
            _referenceTreeView = new MTreeView();
            _referenceTreeView.AddToClassList(USS_RELATION_REFERENCE_TREE_CLASS);
            _referenceTreeView.FixedItemHeight = 20;

            // 设置引用TreeView的回调函数
            _referenceTreeView.onMakeItem = m_makeReferenceItem;
            _referenceTreeView.onBindItem = m_bindReferenceItem;
            _referenceTreeView.onSelectionChanged += m_onReferenceSelectionChanged;

            container.Add(_referenceTreeView);
        }

        private VisualElement m_makeReferenceItem()
        {
            return new ReferenceTreeItemView(this);
        }

        private void m_bindReferenceItem(VisualElement element, MTreeViewItemData item)
        {
            var treeItemView = element as ReferenceTreeItemView;
            if (treeItemView == null) return;

            treeItemView.Refresh(item.Data);
        }

        private void m_onReferenceSelectionChanged(IEnumerable<MTreeViewItemData> selectedItems)
        {
            if (!AssetMonitorConfig.Instance.SelectInProject)
                return;
            foreach (var item in selectedItems)
            {
                if (item.Data is RelationInfo relationData && !string.IsNullOrEmpty(relationData.Guid))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(relationData.Guid));
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }
        }
        #endregion


    }
}
