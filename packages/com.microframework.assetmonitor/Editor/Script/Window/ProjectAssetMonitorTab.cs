using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    internal partial class ProjectAssetMonitorTab : BaseAssetMonitorTab
    {
        protected internal override string title => "项目";

        protected internal override string icon => "Project";

        private VisualElement _initPanel = null;

        private VisualElement _projectPanel = null;


        private TwoPaneSplitView _splitView;
        private ToolbarSearchField _searchField;
        private string _currentSearchText = "";
        //文件树
        private TreeView _folderTreeView;
        // 添加引用TreeView相关字段
        private TreeView _referenceTreeView;

        private VisualElement _folderTreePanel;
        private VisualElement _referenceTreePanel;

        //引用类型
        private Label _refTitleLabel;

        private List<TreeViewItemData<AssetInfoRecord>> _allItems = new List<TreeViewItemData<AssetInfoRecord>>();
        private List<TreeViewItemData<RelationInfo>> _relationItems = new List<TreeViewItemData<RelationInfo>>();
        private int _relationNextId = 0;
        private int nextId = 0;

        public override void Init(AssetMonitorPanel panel)
        {
            base.Init(panel);
            _initPanel = new VisualElement();
            _initPanel.AddToClassList(USS_PROJECT_INIT_CLASS);
            Button refreshBtn = new Button(m_refreshBtnClick);
            refreshBtn.AddToClassList(USS_PROJECT_INIT_BTN_CLASS);
            refreshBtn.text = "初始化依赖";
            _initPanel.Add(refreshBtn);
            this.Add(_initPanel);

            _projectPanel = new VisualElement();
            _projectPanel.AddToClassList(USS_PROJECT_CLASS);
            this.Add(_projectPanel);

            _searchField = new ToolbarSearchField();
            _searchField.style.width = Length.Auto();
            // 添加搜索事件监听
            _searchField.RegisterCallback<KeyDownEvent>(OnSearchKeyDown);
            _projectPanel.Add(_searchField);
            _splitView = new TwoPaneSplitView(0, AssetMonitorConfig.Instance.ProjectFiexdSize, TwoPaneSplitViewOrientation.Vertical);
            _folderTreePanel = new VisualElement();
            _referenceTreePanel = new VisualElement();
            _folderTreePanel.AddToClassList(USS_PROJECT_FLODER_CLASS);
            _referenceTreePanel.AddToClassList(USS_PROJECT_REFERENCE_CLASS);
            _splitView.Add(_folderTreePanel);
            _splitView.Add(_referenceTreePanel);
            _projectPanel.Add(_splitView);
            _splitView.RegisterCallback<GeometryChangedEvent>(m_spliteViewGeometryChanged);
            _folderTreeView = new TreeView();
            _folderTreeView.AddToClassList(USS_PROJECT_FLODER_CLASS);
            _folderTreeView.fixedItemHeight = 24;

            // 设置TreeView的回调函数
            _folderTreeView.makeItem = MakeTreeItem;
            _folderTreeView.bindItem = BindTreeItem;

            // 设置选择回调
            _folderTreeView.selectionChanged += OnSelectionChanged;
            _folderTreePanel.Add(_folderTreeView);
            InitReferenceTreeView(_referenceTreePanel);
            if (AssetMonitorConfig.Instance.IsInitialized)
            {
                _projectPanel.style.display = DisplayStyle.Flex;
                _initPanel.style.display = DisplayStyle.None;
                RefreshFileTree();
            }
            else
            {
                _projectPanel.style.display = DisplayStyle.None;
                _initPanel.style.display = DisplayStyle.Flex;
            }

            //// 初始化引用关系TreeView

            //// 初始加载文件树
            //RefreshFileTree();
        }

        private void m_onPaneGeometryChanged(GeometryChangedEvent evt)
        {
            AssetMonitorConfig.Instance.ProjectFiexdSize = _splitView.fixedPane.style.height.value.value;
        }

        private void m_refreshBtnClick()
        {
            AssetMonitorTools.InitFiles();
            _projectPanel.style.display = DisplayStyle.Flex;
            _initPanel.style.display = DisplayStyle.None;
            RefreshFileTree();

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
                var newSearchText = _searchField.value;
                if (_currentSearchText != newSearchText)
                {
                    _currentSearchText = newSearchText;
                    m_searchFolderTree();
                }

                // 阻止事件继续传播
                evt.StopPropagation();
                evt.PreventDefault();
            }
            // 可选：按ESC键清空搜索
            else if (evt.keyCode == KeyCode.Escape)
            {
                _searchField.value = "";
                _currentSearchText = "";
                m_searchFolderTree();

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
        private void m_selectTreeViewItem(TreeViewItemData<AssetInfoRecord> item, bool expandAll = false)
        {
            if (expandAll)
                _folderTreeView.ExpandAll();
            else
                _folderTreeView.ExpandItem(item.id, true);
            var parentItem = item;
            var childItem = parentItem.children.FirstOrDefault();
            while (childItem.data != null)
            {
                parentItem = childItem;
                childItem = childItem.children.FirstOrDefault();
            }
            _folderTreeView.SetSelectionById(parentItem.id);
            _folderTreeView.ScrollToItemById(parentItem.id);
        }

        #endregion

        #region 文件树

        //生成文件树
        private VisualElement MakeTreeItem()
        {
            return new FolderTreeItemView(this);
        }
        //绑定文件树
        private void BindTreeItem(VisualElement element, int index)
        {
            var record = _folderTreeView.GetItemDataForIndex<AssetInfoRecord>(index);
            if (record == null) return;
            var treeItemView = element as FolderTreeItemView;
            if (treeItemView == null) return;
            treeItemView.Refresh(record);
        }

        private void RefreshFileTree()
        {
            _allItems.Clear();
            nextId = 0;

            // 扫描Assets文件夹
            AssetInfoRecord record = AssetMonitorTools.GetRecordByPath("Assets", false);
            if (record != null)
            {
                var assetsChildren = ScanDirectory(record);
                var assetsItem = new TreeViewItemData<AssetInfoRecord>(nextId++, record, assetsChildren);
                _allItems.Add(assetsItem);
            }

            // 扫描Packages文件夹
            record = AssetMonitorTools.GetRecordByPath("Packages", false);
            if (record != null)
            {
                var assetsChildren = ScanDirectory(record);
                var assetsItem = new TreeViewItemData<AssetInfoRecord>(nextId++, record, assetsChildren);
                _allItems.Add(assetsItem);
            }

            // 设置TreeView的数据源
            _folderTreeView.SetRootItems(_allItems);
            _folderTreeView.Rebuild();

        }

        private List<TreeViewItemData<AssetInfoRecord>> ScanDirectory(AssetInfoRecord record)
        {
            var children = new List<TreeViewItemData<AssetInfoRecord>>();
            try
            {
                // 获取所有子目录
                foreach (var child in record.Childs)
                {
                    var dirChildren = ScanDirectory(child);
                    var dirTreeItem = new TreeViewItemData<AssetInfoRecord>(nextId++, child, dirChildren);
                    children.Add(dirTreeItem);
                }
                children.Sort((a, b) => b.data.Kind.CompareTo(a.data.Kind));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"扫描目录时出错 {record.ParentPath}: {e.Message}");
            }
            return children;
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                if (item is AssetInfoRecord treeData)
                {
                    //TODO 选中文件时的逻辑
                    Debug.Log($"选中: {treeData.FilePath} ({(treeData.IsDirectory ? "文件夹" : "文件")})");
                    if (!treeData.IsDirectory)
                    {
                        UpdateReferenceTreeView(treeData.FilePath);
                        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(treeData.FilePath);
                        if (asset != null)
                        {
                            Selection.activeObject = asset;
                            EditorGUIUtility.PingObject(asset);
                        }
                    }
                    else
                    {
                        // 如果选中的是文件夹，清空引用关系显示
                        UpdateReferenceTreeView("");
                    }
                }
            }
        }

        #endregion

        #region 引用关系TreeView

        private void InitReferenceTreeView(VisualElement container)
        {
            // 创建标题标签
            _refTitleLabel = new Label("引用关系");
            _refTitleLabel.AddToClassList(USS_PROJECT_REFERENCE_TITLE_CLASS);
            //_referenceTypeLabel.style.fontSize = 14;
            //_referenceTypeLabel.style.paddingTop = 5;
            //_referenceTypeLabel.style.paddingBottom = 5;
            //_referenceTypeLabel.style.paddingLeft = 5;
            //_referenceTypeLabel.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            container.Add(_refTitleLabel); 

            // 创建引用TreeView
            _referenceTreeView = new TreeView();
            _referenceTreeView.AddToClassList(USS_PROJECT_FLODER_CLASS);
            _referenceTreeView.fixedItemHeight = 20;

            // 设置引用TreeView的回调函数
            _referenceTreeView.makeItem = MakeReferenceItem;
            _referenceTreeView.bindItem = BindReferenceItem;
            _referenceTreeView.selectionChanged += OnReferenceSelectionChanged;

            container.Add(_referenceTreeView);
        }

        private VisualElement MakeReferenceItem()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;

            // 图标
            var icon = new Image();
            icon.name = "icon";
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginRight = 5;
            icon.scaleMode = ScaleMode.ScaleToFit;
            container.Add(icon);

            // 文件名标签
            var nameLabel = new Label();
            nameLabel.name = "name-label";
            nameLabel.style.flexGrow = 1;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            container.Add(nameLabel);

            // 类型标签
            var typeLabel = new Label();
            typeLabel.name = "type-label";
            typeLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            typeLabel.style.fontSize = 10;
            typeLabel.style.marginLeft = 5;
            container.Add(typeLabel);

            return new ReferenceTreeItemView(this);
        }

        private void BindReferenceItem(VisualElement element, int index)
        {
            var relationData = _referenceTreeView.GetItemDataForIndex<RelationInfo>(index);
            if (relationData == null) return;
            var treeItemView = element as ReferenceTreeItemView;
            if (treeItemView == null) return;
            treeItemView.Refresh(relationData);

            //var icon = element.Q<Image>("icon");
            //var nameLabel = element.Q<Label>("name-label");
            //var typeLabel = element.Q<Label>("type-label");

            //// 设置图标
            //icon.image = AssetMonitorTools.GetIconByGuid(relationData.Guid);  //GetReferenceTypeIcon(relationData);
            //// 设置名称
            //nameLabel.text = AssetDatabase.GUIDToAssetPath(relationData.Guid);
            //// 设置类型
            //typeLabel.text = AssetDatabase.GetMainAssetTypeAtPath(nameLabel.text)?.Name;// relationData.Guid;

        }


        private void OnReferenceSelectionChanged(IEnumerable<object> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                if (item is RelationInfo relationData && !string.IsNullOrEmpty(relationData.Guid))
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

        private void UpdateReferenceTreeView(string selectedAssetPath)
        {
            _relationItems.Clear();
            _relationNextId = 0;

            if (string.IsNullOrEmpty(selectedAssetPath))
            {
                _referenceTreeView.SetRootItems(_relationItems);
                _referenceTreeView.Rebuild();
                return;
            }

            var record = AssetMonitorTools.GetRecordByPath(AssetMonitorTools.FormatPath(selectedAssetPath), false);
            if (record == null)
            {
                _referenceTreeView.SetRootItems(_relationItems);
                _referenceTreeView.Rebuild();
                return;
            }

            // 添加引用此资源的其他资源
            if (record.ReferenceRelations.Count > 0)
            {
                //var referencedByData = new ReferenceData("被引用", "", "分组");
                var relationByData = new RelationInfo();
                relationByData.Guid = record.Guid;
                var relationByChildren = new List<TreeViewItemData<RelationInfo>>();

                foreach (var relation in record.ReferenceRelations)
                {
                    //var refData = new ReferenceData(
                    //    Path.GetFileName(reference.FromAssetPath) ?? "未知",
                    //    reference.FromAssetPath,
                    //    "引用"
                    //);
                    var refItem = new TreeViewItemData<RelationInfo>(_relationNextId++, relation);
                    relationByChildren.Add(refItem);
                }

                var referencedByItem = new TreeViewItemData<RelationInfo>(_relationNextId++, relationByData, relationByChildren);
                _relationItems.Add(referencedByItem);
            }

            // 更新TreeView
            _referenceTreeView.SetRootItems(_relationItems);
            _referenceTreeView.Rebuild();

            // 展开所有节点
            _referenceTreeView.ExpandAll();
        }

        #endregion


    }
}
