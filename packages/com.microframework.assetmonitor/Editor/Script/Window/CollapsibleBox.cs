using System;
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    public class CollapsibleBox : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<CollapsibleBox, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Title = new UxmlStringAttributeDescription { name = "title", defaultValue = "标题" };
            UxmlBoolAttributeDescription m_Expanded = new UxmlBoolAttributeDescription { name = "expanded", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var box = ve as CollapsibleBox;
                box.Title = m_Title.GetValueFromBag(bag, cc);
                box.Expanded = m_Expanded.GetValueFromBag(bag, cc);
            }
        }

        // 事件
        public event Action<bool> OnExpandedChanged;

        // 私有字段
        private VisualElement _container;
        private Toggle _collapseToggle;

        // 公共属性
        public string Title
        {
            get => _collapseToggle?.text ?? "";
            set
            {
                if (_collapseToggle != null)
                    _collapseToggle.text = value;
            }
        }

        public bool Expanded
        {
            get => _collapseToggle?.value ?? false;
            set
            {
                if (_collapseToggle != null && _collapseToggle.value != value)
                {
                    _collapseToggle.value = value;
                    UpdateContentVisibility();
                }
            }
        }
        public VisualElement ContentContainer => _container;

        // 构造函数
        public CollapsibleBox() : this("标题", true) { }

        public CollapsibleBox(string title, bool expanded = true)
        {
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(EDITOR_DIR, "Resources/CollapsibleBox.uss")));
            AddToClassList("collapsible-box");
            CreateWithFoldout();
            Title = title;
            Expanded = expanded;
        }

        private void CreateWithFoldout()
        {
            // 使用 Foldout 作为主容器
            _collapseToggle = new Toggle();
            _collapseToggle.AddToClassList("collapsible-box__foldout");
            _collapseToggle.AddToClassList(Foldout.toggleUssClassName);

            // 创建内容区域
            _container = new VisualElement();
            _container.AddToClassList("collapsible-box__container");

            // 监听 Foldout 的变化
            _collapseToggle.RegisterValueChangedCallback(OnFoldoutValueChanged);

            // 添加到主容器
            hierarchy.Add(_collapseToggle);
            hierarchy.Add(_container);
        }
        private void OnFoldoutValueChanged(ChangeEvent<bool> evt)
        {
            UpdateContentVisibility();
            OnExpandedChanged?.Invoke(evt.newValue);
        }
        private void UpdateContentVisibility()
        {
            if (_container != null)
            {
                if (Expanded)
                {
                    AddToClassList("expanded");
                }
                else
                {
                    RemoveFromClassList("expanded");
                }
            }
        }

        // 便捷方法
        public void AddContent(VisualElement element)
        {
            _container?.Add(element);
        }

        public void ClearContent()
        {
            _container?.Clear();
        }

        // 重写 Add 方法
        public void AddElement(VisualElement child)
        {
            if (_container != null)
            {
                _container.Add(child);
            }
            else
            {
                base.Add(child);
            }
        }
    }
}
