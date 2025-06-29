using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.CM.Common.Serialization.PacketFileReader;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    partial class ProjectAssetMonitorTab
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
            private readonly ProjectAssetMonitorTab _projectAssetMonitorTab;
            public FolderTreeItemView(ProjectAssetMonitorTab tab)
            {
                _projectAssetMonitorTab = tab;
                this.AddToClassList(USS_PROJECT_FLODER_ITEM_CLASS);

                _iconImage = new Image();
                _iconImage.name = "icon";
                _iconImage.AddToClassList(USS_PROJECT_FLODER_ITEM_ICON_CLASS);
                _iconImage.scaleMode = ScaleMode.ScaleToFit;
                this.Add(_iconImage);

                _nameLabel = new Label();
                _nameLabel.name = "name-label";
                _nameLabel.AddToClassList(USS_PROJECT_FLODER_ITEM_NAME_CLASS);
                this.Add(_nameLabel);

                _sizeLabel = new Label();
                _sizeLabel.name = "size-label";
                _sizeLabel.AddToClassList(USS_PROJECT_FLODER_ITEM_SIZE_CLASS);
                this.Add(_sizeLabel);

                _countLabel = new Label();
                _countLabel.name = "count-label";
                _countLabel.AddToClassList(USS_PROJECT_FLODER_ITEM_COUNT_CLASS);
                this.Add(_countLabel);

                _refButton = new Button();
                _refButton.name = "ref-btn";
                _refButton.AddToClassList(USS_PROJECT_FLODER_ITEM_REF_BTN_CLASS);
                this.Add(_refButton);

                _depButton = new Button();
                _depButton.name = "dep-btn";
                _depButton.AddToClassList(USS_PROJECT_FLODER_ITEM_DEP_BTN_CLASS);
                this.Add(_depButton);

            }

            internal void Refresh(AssetInfoRecord assetRecord)
            {
                _record = assetRecord;
                _refButton.text = AssetMonitorTools.FormatRefCount(_record.ReferenceRelations.Count);
                _depButton.text = AssetMonitorTools.FormatRefCount(_record.DependencyRelations.Count);
                // 设置图标
                _iconImage.image = AssetMonitorTools.GetIconByPath(_record.FilePath);

                // _record
                _nameLabel.text = _record.FileName;

                _sizeLabel.text = AssetMonitorTools.FormatSize(_record.Size);

                if (_record.IsDirectory)
                    _countLabel.text = $"({_record.Count} 项)";
                else
                    _countLabel.text = "";
            }
        }

        public class ReferenceTreeItemView : VisualElement
        {
            //图标
            private Image _iconImage;
            //文件夹名称
            private Label _nameLabel;
            //文件信息标签（大小）
            private Label _sizeLabel;
            //引用按钮
            private Button _refButton;
            //依赖按钮
            private Button _depButton;

            private AssetInfoRecord _record;
            private readonly ProjectAssetMonitorTab _projectAssetMonitorTab;
            public ReferenceTreeItemView(ProjectAssetMonitorTab tab)
            {
                _projectAssetMonitorTab = tab;
                this.AddToClassList(USS_PROJECT_FLODER_ITEM_CLASS);

                _iconImage = new Image();
                _iconImage.name = "icon";
                _iconImage.AddToClassList(USS_PROJECT_FLODER_ITEM_ICON_CLASS);
                _iconImage.scaleMode = ScaleMode.ScaleToFit;
                this.Add(_iconImage);

                _nameLabel = new Label();
                _nameLabel.name = "name-label";
                _nameLabel.AddToClassList(USS_PROJECT_FLODER_ITEM_NAME_CLASS);
                this.Add(_nameLabel);

                _sizeLabel = new Label();
                _sizeLabel.name = "size-label";
                _sizeLabel.AddToClassList(USS_PROJECT_FLODER_ITEM_SIZE_CLASS);
                this.Add(_sizeLabel);


                _refButton = new Button();
                _refButton.name = "ref-btn";
                _refButton.AddToClassList(USS_PROJECT_FLODER_ITEM_REF_BTN_CLASS);
                this.Add(_refButton);

                _depButton = new Button();
                _depButton.name = "dep-btn";
                _depButton.AddToClassList(USS_PROJECT_FLODER_ITEM_DEP_BTN_CLASS);
                this.Add(_depButton);

            }

            internal void Refresh(AssetInfoRecord assetRecord)
            {
                _record = assetRecord;
                _refButton.text = AssetMonitorTools.FormatRefCount(_record.ReferenceRelations.Count);
                _depButton.text = AssetMonitorTools.FormatRefCount(_record.DependencyRelations.Count);
                // 设置图标
                _iconImage.image = AssetMonitorTools.GetIconByPath(_record.FilePath);

                // _record
                _nameLabel.text = AssetMonitorTools.GuidToPath(_record.Guid);

                _sizeLabel.text = AssetMonitorTools.FormatSize(_record.Size);
            }

            internal void Refresh(RelationInfo relationInfo)
            {
                _record = AssetMonitorTools.GetRecordByGuid(relationInfo.Guid);
                if (_record == null) return;
                _refButton.text = AssetMonitorTools.FormatRefCount(_record.ReferenceRelations.Count);
                _depButton.text = AssetMonitorTools.FormatRefCount(_record.DependencyRelations.Count);
                // 设置图标
                _iconImage.image = AssetMonitorTools.GetIconByPath(_record.FilePath);

                // _record
                _nameLabel.text = AssetMonitorTools.GuidToPath(_record.Guid);

                _sizeLabel.text = AssetMonitorTools.FormatSize(_record.Size);
            }
        }

    }
}
