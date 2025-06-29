using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.AssetMonitor
{
    partial class AssetMonitorManager
    {
        private static readonly string AssetsFolderPath = Application.dataPath;
        private static readonly string FullProjectPath = Path.GetFullPath(Path.Combine(AssetsFolderPath, "../"));
        private static readonly int AssetsFolderIndex = FullProjectPath.Length - 1;
        /// <summary>
        /// 斜杠转换
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string s_enforceSlashes(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Replace('\\', '/');
        }

        /// <summary>
        /// 获取项目相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string s_getProjectRelativePath(string path)
        {
            if (!Path.IsPathRooted(path))
                return path;

            if (Path.GetFullPath(path).StartsWith(FullProjectPath))
            {
                return path.Substring(AssetsFolderIndex + 1);
            }

            return path;
        }
    }
}
