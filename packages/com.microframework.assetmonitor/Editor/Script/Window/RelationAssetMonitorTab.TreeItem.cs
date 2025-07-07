using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    partial class RelationAssetMonitorTab
    {

        /// <summary>
        /// 项目树节点视图
        /// </summary>
        public class FolderTreeItemView : VisualElement
        {
            //图标
            private Image _iconImage;
            //文件夹名称
            private Label _nameLabel;
            //文件信息标签（大小）
            private Label _sizeLabel;
            //文件数量标签（仅文件夹显示）
            private Label _countLabel;
            //引用按钮
            private Button _refButton;
            //依赖按钮
            private Button _depButton;

            private AssetInfoRecord _record;
            private MTreeItemData _treeItemData;
            private List<CommandInfo> _commands = new List<CommandInfo>();
            private readonly RelationAssetMonitorTab _projectAssetMonitorTab;
            public FolderTreeItemView(RelationAssetMonitorTab tab)
            {
                _projectAssetMonitorTab = tab;
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

                _sizeLabel = new Label();
                _sizeLabel.name = "size-label";
                _sizeLabel.AddToClassList(USS_RELATION_FLODER_ITEM_SIZE_CLASS);
                this.Add(_sizeLabel);

                _countLabel = new Label();
                _countLabel.name = "count-label";
                _countLabel.AddToClassList(USS_RELATION_FLODER_ITEM_COUNT_CLASS);
                this.Add(_countLabel);

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

            internal void Refresh(MTreeItemData item)
            {
                _treeItemData = item;
                _record = item.GetData<AssetInfoRecord>();
                if (_record == null)
                    return;
                _refButton.text = AssetMonitorTools.FormatRefCount(_record.ReferenceRelations.Count);
                _depButton.text = AssetMonitorTools.FormatRefCount(_record.DependencyRelations.Count);
                // 设置图标
                _iconImage.image = AssetMonitorTools.GetIconByAssetPath(_record.AssetPath);

                // _record
                _nameLabel.text = _record.AssetName;

                _sizeLabel.text = AssetMonitorTools.FormatSize(_record.Size);

                if (_record.IsFolder)
                    _countLabel.text = $"({_record.Count} 项)";
                else
                    _countLabel.text = "";
            }
            private void m_onRefClick()
            {
                if (_record == null) return;
                _projectAssetMonitorTab?.ShowReferenceInfoByGuid(_record.Guid);
            }

            private void m_onDepClick()
            {
                if (_record == null) return;
                _projectAssetMonitorTab?.ShowReferenceInfoByGuid(_record.Guid, false);
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
                    if (item.Value.Command.OnFilter(this._record, CommandType.Folder))
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
                var clone = m_cloneAssetTree(this._record, null, _treeItemData);
                info.Command.OnExecute(clone, CommandType.Folder);
                AssetInfoRecord.PoolReturn(clone);
            }

            private AssetInfoRecord m_cloneAssetTree(AssetInfoRecord curRecord, AssetInfoRecord parent = null, MTreeItemData treeItem = null)
            {
                AssetInfoRecord root = (AssetInfoRecord)curRecord.Clone();
                root.Parent = parent;
                if (treeItem.HasChildren)
                {
                    foreach (var item in treeItem.Children)
                    {
                        if (item.Data is AssetInfoRecord child)
                        {
                            root.Childs.Add(m_cloneAssetTree(child, root, item));
                        }
                    }
                }
                return root;
            }

        }

        public class ReferenceTreeItemView : VisualElement
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
            private readonly RelationAssetMonitorTab _projectAssetMonitorTab;
            public ReferenceTreeItemView(RelationAssetMonitorTab tab)
            {
                _projectAssetMonitorTab = tab;
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


            internal void Refresh(object data)
            {
                switch (data)
                {
                    case string str:
                        m_refresh(str);
                        break;
                    case RelationInfo relationInfo:
                        m_refresh(relationInfo);
                        break;
                    case VerifyResult verifyResult:
                        m_refresh(verifyResult);
                        break;
                    default:
                        break;
                }
            }
            private void m_refresh(RelationInfo relationInfo)
            {
                _record = AssetMonitorTools.GetRecordByGuid(relationInfo.Guid);
                if (_record == null)
                {
                    //missing了
                    this.AddToClassList(USS_RELATION_REFERENCE_TREE_MISSING_CLASS);
                    _typeLabel.style.display = DisplayStyle.Flex;
                    _typeLabel.text = "Missing";
                    _nameLabel.text = relationInfo.Guid;
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
                _nameLabel.text = _record.AssetPath;

                _typeLabel.text = _record.AssetType;
            }

            private void m_refresh(VerifyResult verifyResult)
            {
                _record = null;
                _refButton.style.display = DisplayStyle.None;
                _depButton.style.display = DisplayStyle.None;
                _iconImage.style.display = DisplayStyle.None;
                _typeLabel.style.display = DisplayStyle.None;
                VerifierInfo verifierInfo = AssetMonitorTools.GetVerifierInfoByType(verifyResult.TypeName);
                if (verifierInfo == null)
                    _nameLabel.text = $"没有找到验证器: {verifyResult.TypeName}";
                else
                    _nameLabel.text = $"{(verifyResult.IsValid ? "<color=#00ff00>[成功]</color>" : "<color=#ff0000>[失败]</color>")} {verifierInfo.Verifier.Name}";


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

            private void m_onRefClick()
            {
                if (_record == null) return;
                _projectAssetMonitorTab?.ShowReferenceInfoByGuid(_record.Guid, true, AssetMonitorConfig.Instance.AutoExpandedTree);
            }

            private void m_onDepClick()
            {
                if (_record == null) return;
                _projectAssetMonitorTab?.ShowReferenceInfoByGuid(_record.Guid, false, AssetMonitorConfig.Instance.AutoExpandedTree);
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
                    if (item.Value.Command.OnFilter(this._record, CommandType.Relation))
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
                var clone = m_cloneAssetTree();
                info.Command.OnExecute(clone, CommandType.Folder);
                AssetInfoRecord.PoolReturn(clone);
            }
            private AssetInfoRecord m_cloneAssetTree()
            {
                var rootItem = _projectAssetMonitorTab._referenceTreeView.RootItems.FirstOrDefault();
                if (rootItem == null)
                    return (AssetInfoRecord)_record.Clone();
                var rootRelation = rootItem.GetData<RelationInfo>();
                if (rootRelation == null)
                    return (AssetInfoRecord)_record.Clone();
                if (rootRelation.Guid != _record.Guid)
                    return (AssetInfoRecord)_record.Clone();

                AssetInfoRecord root = (AssetInfoRecord)_record.Clone();
                root.Parent = null;

                if (rootItem.HasChildren)
                {
                    foreach (var item in rootItem.Children)
                    {
                        if (item.Data is RelationInfo child)
                        {
                            var childInfo = AssetMonitorTools.GetRecordByGuid(child.Guid).Clone() as AssetInfoRecord;
                            if (childInfo == null)
                                continue;
                            childInfo.Parent = root;
                            root.Childs.Add(childInfo);
                        }
                    }
                }
                return root;
            }
        }

    }
}
