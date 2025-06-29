using Codice.Client.BaseCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控管理器
    /// </summary>
    internal static partial class AssetMonitorManager
    {
        //private static readonly Type assetSettingsKindType = typeof(AssetSettingsKind);
        /// <summary>
        /// 构造方法
        /// </summary>
        static AssetMonitorManager()
        {
            //EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        /// <summary>
        /// 同步资源引用信息
        /// </summary>
        public static void SyncAssetsReferences()
        {
            parseFiles(AssetDatabase.GetAllAssetPaths());
            AssetMonitorConfig.Save();

        }

        private static void s_parseFiles(string[] paths)
        { 
        }

        /// <summary>
        /// 解析资源文件
        /// </summary>
        /// <param name="paths"></param>
        private static void parseFiles(string[] allAssetPaths)
        {
            //try
            //{
            //    var validNewAssets = new List<RawAssetMonitorInfo>(allAssetPaths.Length);
            //    var validExistingAssetsGuids = new HashSet<string>();

            //    foreach (var item in AssetMonitorConfig.Instance.AssetInfoDict)
            //    {
            //        validExistingAssetsGuids.Add(item.Key);
            //    }

            //    foreach (var assetPath in allAssetPaths)
            //    {
            //        var guid = AssetDatabase.AssetPathToGUID(assetPath);
            //        if (validExistingAssetsGuids.Contains(guid))
            //            continue;

            //        AssetKind kind = s_getAssetKind(assetPath);
            //        if (kind == AssetKind.Unsupported) continue;

            //        if (!File.Exists(assetPath)) continue;
            //        if (AssetDatabase.IsValidFolder(assetPath)) continue;

            //        var rawInfo = new RawAssetMonitorInfo
            //        {
            //            path = s_enforceSlashes(assetPath),
            //            guid = guid,
            //            kind = kind,
            //        };
            //        var asset = AssetMonitorInfo.Create(rawInfo);
            //        if (asset != null)
            //        {
            //            AssetMonitorConfig.Instance.AssetInfoDict.Add(guid, asset);
            //        }
            //        validNewAssets.Add(rawInfo);
            //    }
            //    int count = validNewAssets.Count;
            //    for (int i = 0; i < count; i++)
            //    {
            //        RawAssetMonitorInfo raw = validNewAssets[i];
            //        string rawPath = raw.path;
            //        EditorUtility.DisplayProgressBar("资源处理:...", rawPath, i / (float)count);
            //        var settingsKind = raw.kind == AssetKind.Settings ? s_getSettingsKind(rawPath) : AssetSettingsKind.Undefined;
            //        var asset = AssetMonitorTools.GetAssetMonitorInfoByGuid(raw.guid); // AssetMonitorInfo.Create(raw);
            //        if (asset != null)
            //        {
            //            asset.UpdateIfNeeded();
            //        }
            //        //AssetMonitorConfig.Instance.AssetInfoDict.Add(raw.guid, asset);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Debug.LogError(ex.StackTrace);
            //    Debug.LogError(ex.Message);
            //}
            //finally
            //{
            //    EditorUtility.ClearProgressBar();
            //}

        }
        /// <summary>
        /// 解析单一资源文件
        /// </summary>
        /// <param name="paths"></param>
        private static void s_parseFile(string path)
        {

        }

        //private static AssetKind s_getAssetKind(string path)
        //{
        //    if (!Path.IsPathRooted(path))
        //    {
        //        if (path.IndexOf("Assets/", StringComparison.Ordinal) == 0)
        //            return AssetKind.Regular;

        //        if (path.IndexOf("ProjectSettings/", StringComparison.Ordinal) == 0)
        //            return AssetKind.Settings;

        //        if (path.IndexOf("Packages/", StringComparison.Ordinal) == 0)
        //        {
        //            var projectRelativePath = s_enforceSlashes(s_getProjectRelativePath(Path.GetFullPath(path)));
        //            return projectRelativePath.IndexOf("Packages/", StringComparison.Ordinal) == 0 ?
        //                AssetKind.FromEmbeddedPackage :
        //                AssetKind.FromPackage;
        //        }
        //    }
        //    else
        //    {
        //        if (path.IndexOf("/unity/cache/packages/", StringComparison.OrdinalIgnoreCase) > 0)
        //            return AssetKind.FromPackage;
        //    }

        //    return AssetKind.Unsupported;
        //}
        //private static AssetSettingsKind s_getSettingsKind(string assetPath)
        //{
        //    var result = AssetSettingsKind.UnknownSettingAsset;

        //    var fileName = Path.GetFileNameWithoutExtension(assetPath);
        //    if (!string.IsNullOrEmpty(fileName))
        //    {
        //        try
        //        {
        //            result = (AssetSettingsKind)Enum.Parse(assetSettingsKindType, fileName);
        //        }
        //        catch (Exception)
        //        {
        //            // ignored
        //        }
        //    }

        //    return result;
        //}

    }
}
