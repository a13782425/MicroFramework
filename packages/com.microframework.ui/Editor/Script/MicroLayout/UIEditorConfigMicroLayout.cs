using MFramework.Core;
using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.UI.Editor
{
    internal class UIEditorConfigMicroLayout : BaseMicroLayout
    {
        public override string Title => "UI/编辑器配置";
        private UIEditorConfig _config = default;

        private ListView _exportList = default;
        private MicroClassGroup _classGroup = new MicroClassGroup();

        private MicroFolderField _prefabFolderField;
        private MicroFolderField _codeFolderField;
        private MicroFolderField _codeGenFolderField;

        public override bool Init()
        {
            _config = MicroEditorConfig.Instance.GetEditorConfig<UIEditorConfig>();
            HelpBox helpBox = new HelpBox($"导出组件Tag为：{UIEditorConfig.TAG_NAME}", HelpBoxMessageType.None);
            helpBox.style.fontSize = 14;
            this.Add(helpBox);
            helpBox = new HelpBox($"导出关联的Widget前缀为：{UIEditorConfig.WIDGET_HEAD}\r\n\t既{UIEditorConfig.WIDGET_HEAD}xxxx，它会与已存在的Widget预制进行关联", HelpBoxMessageType.None);
            helpBox.style.fontSize = 14;
            this.Add(helpBox);

            TextField namespaceField = new TextField("脚本根命名空间:");
            namespaceField.value = _config.RootNamespace;
            namespaceField.RegisterValueChangedCallback(m_onNamespaceChanged);

            _prefabFolderField = new MicroFolderField("预制体文件夹:");
            _prefabFolderField.RegisterValueChangedCallback((e) => _config.PrefabRootPath = e.newValue);
            _codeFolderField = new MicroFolderField("代码文件夹:");
            _codeFolderField.RegisterValueChangedCallback((e) => _config.CodeRootPath = e.newValue);
            _codeGenFolderField = new MicroFolderField("生成代码文件夹:");
            _codeGenFolderField.RegisterValueChangedCallback((e) => _config.CodeGenRootPath = e.newValue);


            VisualElement listViewContainer = new VisualElement();
            listViewContainer.style.flexGrow = 1;
            listViewContainer.style.marginBottom = 24;
            listViewContainer.style.height = new Length(100, LengthUnit.Percent);

            _exportList = new ListView(_config.Exports, 24);
            _exportList.style.flexGrow = 1;
            _exportList.headerTitle = "组件前缀列表:";
            _exportList.showFoldoutHeader = true;
            _exportList.showBoundCollectionSize = false;
            _exportList.showAddRemoveFooter = true;
            _exportList.reorderable = false;
            _exportList.makeItem = m_makItem;
            _exportList.itemsAdded += m_itemsAdded;
            _exportList.bindItem = m_bindItem;
            _exportList.style.height = new Length(100, LengthUnit.Percent);
            _exportList.style.flexShrink = 1;  // 允许收缩
            _exportList.style.flexBasis = 0;   // 基础尺寸为0
            _exportList.horizontalScrollingEnabled = true;
            listViewContainer.Add(_exportList);

            this.Add(namespaceField);
            this.Add(_prefabFolderField);
            this.Add(_codeFolderField);
            this.Add(_codeGenFolderField);
            this.Add(listViewContainer);
            return base.Init();
        }

        public override void ShowUI()
        {
            base.ShowUI();
            _prefabFolderField.value = _config.PrefabRootPath;
            _codeFolderField.value = _config.CodeRootPath;
            _codeGenFolderField.value = _config.CodeGenRootPath;
        }

        private void m_itemsAdded(IEnumerable<int> enumerable)
        {
            foreach (var item in enumerable)
            {
                var export = new UIExportConfig();
                export.UIType = new MicroClassSerializer();
                this._config.Exports[item] = export;
            }
        }

        private void m_bindItem(VisualElement element, int arg2)
        {
            element.userData = _config.Exports[arg2];
            TextField textField = element.Q<TextField>("ExportPrefix");
            textField.SetValueWithoutNotify(_config.Exports[arg2].UIPrefix);
            MicroClassElement classElement = element.Q<MicroClassElement>("ExportClass");
            classElement.value = _config.Exports[arg2].UIType;
        }

        private VisualElement m_makItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;
            TextField textField = new TextField();
            textField.name = "ExportPrefix";
            textField.RegisterValueChangedCallback(m_prefixChanged);
            textField.style.flexGrow = 1;
            textField.style.marginLeft = 8;
            textField.style.width = new Length(38, LengthUnit.Percent);
            element.Add(textField);
            MicroClassElement classElement = new MicroClassElement("", null, _classGroup, typeof(Component));
            classElement.name = "ExportClass";
            classElement.style.flexGrow = 1;
            classElement.style.width = new Length(58, LengthUnit.Percent);
            element.Add(classElement);
            return element;
        }

        private void m_prefixChanged(ChangeEvent<string> evt)
        {
            TextField textField = evt.target as TextField;
            if (textField == null)
                return;
            if (string.IsNullOrWhiteSpace(evt.newValue))
                return;
            UIExportConfig config = textField.parent.userData as UIExportConfig;
            config.UIPrefix = textField.value;
        }

        private void m_onNamespaceChanged(ChangeEvent<string> evt)
        {
            _config.RootNamespace = evt.newValue;
        }
    }
}
