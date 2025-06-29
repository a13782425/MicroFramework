using MFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MFramework.Runtime
{
    /// <summary>
    /// 界面模块
    /// </summary>
    public class ResourceModule : IResourceModule
    {
        private bool _isInit = false;
        public bool IsInit => _isInit;

        public T LoadSync<T>(string resPath) where T : Object
        {
            return LoadSync(resPath, typeof(T)) as T;
        }

        public Object LoadSync(string resPath, Type resType)
        {
            return Resources.Load(resPath, resType);
        }


        public ResHandler Load<T>(string resPath, ResLoadDelegate<T> callback, object arg = null) where T : Object
        {
            T t = LoadSync<T>(resPath);
            callback?.Invoke(t, arg);
            return new ResHandler() { isDone = true, asset = t };
        }

        public ResHandler Load(string resPath, Type resType, ResLoadDelegate<Object> callback, object arg = null)
        {
            Object obj = LoadSync(resPath, resType);
            callback?.Invoke(obj, arg);
            return new ResHandler() { isDone = true, asset = obj };
        }

        public void OnDestroy()
        {
            Resources.UnloadUnusedAssets();
        }

        public void OnInit()
        {
            _isInit = true;
        }

        public void OnLogicUpdate(float deltaTime)
        {
        }

        public void OnResume()
        {
        }

        public void OnSuspend()
        {
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void UnloadAsset(string resPath, int reference = 1)
        {
            
        }
    }
}
