using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace MFramework.Runtime
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

        static UIModuleUtils()
        {
            onModuleReady();
            MicroContext.onModuleReady += onModuleReady;
        }

        private static void onModuleReady()
        {
            resourceModule = MicroContext.GetModule<IResourceModule>();
            uiModule = MicroContext.GetModule<IUIModule>();
        }
        /// <summary>
        /// 初始化UI池
        /// </summary>
        /// <param name="root"></param>
        internal static void Init(Transform root)
        {
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
                        Debug.LogError($"界面Id:{instanceId}重复");
                    Interlocked.Exchange(ref _valueLock, FALSE);
                    return;
                }
            } while (true);
        }

        /// <summary>
        /// 创建界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T CreateView<T>(T uiView, Transform parent) where T : UIView, new()
        {
            Type uiType = typeof(T);
            if (!_uiConfigs.TryGetValue(uiType, out UIConfig config))
            {
                UIConfigAttribute attr = uiType.GetCustomAttribute<UIConfigAttribute>();
                if (attr == null)
                {
                    throw new NullReferenceException("界面特性：UIConfigAttribute为Null，请点击生成按钮");
                }
                config = new UIConfig();
                config.uiType = uiType;
                config.uiPath = attr.UIPath;
                if (uiView is UIWidget && uiView is IWidgetPool)
                    config.usePool = true;
                _uiConfigs.Add(uiType, config);
            }
            uiView.state = UIState.Load;
            if (uiView is UIWidget)
            {
                CreateWidget(uiView, parent, config);
            }
            else
            {
                CreatePanel(uiView, parent, config);
            }
            return uiView;
            //Type panelType = typeof(T);
            //UIConfigAttribute uiConfig = panelType.GetCustomAttribute<UIConfigAttribute>();
            //if (uiConfig == null)
            //{
            //    throw new NullReferenceException("界面特性：UIConfigAttribute为Null，请点击生成按钮");
            //}
            //GameObject origin = resourceModule.LoadSync<GameObject>(uiConfig.UIPath);
            //if (origin == null)
            //{
            //    throw new NullReferenceException($"界面预制体没有找到：{uiConfig.UIPath}");
            //}

            //GameObject uiObj = GameObject.Instantiate<GameObject>(origin, parent);
            //view.gameObject = uiObj;
            //view.Internal_Create();
            //return view;
        }
        private static void CreatePanel<T>(T uiView, Transform parent, UIConfig config) where T : UIView, new()
        {
            resourceModule.Load<GameObject>(config.uiPath, (origin, args) =>
            {
                if (origin == null)
                {
                    uiView.state = UIState.Error;
                    throw new NullReferenceException($"界面预制体没有找到：{config.uiPath}");
                }
                GameObject uiObj = GameObject.Instantiate<GameObject>(origin, parent);
                uiView.gameObject = uiObj;
                uiView.Internal_Create();
            });
        }

        private static void CreateWidget<T>(T uiView, Transform parent, UIConfig config) where T : UIView, new()
        {
            if (config.usePool && config.originObj != null)
            {
                GameObject uiObj = GetUIView<T>();
                if (uiObj != null)
                {
                    uiObj.transform.SetParent(parent);
                    uiObj.SetActive(true);
                    uiObj.transform.localScale = Vector3.one;
                    uiView.gameObject = uiObj;
                    uiView.Internal_Create();
                    return;
                }
            }
            resourceModule.Load<GameObject>(config.uiPath, (origin, args) =>
            {
                if (origin == null)
                {
                    uiView.state = UIState.Error;
                    throw new NullReferenceException($"界面预制体没有找到：{config.uiPath}");
                }
                if (config.usePool)
                    config.originObj = origin;
                GameObject uiObj = GameObject.Instantiate<GameObject>(origin, parent);
                uiView.gameObject = uiObj;
                uiView.Internal_Create();
            });

        }
        internal static GameObject GetUIView<T>() where T : UIView, new()
        {
            if (_uiConfigs.TryGetValue(typeof(T), out UIConfig config))
                return config.Get();
            return null;
        }
        internal static void RecoverUIView(UIView view)
        {
            if (view.gameObject == null)
#if UNITY_EDITOR
                throw new NullReferenceException($"当前界面{view},没有GameObject");
#else
                return;
#endif
            if (_uiConfigs.TryGetValue(view.GetType(), out UIConfig config))
                config.Recover(view.gameObject);
        }

        private class UIConfig
        {
            public Type uiType;
            public string uiPath;
            public bool usePool = false;
            public float lastUseTime;
            public GameObject originObj;
            public Queue<GameObject> queues = new Queue<GameObject>();

            internal GameObject Get()
            {
                if (!usePool)
                    return null;
                while (queues.Count > 0)
                {
                    GameObject obj = queues.Dequeue();
                    if (obj != null)
                        return obj;
                }
                return GameObject.Instantiate(originObj);
            }
            internal void Recover(GameObject obj)
            {
                if (!usePool)
                    return;
                if (obj != null)
                {
                    obj.transform.SetParent(_uiPoolRoot.transform);
                    queues.Enqueue(obj);
                }
            }
        }
    }
}
