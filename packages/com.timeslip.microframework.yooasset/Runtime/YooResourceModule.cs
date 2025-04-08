using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YooAsset;

namespace MFramework.Runtime
{
    internal class YooResourceModule : IResourceModule, IMicroUpdate, IMicroLogicUpdate
    {

        public bool IsInit { get; private set; }

        public int LogicFrame { get => 10; set => _ = value; }

        public ResHandler Load<T>(string resPath, ResLoadDelegate<T> callback, object arg = null) where T : UnityEngine.Object
        {
            return default;
        }

        public ResHandler Load(string resPath, Type resType, ResLoadDelegate<UnityEngine.Object> callback, object arg = null)
        {
            return default;
        }

        public T LoadSync<T>(string resPath) where T : UnityEngine.Object
        {
            return default;
        }

        public UnityEngine.Object LoadSync(string resPath, Type resType)
        {
            return default;
        }
        public void UnloadAsset(string resPath, int reference = 1)
        {

        }
        public void OnDestroy()
        {

        }

        public void OnInit()
        {
            // 初始化资源系统
            YooAssets.Initialize();
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
    }
}
