using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 默认资源路径处理器
    /// </summary>

    internal class DefaultResourcePathProcessor : IResourcePathProcessor
    {
        private const string RESOURCE_ROOT = "Resources";

        public string GetAssetPath(string originPath)
        {
            int index = originPath.LastIndexOf(RESOURCE_ROOT);
            if (index == -1)
            {
                return originPath;
            }
            else
            {
                string path = originPath.Substring(index + RESOURCE_ROOT.Length + 1).Replace("\\", "/");
                string ext = Path.GetExtension(path);
                if (ext != null && ext.Length > 0)
                    path = path.Substring(0, path.Length - ext.Length);
                return path;
            }

        }

        public bool IsValid(string originPath)
        {
            return originPath.LastIndexOf(RESOURCE_ROOT) != -1;
        }
    }
}
