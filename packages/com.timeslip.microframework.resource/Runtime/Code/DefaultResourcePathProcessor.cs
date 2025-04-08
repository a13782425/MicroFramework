using System;
using System.Collections.Generic;
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
        public string GetResourcePath(string originPath)
        {
            Debug.LogError("原路径：" + originPath);
            int index = originPath.LastIndexOf("Resources/");
            if (index == -1)
            {
                return null;
            }
            else
            {
                string path = originPath.Substring(index + 10);
                Debug.LogError("新路径：" + path);
                return path;
            }

        }

        public bool IsValid(string originPath)
        {
            return originPath.Contains("Resources/");
        }
    }
}
