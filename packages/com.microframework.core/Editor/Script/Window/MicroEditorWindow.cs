using Codice.CM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    internal sealed class MicroEditorWindow : EditorWindow
    {
        private static BaseMicroLayout s_currentMicroLayout;
        internal class MTreeViewItem : ITreeViewItem
        {
            private List<ITreeViewItem> _children = new List<ITreeViewItem>();
            public string name { get; set; }
            public object userData { get; set; }
            public ITreeViewItem parent { get; set; }
            public bool hasChildren => children.Count > 0;

            public BaseMicroLayout microLayout { get; set; }
            public List<ITreeViewItem> children => _children;

            public int index { get; set; }

            public string tooltip => microLayout.GetType().FullName;

            public void OnSelect()
            {
            }

            public void OnClick()
            {
                var window = MicroContextEditor.GetMicroEditorWindow();
                if (s_currentMicroLayout != null)
                {
                    s_currentMicroLayout?.HideUI();
                    s_currentMicroLayout.panel.style.flexGrow = 0;
                    s_currentMicroLayout.panel.style.display = DisplayStyle.None;
                }
                window.rightPane.Clear();
                window.rightPane.Add(microLayout.panel);
                microLayout.panel.style.flexGrow = 1;
                microLayout.panel.style.display = DisplayStyle.Flex;
                microLayout.ShowUI();
                MicroContextEditor.SetLastSelectTreeNode(microLayout.Title);
                s_currentMicroLayout = microLayout;
            }

            public void AddChild(ITreeViewItem child)
            {
                children.Add(child);
                child.parent = this;
            }

            public void RemoveChild(ITreeViewItem child)
            {
                children.Remove(child);
                child.parent = null;
            }
        }

        private const string STYLE_SHEET = "UIToolkit\\Uss\\MicroEditorWindow";


        public MTreeView treeView { get; private set; }

        private MicroDropdownField _configSelectField;

        public TwoPaneSplitView splitView { get; private set; }

        /// <summary>
        /// 左边面板
        /// </summary>
        public VisualElement leftPane { get; private set; }
        /// <summary>
        /// 右边面板
        /// </summary>
        public VisualElement rightPane { get; private set; }


        private VisualElement _configSelectContanier;

        private Button _configSelectEditButton;

        private TextField _configNameField;

        internal IMicroLogger logger { get; private set; }
        private List<MicroLayoutModel> _microLayoutModels = new List<MicroLayoutModel>();
        private static bool isRunAwake = false;
        private void Awake()
        {
            isRunAwake = true;
        }
        private void OnEnable()
        {
            logger = MicroLogger.GetMicroLogger(this.GetType().Name);
            if (isRunAwake)
            {
                m_delayEnable();
            }
            else
            {
                EditorApplication.delayCall += m_delayEnable;
            }
        }

        private void Update()
        {
            s_currentMicroLayout?.OnUpdate();
        }

        private void OnDisable()
        {
            MicroEditorConfig.Instance?.Save();
            MicroRuntimeConfig.CurrentConfig?.Save();
            isRunAwake = false;
        }
        private void OnDestroy()
        {
            foreach (var item in _microLayoutModels)
            {
                item.MicroLayout.Exit();
            }
        }
        private void m_delayEnable()
        {
            EditorApplication.delayCall -= m_delayEnable;
            titleContent = new GUIContent("微框架工具库");
            this.minSize = new Vector2(640, 480);
            this.rootVisualElement.AddStyleSheet(STYLE_SHEET);
            splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            this.rootVisualElement.Add(splitView);
            leftPane = new VisualElement();
            leftPane.AddToClassList("window-splitter-leftpane");
            leftPane.RegisterCallback<ContextClickEvent>(e => e.StopImmediatePropagation());
            rightPane = new VisualElement();
            rightPane.AddToClassList("window-splitter-rightpane");
            rightPane.RegisterCallback<ContextClickEvent>(e => e.StopImmediatePropagation());
            splitView.Add(leftPane);
            splitView.Add(rightPane);
            _microLayoutModels.AddRange(MicroContextEditor.GetMicroLayouts());
            m_createConfigSelectContanier();

            treeView = new MTreeView();
            m_generateLayoutModel(treeView.items);
            m_sortTreeView(treeView.items);


            leftPane.Add(treeView);
            treeView.onBindItem += m_onBindItem;
            treeView.onMakeItem += m_onMakeItem;
            treeView.Rebuild();
            m_selectLastIndex();
            this.rootVisualElement.parent?.RegisterCallback<KeyDownEvent>(onKeyDownEvent);
        }

        private void m_createConfigSelectContanier()
        {
            _configSelectContanier = new VisualElement();
            _configSelectContanier.AddToClassList("config-select-container");
            Label label = new Label("配置文件");
            _configSelectContanier.Add(label);
            _configSelectField = new MicroDropdownField();
            MicroDropdownContent content = new MicroDropdownContent();
            content.AppendValue("默认配置");
            content.AppendCustomItem((visualElement) =>
            {
                var button = new Button() { text = "添加配置" };
                button.clicked += () =>
                {
                    Debug.LogError("添加配置");
                    EditorUtility.SaveFilePanel("添加配置", "", "MicroConfig", "json");
                };
                visualElement.Add(button);

            });
            _configSelectField.RegisterValueChangedCallback(m_configSelectChanged);
            _configSelectField.getContent += m_getContent;
            _configSelectContanier.Add(_configSelectField);
            _configNameField = new TextField();
            _configNameField.AddToClassList("config-name-field");
            _configNameField.style.display = DisplayStyle.None;
            _configNameField.RegisterCallback<FocusOutEvent>(delegate
            {
                m_onEditConfigNameFinished();
            });
            _configNameField.RegisterCallback<KeyDownEvent>(m_configNameEditOnKeyDown);
            _configSelectContanier.Add(_configNameField);
            _configSelectEditButton = new Button();
            _configSelectEditButton.AddToClassList("config-select-edit-button");
            _configSelectContanier.Add(_configSelectEditButton);
            _configSelectEditButton.clicked += m_configSelectEditButton_clicked;
            leftPane.Add(_configSelectContanier);
            m_checkRuntimeConfig();
        }
        /// <summary>
        /// 检查运行时配置
        /// </summary>
        private void m_checkRuntimeConfig()
        {
            if (_configSelectField == null)
                return;

            if (MicroRuntimeConfig.CurrentConfig != null)
            {
                MicroEditorConfig.Instance.SelectConfigName = MicroRuntimeConfig.CurrentConfig.ConfigName;
                _configSelectField.value = MicroEditorConfig.Instance.SelectConfigName;
                return;
            }
            var configNames = MicroContextEditor.GetRuntimeConfigNames();
            if (configNames.Count == 0)
                MicroContextEditor.GetRuntimeConfig();
            configNames = MicroContextEditor.GetRuntimeConfigNames();
            int index = configNames.FindIndex(a => a.displayName == MicroEditorConfig.Instance.SelectConfigName);
            index = index == -1 ? 0 : index;
            MicroRuntimeConfig.CurrentConfig = MicroContextEditor.GetRuntimeConfig(configNames[index].value);

            if (MicroRuntimeConfig.CurrentConfig != null)
            {
                MicroEditorConfig.Instance.SelectConfigName = MicroRuntimeConfig.CurrentConfig.ConfigName;
                _configSelectField.value = MicroEditorConfig.Instance.SelectConfigName;
            }
        }

        /// <summary>
        /// 配置下拉列表改变
        /// </summary>
        /// <param name="evt"></param>
        private void m_configSelectChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == MicroEditorConfig.Instance.SelectConfigName)
                return;

            MicroEditorConfig.Instance.SelectConfigName = evt.newValue;
            foreach (var item in MicroContextEditor.GetRuntimeConfigNames())
            {
                if (item.displayName == MicroEditorConfig.Instance.SelectConfigName)
                {
                    MicroRuntimeConfig.CurrentConfig = MicroContextEditor.GetRuntimeConfig(item.value);
                    _microLayoutModels.ForEach(a => a.MicroLayout.OnRuntimeConfigChanged());
                    break;
                }
            }
        }
        /// <summary>
        /// 配置名称编辑按键
        /// </summary>
        /// <param name="evt"></param>
        private void m_configNameEditOnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    _configNameField.SetPropertyEx("ConfigNameCancel", "1");
                    _configNameField.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    evt.StopImmediatePropagation();
                    break;
                case KeyCode.Return:
                    _configNameField.SetPropertyEx("ConfigNameCancel", "0");
                    _configNameField.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    evt.StopImmediatePropagation();
                    break;
            }
        }
        private void onKeyDownEvent(KeyDownEvent evt)
        {
            if (!evt.ctrlKey)
                return;
            switch (evt.keyCode)
            {
                case KeyCode.S:
                    MicroEditorConfig.Instance?.Save();
                    MicroRuntimeConfig.CurrentConfig?.Save();
                    this.ShowNotification(new GUIContent("配置保存成功"));
                    break;
                default:
                    return;
            }
            evt.StopImmediatePropagation();
        }
        /// <summary>
        /// 配置名称修改完成
        /// </summary>
        private void m_onEditConfigNameFinished()
        {
            _configNameField.style.display = DisplayStyle.None;
            _configSelectField.style.display = DisplayStyle.Flex;
            object configNameCancel = _configNameField.GetPropertyEx("ConfigNameCancel");
            if (configNameCancel != null && configNameCancel.ToString() == "1")
            {
                _configNameField.SetPropertyEx("ConfigNameCancel", "0");
                return;
            }
            _configNameField.SetPropertyEx("ConfigNameCancel", "0");
            string newName = _configNameField.value;
            foreach (var item in MicroContextEditor.GetRuntimeConfigNames())
            {
                if (item.displayName == newName)
                {
                    this.ShowNotification(new GUIContent("配置名称已存在"));
                    return;
                }
            }
            MicroRuntimeConfig.CurrentConfig.ConfigName = newName;
            MicroRuntimeConfig.CurrentConfig.Save();
            _configSelectField.value = newName;
        }
        /// <summary>
        /// 配置名称修改按钮点击
        /// </summary>
        private void m_configSelectEditButton_clicked()
        {
            if (_configSelectField.value == MicroContextEditor.DEFAULT_CONFIG_NAME)
            {
                this.ShowNotification(new GUIContent("默认配置不能修改名字"));
                return;
            }
            _configNameField.value = _configSelectField.value;
            _configNameField.style.display = DisplayStyle.Flex;
            _configSelectField.style.display = DisplayStyle.None;
            _configNameField.Focus();
        }
        /// <summary>
        /// 获取下拉列表内容
        /// </summary>
        /// <returns></returns>
        private MicroDropdownContent m_getContent()
        {
            MicroDropdownContent content = new MicroDropdownContent();

            var configNames = MicroContextEditor.GetRuntimeConfigNames();
            configNames.Sort((a, b) =>
            {
                if (a.displayName == MicroContextEditor.DEFAULT_CONFIG_NAME)
                    return -1;
                else if (b.displayName == MicroContextEditor.DEFAULT_CONFIG_NAME)
                    return 1;
                return a.displayName.CompareTo(b.displayName);
            });
            foreach (var item in configNames)
            {
                content.AppendValue(item);
            }
            content.AppendSeparator();
            content.AppendCustomItem((visualElement) =>
            {
                var button = new Button() { text = "添加配置" };
                button.clicked += () =>
                {
                    m_createRuntimeConfig();
                };
                visualElement.Add(button);

            });
            return content;
        }
        /// <summary>
        /// 创建运行时配置
        /// </summary>
        private void m_createRuntimeConfig()
        {
            string filePath = EditorUtility.SaveFilePanelInProject("添加配置", "MicroRuntimeConfig", "asset", "");
            if (string.IsNullOrWhiteSpace(filePath))
            {
                this.ShowNotification(new GUIContent("配置路径不能为空"));
                return;
            }
            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            MicroRuntimeConfig runtimeConfig = MicroContextEditor.GetRuntimeConfig(filePath);
            string fileName, tempName;
            fileName = tempName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            int index = 0;
            foreach (var item in MicroContextEditor.GetRuntimeConfigNames())
            {
                if (item.displayName == fileName)
                {
                    index += 1;
                    tempName = fileName + index.ToString(); ;
                }
            }
            runtimeConfig.ConfigName = tempName;
            AssetDatabase.Refresh();
            this.ShowNotification(new GUIContent("配置文件创建成功"));
            MicroRuntimeConfig.CurrentConfig = runtimeConfig;
            _configSelectField.value = tempName;
        }

        /// <summary>
        /// 选中最后一次选的索引
        /// </summary>
        private void m_selectLastIndex()
        {
            if (MicroEditorConfig.Instance == null)
                return;
            string[] strs = MicroContextEditor.GetLastSelectTreeNode().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length == 0)
            {
                return;
            }
            ITreeViewItem root = treeView.items.FirstOrDefault(a => a.name == strs[0]);
            int index = 1;
            while (root != null)
            {
                treeView.OnItemChosen(root);
                if (index < strs.Length)
                {
                    root = root.children.FirstOrDefault(a => a.name == strs[index]);
                    index++;
                }
                else
                {
                    break;
                }
            }
            treeView.OnItemSelect(root);
        }
        private void m_sortTreeView(List<ITreeViewItem> items)
        {
            items.Sort((a, b) =>
            {
                var first = a as MTreeViewItem;
                var second = b as MTreeViewItem;
                if (first.microLayout is RootMicroLayout)
                    return -1;
                else if (second.microLayout is RootMicroLayout)
                    return 1;
                else if (first.microLayout.Priority > second.microLayout.Priority)
                    return 1;
                else if (first.microLayout.Priority < second.microLayout.Priority)
                    return -1;
                return 0;
            });
            foreach (var item in items)
            {
                if (item.hasChildren)
                {
                    m_sortTreeView(item.children);
                }
            }
        }
        private void m_generateLayoutModel(List<ITreeViewItem> rootList)
        {
            MTreeViewItem rootTreeItem = new MTreeViewItem();
            rootTreeItem.children.AddRange(rootList);
            foreach (MicroLayoutModel item in _microLayoutModels)
            {
                item.MicroLayout.window = this;
                item.MicroLayout.panel = new VisualElement();
                item.MicroLayout.panel.name = "container";
                if (!item.MicroLayout.Init())
                {
                    logger.LogError($"视图:{item.MicroLayout.Title}初始化失败");
                    continue;
                }
                MTreeViewItem parent = rootTreeItem;
                for (int i = 0; i < item.TitleLayers.Length - 1; i++)
                {
                    string group = item.TitleLayers[i];
                    MTreeViewItem tempItem = parent.children.FirstOrDefault(a => a.name == group) as MTreeViewItem;
                    if (tempItem == null)
                    {
                        tempItem = new MTreeViewItem();
                        tempItem.name = group;
                        tempItem.microLayout = new DefaultMicroLayout();
                        tempItem.microLayout.window = this;
                        tempItem.microLayout.panel = new VisualElement();
                        tempItem.microLayout.panel.name = "container";
                        parent.AddChild(tempItem);
                    }
                    if (tempItem.microLayout is DefaultMicroLayout defaultMicro)
                    {
                        defaultMicro.SetPriority(item.Priority);
                    }
                    parent = tempItem;
                }
                string curName = item.TitleLayers[item.TitleLayers.Length - 1];
                MTreeViewItem childItem = parent.children.FirstOrDefault(a => a.name == curName) as MTreeViewItem;
                if (childItem == null || (childItem.microLayout is { } layout && layout.GetType() != typeof(DefaultMicroLayout)))
                {
                    childItem = new MTreeViewItem();
                    childItem.name = curName;
                    parent.AddChild(childItem);
                }
                childItem.microLayout = item.MicroLayout;
            }
            rootList.AddRange(rootTreeItem.children);
        }
        private VisualElement m_onMakeItem()
        {
            return new Label();
        }
        private void m_onBindItem(VisualElement element, ITreeViewItem item)
        {
            Label label = element.Q<Label>();
            if (label != null)
            {
                label.text = item.name;
            }
        }

        private void OnFocus()
        {
            m_checkRuntimeConfig();
        }
    }
}
