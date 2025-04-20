using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    internal class RootMicroLayout : BaseMicroLayout
    {
        public override string Title => "微框架";

        public override int Priority => int.MinValue;

        private MicroRuntimeConfig _config;
        private ListView _moduleListView;
        public override bool Init()
        {
            _config = MicroRuntimeConfig.CurrentConfig;
            _moduleListView = new ListView(_config.InitModules);
            _moduleListView.makeItem += m_makeItem;
            _moduleListView.bindItem += m_bindItem;
            _moduleListView.itemsAdded += m_itemsAdded;
            _moduleListView.itemsRemoved += m_itemsRemoved;
            _moduleListView.fixedItemHeight = 32;
            _moduleListView.reorderMode = ListViewReorderMode.Animated;
            _moduleListView.reorderable = true;
            _moduleListView.showAddRemoveFooter = true;
            _moduleListView.showFoldoutHeader = true;
            _moduleListView.headerTitle = "自定义加载模块:";
            Label label = new Label();
            label.text = "微框架配置";
            label.AddToClassList(MicroStyles.H1);
            panel.Add(label);
            panel.Add(_moduleListView);
            return true;
        }

        private void m_itemsRemoved(IEnumerable<int> enumerable)
        {
        }

        private void m_itemsAdded(IEnumerable<int> enumerable)
        {
            foreach (var item in enumerable)
            {
                this._config.InitModules[item] = new MicroClassSerializer();
            }
        }

        private void m_bindItem(VisualElement element, int arg2)
        {
            var microClassElement = element as MicroClassElement;
            microClassElement.value = this._config.InitModules[arg2];
        }

        private VisualElement m_makeItem()
        {
            var classElement = new MicroClassElement(null, null, typeof(IMicroModule));
            classElement.onCustomFilterType += ClassElement_onCustomFilterType;
            classElement.style.height = 26;
            return classElement;
        }

        private bool ClassElement_onCustomFilterType(Type arg)
        {
            return arg.GetCustomAttribute<IgnoreAttribute>() == null;
        }

        public override void ShowUI()
        {
        }
        //private void m_onGui()
        //{
        //    _config.AutoRegisterModule = EditorGUILayout.ToggleLeft("是否自动加载全部模块", _config.AutoRegisterModule);
        //    if (!_config.AutoRegisterModule)
        //    {
        //        _reorderableList.DoLayoutList();
        //    }
        //}
    }
}
