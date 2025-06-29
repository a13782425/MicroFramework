using MFramework.Core;
using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Slider = UnityEngine.UIElements.Slider;

namespace MFramework.UI.Editor
{
    internal class UIMicroLayout : BaseMicroLayout
    {
        public override string Title => "UI";
        private UIRuntimeConfig _runtimeConfig;
        private ListView _layerListView;
        public override bool Init()
        {
            _runtimeConfig = MicroRuntimeConfig.CurrentConfig.GetRuntimeConfig<UIRuntimeConfig>();

            Box box = new Box();
            this.Add(box);
            box.AddToClassList(MicroStyles.Box);
            Vector2IntField designResolutionField = new Vector2IntField("设计分辨率:");
            designResolutionField.value = _runtimeConfig.DesignResolution;
            designResolutionField.RegisterValueChangedCallback(m_designResolutionChanged);

            EnumField matchModeField = new EnumField("屏幕匹配模式:", _runtimeConfig.MatchMode);
            matchModeField.RegisterValueChangedCallback(m_matchModeChanged);

            Slider matchWidthOrHeightField = new Slider("匹配宽度还是高度:", 0, 1);
            matchWidthOrHeightField.showInputField = true;
            matchWidthOrHeightField.value = _runtimeConfig.MatchWidthOrHeight;
            matchWidthOrHeightField.RegisterValueChangedCallback(m_matchWidthOrHeightChanged);
            matchWidthOrHeightField[matchWidthOrHeightField.childCount - 1].style.alignItems = Align.Center;
            _layerListView = new ListView(_runtimeConfig.Layers);
            _layerListView.headerTitle = "UI层级顺序:";
            _layerListView.showFoldoutHeader = true;
            _layerListView.showBoundCollectionSize = false;
            _layerListView.showAddRemoveFooter = false;
            _layerListView.reorderable = true;
            _layerListView.reorderMode = ListViewReorderMode.Animated;
            _layerListView.bindItem = m_bindItem;
            _layerListView.makeItem = m_makeItem;
            _layerListView.style.flexGrow = 1;
            _layerListView.AddToClassList(MicroStyles.Box);
            box.Add(designResolutionField);
            box.Add(matchModeField);
            box.Add(matchWidthOrHeightField);
            this.Add(_layerListView);
            Button button = new Button(m_onClick);
            button.text = "生成界面";
            button.AddToClassList(MicroStyles.H2);
            this.Add(button);
            return base.Init();
        }

        private void m_onClick()
        {
        }

        private VisualElement m_makeItem()
        {
            Label label = new Label();
            return label;
        }

        private void m_bindItem(VisualElement element, int arg2)
        {
            Label label = element.Q<Label>();
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.textShadow = new TextShadow()
            {
                color = Color.black,
                blurRadius = 1f,
                offset = new Vector2(1f, -1f)
            };
            label.text = _runtimeConfig.Layers[arg2].ToString();
        }

        private void m_matchWidthOrHeightChanged(ChangeEvent<float> evt)
        {
            _runtimeConfig.MatchWidthOrHeight = evt.newValue;
        }

        private void m_matchModeChanged(ChangeEvent<Enum> evt)
        {
            _runtimeConfig.MatchMode = (CanvasScaler.ScreenMatchMode)evt.newValue;
        }

        private void m_designResolutionChanged(ChangeEvent<Vector2Int> evt)
        {
            _runtimeConfig.DesignResolution = evt.newValue;
        }
    }
}
