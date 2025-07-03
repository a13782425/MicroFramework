using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 文件夹选择字段
    /// </summary>
    public class MicroFolderField : VisualElement
    {
        private const string STYLE_SHEET = "UIToolkit\\Element\\MicroFolderField";
        private const string SELECT_FOLDER_LABEL = "选择文件夹";
        private const string INVALID_PATH_MESSAGE = "路径必须为 Assets 或 Packages 相对路径（例：Assets/YourFolder 或 Packages/YourPackage）";

        private Label _labelElement;
        private TextField _textField;
        private Button _selectButton;
        private HelpBox _validationHelpBox;
        private VisualElement _inputContainer;
        private Label _placeholderLabel;
        private bool _isValidPath = false;
        private string _value = "";
        private string _placeholderText = "";

        private bool _mustAssetsPath = true;
        /// <summary>
        /// 是否必须为 AssetsDatabase 识别的路径
        /// </summary>
        public bool mustAssetsPath
        {
            get => _mustAssetsPath;
            set
            {
                _mustAssetsPath = value;
                UpdateValidation();
            }
        }

        private bool _isReadOnly = false;
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool isReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                _textField.isReadOnly = value;
                _selectButton.SetEnabled(!value);
            }
        }

        /// <summary>
        /// 字段值
        /// </summary>
        public string value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    var oldValue = _value;
                    _value = value ?? "";
                    _textField.SetValueWithoutNotify(_value);
                    UpdatePlaceholderVisibility();
                    UpdateValidation();

                    // 触发值变化事件
                    using (ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(oldValue, _value))
                    {
                        changeEvent.target = this;
                        SendEvent(changeEvent);
                    }
                }
            }
        }

        /// <summary>
        /// 标签文本
        /// </summary>
        public string label
        {
            get => _labelElement?.text ?? "";
            set
            {
                if (_labelElement != null)
                {
                    _labelElement.text = value;
                    _labelElement.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        /// <summary>
        /// 占位符文本
        /// </summary>
        public string placeholder
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value ?? "";
                _placeholderLabel.text = _placeholderText;
                UpdatePlaceholderVisibility();
            }
        }

        public MicroFolderField() : this(null) { }

        public MicroFolderField(string label)
        {
            this.AddStyleSheet(STYLE_SHEET);
            // 添加样式类
            AddToClassList("micro-folder-field");

            CreateUI();
            SetupEventHandlers();

            // 设置标签
            this.label = label;

            // 初始验证
            UpdateValidation();
            UpdatePlaceholderVisibility();
        }

        private void CreateUI()
        {
            // 设置主容器样式
            style.flexDirection = FlexDirection.Column;

            // 创建主要内容容器
            var mainContainer = new VisualElement();
            mainContainer.AddToClassList("micro-folder-field-main");
            //mainContainer.style.flexDirection = FlexDirection.Row;
            //mainContainer.style.alignItems = Align.Center;

            // 创建标签
            _labelElement = new Label();
            _labelElement.AddToClassList("micro-folder-field-label");

            // 创建输入容器（水平布局）
            _inputContainer = new VisualElement();
            _inputContainer.AddToClassList("micro-folder-field-input-container");
            _inputContainer.style.position = Position.Relative; // 为placeholder定位做准备

            // 创建文本输入框
            _textField = new TextField()
            {
                isReadOnly = true
            };
            _textField.AddToClassList("micro-folder-field-text");

            // 创建占位符标签
            _placeholderLabel = new Label("请选择文件夹");
            _placeholderLabel.AddToClassList("micro-folder-field-placeholder");
            _placeholderLabel.pickingMode = PickingMode.Ignore; // 不拦截鼠标事件

            // 创建选择按钮
            _selectButton = new Button(OnSelectButtonClicked)
            {
                text = SELECT_FOLDER_LABEL
            };
            //_selectButton.AddToClassList("folder-field-button");
            //_selectButton.style.width = 100;
            //_selectButton.style.flexShrink = 0;

            // 创建验证提示框
            _validationHelpBox = new HelpBox(INVALID_PATH_MESSAGE, HelpBoxMessageType.Error);
            _validationHelpBox.AddToClassList("micro-folder-field-validation");
            _validationHelpBox.style.display = DisplayStyle.None;

            // 组装UI
            _inputContainer.Add(_textField);
            _inputContainer.Add(_placeholderLabel); // 添加placeholder
            //_inputContainer.Add(_selectButton);

            mainContainer.Add(_labelElement);
            mainContainer.Add(_inputContainer);
            mainContainer.Add(_selectButton);

            Add(mainContainer);
            Add(_validationHelpBox);
        }

        private void SetupEventHandlers()
        {
            // 文本框值变化事件（如果不是只读）
            _textField.RegisterValueChangedCallback(OnTextFieldValueChanged);

            // 文本框失去焦点时验证
            _textField.RegisterCallback<FocusOutEvent>(evt => UpdateValidation());

            // 文本框获得焦点时隐藏placeholder
            _textField.RegisterCallback<FocusInEvent>(evt => UpdatePlaceholderVisibility());

            // 文本框失去焦点时显示placeholder
            _textField.RegisterCallback<FocusOutEvent>(evt => UpdatePlaceholderVisibility());
        }

        private void OnTextFieldValueChanged(ChangeEvent<string> evt)
        {
            if (!_textField.isReadOnly)
            {
                value = evt.newValue;
            }
            UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            if (_placeholderLabel == null) return;

            // 只有在文本为空且没有焦点时才显示placeholder
            bool shouldShowPlaceholder = string.IsNullOrEmpty(_value) &&
                                       !string.IsNullOrEmpty(_placeholderText) &&
                                       _textField.focusController?.focusedElement != _textField;

            _placeholderLabel.style.display = shouldShowPlaceholder ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnSelectButtonClicked()
        {
            // 获取当前路径作为初始路径
            string currentPath = value;
            string initialPath = Directory.Exists(currentPath) ? currentPath : Application.dataPath; ;

            // 弹出文件夹选择窗口
            string selectedPath = EditorUtility.OpenFolderPanel("选择文件夹", initialPath, "");

            if (!string.IsNullOrEmpty(selectedPath))
            {
                ProcessSelectedPath(selectedPath);
            }
        }

        private void ProcessSelectedPath(string selectedPath)
        {
            if (mustAssetsPath)
            {
                // 转换为项目相对路径
                string projectRelativePath = GetProjectRelativePath(selectedPath);
                value = string.IsNullOrWhiteSpace(projectRelativePath) ? selectedPath : projectRelativePath;
            }
            else
            {
                value = selectedPath;
            }
            UpdateValidation();
        }

        private string GetProjectRelativePath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return "";

            try
            {
                return FileUtil.GetProjectRelativePath(absolutePath);
                //string projectPath = Directory.GetParent(Application.dataPath).FullName;
                //string normalizedProjectPath = projectPath.Replace('\\', '/');
                //string normalizedAbsolutePath = absolutePath.Replace('\\', '/');

                //if (normalizedAbsolutePath.StartsWith(normalizedProjectPath))
                //{
                //    string relativePath = normalizedAbsolutePath.Substring(normalizedProjectPath.Length + 1);
                //    return relativePath;
                //}
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"转换路径失败: {ex.Message}");
            }

            return "";
        }

        private void UpdateValidation()
        {
            if (mustAssetsPath)
            {
                ValidateAssetsPath(value);
            }
            else
            {
                ValidateGeneralPath(value);
            }
        }

        private void ValidateAssetsPath(string path)
        {
            bool isValid = Directory.Exists(path) && AssetDatabase.IsValidFolder(path);

            _isValidPath = isValid;
            _validationHelpBox.style.display = isValid || string.IsNullOrEmpty(path) ? DisplayStyle.None : DisplayStyle.Flex;

            // 更新文本框样式
            _textField.RemoveFromClassList("error");
            if (!isValid && !string.IsNullOrEmpty(path))
            {
                _textField.AddToClassList("error");
            }
        }

        private void ValidateGeneralPath(string path)
        {
            _isValidPath = false;
            _validationHelpBox.style.display = DisplayStyle.None; // 一般路径不显示验证错误

            // 更新文本框样式
            _textField.RemoveFromClassList("error");
        }

        /// <summary>
        /// 检查当前路径是否有效
        /// </summary>
        public bool IsValidPath()
        {
            return _isValidPath;
        }

        /// <summary>
        /// 设置占位符文本
        /// </summary>
        public void SetPlaceholder(string placeholderText)
        {
            placeholder = placeholderText;
        }

        /// <summary>
        /// 设置按钮文本
        /// </summary>
        public void SetButtonText(string buttonText)
        {
            _selectButton.text = buttonText;
        }

        /// <summary>
        /// 注册值变化回调
        /// </summary>
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            RegisterCallback(callback);
        }

        /// <summary>
        /// 注销值变化回调
        /// </summary>
        public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            UnregisterCallback(callback);
        }

        // UXML支持
        public new class UxmlFactory : UxmlFactory<MicroFolderField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription { name = "label", defaultValue = "" };
            UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value", defaultValue = "" };
            UxmlBoolAttributeDescription m_MustAssetsPath = new UxmlBoolAttributeDescription { name = "must-assets-path", defaultValue = true };
            UxmlBoolAttributeDescription m_IsReadOnly = new UxmlBoolAttributeDescription { name = "readonly", defaultValue = false };
            UxmlStringAttributeDescription m_Placeholder = new UxmlStringAttributeDescription { name = "placeholder", defaultValue = "" };
            UxmlStringAttributeDescription m_ButtonText = new UxmlStringAttributeDescription { name = "button-text", defaultValue = SELECT_FOLDER_LABEL };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var folderField = ve as MicroFolderField;
                folderField.label = m_Label.GetValueFromBag(bag, cc);
                folderField.value = m_Value.GetValueFromBag(bag, cc);
                folderField.mustAssetsPath = m_MustAssetsPath.GetValueFromBag(bag, cc);
                folderField.isReadOnly = m_IsReadOnly.GetValueFromBag(bag, cc);
                folderField.placeholder = m_Placeholder.GetValueFromBag(bag, cc);

                string buttonText = m_ButtonText.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(buttonText))
                {
                    folderField.SetButtonText(buttonText);
                }
            }
        }
        //private const string SELECT_FOLDER_LABEL = "选择文件夹";
        //private const string INVALID_PATH_MESSAGE = "路径必须为 Assets 或 Packages 相对路径（例：Assets/YourFolder 或 Packages/YourPackage）";

        //private TextField _textField;
        //private Button _selectButton;
        //private HelpBox _validationHelpBox;
        //private bool _isValidPath = false;

        //private bool _mustAssetsPath = true;
        ///// <summary>
        ///// 是否必须为 AssetsDatabase 识别的路径
        ///// </summary>
        //public bool mustAssetsPath
        //{
        //    get => _mustAssetsPath;
        //    set => _mustAssetsPath = value;
        //}

        //public override string value
        //{
        //    get => base.value;
        //    set
        //    {
        //        base.value = value;
        //        _textField.SetValueWithoutNotify(value);
        //    }
        //}

        //public MicroFolderField(string label) : base(label, null)
        //{
        //    // 创建水平布局容器
        //    //visualInput.style.flexDirection = FlexDirection.Row;

        //    // 创建文本输入框
        //    _textField = new TextField()
        //    {
        //        isReadOnly = true,
        //        style = { flexGrow = 1 }
        //    };

        //    // 创建选择按钮
        //    _selectButton = new Button(OnSelectButtonClicked)
        //    {
        //        text = SELECT_FOLDER_LABEL,
        //        style = { width = 120 }
        //    };

        //    // 创建验证提示框
        //    _validationHelpBox = new HelpBox(INVALID_PATH_MESSAGE, HelpBoxMessageType.Error)
        //    {
        //        style = { display = DisplayStyle.None }
        //    };

        //    // 组合UI元素
        //    this.Add(_textField);
        //    this.Add(_selectButton);
        //    //this.Add(container);
        //    this.Add(_validationHelpBox);
        //}

        //private void OnSelectButtonClicked()
        //{
        //    // 弹出文件夹选择窗口
        //    string selectedPath = EditorUtility.OpenFolderPanel("选择文件夹", value, "");

        //    if (!string.IsNullOrEmpty(selectedPath))
        //    {
        //        if (mustAssetsPath)
        //        {
        //            _validationHelpBox.style.display = DisplayStyle.None;
        //            // 转换为项目相对路径
        //            string projectRelativePath = FileUtil.GetProjectRelativePath(selectedPath);
        //            // 更新字段值并验证
        //            value = projectRelativePath;
        //            ValidatePath(projectRelativePath);
        //        }
        //        else
        //        {
        //            value = selectedPath;
        //        }
        //        NotifyValueChanged();
        //    }
        //}

        //private void ValidatePath(string path)
        //{
        //    bool isValid = !string.IsNullOrEmpty(path);

        //    _isValidPath = isValid;
        //    _validationHelpBox.style.display = isValid ? DisplayStyle.None : DisplayStyle.Flex;


        //}

        //private void NotifyValueChanged()
        //{
        //    // 触发值变化事件
        //    using (ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(value, value))
        //    {
        //        changeEvent.target = this;
        //        SendEvent(changeEvent);
        //    }
        //}

    }
}
