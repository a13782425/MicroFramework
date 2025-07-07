using MFramework.Core;
using MFramework.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace MFramework.UI
{
    /// <summary>
    /// UI模块工具类
    /// </summary>
    internal static class UIModuleUtils
    {
        private const int FALSE = 0;
        private const int TRUE = 1;
        private static int _valueLock = 0;
        private static int _instanceId = int.MinValue;
        private static Queue<int> _instanceCacheQueue = new Queue<int>();


        private static Transform _uiPoolRoot = null;
        /// <summary>
        /// 类型对应路径
        /// </summary>
        private static Dictionary<Type, UIConfig> _uiConfigs = new Dictionary<Type, UIConfig>();
        internal static IResourceModule resourceModule { get; private set; }
        internal static IUIModule uiModule { get; private set; }

        private static bool _isInit = false;

        //是否强制回收对象池
        private static bool _forceRecycle = false;

        //闲置回收时间
        private const float IDLE_TIME = 60f;

        static UIModuleUtils()
        {
            onModuleReady();
#if !UNITY_WEBGL
            Application.lowMemory += m_lowMemory;
#else
            _forceRecycle = false;
#endif
            MicroContext.onModuleReady += onModuleReady;
        }
        private static void onModuleReady()
        {
            resourceModule = MicroContext.GetModule<IResourceModule>();
            uiModule = MicroContext.GetModule<IUIModule>();
            s_poolInit(null);
            MicroContext.onModuleReady -= onModuleReady;
        }
        /// <summary>
        /// 获取实例ID
        /// </summary>
        /// <returns></returns>
        internal static int GetInstanceId()
        {
            do
            {
                if (Interlocked.CompareExchange(ref _valueLock, TRUE, FALSE) == FALSE)
                {
                    int result = _instanceId;
                    if (_instanceCacheQueue.Count == 0)
                        _instanceId++;
                    else
                        result = _instanceCacheQueue.Dequeue();
                    Interlocked.Exchange(ref _valueLock, FALSE);
                    return result;
                }
            } while (true);
        }
        /// <summary>
        /// 回收实例ID
        /// </summary>
        /// <returns></returns>
        internal static void RecoverInstanceId(int instanceId)
        {
            do
            {
                if (Interlocked.CompareExchange(ref _valueLock, TRUE, FALSE) == FALSE)
                {
                    if (!_instanceCacheQueue.Contains(instanceId))
                        _instanceCacheQueue.Enqueue(instanceId);
                    else
                        MicroLogger.LogError($"界面Id:{instanceId}重复");
                    Interlocked.Exchange(ref _valueLock, FALSE);
                    return;
                }
            } while (true);
        }

        internal static T CreateView<T>(Transform parent, UIView uiParent, object data) where T : UIView, new()
        {
            if (resourceModule == null)
                throw new NullReferenceException("资源模块为Null");

            T view = new T();
            Type viewType = typeof(T);
            if (!_uiConfigs.TryGetValue(viewType, out UIConfig config))
            {
                UIConfigAttribute attr = viewType.GetCustomAttribute<UIConfigAttribute>();
                if (attr == null)
                    throw new NullReferenceException("界面特性：UIConfigAttribute为Null，请点击生成按钮");
                config = new UIConfig();
                config.uiType = viewType;
                config.uiPath = attr.UIPath;
                if (view is UIWidget && view is IWidgetPool)
                    config.usePool = true;
                _uiConfigs.Add(typeof(T), config);
            }
            view.State = UIState.Load;
            if (view is UIPanel uipanel)
            {
                m_createPanel(view, parent ?? uiModule.GetLayer(uipanel.LayerEnum), uiParent, data, config);
            }
            else
            {
                m_createWidget(view, parent, uiParent, data, config);
            }
            return view;
        }
        internal static UIPanel CreateView(Type viewType, Transform parent, UIView uiParent, object data)
        {
            if (resourceModule == null)
                throw new NullReferenceException("资源模块为Null");

            UIPanel view = Activator.CreateInstance(viewType) as UIPanel;
            if (view == null)
                throw new NullReferenceException($"无法创建UIPanel, 类型: {viewType.FullName}");
            if (!_uiConfigs.TryGetValue(viewType, out UIConfig config))
            {
                UIConfigAttribute attr = viewType.GetCustomAttribute<UIConfigAttribute>();
                if (attr == null)
                    throw new NullReferenceException("界面特性：UIConfigAttribute为Null，请点击生成按钮");
                config = new UIConfig();
                config.uiType = viewType;
                config.uiPath = attr.UIPath;
                _uiConfigs.Add(viewType, config);
            }
            view.State = UIState.Load;
            m_createPanel(view, parent ?? uiModule.GetLayer(view.LayerEnum), uiParent, data, config);
            return view;
        }

        /// <summary>
        /// 释放UI资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal static void ReleaseUIAsset(UIView view)
        {
            if (view.GameObject == null)
#if UNITY_EDITOR
                throw new NullReferenceException($"当前界面{view},没有GameObject");
#else
                return;
#endif
            if (_uiConfigs.TryGetValue(view.GetType(), out UIConfig config))
            {
                if (config.usePool)
                {
                    view.RectTransform.sizeDelta = config.originSize;
                    view.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    view.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                }
                config.Recover(view.GameObject);
            }
        }

        /// <summary>
        /// 释放UI资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal static void UnloadAsset(string assetPath)
        {
            if (resourceModule == null)
                return;
            if (string.IsNullOrWhiteSpace(assetPath))
                return;
            resourceModule.UnloadAsset(assetPath);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        internal static void OnSuspend()
        {
            resourceModule = null;
        }

        /// <summary>
        /// 恢复 
        /// </summary>
        internal static void OnResume()
        {
            resourceModule = MicroContext.GetModule<IResourceModule>();
        }
        /// <summary>
        /// 逻辑更新
        /// 用于处理对象池缓存
        /// </summary>
        internal static void OnLogicUpdate()
        {
            if (_forceRecycle)
            {
                foreach (var item in _uiConfigs)
                    if (item.Value.Clear())
                        UnloadAsset(item.Value.uiPath);
            }
            else
            {
                foreach (var item in _uiConfigs)
                    if (MicroContext.GameTime - item.Value.lastUseTime > IDLE_TIME)
                    {
                        if (item.Value.Clear())
                            UnloadAsset(item.Value.uiPath);
                    }
            }
        }

#if !UNITY_WEBGL
        private static void m_lowMemory()
        {
            _forceRecycle = true;
        }
#endif

        private static void m_createPanel<T>(T uiView, Transform parent, UIView uiParent, object data, UIConfig config) where T : UIView, new()
        {
            if (config.originObj != null)
            {
                GameObject uiObj = config.Create(parent);
                uiView.Internal_Create(uiObj, uiParent, data);
                return;
            }
            resourceModule.Load<GameObject>(config.uiPath, (origin, args) =>
            {
                if (origin == null)
                {
                    uiView.State = UIState.Error;
                    throw new NullReferenceException($"界面预制体没有找到：{config.uiPath}");
                }
                config.originObj = origin;
                GameObject uiObj = config.Create(parent);
                uiView.Internal_Create(uiObj, uiParent, data);
            });
        }

        private static void m_createWidget<T>(T uiView, Transform parent, UIView uiParent, object data, UIConfig config) where T : UIView, new()
        {
            if (config.usePool && config.originObj != null)
            {
                GameObject uiObj = config.Get(parent);
                if (uiObj != null)
                {
                    uiObj.transform.SetParent(parent);
                    uiObj.transform.localScale = Vector3.one;
                    uiObj.transform.localPosition = Vector3.zero;
                    uiObj.SetActive(true);
                    uiView.Internal_Create(uiObj, uiParent, data);
                    return;
                }
            }
            resourceModule.Load<GameObject>(config.uiPath, (origin, args) =>
             {
                 if (origin == null)
                 {
                     uiView.State = UIState.Error;
                     throw new NullReferenceException($"界面预制体没有找到：{config.uiPath}");
                 }
                 config.originSize = origin.GetComponent<RectTransform>().sizeDelta;
                 if (config.usePool)
                     config.originObj = origin;
                 GameObject uiObj = config.Create(parent);
                 uiObj.transform.localScale = Vector3.one;
                 uiView.Internal_Create(uiObj, uiParent, data);
             });

        }
        /// <summary>
        /// 初始化UI池
        /// </summary>
        /// <param name="root"></param>
        private static void s_poolInit(Transform root)
        {
            if (_isInit)
                return;
            _isInit = true;
            var poolRoot = new GameObject("UIPoolRoot", typeof(RectTransform), typeof(Canvas));
            _uiPoolRoot = poolRoot.transform;
            poolRoot.transform.SetParent(root);
            poolRoot.transform.localScale = Vector3.zero;
            RectTransform rectTransform = poolRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            Canvas canvas = poolRoot.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = -1;
        }

        private class UIConfig
        {
            public Type uiType;
            public string uiPath;
            public bool usePool = false;
            public float lastUseTime;
            public Vector2 originSize;
            public GameObject originObj;
            private int _useCount = 0;
            private Queue<GameObject> _queues = new Queue<GameObject>();

            internal bool Clear()
            {
                if (_useCount != 0)
                    return false;
                if (originObj == null)
                    return false;

                while (_queues.Count > 0)
                {
                    var q = _queues.Dequeue();
                    GameObject.Destroy(q);
                }
                _queues.Clear();
                GameObject.Destroy(originObj);
                return true;
            }

            internal GameObject Get(Transform parent = null)
            {
                if (!usePool)
                    return Create(parent);
                while (_queues.Count > 0)
                {
                    GameObject obj = _queues.Dequeue();
                    if (obj != null)
                        return obj;
                }
                lastUseTime = MicroContext.GameTime;
                return Create(parent);
            }
            internal void Recover(GameObject obj)
            {
                _useCount--;
                if (!usePool)
                {
                    GameObject.Destroy(obj);
                    return;
                }
                if (obj != null)
                {
                    obj.transform.SetParent(_uiPoolRoot.transform);
                    obj.transform.localRotation = Quaternion.identity;
                    obj.transform.localScale = Vector3.one;
                    _queues.Enqueue(obj);
                }
            }
            internal GameObject Create(Transform parent = null)
            {
                if (originObj == null)
                    return null;
                lastUseTime = MicroContext.GameTime;
                return GameObject.Instantiate(originObj, parent);
            }
        }
    }
}