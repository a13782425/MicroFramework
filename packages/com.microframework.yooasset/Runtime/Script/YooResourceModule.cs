using MFramework.Core;
using System;
using System.Collections;
using YooAsset;

namespace MFramework.Runtime
{

    internal class YooResourceModule : IResourceModule, IMicroUpdate, IMicroLogicUpdate
    {

        public bool IsInit { get; private set; }

        public int LogicFrame { get => 10; set => _ = value; }

        public ResHandler Load<T>(string resPath, ResLoadDelegate<T> callback, object arg = null) where T : UnityEngine.Object
        {
            var operation = YooAssets.LoadAssetAsync<T>(resPath);
            ResHandler resHandler = new ResHandler();
            operation.Completed += (op) =>
            {
                if (resHandler.IsCancel)
                {
                    op.Dispose();
                    return;
                }
                if (op.Status == EOperationStatus.Succeed)
                {
                    resHandler.Asset = op.AssetObject;
                    resHandler.ErrorMessage = null;
                    callback?.Invoke(resHandler.Asset as T, arg);
                }
                else
                {
                    MicroLogger.LogError($"加载资源失败：{op.LastError}");
                    resHandler.ErrorMessage = op.LastError;
                }
                resHandler.IsDone = true;
            };
            return resHandler;
        }

        public ResHandler Load(string resPath, Type resType, ResLoadDelegate<UnityEngine.Object> callback, object arg = null)
        {
            var operation = YooAssets.LoadAssetAsync(resPath, resType);
            ResHandler resHandler = new ResHandler();
            operation.Completed += (op) =>
            {
                if (resHandler.IsCancel)
                {
                    op.Dispose();
                    return;
                }
                if (op.Status == EOperationStatus.Succeed)
                {
                    resHandler.Asset = op.AssetObject;
                    resHandler.ErrorMessage = null;
                    callback?.Invoke(resHandler.Asset, arg);
                }
                else
                {
                    MicroLogger.LogError($"加载资源失败：{op.LastError}");
                    resHandler.ErrorMessage = op.LastError;
                }
                resHandler.IsDone = true;
            };
            return resHandler;
        }

        public T LoadSync<T>(string resPath) where T : UnityEngine.Object
        {
            return YooAssets.LoadAssetSync<T>(resPath).AssetObject as T;
        }

        public UnityEngine.Object LoadSync(string resPath, Type resType)
        {
            return YooAssets.LoadAssetSync(resPath, resType).AssetObject;
        }
        public void UnloadAsset(string resPath, int reference = 1)
        {

        }
        public void OnDestroy()
        {

        }

        public void OnInit()
        {
            MicroContext.StartCoroutine(m_startYooasset());
        }

        private IEnumerator m_startYooasset()
        {
            YooAssets.Initialize();

            // 创建默认的资源包
            YooAssets.CreatePackage("Default");

            // 获取指定的资源包，如果没有找到会报错
            ResourcePackage package = YooAssets.GetPackage("Default");
            YooAssets.SetDefaultPackage(package);
#if UNITY_EDITOR
            var buildResult = EditorSimulateModeHelper.SimulateBuild(new EditorSimulateBuildParam() { PackageName = "Default" });
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
#else
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            OfflinePlayModeParameters initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
#endif

            yield return initOperation;
            if (initOperation.Status == EOperationStatus.Succeed)
            { 
                MicroLogger.Log("资源包初始化成功！");
            }
            else
            {
                MicroLogger.LogError($"资源包：{initOperation.Error}");
                yield break;
            }
            //更新Package版本
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                MicroLogger.Log("Package版本号获取成功！");
            }
            else
            {
                //更新失败
                MicroLogger.LogError($"Package版本号：{operation.Error}");
                yield break;
            }
            var manifestOperation = package.UpdatePackageManifestAsync(operation.PackageVersion);
            yield return manifestOperation;
            if (manifestOperation.Status == EOperationStatus.Succeed)
                MicroLogger.Log("资源装载成功,可以加载资源！");
            else
                MicroLogger.LogError($"资源装载失败：{initOperation.Error}");
            IsInit = true;
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
