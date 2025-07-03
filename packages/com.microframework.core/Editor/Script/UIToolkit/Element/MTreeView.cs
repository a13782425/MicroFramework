using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 树形视图节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MTreeViewItemData
    {
        private readonly object _data;

        private readonly List<MTreeViewItemData> _children;
        public object Data => _data;

        public List<MTreeViewItemData> Children => _children;

        public bool HasChildren => _children != null && _children.Count > 0;

        public T GetData<T>() => _data is T a ? a : default(T);

        /// <summary>
        /// 当前树节点的索引
        /// </summary>
        internal int Index { get; set; }

        internal MTreeViewItemData Parent { get; private set; }

        public MTreeViewItemData(object data, List<MTreeViewItemData> children = null)
        {
            _data = data;
            _children = new List<MTreeViewItemData>();
            if (children != null)
                AddChildren(children);
        }
        public void AddChild(MTreeViewItemData child)
        {
            if (child.Parent != null)
                child.Parent.RemoveChild(child);
            child.Parent = this;
            _children.Add(child);
        }
        public void AddChildren(IList<MTreeViewItemData> children)
        {
            foreach (MTreeViewItemData child in children)
            {
                if (child.Parent != null)
                    child.Parent.RemoveChild(child);
                AddChild(child);
            }
        }

        public void RemoveChild(MTreeViewItemData child)
        {
            if (_children.Remove(child) && child.Parent == this)
                child.Parent = null;
        }
    }

    /// <summary>
    /// 树形视图
    /// </summary>
    public class MTreeView : VisualElement
    {
        private const string STYLE_SHEET = "UIToolkit\\Element\\MTreeView";

        private const string USS_BASE_CLASS = "mtree-view";
        private const string USS_TREE_LISTVIEW_CLASS = USS_BASE_CLASS + "__listview";

        private const string USS_TREE_LISTVIEW_SCROLL_CLASS = USS_TREE_LISTVIEW_CLASS + "-scroll";

        private const string USS_TREE_ITEM_CLASS = USS_BASE_CLASS + "__item";
        private const string USS_TREE_ITEM_CONTAINER_CLASS = USS_TREE_ITEM_CLASS + "-container";


        private ListView _listView;
        private ScrollView _listViewScroll;
        private List<TreeViewItemWrapper> _itemWrappers = new List<TreeViewItemWrapper>();
        private List<MTreeViewItemData> _items = new List<MTreeViewItemData>();
        public List<MTreeViewItemData> RootItems => _items;
        public float FixedItemHeight { get => _listView.fixedItemHeight; set => _listView.fixedItemHeight = value; }
        public SelectionType SelectionType { get => _listView.selectionType; set => _listView.selectionType = value; }
        public int SelectedIndex { get => _listView.selectedIndex; set => _listView.selectedIndex = value; }

        private Action<VisualElement, MTreeViewItemData> _onBindItem;
        public Action<VisualElement, MTreeViewItemData> onBindItem
        {
            get => _onBindItem;
            set
            {
                if (value == null)
                {
                    if (_onBindItem == m_defaultBindTreeItem)
                        return;
                    else
                    {
                        _onBindItem = m_defaultBindTreeItem;
                        Rebuild();
                    }
                }
                else
                {
                    if (_onBindItem != value)
                    {
                        _onBindItem = value;
                        Rebuild();
                    }
                }
            }
        }

        private Func<VisualElement> _onMakeItem;
        public Func<VisualElement> onMakeItem
        {
            get => _onMakeItem;
            set
            {
                if (value == null)
                {
                    if (_onMakeItem == m_defaultMakeTreeItem)
                        return;
                    else
                    {
                        _onMakeItem = m_defaultMakeTreeItem;
                        Rebuild();
                    }
                }
                else
                {
                    if (_onMakeItem != value)
                    {
                        _onMakeItem = value;
                        Rebuild();
                    }
                }
            }
        }

        private Action<VisualElement> _onDestroyItem;
        public Action<VisualElement> onDestroyItem { get => _onDestroyItem; set => _onDestroyItem = value; }

        private Action<VisualElement, MTreeViewItemData> _onUnbindItem;
        public Action<VisualElement, MTreeViewItemData> onUnbindItem { get => _onUnbindItem; set => _onUnbindItem -= value; }

        /// <summary>
        /// 双击树节点
        /// </summary>
        public event Action<IEnumerable<MTreeViewItemData>> onItemsChosen;
        /// <summary>
        /// 选中树节点
        /// </summary>
        public event Action<IEnumerable<MTreeViewItemData>> onSelectionChanged;

        /// <summary>
        /// 打开列表
        /// </summary>
        private HashSet<MTreeViewItemData> _expandedItems = new HashSet<MTreeViewItemData>();

        /// <summary>
        /// 树节点数据对应的索引
        /// </summary>
        private Dictionary<MTreeViewItemData, int> _itemIndexDict = new Dictionary<MTreeViewItemData, int>();

        private bool _expandAll = false;

        public MTreeView()
        {
            this.AddStyleSheet(STYLE_SHEET);
            _listView = new ListView();
            _listView.name = "tree-view_list-view";
            _listView.AddToClassList(USS_TREE_LISTVIEW_CLASS);
            _listView.itemsSource = _itemWrappers;
            _listView.showAddRemoveFooter = false;
            _listView.showBoundCollectionSize = false;
            _listView.showFoldoutHeader = false;
            _listView.reorderable = false;
            _listView.horizontalScrollingEnabled = false;
            _listView.makeItem = m_makeTreeItem;
            _listView.bindItem = m_bindTreeItem;
            _listView.unbindItem = m_unbindTreeItem;
            _listView.destroyItem = m_destroyTreeItem;
#if UNITY_2022_1_OR_NEWER
            _listView.itemsChosen += m_onItemsChosen;
            _listView.selectionChanged += m_onSelectionChange;
#else
            _listView.onItemsChosen += m_onItemsChosen;
            _listView.onSelectionChange += m_onSelectionChange;
#endif
            _listViewScroll = _listView.Q<ScrollView>();
            _listViewScroll.contentViewport.AddToClassList(USS_TREE_LISTVIEW_SCROLL_CLASS);
            _listViewScroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
            _listViewScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _onBindItem = m_defaultBindTreeItem;
            _onMakeItem = m_defaultMakeTreeItem;
            this.Add(_listView);

        }

        /// <summary>
        /// 设置根节点
        /// </summary>
        /// <param name="items"></param>
        public void SetRootItems(IList<MTreeViewItemData> items)
        {
            _items.Clear();
            _items.AddRange(items);
            Rebuild();
        }
        /// <summary>
        /// 展开指定节点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="expandAllChildren"></param>
        internal void ExpandItem(MTreeViewItemData item, bool expandAllChildren = false)
        {
            var parent = item.Parent;
            while (parent != null)
            {
                if (!_expandedItems.Contains(parent))
                    _expandedItems.Add(parent);
                parent = parent.Parent;
            }
            m_expandItem(item, expandAllChildren);
            Rebuild();
        }
        /// <summary>
        /// 展开所有
        /// </summary>
        public void ExpandAll()
        {
            _expandAll = true;
            _expandedItems.Clear();
            Rebuild();
            _expandAll = false;
        }

        /// <summary>
        /// 折叠所有
        /// </summary>
        public void CollapseAll()
        {
            _expandedItems.Clear();
            Rebuild();
        }

        /// <summary>
        /// 折叠所有
        /// </summary>
        public void CollapseItem(MTreeViewItemData item, bool collapseAllChildren = false)
        {
            m_collapseItem(item, collapseAllChildren);
            Rebuild();
        }

        /// <summary>
        /// 设置选中并滚动到节点
        /// <para>只能选中展开的节点</para>
        /// </summary>
        /// <param name="treeItem"></param>
        /// <param name="scrollToSelection"></param>
        public void SetItemChosen(MTreeViewItemData treeItem, bool scrollToSelection = false)
        {
            SetSelection(treeItem, scrollToSelection);
            this.onItemsChosen?.Invoke(new List<MTreeViewItemData> { treeItem });
        }
        /// <summary>
        /// 设置选中并滚动到第一个节点
        /// <para>只能选中展开的节点</para>
        /// </summary>
        /// <param name="treeItems"></param>
        /// <param name="scrollToSelection"></param>
        public void SetItemChosen(List<MTreeViewItemData> treeItems, bool scrollToSelection = false)
        {
            SetSelection(treeItems, scrollToSelection);
            this.onItemsChosen?.Invoke(treeItems.ToList());
        }

        /// <summary>
        /// 设置选中并滚动到节点
        /// <para>只能选中展开的节点</para>
        /// </summary>
        /// <param name="treeItem"></param>
        /// <param name="scrollToSelection"></param>
        public void SetSelection(MTreeViewItemData treeItem, bool scrollToSelection = false)
        {
            _listView.SetSelection(new List<int> { treeItem.Index });
            if (scrollToSelection)
                _listView.ScrollToItem(treeItem.Index);
        }
        /// <summary>
        /// 设置选中并滚动到第一个节点
        /// <para>只能选中展开的节点</para>
        /// </summary>
        /// <param name="treeItems"></param>
        /// <param name="scrollToSelection"></param>
        public void SetSelection(List<MTreeViewItemData> treeItems, bool scrollToSelection = false)
        {
            _listView.SetSelection(treeItems.Select(a => a.Index));
            if (scrollToSelection)
                _listView.ScrollToItem(treeItems.FirstOrDefault().Index);
        }

        /// <summary>
        /// 设置选中且不发送通知
        /// <para>只能选中展开的节点</para>
        /// </summary>
        /// <param name="treeItem"></param>
        /// <param name="scrollToSelection"></param>
        public void SetSelectionWithoutNotify(MTreeViewItemData treeItem, bool scrollToSelection = false)
        {
            _listView.SetSelectionWithoutNotify(new List<int> { treeItem.Index });
            if (scrollToSelection)
                _listView.ScrollToItem(treeItem.Index);
        }
        /// <summary>
        /// 设置选中且不发送通知
        /// <para>只能选中展开的节点</para>
        /// </summary>
        /// <param name="treeItems"></param>
        /// <param name="scrollToSelection"></param>
        public void SetSelectionWithoutNotify(List<MTreeViewItemData> treeItems, bool scrollToSelection = false)
        {
            _listView.SetSelectionWithoutNotify(treeItems.Select(a => a.Index));
            if (scrollToSelection)
                _listView.ScrollToItem(treeItems.FirstOrDefault().Index);
        }
        /// <summary>
        /// 刷新列表
        /// </summary>
        public void Rebuild()
        {
            m_generateWrappers();
            if (_listView != null)
                _listView.Rebuild();
        }

        /// <summary>
        /// 生成wrappers
        /// </summary>
        private void m_generateWrappers()
        {
            _itemWrappers.Clear();
            m_createWrappers(_items, 0, _itemWrappers);

        }
        /// <summary>
        /// 创建wrapper
        /// </summary>
        /// <param name="treeViewItems"></param>
        /// <param name="depth"></param>
        /// <param name="list"></param>
        private void m_createWrappers(List<MTreeViewItemData> treeViewItems, int depth, List<TreeViewItemWrapper> list)
        {
            foreach (MTreeViewItemData item in treeViewItems)
            {
                TreeViewItemWrapper wrapper = default(TreeViewItemWrapper);
                wrapper.Depth = depth;
                wrapper.Item = item;
                item.Index = list.Count;
                list.Add(wrapper);
                if (_expandAll || (m_isExpanded(wrapper) && item.HasChildren))
                {
                    if (!_expandedItems.Contains(item))
                        _expandedItems.Add(item);
                    m_createWrappers(item.Children, depth + 1, list);
                }
            }
        }
        /// <summary>
        /// 判断一个Item是否是打开的状态
        /// </summary>
        /// <param name="treeItem"></param>
        /// <returns></returns>
        private bool m_isExpanded(TreeViewItemWrapper wrapper)
        {
            return _expandedItems.Contains(wrapper.Item);
        }
        /// <summary>
        /// 展开一个Item及其子项
        /// </summary>
        /// <param name="item"></param>
        /// <param name="expandAllChildren"></param>
        private void m_expandItem(MTreeViewItemData item, bool expandAllChildren = false)
        {
            if (!_expandedItems.Contains(item))
                _expandedItems.Add(item);
            if (expandAllChildren)
            {
                foreach (var child in item.Children)
                {
                    m_expandItem(child, expandAllChildren);
                }
            }
        }

        /// <summary>
        /// 打开一个Item
        /// </summary>
        /// <param name="wrapper"></param>
        private void m_expandItem(TreeViewItemWrapper wrapper)
        {
            if (!wrapper.Item.HasChildren)
                return;
            if (_expandedItems.Contains(wrapper.Item))
                return;

            int index = _listView.itemsSource.IndexOf(wrapper);
            List<TreeViewItemWrapper> wrappers = new List<TreeViewItemWrapper>();
            m_createWrappers(wrapper.Item.Children, _itemWrappers[index].Depth + 1, wrappers);
            _itemWrappers.InsertRange(index + 1, wrappers);
            _expandedItems.Add(wrapper.Item);
            _listView.Rebuild();
        }
        private void m_collapseItem(MTreeViewItemData item, bool collapseAllChildren)
        {
            if (_expandedItems.Contains(item))
                _expandedItems.Remove(item);

            if (collapseAllChildren)
            {
                foreach (var child in item.Children)
                {
                    m_collapseItem(child, collapseAllChildren);
                }
            }
        }
        /// <summary>
        /// 折叠一个Item
        /// </summary>
        /// <param name="wrapper"></param>
        private void m_collapseItem(TreeViewItemWrapper wrapper)
        {
            if (!wrapper.Item.HasChildren)
                return;
            if (!_expandedItems.Contains(wrapper.Item))
                return;
            int index = _listView.itemsSource.IndexOf(wrapper);
            _expandedItems.Remove(wrapper.Item);
            int num = 0;
            int i = index + 1;
            for (int depth = wrapper.Depth; i < _itemWrappers.Count && _itemWrappers[i].Depth > depth; i++)
                num++;
            _itemWrappers.RemoveRange(index + 1, num);
            _listView.Rebuild();
        }
        private VisualElement m_makeTreeItem()
        {
            TreeItemView itemView = new TreeItemView(this);
            itemView.MakeItem();
            return itemView;
        }
        private void m_bindTreeItem(VisualElement element, int index)
        {
            TreeItemView itemView = element as TreeItemView;
            if (itemView == null)
                return;
            TreeViewItemWrapper itemWrapper = _itemWrappers[index];
            itemView.BindItem(itemWrapper);
        }
        private void m_unbindTreeItem(VisualElement element, int index)
        {
            TreeItemView itemView = element as TreeItemView;
            if (itemView == null)
                return;
            itemView.UnbindItem();
        }
        private void m_destroyTreeItem(VisualElement element)
        {
            TreeItemView itemView = element as TreeItemView;
            if (itemView == null)
                return;
            itemView.DestroyItem();
        }
        private VisualElement m_defaultMakeTreeItem()
        {
            return new Label();
        }
        private void m_defaultBindTreeItem(VisualElement element, object item)
        {
            if (element is Label label)
                label.text = item?.GetType().FullName;
        }

        /// <summary>
        /// 列表选择发生改变(双击)
        /// </summary>
        /// <param name="enumerable"></param>
        private void m_onItemsChosen(IEnumerable<object> enumerable)
        {

            IEnumerable<TreeViewItemWrapper> wrapper = enumerable.OfType<TreeViewItemWrapper>();
            IEnumerable<MTreeViewItemData> items = wrapper.Select(a => a.Item);

            wrapper.ToList().ForEach(item =>
            {
                if (item.HasChildren)
                {
                    if (!m_isExpanded(item))
                    {
                        m_expandItem(item);
                    }
                    else
                    {
                        m_collapseItem(item);
                    }
                }
            });

            onItemsChosen?.Invoke(items);
        }
        private void m_onSelectionChange(IEnumerable<object> enumerable)
        {
            onSelectionChanged?.Invoke(enumerable.OfType<TreeViewItemWrapper>().Select(a => a.Item));
        }

        private class TreeItemView : VisualElement
        {
            private VisualElement _indentVisualElement;

            private Toggle _collapseToggle;

            private MTreeView _treeView;
            private VisualElement _container;

            private VisualElement _customElement;

            private TreeViewItemWrapper _itemWrapper;

            public TreeItemView(MTreeView treeView)
            {
                _treeView = treeView;
                this.AddToClassList(USS_TREE_ITEM_CLASS);
                //visualElement.style.flexDirection = FlexDirection.Row;
                _indentVisualElement = new VisualElement();
                this.Add(_indentVisualElement);
                _collapseToggle = new Toggle();
                _collapseToggle.AddToClassList(Foldout.toggleUssClassName);
                _collapseToggle.RegisterValueChangedCallback(m_toggleExpandedState);
                this.Add(_collapseToggle);
                _container = new VisualElement();
                _container.AddToClassList(USS_TREE_ITEM_CONTAINER_CLASS);
                this.Add(_container);
            }

            public void AddElement(VisualElement child)
            {
                _container.Add(child);
            }

            private void m_toggleExpandedState(ChangeEvent<bool> evt)
            {
                if (_itemWrapper.Item == null)
                    return;
                bool flag = _treeView.m_isExpanded(_itemWrapper);
                if (flag)
                    _treeView.m_collapseItem(_itemWrapper);
                else
                    _treeView.m_expandItem(_itemWrapper);
            }

            //刷新Item
            internal void MakeItem()
            {
                if (_treeView._onMakeItem != null)
                {
                    _customElement = _treeView._onMakeItem.Invoke();
                    AddElement(_customElement);
                }
            }

            //刷新Item
            internal void BindItem(TreeViewItemWrapper itemWrapper)
            {
                _itemWrapper = itemWrapper;
                MTreeViewItemData item = itemWrapper.Item;
                _indentVisualElement.style.width = itemWrapper.Depth * 16;
                _collapseToggle.SetValueWithoutNotify(_treeView.m_isExpanded(itemWrapper));
                _collapseToggle.userData = itemWrapper;
                _collapseToggle.visible = item.HasChildren;
                _treeView._onBindItem?.Invoke(_customElement, item);
            }

            internal void UnbindItem()
            {
                MTreeViewItemData item = _itemWrapper.Item;
                _itemWrapper = TreeViewItemWrapper.Empty;
                if (item != null)
                    _treeView._onUnbindItem?.Invoke(_customElement, item);
            }

            internal void DestroyItem()
            {
                _treeView._onDestroyItem?.Invoke(_customElement);
                _customElement.RemoveFromHierarchy();
            }
        }
        private struct TreeViewItemWrapper
        {
            public int Depth;

            public MTreeViewItemData Item;

            //public int index;

            public bool HasChildren => Item.HasChildren;

            public static TreeViewItemWrapper Empty;
        }
    }
    //    public class MTreeView : VisualElement
    //    {
    //        private struct TreeViewItemWrapper
    //        {
    //            public int depth;

    //            public ITreeViewItem item;

    //            public bool hasChildren => item.hasChildren;
    //        }
    //        private const string TREEVIEW_STYLE = "UIToolkit\\Uss\\MTreeView";

    //        private const string TOGGLE_NAME = "tree-view-item-toggle";
    //        private const string ITEM_NAME = "tree-view-item";
    //        private const string ITEM_PARENT_NAME = "tree-view-item-parent";
    //        private const string ITEM_INDENT_NAME = "tree-view-item-indent";
    //        private const string ITEM_INDENT_CONTENT_NAME = "tree-view-item-indent-content";
    //        private ListView _listView;
    //        //private ScrollView _listViewScroll;

    //        private List<TreeViewItemWrapper> _itemWrappers = new List<TreeViewItemWrapper>();
    //        private List<ITreeViewItem> _items = new List<ITreeViewItem>();
    //        public List<ITreeViewItem> items => _items;
    //        public event Action<VisualElement, ITreeViewItem> onBindItem;
    //        public event Func<VisualElement> onMakeItem;

    //        public SelectionType selectionType { get => _listView.selectionType; set => _listView.selectionType = value; }
    //        public int selectedIndex
    //        {
    //            get => _listView.selectedIndex;
    //            set
    //            {
    //                _listView.selectedIndex = value;
    //                if (value >= 0 && value < items.Count)
    //                {
    //                    items[value].OnClick();
    //                }
    //            }
    //        }
    //        /// <summary>
    //        /// 打开列表
    //        /// </summary>
    //        private HashSet<ITreeViewItem> _expandedItems = new HashSet<ITreeViewItem>();
    //        public MTreeView()
    //        {
    //            this.AddStyleSheet(TREEVIEW_STYLE);
    //            _listView = new ListView();
    //            _listView.name = "tree-view_list-view";
    //            _listView.AddToClassList("tree-view_list-view");
    //            _listView.itemsSource = _itemWrappers;
    //            _listView.fixedItemHeight = 24;
    //            _listView.makeItem = m_makeTreeItem;
    //            _listView.bindItem = m_bindTreeItem;
    //#if UNITY_2022_1_OR_NEWER
    //            _listView.itemsChosen += m_onItemsChosen;
    //            _listView.selectionChanged += m_onSelectionChange;
    //#else
    //            _listView.onItemsChosen += m_onItemsChosen;
    //            _listView.onSelectionChange += m_onSelectionChange;
    //#endif
    //            var listViewScroll = _listView.Q<ScrollView>();

    //            listViewScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
    //            listViewScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
    //            selectionType = SelectionType.Single;
    //            this.Add(_listView);
    //        }

    //        /// <summary>
    //        /// 刷新列表
    //        /// </summary>
    //        public void Rebuild()
    //        {
    //            m_generateWrappers();
    //            if (_listView != null)
    //            {
    //                _listView.Rebuild();
    //            }
    //        }
    //        public void OnItemChosen(ITreeViewItem obj)
    //        {
    //            if (obj == null)
    //                return;
    //            TreeViewItemWrapper wrapper = _itemWrappers.FirstOrDefault(a => a.item == obj);
    //            if (wrapper.item != null)
    //            {
    //                m_onItemsChosen(new object[] { wrapper });
    //            }
    //        }
    //        public void OnItemSelect(ITreeViewItem obj)
    //        {
    //            if (obj == null)
    //                return;
    //            int index = -1;
    //            for (int i = 0, length = _itemWrappers.Count; i < length; i++)
    //            {
    //                if (_itemWrappers[i].item == obj)
    //                {
    //                    index = i;
    //                    break;
    //                }
    //            }
    //            _listView.selectedIndex = index;
    //        }
    //        /// <summary>
    //        /// 生成wrappers
    //        /// </summary>
    //        private void m_generateWrappers()
    //        {
    //            _itemWrappers.Clear();

    //            m_createWrappers(items, 0, _itemWrappers);

    //        }
    //        /// <summary>
    //        /// 创建wrapper
    //        /// </summary>
    //        /// <param name="treeViewItems"></param>
    //        /// <param name="depth"></param>
    //        /// <param name="list"></param>
    //        private void m_createWrappers(List<ITreeViewItem> treeViewItems, int depth, List<TreeViewItemWrapper> list)
    //        {
    //            foreach (ITreeViewItem item in treeViewItems)
    //            {
    //                TreeViewItemWrapper wrapper = default(TreeViewItemWrapper);
    //                wrapper.depth = depth;
    //                wrapper.item = item;
    //                wrapper.item.index = list.Count;
    //                list.Add(wrapper);
    //                if (m_isExpanded(wrapper) && item.hasChildren)
    //                {
    //                    m_createWrappers(item.children, depth + 1, list);
    //                }
    //            }
    //        }
    //        private void m_bindTreeItem(VisualElement element, int index)
    //        {
    //            ITreeViewItem item = _itemWrappers[index].item;
    //            VisualElement visualElement = element.Q(ITEM_INDENT_CONTENT_NAME);
    //            visualElement.Clear();
    //            for (int i = 0; i < _itemWrappers[index].depth; i++)
    //            {
    //                VisualElement visualElement2 = new VisualElement();
    //                visualElement2.AddToClassList(ITEM_INDENT_NAME);
    //                visualElement.Add(visualElement2);
    //            }
    //            element.tooltip = item.tooltip;
    //            Toggle toggle = element.Q<Toggle>(TOGGLE_NAME);
    //            toggle.SetValueWithoutNotify(m_isExpanded(_itemWrappers[index]));
    //            toggle.userData = index;
    //            if (item.hasChildren)
    //            {
    //                toggle.visible = true;
    //                element.AddToClassList(ITEM_PARENT_NAME);
    //            }
    //            else
    //            {
    //                toggle.visible = false;
    //                if (element.ClassListContains(ITEM_PARENT_NAME))
    //                {
    //                    element.RemoveFromClassList(ITEM_PARENT_NAME);
    //                }
    //            }
    //            onBindItem?.Invoke(element, item);
    //        }
    //        private VisualElement m_makeTreeItem()
    //        {
    //            VisualElement visualElement = new VisualElement();
    //            visualElement.AddToClassList(ITEM_NAME);
    //            visualElement.style.flexDirection = FlexDirection.Row;
    //            VisualElement indentContent = new VisualElement();
    //            indentContent.name = ITEM_INDENT_CONTENT_NAME;
    //            indentContent.style.flexDirection = FlexDirection.Row;
    //            indentContent.AddToClassList(ITEM_INDENT_CONTENT_NAME);
    //            visualElement.Add(indentContent);
    //            Toggle toggle = new Toggle
    //            {
    //                name = TOGGLE_NAME
    //            };
    //            toggle.AddToClassList(Foldout.toggleUssClassName);
    //            toggle.RegisterValueChangedCallback(m_toggleExpandedState);
    //            visualElement.Add(toggle);

    //            if (onMakeItem != null)
    //            {
    //                visualElement.Add(onMakeItem.Invoke());
    //            }
    //            return visualElement;
    //        }
    //        /// <summary>
    //        /// 列表选择发生改变(双击)
    //        /// </summary>
    //        /// <param name="obj"></param>
    //        private void m_onItemsChosen(IEnumerable<object> obj)
    //        {
    //            obj.OfType<TreeViewItemWrapper>().ToList().ForEach(item =>
    //            {
    //                if (item.hasChildren)
    //                {
    //                    if (!m_isExpanded(item))
    //                    {
    //                        m_expandItem(item);
    //                    }
    //                    else
    //                    {
    //                        m_collapseItem(item);
    //                    }
    //                }
    //                item.item.OnClick();
    //            });
    //        }
    //        /// <summary>
    //        /// 列表选择发生改变
    //        /// </summary>
    //        /// <param name="obj"></param>
    //        private void m_onSelectionChange(IEnumerable<object> obj)
    //        {
    //            obj.OfType<TreeViewItemWrapper>().ToList().ForEach(item =>
    //            {
    //                item.item.OnSelect();
    //            });
    //        }
    //        /// <summary>
    //        /// 开关的状态发生改变
    //        /// </summary>
    //        /// <param name="evt"></param>
    //        private void m_toggleExpandedState(ChangeEvent<bool> evt)
    //        {
    //            Toggle toggle = evt.target as Toggle;
    //            int index = (int)toggle.userData;
    //            var wrapper = _itemWrappers[index];
    //            bool flag = m_isExpanded(wrapper);
    //            if (flag)
    //            {
    //                m_collapseItem(wrapper);
    //            }
    //            else
    //            {
    //                m_expandItem(wrapper);
    //            }
    //        }

    //        /// <summary>
    //        /// 判断一个Item是否是打开的状态
    //        /// </summary>
    //        /// <param name="treeItem"></param>
    //        /// <returns></returns>
    //        private bool m_isExpanded(TreeViewItemWrapper wrapper)
    //        {
    //            return _expandedItems.Contains(wrapper.item);
    //        }
    //        /// <summary>
    //        /// 打开一个Item
    //        /// </summary>
    //        /// <param name="wrapper"></param>
    //        private void m_expandItem(TreeViewItemWrapper wrapper)
    //        {
    //            if (wrapper.item.hasChildren)
    //            {
    //                int index = _listView.itemsSource.IndexOf(wrapper);
    //                List<TreeViewItemWrapper> wrappers = new List<TreeViewItemWrapper>();
    //                m_createWrappers(wrapper.item.children, _itemWrappers[index].depth + 1, wrappers);
    //                _itemWrappers.InsertRange(index + 1, wrappers);
    //                _expandedItems.Add(wrapper.item);
    //                _listView.Rebuild();
    //            }
    //        }
    //        /// <summary>
    //        /// 折叠一个Item
    //        /// </summary>
    //        /// <param name="wrapper"></param>
    //        private void m_collapseItem(TreeViewItemWrapper wrapper)
    //        {
    //            if (wrapper.item.hasChildren)
    //            {
    //                int index = _listView.itemsSource.IndexOf(wrapper);
    //                _expandedItems.Remove(wrapper.item);
    //                int num = 0;
    //                int i = index + 1;
    //                for (int depth = wrapper.depth; i < _itemWrappers.Count && _itemWrappers[i].depth > depth; i++)
    //                {
    //                    num++;
    //                }
    //                _itemWrappers.RemoveRange(index + 1, num);
    //                _listView.Rebuild();
    //            }
    //        }
    //    }
}
