using MFramework.Core;
using MFramework.Core.Editor;
using MFramework.Runtime;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace MFramework.Editor
{
    internal class UIMicroLayout : BaseMicroLayout
    {
        public override string Title => "UI(MVVM)";

        private UIEditorConfig _config;
        private UIRuntimeConfig _runtimeConfig;
        private ReorderableList _reorderableList;
        public override bool Init()
        {
            _runtimeConfig = MicroRuntimeConfig.CurrentConfig.GetRuntimeConfig<UIRuntimeConfig>();
            _reorderableList = new ReorderableList(_runtimeConfig.Layers, typeof(UILayer), true, true, false, false);
            _reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "UI层级顺序:");
            };
            _reorderableList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _runtimeConfig.Layers[index];
                    rect.y += 2;
                    EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        element.ToString()
                        );
                };
            IMGUIContainer container = new IMGUIContainer(m_onGui);
            container.style.marginTop = 4;
            container.style.marginBottom = 4;
            container.style.marginLeft = 4;
            container.style.marginRight = 4;
            container.style.flexGrow = 1;
            panel.Add(container);
            Button button = new Button(m_onClick);
            button.text = "生成界面";
            button.AddToClassList(MicroStyles.H2);
            //panel.Add(_runtimeConfig.DrawUI());
            panel.Add(button);
            return base.Init();
        }

        private void m_onGui()
        {
            _runtimeConfig.DesignResolution = EditorGUILayout.Vector2IntField("UI设计分辨率:", _runtimeConfig.DesignResolution);
            System.Enum @enum = EditorGUILayout.EnumPopup("屏幕匹配模式:", _runtimeConfig.MatchMode);
            Enum.TryParse<CanvasScaler.ScreenMatchMode>(@enum.ToString(), out _runtimeConfig.MatchMode);
            _runtimeConfig.MatchWidthOrHeight = EditorGUILayout.Slider("匹配宽度还是高度:", _runtimeConfig.MatchWidthOrHeight, 0, 1);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(150);
            EditorGUILayout.LabelField("Width");
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Height");
            EditorGUILayout.EndHorizontal();
            _reorderableList.DoLayoutList();
        }

        private void m_onClick()
        {
            UIModuleEditor.GenerateView();
        }
    }
}
