using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    public class MTreeView : VisualElement
    {
        private struct TreeViewItemWrapper
        {
            public int depth;

            public ITreeViewItem item;

            public bool hasChildren => item.hasChildren;
        }
        private const string TREEVIEW_STYLE = "UIToolkit\\Uss\\MTreeView";

        private const string TOGGLE_NAME = "tree-view-item-toggle";
        private const string ITEM_NAME = "tree-view-item";
        private const string ITEM_PARENT_NAME = "tree-view-item-parent";
        private const string ITEM_INDENT_NAME = "tree-view-item-indent";
        private const string ITEM_INDENT_CONTENT_NAME = "tree-view-item-indent-content";
        private ListView _listView;
        //private ScrollView _listViewScroll;

        private List<TreeViewItemWrapper> _itemWrappers = new List<TreeViewItemWrapper>();
        private List<ITreeViewItem> _items = new List<ITreeViewItem>();
        public List<ITreeViewItem> items => _items;
        public event Action<VisualElement, ITreeViewItem> onBindItem;
        public event Func<VisualElement> onMakeItem;

        public SelectionType selectionType { get => _listView.selectionType; set => _listView.selectionType = value; }
        public int selectedIndex
        {
            get => _listView.selectedIndex;
            set
            {
                _listView.selectedIndex = value;
                if (value >= 0 && value < items.Count)
                {
                    items[value].OnClick();
                }
            }
        }
        /// <summary>
        /// 打开列表
        /// </summary>
        private HashSet<ITreeViewItem> _expandedItems = new HashSet<ITreeViewItem>();
        public MTreeView()
        {
            this.AddStyleSheet(TREEVIEW_STYLE);
            _listView = new ListView();
            _listView.name = "tree-view_list-view";
            _listView.AddToClassList("tree-view_list-view");
            _listView.itemsSource = _itemWrappers;
            _listView.fixedItemHeight = 24;
            _listView.makeItem = m_makeTreeItem;
            _listView.bindItem = m_bindTreeItem;
#if UNITY_2022_1_OR_NEWER
            _listView.itemsChosen += m_onItemsChosen;
            _listView.selectionChanged += m_onSelectionChange;
#else
            _listView.onItemsChosen += m_onItemsChosen;
            _listView.onSelectionChange += m_onSelectionChange;
#endif
            var listViewScroll = _listView.Q<ScrollView>();

            listViewScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            listViewScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            selectionType = SelectionType.Single;
            this.Add(_listView);
        }

        /// <summary>
        /// 刷新列表
        /// </summary>
        public void Rebuild()
        {
            m_generateWrappers();
            if (_listView != null)
            {
                _listView.Rebuild();
            }
        }
        public void OnItemChosen(ITreeViewItem obj)
        {
            if (obj == null)
                return;
            TreeViewItemWrapper wrapper = _itemWrappers.FirstOrDefault(a => a.item == obj);
            if (wrapper.item != null)
            {
                m_onItemsChosen(new object[] { wrapper });
            }
        }
        public void OnItemSelect(ITreeViewItem obj)
        {
            if (obj == null)
                return;
            int index = -1;
            for (int i = 0, length = _itemWrappers.Count; i < length; i++)
            {
                if (_itemWrappers[i].item == obj)
                {
                    index = i;
                    break;
                }
            }
            _listView.selectedIndex = index;
        }
        /// <summary>
        /// 生成wrappers
        /// </summary>
        private void m_generateWrappers()
        {
            _itemWrappers.Clear();

            m_createWrappers(items, 0, _itemWrappers);

        }
        /// <summary>
        /// 创建wrapper
        /// </summary>
        /// <param name="treeViewItems"></param>
        /// <param name="depth"></param>
        /// <param name="list"></param>
        private void m_createWrappers(List<ITreeViewItem> treeViewItems, int depth, List<TreeViewItemWrapper> list)
        {
            foreach (ITreeViewItem item in treeViewItems)
            {
                TreeViewItemWrapper wrapper = default(TreeViewItemWrapper);
                wrapper.depth = depth;
                wrapper.item = item;
                wrapper.item.index = list.Count;
                list.Add(wrapper);
                if (m_isExpanded(wrapper) && item.hasChildren)
                {
                    m_createWrappers(item.children, depth + 1, list);
                }
            }
        }
        private void m_bindTreeItem(VisualElement element, int index)
        {
            ITreeViewItem item = _itemWrappers[index].item;
            VisualElement visualElement = element.Q(ITEM_INDENT_CONTENT_NAME);
            visualElement.Clear();
            for (int i = 0; i < _itemWrappers[index].depth; i++)
            {
                VisualElement visualElement2 = new VisualElement();
                visualElement2.AddToClassList(ITEM_INDENT_NAME);
                visualElement.Add(visualElement2);
            }
            Toggle toggle = element.Q<Toggle>(TOGGLE_NAME);
            toggle.SetValueWithoutNotify(m_isExpanded(_itemWrappers[index]));
            toggle.userData = index;
            if (item.hasChildren)
            {
                toggle.visible = true;
                element.AddToClassList(ITEM_PARENT_NAME);
            }
            else
            {
                toggle.visible = false;
                if (element.ClassListContains(ITEM_PARENT_NAME))
                {
                    element.RemoveFromClassList(ITEM_PARENT_NAME);
                }
            }
            onBindItem?.Invoke(element, item);
        }
        private VisualElement m_makeTreeItem()
        {
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList(ITEM_NAME);
            visualElement.style.flexDirection = FlexDirection.Row;
            VisualElement indentContent = new VisualElement();
            indentContent.name = ITEM_INDENT_CONTENT_NAME;
            indentContent.style.flexDirection = FlexDirection.Row;
            indentContent.AddToClassList(ITEM_INDENT_CONTENT_NAME);
            visualElement.Add(indentContent);
            Toggle toggle = new Toggle
            {
                name = TOGGLE_NAME
            };
            toggle.AddToClassList(Foldout.toggleUssClassName);
            toggle.RegisterValueChangedCallback(m_toggleExpandedState);
            visualElement.Add(toggle);

            if (onMakeItem != null)
            {
                visualElement.Add(onMakeItem.Invoke());
            }
            return visualElement;
        }
        /// <summary>
        /// 列表选择发生改变(双击)
        /// </summary>
        /// <param name="obj"></param>
        private void m_onItemsChosen(IEnumerable<object> obj)
        {
            obj.OfType<TreeViewItemWrapper>().ToList().ForEach(item =>
            {
                if (item.hasChildren)
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
                item.item.OnClick();
            });
        }
        /// <summary>
        /// 列表选择发生改变
        /// </summary>
        /// <param name="obj"></param>
        private void m_onSelectionChange(IEnumerable<object> obj)
        {
            obj.OfType<TreeViewItemWrapper>().ToList().ForEach(item =>
            {
                item.item.OnSelect();
            });
        }
        /// <summary>
        /// 开关的状态发生改变
        /// </summary>
        /// <param name="evt"></param>
        private void m_toggleExpandedState(ChangeEvent<bool> evt)
        {
            Toggle toggle = evt.target as Toggle;
            int index = (int)toggle.userData;
            var wrapper = _itemWrappers[index];
            bool flag = m_isExpanded(wrapper);
            if (flag)
            {
                m_collapseItem(wrapper);
            }
            else
            {
                m_expandItem(wrapper);
            }
        }

        /// <summary>
        /// 判断一个Item是否是打开的状态
        /// </summary>
        /// <param name="treeItem"></param>
        /// <returns></returns>
        private bool m_isExpanded(TreeViewItemWrapper wrapper)
        {
            return _expandedItems.Contains(wrapper.item);
        }
        /// <summary>
        /// 打开一个Item
        /// </summary>
        /// <param name="wrapper"></param>
        private void m_expandItem(TreeViewItemWrapper wrapper)
        {
            if (wrapper.item.hasChildren)
            {
                int index = _listView.itemsSource.IndexOf(wrapper);
                List<TreeViewItemWrapper> wrappers = new List<TreeViewItemWrapper>();
                m_createWrappers(wrapper.item.children, _itemWrappers[index].depth + 1, wrappers);
                _itemWrappers.InsertRange(index + 1, wrappers);
                _expandedItems.Add(wrapper.item);
                _listView.Rebuild();
            }
        }
        /// <summary>
        /// 折叠一个Item
        /// </summary>
        /// <param name="wrapper"></param>
        private void m_collapseItem(TreeViewItemWrapper wrapper)
        {
            if (wrapper.item.hasChildren)
            {
                int index = _listView.itemsSource.IndexOf(wrapper);
                _expandedItems.Remove(wrapper.item);
                int num = 0;
                int i = index + 1;
                for (int depth = wrapper.depth; i < _itemWrappers.Count && _itemWrappers[i].depth > depth; i++)
                {
                    num++;
                }
                _itemWrappers.RemoveRange(index + 1, num);
                _listView.Rebuild();
            }
        }
    }
}
