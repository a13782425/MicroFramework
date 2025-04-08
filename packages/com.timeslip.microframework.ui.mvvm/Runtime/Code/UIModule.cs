using MFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    /// <summary>
    /// 界面模块
    /// </summary>
    [RequireType(typeof(IResourceModule))]
    public class UIModule : IUIModule
    {
        private bool _isInit = false;
        private EventSystem _curSystem;
        private GameObject _mainObj = null;
        private Dictionary<UILayer, RectTransform> _layerTranDic = null;
        private Dictionary<Type, UIPanel> _panelDic = null;
        private UIRuntimeConfig _runtimeConfig = null;
        public Canvas MainCanvas { get; private set; }
        bool IMicroModule.IsInit => _isInit;

        public bool IsOpen<T>() where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out UIPanel panel))
            {
                return panel.activeSelf;
            }
            return false;
        }
        public T CreatePanel<T>(object data = null) where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out UIPanel panel))
            {
                if (panel.state == UIState.Close)
                    _panelDic.Remove(typeof(T));
                else
                    return panel as T;
            }
            T tPanel = new T();
            tPanel.data = data;
            Transform parent = _layerTranDic[tPanel.LayerEnum];
            UIModuleUtils.CreateView(tPanel, parent);
            _panelDic.Add(typeof(T), tPanel);
            return tPanel;
        }
        public T GetPanel<T>() where T : UIPanel, new()
        {
            CheckState();
            UIPanel panel = default;
            if (_panelDic.TryGetValue(typeof(T), out panel))
            {
                if (panel.state == UIState.Close)
                    panel = CreatePanel<T>();
            }
            else
            {
                panel = CreatePanel<T>();
            }
            return panel as T;
        }
        public T ShowPanel<T>(object data = null) where T : UIPanel, new()
        {
            CheckState();
            UIPanel panel = default;
            if (_panelDic.TryGetValue(typeof(T), out panel))
            {
                if (panel.state == UIState.Close)
                    panel = CreatePanel<T>(data);
                else
                    panel.Show(data);
            }
            else
                panel = CreatePanel<T>(data);
            return panel as T;
        }
        public void HidePanel(UIPanel panel)
        {
            CheckState();
            if (panel.state == UIState.Close)
            {
                if (_panelDic.ContainsKey(panel.GetType()))
                    _panelDic.Remove(panel.GetType());
                return;
            }
            panel.Hide();
        }
        public void HidePanel<T>() where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out var panel))
            {
                HidePanel(panel);
            }
        }
        public void HidePanelByLayer(UILayer layerEnum)
        {
            CheckState();
            return;
        }
        public void HideAllPanel()
        {
            CheckState();
            return;
        }
        public void ClosePanel(UIPanel panel)
        {
            CheckState();
            if (_panelDic.ContainsKey(panel.GetType()))
            {
                _panelDic.Remove(panel.GetType());
                if (panel.state != UIState.Close)
                {
                    panel.Close();
                }
            }
        }
        public void ClosePanel<T>() where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out var panel))
            {
                _panelDic.Remove(typeof(T));
                if (panel.state != UIState.Close)
                {
                    panel.Close();
                }
            }
        }
        public void ClosePanelByLayer(UILayer layerEnum)
        {
            CheckState();
            return;
        }
        public void CloseAllPanel()
        {
            CheckState();
            return;
        }

        private void CheckState()
        {
            if (this.GetState() != ModuleState.Running)
                throw new Exception("UI模块没有初始化完成不能使用");
        }
        void IMicroModule.OnInit()
        {
            _layerTranDic = new Dictionary<UILayer, RectTransform>();
            _panelDic = new Dictionary<Type, UIPanel>();
            _runtimeConfig = MicroRuntimeConfig.CurrentConfig.GetRuntimeConfig<UIRuntimeConfig>();
            m_createMainObj();
            m_createLayer();
            UIModuleUtils.Init(_mainObj.transform);
            _isInit = true;
        }
        void IMicroModule.OnResume()
        {
        }
        void IMicroModule.OnSuspend()
        {
        }
        void IMicroModule.OnDestroy()
        {

        }
        private void m_createMainObj()
        {
            _mainObj = new GameObject("MainUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _mainObj.transform.SetParent(this.GetTransform());
            MainCanvas = _mainObj.GetComponent<Canvas>();
            MainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _mainObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = _runtimeConfig.MatchMode;
            scaler.matchWidthOrHeight = _runtimeConfig.MatchWidthOrHeight;
            scaler.referenceResolution = _runtimeConfig.DesignResolution;
            var eventObj = new GameObject("System", typeof(EventSystem), typeof(StandaloneInputModule));
            eventObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            eventObj.transform.SetParent(this.GetTransform());
        }
        private void m_createLayer()
        {
            int length = _runtimeConfig.Layers.Count;
            for (int i = 0; i < length; i++)
            {
                UILayer uILayerEnum = _runtimeConfig.Layers[i];
                GameObject obj = new GameObject(uILayerEnum.ToString(), typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                obj.transform.SetParent(_mainObj.transform);
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;

                rectTransform.offsetMax = Vector2.zero;
                rectTransform.offsetMin = Vector2.zero;
                Canvas canvas = obj.GetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = i;
                _layerTranDic.Add(uILayerEnum, rectTransform);
            }
        }
    }
}
