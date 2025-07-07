using MFramework.Core;
using MFramework.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFramework.UI
{
    /// <summary>
    /// 默认界面模块
    /// </summary>
    [RequireType(typeof(IResourceModule))]
    public class UIModule : IUIModule, IMicroLogicUpdate
    {
        private readonly static Type PANEL_TYPE = typeof(UIPanel);
        private bool _isInit = false;
        private EventSystem _curSystem;
        private GameObject _mainObj = null;
        private Dictionary<UILayer, RectTransform> _layerTranDic = null;
        private Dictionary<Type, UIPanel> _panelDic = null;
        private UIRuntimeConfig _runtimeConfig = null;
        /// <summary>
        /// 界面堆栈
        /// </summary>
        private Stack<StackPanelDto> _panelStack = new Stack<StackPanelDto>();

        private MicroPool<StackPanelDto> _stackPool = null;
        public Canvas MainCanvas { get; private set; }

        public event PanelStateDelegate onPanelStateChanged;

        private IMicroLogger _logger;

        bool IMicroModule.IsInit => _isInit;

        int IMicroLogicUpdate.LogicFrame { get => 10; set => _ = value; }
        public bool IsOpen(Type type)
        {
            CheckState();
            foreach (var item in _panelDic)
            {
                if (item.Key == type)
                {
                    return item.Value.ActiveSelf;
                }
            }
            return false;
        }
        public bool IsOpen(string panelClassName)
        {
            CheckState();
            foreach (var item in _panelDic)
            {
                if (item.Value.GetType().FullName == panelClassName)
                {
                    return item.Value.ActiveSelf;
                }
            }
            return false;
        }
        public bool IsOpen<T>() where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out UIPanel panel))
            {
                return panel.ActiveSelf;
            }
            return false;
        }
        public UIPanel CreatePanel(Type panelType, object data = null)
        {
            CheckState();
            if (_panelDic.TryGetValue(panelType, out UIPanel panel))
            {
                if (panel.State == UIState.Closed)
                    _panelDic.Remove(panelType);
                else
                    return panel;
            }
            else
            {
                if (!PANEL_TYPE.IsAssignableFrom(panelType))
                    throw new System.Exception("panelType is not UIPanel");
            }
            UIPanel tPanel = UIModuleUtils.CreateView(panelType, null, null, data);
            _panelDic.Add(panelType, tPanel);
            return tPanel;
        }
        public T CreatePanel<T>(object data = null) where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out UIPanel panel))
            {
                if (panel.State == UIState.Closed)
                    _panelDic.Remove(typeof(T));
                else
                    return panel as T;
            }
            T tPanel = UIModuleUtils.CreateView<T>(null, null, data);
            _panelDic.Add(typeof(T), tPanel);
            return tPanel;
        }
        public UIPanel ShowPanel(Type panelType, object data = null)
        {
            CheckState();
            UIPanel panel = default;
            if (_panelDic.TryGetValue(panelType, out panel))
            {
                if (panel.State == UIState.Closed)
                    panel = CreatePanel(panelType, data);
                else
                    panel.Show(data);
            }
            else
            {
                if (!PANEL_TYPE.IsAssignableFrom(panelType))
                    throw new System.Exception("panelType is not UIPanel");
                panel = CreatePanel(panelType, data);
            }
            return panel;
        }
        public T ShowPanel<T>(object data = null) where T : UIPanel, new()
        {
            CheckState();
            UIPanel panel = default;
            if (_panelDic.TryGetValue(typeof(T), out panel))
            {
                if (panel.State == UIState.Closed)
                    panel = CreatePanel<T>(data);
                else
                    panel.Show(data);
            }
            else
                panel = CreatePanel<T>(data);
            return panel as T;
        }
        public UIPanel GetPanel(Type panelType)
        {
            CheckState();
            UIPanel panel = default;
            if (_panelDic.TryGetValue(panelType, out panel))
            {
                if (panel.State == UIState.Closed)
                    panel = null;
            }
            else
            {
                panel = null;
            }
            return panel;
        }
        public T GetPanel<T>() where T : UIPanel, new()
        {
            return GetPanel(typeof(T)) as T;
        }
        public UIPanel GetTopPanel()
        {
            CheckState();
            if (_panelStack.Count > 0)
                return _panelStack.Peek().panel;
            return null;
        }
        public void HidePanel(UIPanel panel)
        {
            CheckState();
            //if (panel.state == UIState.Close)
            //{
            //    if (_panelDic.ContainsKey(panel.GetType()))
            //        _panelDic.Remove(panel.GetType());
            //    return;
            //}
            //panel.Hide();
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
                //if (panel.state != UIState.Close)
                //{
                //    panel.Close();
                //}
            }
        }
        public void ClosePanel<T>() where T : UIPanel, new()
        {
            CheckState();
            if (_panelDic.TryGetValue(typeof(T), out var panel))
            {
                _panelDic.Remove(typeof(T));
                //if (panel.state != UIState.Close)
                //{
                //    panel.Close();
                //}
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
        public Transform GetLayer(UILayer layer)
        {
            if (_layerTranDic.TryGetValue(layer, out RectTransform transform))
                return transform;
            return null;
        }
        private void CheckState()
        {
            if (this.GetState() != ModuleState.Running)
                throw new Exception("UI模块没有初始化完成不能使用");
        }
        void IMicroModule.OnInit()
        {
            _logger = MicroLogger.GetMicroLogger(this.GetType().Name);
            _layerTranDic = new Dictionary<UILayer, RectTransform>();
            _panelDic = new Dictionary<Type, UIPanel>();
            _runtimeConfig = MicroRuntimeConfig.CurrentConfig.GetRuntimeConfig<UIRuntimeConfig>();
            _stackPool = new MicroPool<StackPanelDto>(() => new StackPanelDto(), (a) => a.Clear());
            m_createMainObj();
            m_createLayer();
            _isInit = true;
        }
        void IMicroModule.OnResume()
        {
            UIModuleUtils.OnResume();
        }
        void IMicroModule.OnSuspend()
        {
            UIModuleUtils.OnSuspend();
        }
        void IMicroModule.OnDestroy()
        {

        }
        void IMicroLogicUpdate.OnLogicUpdate(float deltaTime)
        {
            UIModuleUtils.OnLogicUpdate();
        }

        #region 内部方法

        internal void Internal_Create(UIView view)
        {
            if (view is not UIPanel panel)
                return;
            try
            {
                onPanelStateChanged?.Invoke(view, UIState.Loaded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }
        internal void Internal_Show(UIView view)
        {
            if (view is not UIPanel panel)
                return;

            if (panel.IsStackUI)
            {
                //if (_panelStack.Count > 0)
                //{
                //    var preStack = _panelStack.Peek();
                //    preStack.snapshootData = preStack.panel.PushStack();
                //}
                var stackDto = _stackPool.Get();
                stackDto.panel = panel;
                _panelStack.Push(stackDto);
            }
            try
            {
                onPanelStateChanged?.Invoke(view, UIState.Showed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

        }
        internal void Internal_Hide(UIView view)
        {
            if (view is not UIPanel panel)
                return;

            if (panel.IsStackUI)
            {
                if (_panelStack.Count > 0)
                {
                    var prePanel = _panelStack.Pop();
                    if (prePanel.panel != panel)
                    {
                        _logger.LogError($"堆栈信息错误, 当前界面:{panel.GetType().FullName}  堆栈界面:{prePanel.panel.GetType().FullName}");
                    }
                    _stackPool.Recover(prePanel);
                }
                //if (_panelStack.Count > 0)
                //{
                //    var prePanel = _panelStack.Pop();
                //    if (prePanel.panel != panel)
                //    {
                //        _logger.LogError($"堆栈信息错误, 当前界面:{panel.GetType().FullName}  堆栈界面:{prePanel.panel.GetType().FullName}");
                //    }
                //}
            }
            try
            {
                onPanelStateChanged?.Invoke(view, UIState.Hidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }
        internal void Internal_Close(UIView view)
        {
            if (view is not UIPanel panel)
                return;
            try
            {
                onPanelStateChanged?.Invoke(view, UIState.Closed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        #endregion

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
            _curSystem = eventObj.GetComponent<EventSystem>();
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
        private class StackPanelDto
        {
            public UIPanel panel;
            /// <summary>
            /// 是否在使用中
            /// </summary>
            public object snapshootData;

            public void Clear()
            {
                panel = null;
                snapshootData = null;
            }
        }
    }
}
