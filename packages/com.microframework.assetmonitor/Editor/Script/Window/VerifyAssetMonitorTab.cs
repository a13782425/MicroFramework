using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    internal class VerifyAssetMonitorTab : BaseAssetMonitorTab
    {
        protected internal override string title => "验证结果";

        protected internal override string icon => "VisibilityOn";

        private ScrollView _scrollView;

        private List<VerifyResultItem> _verifyResultItems = new List<VerifyResultItem>();
        public override void Init(AssetMonitorWindow window)
        {
            base.Init(window);

            this.container.AddToClassList(USS_VERIFY_CLASS);

            _scrollView = new ScrollView();
            _scrollView.style.flexGrow = 1;
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            this.Add(_scrollView);

            foreach (var item in AssetMonitorConfig.Instance.VerifierInfoDict)
            {
                VerifyResultItem veriftItem = new VerifyResultItem(this, item.Value);
                _verifyResultItems.Add(veriftItem);
                _scrollView.Add(veriftItem.View);
            }
        }
        public override void Show()
        {
            foreach (var item in _verifyResultItems)
            {
                item.Show();
            }
        }

        private class VerifyResultItem
        {
            public VisualElement View => _collapsibleBox;
            private CollapsibleBox _collapsibleBox;
            private ListView _listView;
            private readonly VerifierInfo _verifierInfo;
            private readonly VerifyAssetMonitorTab _tab;
            public VerifierInfo Info => _verifierInfo;

            private List<VerifyResult> _verifyResults = new List<VerifyResult>();

            public VerifyResultItem(VerifyAssetMonitorTab tab, VerifierInfo verifierInfo)
            {
                _tab = tab;
                _verifierInfo = verifierInfo;
                _collapsibleBox = new CollapsibleBox(verifierInfo.GetDisplayName(), false);
                _collapsibleBox.AddToClassList(USS_VERIFY_RESULT_CLASS);
                _listView = new ListView();
                _listView.itemsSource = _verifyResults;
                _listView.showAddRemoveFooter = false;
                _listView.showFoldoutHeader = false;
                _listView.showBoundCollectionSize = false;
                _listView.selectionType = SelectionType.Single;
                _listView.makeItem = m_makeItem;
                _listView.bindItem = m_bindItem;
#if UNITY_2022_1_OR_NEWER
                _listView.selectionChanged += m_onSelectionChange;
#else
                _listView.onSelectionChange += m_onSelectionChange;
#endif
                _collapsibleBox.AddElement(_listView);
            }

            private void m_onSelectionChange(IEnumerable<object> obj)
            {
                if (!AssetMonitorConfig.Instance.SelectInProject)
                    return;
                foreach (var item in obj)
                {
                    if (item is not VerifyResult result)
                        continue;
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(result.Guid));
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }

            private void m_bindItem(VisualElement element, int index)
            {
                var itemView = element as VerifyTreeItemView;
                if (itemView == null || index >= _verifyResults.Count)
                    return;
                itemView.RefreshItem(_verifyResults[index]);
            }

            private VisualElement m_makeItem()
            {
                return new VerifyTreeItemView(this._tab);
            }

            internal void Show()
            {
                _verifyResults.Clear();
                string typeName = _verifierInfo.TypeName;
                foreach (var item in AssetMonitorConfig.Instance.GuidAssetRecordDict)
                {
                    var res = item.Value.VerifyResults.FirstOrDefault(a => a.TypeName == typeName);
                    if (res == null)
                        continue;
                    _verifyResults.Add(res);
                }
                _listView.Rebuild();
                var label = _listView.Q<Label>(className: "unity-list-view__empty-label");
                if (label != null)
                    label.text = "当前验证没有匹配到对象";
            }
        }

        private class VerifyTreeItemView : VisualElement
        {
            //图标
            private Image _iconImage;
            //文件夹名称
            private Label _nameLabel;
            //文件信息标签（大小）
            private Label _typeLabel;
            //引用按钮
            private Button _refButton;
            //依赖按钮
            private Button _depButton;
            private List<CommandInfo> _commands = new List<CommandInfo>();
            private AssetInfoRecord _record;
            private readonly VerifyAssetMonitorTab _tab;
            public VerifyTreeItemView(VerifyAssetMonitorTab tab)
            {
                _tab = tab;
                this.AddToClassList(USS_RELATION_FLODER_ITEM_CLASS);

                _iconImage = new Image();
                _iconImage.name = "icon";
                _iconImage.AddToClassList(USS_RELATION_FLODER_ITEM_ICON_CLASS);
                _iconImage.scaleMode = ScaleMode.ScaleToFit;
                this.Add(_iconImage);

                _nameLabel = new Label();
                _nameLabel.name = "name-label";
                _nameLabel.AddToClassList(USS_RELATION_FLODER_ITEM_NAME_CLASS);
                this.Add(_nameLabel);

                _typeLabel = new Label();
                _typeLabel.name = "size-label";
                _typeLabel.AddToClassList(USS_RELATION_FLODER_ITEM_SIZE_CLASS);
                this.Add(_typeLabel);


                _refButton = new Button(m_onRefClick);
                _refButton.name = "ref-btn";
                _refButton.AddToClassList(USS_RELATION_FLODER_ITEM_REF_BTN_CLASS);
                this.Add(_refButton);

                _depButton = new Button(m_onDepClick);
                _depButton.name = "dep-btn";
                _depButton.AddToClassList(USS_RELATION_FLODER_ITEM_DEP_BTN_CLASS);
                this.Add(_depButton);
                this.AddManipulator(new ContextualMenuManipulator(m_buildContextMenu));
            }

            private void m_buildContextMenu(ContextualMenuPopulateEvent evt)
            {
                if (_record == null)
                    return;
                _commands.Clear();
                foreach (var item in AssetMonitorConfig.Instance.CommandInfoDict)
                {
                    if (!item.Value.IsEnabled)
                        continue;
                    if (item.Value.Command.OnFilter(this._record.Guid, CommandType.Verify))
                        _commands.Add(item.Value);
                }
                _commands.Sort((a, b) => a.Command.Priority.CompareTo(b.Command.Priority));
                foreach (var item in _commands)
                {
                    evt.menu.AppendAction(item.Command.Name, m_onMenuClick, DropdownMenuAction.AlwaysEnabled, item);
                }
            }
            private void m_onMenuClick(DropdownMenuAction action)
            {
                CommandInfo info = action.userData as CommandInfo;
                if (info == null || _record == null)
                    return;
                info.Command.OnExecute(_record.Guid, CommandType.Verify);
            }
            private void m_onRefClick()
            {
                if (_record == null) return;
                _tab.window.SelectAsset(_record.Guid, true);
            }

            private void m_onDepClick()
            {
                if (_record == null) return;
                _tab.window.SelectAsset(_record.Guid, false);
            }

            internal void RefreshItem(object data)
            {
                switch (data)
                {
                    case string str:
                        m_refresh(str);
                        break;
                    case VerifyResult result:
                        m_refresh(result);
                        break;
                    default:
                        break;
                }
            }

            private void m_refresh(VerifyResult result)
            {
                _record = AssetMonitorTools.GetRecordByGuid(result.Guid);
                if (_record == null)
                {
                    //missing了
                    this.AddToClassList(USS_RELATION_REFERENCE_TREE_MISSING_CLASS);
                    _typeLabel.style.display = DisplayStyle.Flex;
                    _typeLabel.text = "Missing";
                    _nameLabel.text = result.Guid;
                    return;
                }
                this.RemoveFromClassList(USS_RELATION_REFERENCE_TREE_MISSING_CLASS);
                _refButton.style.display = DisplayStyle.Flex;
                _depButton.style.display = DisplayStyle.Flex;
                _iconImage.style.display = DisplayStyle.Flex;
                _typeLabel.style.display = DisplayStyle.Flex;
                _refButton.text = AssetMonitorTools.FormatRefCount(_record.ReferenceRelations.Count);
                _depButton.text = AssetMonitorTools.FormatRefCount(_record.DependencyRelations.Count);
                // 设置图标
                _iconImage.image = AssetMonitorTools.GetIconByAssetPath(_record.AssetPath);
                // _record
                _nameLabel.text = $"{(result.IsValid ? "<color=#00ff00>[成功]</color>" : "<color=#ff0000>[失败]</color>")} {_record.AssetPath}";

                _typeLabel.text = _record.AssetType;
            }

            private void m_refresh(string str)
            {
                _record = null;
                _refButton.style.display = DisplayStyle.None;
                _depButton.style.display = DisplayStyle.None;
                _iconImage.style.display = DisplayStyle.None;
                _typeLabel.style.display = DisplayStyle.None;
                _nameLabel.text = str;
            }

        }
    }
}
