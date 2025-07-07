using System;
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
            var resHandler = new ResHandler();
            Resources.LoadAsync<T>(resPath).completed += (a) =>
            {
                if (a is ResourceRequest request)
                {
                    resHandler.Asset = request.asset;
                    callback?.Invoke(request.asset as T, arg);
                }
                else
                {
                    callback?.Invoke(default(T), arg);
                }
                resHandler.IsDone = true;
            };
            return resHandler;
        }

        public ResHandler Load(string resPath, Type resType, ResLoadDelegate<Object> callback, object arg = null)
        {
            var resHandler = new ResHandler();
            Resources.LoadAsync(resPath).completed += (a) =>
            {
                if (a is ResourceRequest request)
                {
                    resHandler.Asset = request.asset;
                    callback?.Invoke(request.asset, arg);
                }
                else
                {
                    callback?.Invoke(default(Object), arg);
                }
                resHandler.IsDone = true;
            };
            return resHandler;
        }

        public void OnDestroy()
        {
            Resources.UnloadUnusedAssets();
        }

        public void OnInit()
        {
            _isInit = true;
        }

        public void OnResume()
        {
        }

        public void OnSuspend()
        {
        }

        public void UnloadAsset(string resPath, int reference = 1)
        {
            Resources.UnloadUnusedAssets();
        }
    }
}
