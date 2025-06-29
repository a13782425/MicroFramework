using MFramework.Core;
using MFramework.Runtime;
using System;
using System.Collections.Generic;
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

        static UIModuleUtils()
        {
            onModuleReady();
            MicroContext.onModuleReady += onModuleReady;
        }

        private static void onModuleReady()
        {
            resourceModule = MicroContext.GetModule<IResourceModule>();
            uiModule = MicroContext.GetModule<IUIModule>();
            s_poolInit(null);
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
                        Core.MicroLogger.LogError($"界面Id:{instanceId}重复");
                    Interlocked.Exchange(ref _valueLock, FALSE);
                    return;
                }
            } while (true);
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