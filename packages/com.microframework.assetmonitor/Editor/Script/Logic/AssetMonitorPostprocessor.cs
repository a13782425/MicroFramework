using UnityEditor;

namespace MFramework.AssetMonitor
{
    public class AssetMonitorPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// 监听资源导入
        /// </summary>
        /// <param name="importedAssets">资源导入,修改等(有guid)</param>
        /// <param name="deletedAssets">资源删除(有guid) , 但是File.Exists = false</param>
        /// <param name="movedAssets">移动后的资源(有guid)</param>
        /// <param name="movedFromAssetPaths">移动前的资源(无guid)</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            AssetMonitorTools.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }
    }
}
