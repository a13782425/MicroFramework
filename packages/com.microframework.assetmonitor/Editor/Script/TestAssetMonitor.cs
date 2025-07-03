using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MFramework.AssetMonitor
{
    internal class TestAssetMonitor : IAssetMonitorWatcher
    {
        public string WatchPath => "Assets/Monitor/aaa.txt";

        public string Name => "测试";
        public string Description => "";

        public IEnumerable<string> OnAssetChanged(string filePath)
        {
            return File.ReadAllLines(filePath);
        }
    }
    internal class Test2AssetMonitor : IAssetMonitorWatcher
    {
        public string WatchPath => "Assets/Monitor";

        public string Name => "测试2";

        public string Description => "";

        public IEnumerable<string> OnAssetChanged(string filePath)
        {
            Debug.LogError(filePath);
            return new List<string>();// File.ReadAllLines(filePath);
        }
    }

    internal class Texture2AssetMonitor : IAssetMonitorVerifier
    {
        public string Name => "图片2幂验证";

        public string Description => "";

        //public string VerifyPath => "Assets/20250115-150558.jpg";
        public string VerifyPath => "*.jpg|*.jpeg";

        public bool Verify(AssetInfoRecord record)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Texture>(record.AssetPath);
            if (obj == null)
                return false;
            return s_isPowerOfTwo(obj.width) && s_isPowerOfTwo(obj.height);
        }
        private static bool s_isPowerOfTwo(int n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }
    }
    //internal class Texture3AssetMonitor : IAssetMonitorVerifier
    //{
    //    public string Name => "图片必失败验证";

    //    public string Description => "";

    //    public string VerifyPath => "Assets/20250115-150558.jpg";

    //    public bool Verify(AssetInfoRecord record)
    //    {
    //        return false;
    //    }
    //}
}
