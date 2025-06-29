using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控工具类
    /// </summary>
    internal static class AssetMonitorTools
    {
        // 空字符串数组
        private readonly static string[] StringEmpty = new string[0];

        /// <summary>
        /// 一些特殊路径
        /// </summary>
        private readonly static string[] s_specialPaths = new string[] {
            "Resources/unity_builtin_extra",
            "Library/unity default resources",
            "Library/unity editor resources",
            "Packages"
        };

        /// <summary>
        /// 特殊的GUID
        /// </summary>
        internal const string SPECIAL_GUID = "--------------------------------";
        /// <summary>
        /// 特殊的文件夹
        /// </summary>
        internal const string SPECIAL_FOLDER = "Packages";

        internal static Vector2 ButtonSize = new Vector2(30f, 16f);                   //按钮大小
        internal static Color RefBtnColor = new Color(0, 0.75f, 0, 0.5f);               //引用按钮颜色
        internal static Color DepBtnColor = new Color(0.75f, 0.5f, 0, 0.5f);         //被引用按钮颜色

        #region InitializeOnLoad

        [InitializeOnLoadMethod]
        static void InitiallizeOnLoad()
        {
            s_initSearchType();
            EditorApplication.projectWindowItemOnGUI -= m_onProjectWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += m_onProjectWindowItemOnGUI;
            AssetMonitorConfig.Instance.RefreshConfig();
        }

        /// <summary>
		/// 自定义资源的GUI事件处理方法
		/// </summary>
		private static void m_onProjectWindowItemOnGUI(string guid, Rect rect)
        {
            if (!CheckGuid(guid, false))
                return;
            if (!AssetMonitorConfig.Instance.IsInitialized)
                return;
            //测试阶段先注释掉
            //if (!AssetMonitorConfig.Instance.IsOpen)
            //    return;

            var record = GetRecordByGuid(guid, false);
            if (record == null)
                return;

            Color color = GUI.contentColor;
            {
                Rect depRect = rect;
                depRect.center -= new Vector2(ButtonSize.x * 2, 0);
                depRect.xMin = depRect.xMax - ButtonSize.x;
                depRect.yMax = depRect.yMin + ButtonSize.y;

                Rect refRect = depRect;
                refRect.center -= new Vector2(ButtonSize.x, 0);
                refRect.x -= 2;
                depRect.x -= 2;
                GUI.contentColor = DepBtnColor;
                if (GUI.Button(depRect, FormatRefCount(record.DependencyRelations.Count), "Tab onlyOne"))
                {
                    //按钮点击
                }

                GUI.contentColor = RefBtnColor;
                if (GUI.Button(refRect, FormatRefCount(record.ReferenceRelations.Count), "Tab onlyOne"))
                {
                    //按钮点击
                }
            }
            GUI.contentColor = color;
        }

        #endregion

        #region Tools

        /// <summary>
        /// 获取格式化后的引用数量
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string FormatRefCount(int count) => count > 99 ? "99+" : count.ToString();

        /// <summary>
        /// 格式化字节大小
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion

        #region Search

        private static void s_initSearchType()
        {
            foreach (Type item in TypeCache.GetTypesDerivedFrom<IAssetMonitorSearcher>())
            {
                if (item.IsAbstract || item.IsInterface || item.IsGenericType || item.IsNested || item.IsValueType)
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.Name} is not a valid searcher type.");
                    continue;
                }
                ConstructorInfo[] constructors = item.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                bool hasNonParamCtor = false;
                foreach (var constructor in constructors)
                {
                    if (constructor.GetParameters().Length == 0)
                    {
                        hasNonParamCtor = true;
                        break;
                    }
                }
                if (!hasNonParamCtor)
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.FullName} does not implement non-parameter constructor");
                    continue;
                }
                AssetMonitorConfig.Instance.SearcherTypes.Add(item.FullName, item);
            }
        }

        /// <summary>
        /// 根据参数获取搜索器
        /// </summary>
        /// <param name="opts"></param>
        /// <returns></returns>
        internal static SearcherInfo GetSearcherInfoByOption(string option)
        {
            option = option ?? "";
            option = option.ToLower();
            foreach (var item in AssetMonitorConfig.Instance.SearcherInfoDict)
            {
                if (!item.Value.IsEnabled)
                    continue;
                if (item.Value.Searcher == null)
                    continue;
                if (item.Value.Searcher.SearchOptions.Contains(option))
                    return item.Value;
            }
            return default;
        }

        #endregion

        #region Yaml
        // yaml文件头前缀
        private static string _yamlPrefix = "%YAML";
        // yaml文件头
        private static byte[] _yamlHeadBuffer = new byte[_yamlPrefix.Length];
        //fileID
        private static string _fileIDReg = string.Format("(?<{0}>[0-9-]+)", "fileID");
        //获取guid 的正则
        private static string _guidReg = string.Format("(?<{0}>[a-z0-9]{{{1},{2}}})", "guid", GUID_LENGTH, GUID_LENGTH);
        //获取类型的正则
        private static string _typeReg = string.Format("(?<{0}>[0-9]+)", "type");
        //通用依赖解析正则 {\s*fileID\s*:\s*(?<fileID>[0-9-]+)\s*,\s*guid\s*:\s*(?<guid>[a-z0-9]{32,32})\s*,\s*type\s*:\s*(?<type>[0-9]+)\s*}
        public static string normalGuidPattern = string.Join(@"\s*", "{", "fileID", ":", _fileIDReg, ",", "guid", ":", _guidReg, ",", "type", ":", _typeReg, "}");
        /// <summary>
        /// 判断是不是yaml文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsYamlFile(string path)
        {
            if (Directory.Exists(path))
                return false;
            bool flag = false;

            using FileStream fs = File.OpenRead(path);
            fs.Read(_yamlHeadBuffer, 0, _yamlHeadBuffer.Length);

            flag = System.Text.Encoding.UTF8.GetString(_yamlHeadBuffer) == _yamlPrefix;
            return flag;
        }

        /// <summary>
        /// 根据Yaml获取对应的获取依赖
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Guid[]</returns>
        public static string[] FindDependenciesByYaml(string path)
        {
            if (!IsYamlFile(path))
                return StringEmpty;
            HashSet<string> guids = new HashSet<string>();
            MatchCollection matchs = Regex.Matches(File.ReadAllText(path), normalGuidPattern);
            foreach (Match match in matchs)
            {
                string val = match.Groups["guid"].Value;
                if (CheckGuid(val) && !guids.Contains(val))
                {
                    guids.Add(val);
                }
            }
            return guids.ToArray();
        }

        #endregion

        #region GUID

        private const int GUID_LENGTH = 32;
        public static bool CheckGuid(string guid, bool isLogError = true)
        {
            if (guid.Length != GUID_LENGTH)
            {
                if (isLogError)
                    UnityDebug.LogError("error guid:" + guid);
                return false;
            }

            if (guid == "00000000000000000000000000000000")
                return false;

            return true;
        }

        /// <summary>
        /// 根据Path获取GUID
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string PathToGuid(string path)
        {
            string guid = string.Empty;
            if (!CheckPath(path))
                return string.Empty;
            path = FormatPath(path);
            switch (s_getAssetKind(path))
            {
                case AssetKind.SpecialFolder:
                    return SPECIAL_GUID;
                default:
                    return AssetDatabase.AssetPathToGUID(path);
            }
        }

        /// <summary>
        /// 根据GUID获取Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GuidToPath(string guid)
        {
            if (guid == SPECIAL_GUID)
                return SPECIAL_FOLDER;
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        public static bool IsMissingGUID(string guid)
        {
            string path = GuidToPath(guid);

            if (string.IsNullOrEmpty(path))
                return true;

            if (File.Exists(path) || Directory.Exists(path))
                return false;

            if (s_specialPaths.Contains(path))
                return false;
            return true;
        }

        #endregion

        #region Path

        /// <summary>
        /// 检查路径是否合规
        /// </summary>
        /// <returns></returns>
        public static bool CheckPath(string path)
        {
            // 特殊路径直接返回true
            if (s_specialPaths.Contains(path))
                return true;

            path = FormatPath(path);
            return path.StartsWith("Assets") || path.StartsWith("Packages");
        }

        /// <summary>
        /// 格式化路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FormatPath(string path)
        {
            if (s_specialPaths.Contains(path))
                return path;

            return string.IsNullOrWhiteSpace(path) ? path : FileUtil.GetProjectRelativePath(Path.GetFullPath(path).Replace('\\', '/'));
        }

        public static Texture2D GetIconByPath(string path)
        {
            if (path == SPECIAL_FOLDER)
                return AssetDatabase.GetCachedIcon("Assets") as Texture2D;
            Texture2D tex = AssetDatabase.GetCachedIcon(path) as Texture2D;
            if (!tex)
                tex = InternalEditorUtility.GetIconForFile(path);
            if (!tex)
                tex = s_createDefaultIcon();
            return tex;

            static Texture2D s_createDefaultIcon()
            {
                // 创建一个简单的默认图标
                var defaultIcon = new Texture2D(16, 16);
                var pixels = new Color32[16 * 16];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 128, 128, 255);
                }
                defaultIcon.SetPixels32(pixels);
                defaultIcon.Apply();
                return defaultIcon;
            }
        }

        public static Texture2D GetIconByGuid(string guid) => GetIconByPath(GuidToPath(guid));


        #endregion

        #region 资源处理

        internal static void OnPostprocessAllAssets(string[] importedAssets = null, string[] deletedAssets = null, string[] movedAssets = null, string[] movedFromAssetPaths = null)
        {
            if (!AssetMonitorConfig.Instance.IsOpen)
                return;
            importedAssets = importedAssets ?? StringEmpty;
            deletedAssets = deletedAssets ?? StringEmpty;
            movedAssets = movedAssets ?? StringEmpty;
            movedFromAssetPaths = movedFromAssetPaths ?? StringEmpty;
            //ParseFiles(importedAssets);
            //ParseFiles(deletedAssets);
            //ParseFiles(movedAssets);
            //ParseFiles(movedFromAssetPaths);

        }
        internal static void InitFiles()
        {
            InitFiles(AssetDatabase.GetAllAssetPaths());
        }
        internal static void InitFiles(string[] files)
        {
            ParseFiles(files);
            if (!AssetMonitorConfig.Instance.PathAssetRecordDict.ContainsKey("Packages"))
            {
                AssetInfoRecord.Create(SPECIAL_GUID, "Packages", AssetKind.SpecialFolder);
            }
            foreach (var item in AssetMonitorConfig.Instance.GuidAssetRecordDict.Keys.ToList())
            {
                if (AssetMonitorConfig.Instance.GuidAssetRecordDict.TryGetValue(item, out var record))
                {
                    record.UpdateDepIfNeeded();
                    record.RefreshTree();
                }

            }
            AssetMonitorConfig.Save();
        }
        internal static void ParseFiles(string[] files)
        {
            if (files.Length == 0)
                return;
            for (int i = 0; i < files.Length; i++)
            {
                s_recordFile(files[i]);
            }
        }


        /// <summary>
        /// 获取资源类型
        /// </summary>
        /// <param name="guid"></param>
        internal static string GetAssetTypeByGuid(string guid)
        {
            string path = GuidToPath(guid);
            if (path == SPECIAL_FOLDER)
                return "Packages";
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return type != null ? type.Name : IsMissingGUID(guid) ? "Missing" : "Unity";
        }
        /// <summary>
        /// 获取资源类型
        /// </summary>
        /// <param name="path"></param>
        internal static string GetAssetTypeByPath(string path) => GetAssetTypeByGuid(PathToGuid(path));


        private static void s_recordFile(string path)
        {
            if (!CheckPath(path))
                return;
            string guid = PathToGuid(path);
            if (!CheckGuid(guid))
                return;
            if (!AssetMonitorConfig.Instance.GuidAssetRecordDict.ContainsKey(guid))
            {
                AssetInfoRecord.Create(guid, FormatPath(path), s_getAssetKind(path));
            }
        }

        //internal static void ParseFile(string path)
        //{
        //    if (!CheckPathInAsset(path))
        //        return;
        //    string guid = AssetDatabase.AssetPathToGUID(path);
        //    if (!CheckGuid(guid))
        //        return;
        //    if (!AssetMonitorConfig.Instance.GuidAssetRecordDict.ContainsKey(guid))
        //    {
        //        AssetInfoRecord.Create(guid, path, s_getAssetKind(path));
        //    }
        //}

        private static AssetKind s_getAssetKind(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                if (s_specialPaths.Contains(path))
                    return AssetKind.SpecialRegular;
                else
                    return AssetKind.Regular;
            else if (path == SPECIAL_FOLDER)
                return AssetKind.SpecialFolder;
            else
                return AssetKind.Folder;
        }

        #endregion

        #region AssetInfoRecord

        /// <summary>
        /// 根据guid获取AssetInfoRecord
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        internal static AssetInfoRecord GetRecordByGuid(string guid, bool isCreate = true)
        {
            AssetInfoRecord record = null;
            if (!CheckGuid(guid))
                return record;
            if (AssetMonitorConfig.Instance.GuidAssetRecordDict.TryGetValue(guid, out record))
                return record;
            if (!isCreate)
                return record;
            string path = GuidToPath(guid);
            if (!CheckPath(path))
                return record;
            record = AssetInfoRecord.Create(guid, FormatPath(path), s_getAssetKind(path));
            return record;
        }

        /// <summary>
        /// 通过Path获取AssetInfoRecord
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static AssetInfoRecord GetRecordByPath(string path, bool isCreate = true) => GetRecordByGuid(PathToGuid(path), isCreate);

        #endregion

        #region AssetMonitorInfo

        ///// <summary>
        ///// 获取AssetMonitorInfo
        ///// </summary>
        ///// <param name="guid"></param>
        ///// <returns></returns>
        //public static AssetMonitorInfo GetAssetMonitorInfoByGuid(string guid)
        //{
        //    if (AssetMonitorConfig.Instance.AssetInfoDict.TryGetValue(guid, out AssetMonitorInfo monitorInfo))
        //    {
        //        return monitorInfo;
        //    }
        //    string path = AssetDatabase.GUIDToAssetPath(guid);
        //    monitorInfo = AssetMonitorInfo.Create(guid, FormatPath(path));
        //    AssetMonitorConfig.Instance.AssetInfoDict.Add(guid, monitorInfo);
        //    return monitorInfo;
        //}

        #endregion

    }
}
