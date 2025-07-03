using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static MFramework.AssetMonitor.AssetMonitorConst;
using UnityDebug = UnityEngine.Debug;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控工具类
    /// </summary>
    internal static class AssetMonitorTools
    {

        /// <summary>
        /// 获取Texture2D在Unity中的大小
        /// </summary>
        private static MethodInfo GetTexture2DUnitySize;

        // 空字符串数组
        internal readonly static string[] StringEmpty = new string[0];

        /// <summary>
        /// 一些特殊文件路径
        /// </summary>
        private readonly static string[] s_specialFiles = new string[] {
            "Resources/unity_builtin_extra",
            "Library/unity default resources",
            "Library/unity editor resources",
        };

        private readonly static string[] s_ignoreInitFolder = new string[]
        {
            "Library",
            "Temp",
            "ProjectSettings",
            "UserSettings"
        };

        private readonly static Vector2 ButtonSize = new Vector2(30f, 16f);                   //按钮大小
        private readonly static Color RefBtnColor = new Color(0, 0.75f, 0, 0.5f);               //引用按钮颜色
        private readonly static Color DepBtnColor = new Color(0.75f, 0.5f, 0, 0.5f);         //被引用按钮颜色

        private readonly static List<VerifierInfo> s_verifierInfoList = new List<VerifierInfo>(); //验证器信息缓存列表

        #region InitializeOnLoad

        [InitializeOnLoadMethod]
        static void InitiallizeOnLoad()
        {
            GetTexture2DUnitySize = typeof(TextureImporter).Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetStorageMemorySizeLong", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            s_initCommandType();
            s_initSearchType();
            s_initWatcherType();
            s_initVerifierType();
            EditorApplication.projectWindowItemOnGUI -= m_onProjectWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += m_onProjectWindowItemOnGUI;
            AssetMonitorConfig.Instance.RefreshConfig();
        }

        [MenuItem("Tools/资源监控")]
        private static AssetMonitorWindow ShowAssetMonitorWindow()
        {
            var window = EditorWindow.GetWindow<AssetMonitorWindow>();
            window.titleContent = new GUIContent("资源监控");
            window.minSize = new Vector2(640, 480);
            window.Show();
            return window;
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
            if (!AssetMonitorConfig.Instance.ShowInProject)
                return;

            var record = GetRecordByGuid(guid, false);
            if (record == null)
                return;

            Color color = GUI.contentColor;
            {
                Rect depRect = rect;
                //depRect.center -= new Vector2(ButtonSize.x, 0);
                depRect.xMin = depRect.xMax - ButtonSize.x;
                depRect.yMax = depRect.yMin + ButtonSize.y;

                Rect refRect = depRect;
                refRect.center -= new Vector2(ButtonSize.x, 0);
                depRect.x -= 2;
                refRect.x -= 4;
                GUI.contentColor = DepBtnColor;

                if (record.DependencyRelations.Count > 0 || AssetMonitorConfig.Instance.ShowEmptyReference)
                {
                    if (GUI.Button(depRect, FormatRefCount(record.DependencyRelations.Count), "Tab onlyOne"))
                    {
                        //按钮点击
                        SetSelectAsset(guid, false);
                    }
                }
                GUI.contentColor = RefBtnColor;
                if (record.ReferenceRelations.Count > 0 || AssetMonitorConfig.Instance.ShowEmptyReference)
                {
                    if (GUI.Button(refRect, FormatRefCount(record.ReferenceRelations.Count), "Tab onlyOne"))
                    {
                        //按钮点击
                        SetSelectAsset(guid);
                    }
                }
            }
            GUI.contentColor = color;
        }

        /// <summary>
        /// 初始化命令类型
        /// </summary>
        private static void s_initCommandType()
        {
            foreach (Type item in TypeCache.GetTypesDerivedFrom<IAssetMonitorCommand>())
            {
                if (item.IsAbstract || item.IsInterface || item.IsGenericType || item.IsNested || item.IsValueType)
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.Name} is not a valid command type.");
                    continue;
                }
                if (!m_checkConstructor(item))
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.FullName} does not implement non-parameter constructor");
                    continue;
                }
                AssetMonitorConfig.Instance.CommandTypeDict.Add(item.FullName, item);
            }
        }
        /// <summary>
        /// 初始化搜索类型
        /// </summary>
        private static void s_initSearchType()
        {
            foreach (Type item in TypeCache.GetTypesDerivedFrom<IAssetMonitorSearcher>())
            {
                if (item.IsAbstract || item.IsInterface || item.IsGenericType || item.IsNested || item.IsValueType)
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.Name} is not a valid searcher type.");
                    continue;
                }
                if (!m_checkConstructor(item))
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.FullName} does not implement non-parameter constructor");
                    continue;
                }
                AssetMonitorConfig.Instance.SearcherTypeDict.Add(item.FullName, item);
            }
        }

        /// <summary>
        /// 初始化检测器类型
        /// </summary>
        private static void s_initWatcherType()
        {
            foreach (Type item in TypeCache.GetTypesDerivedFrom<IAssetMonitorWatcher>())
            {
                if (item.IsAbstract || item.IsInterface || item.IsGenericType || item.IsNested || item.IsValueType)
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.Name} is not a valid monitor type.");
                    continue;
                }
                if (!m_checkConstructor(item))
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.FullName} does not implement non-parameter constructor");
                    continue;
                }
                AssetMonitorConfig.Instance.WatcherTypeDict.Add(item.FullName, item);
            }
        }

        /// <summary>
        /// 初始化验证类型
        /// </summary>
        private static void s_initVerifierType()
        {
            foreach (Type item in TypeCache.GetTypesDerivedFrom<IAssetMonitorVerifier>())
            {
                if (item.IsAbstract || item.IsInterface || item.IsGenericType || item.IsNested || item.IsValueType)
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.Name} is not a valid verifier type.");
                    continue;
                }
                if (!m_checkConstructor(item))
                {
                    UnityDebug.LogWarning($"[AssetMonitor] {item.FullName} does not implement non-parameter constructor");
                    continue;
                }
                AssetMonitorConfig.Instance.VerifierTypeDict.Add(item.FullName, item);
            }
        }

        private static bool m_checkConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasNonParamCtor = false;
            foreach (var constructor in constructors)
            {
                if (constructor.GetParameters().Length == 0)
                {
                    hasNonParamCtor = true;
                    break;
                }
            }
            return hasNonParamCtor;
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
        public static string FormatSize(long bytes) => EditorUtility.FormatBytes(bytes);

        /// <summary>
        /// 是否是文件夹
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsFolder(this AssetKind kind) => (AssetKind.Folder & kind) != AssetKind.None;
        /// <summary>
        /// 是否是资源
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsAsset(this AssetKind kind) => !kind.IsFolder();

        /// <summary>
        /// 是否是忽略引用计算的
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsIngoreReference(this AssetKind kind) => (AssetKind.IgnoreReference & kind) != AssetKind.None;

        /// <summary>
        /// 显示资源监控窗口
        /// </summary>
        /// <param name="assetGuid">显示选项</param>
        /// <param name="showReference">显示引用还是依赖</param>
        public static void SetSelectAsset(string assetGuid, bool showReference = true)
        {
            AssetMonitorWindow window = ShowAssetMonitorWindow();
            window.SelectAsset(assetGuid, showReference);
        }

        /// <summary>
        /// 获取一个资源大小
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static long GetAssetUnitySize(AssetInfoRecord record)
        {
            switch (record.AssetType)
            {
                case "Texture2D":
                    return (long)GetTexture2DUnitySize.Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<Texture>(record.AssetPath) });
                default:
                    return record.DiskSize;
            }

        }

        /// <summary>
        /// 根据参数获取搜索器
        /// </summary>
        /// <param name="opts"></param>
        /// <returns></returns>
        internal static WatcherInfo GetWatcherInfoByAssetPath(string assetPath)
        {
            if (!CheckAssetPath(assetPath))
                return default;
            WatcherInfo monitorInfo = default;
            int lastIndex = -1;
            string extension = Path.GetExtension(assetPath).ToLower();
            foreach (var item in AssetMonitorConfig.Instance.WatcherInfoDict)
            {
                var info = item.Value;
                if (!info.IsEnabled)
                    continue;
                if (info.Watcher == null)
                    continue;
                if (info.IsExtension)
                {
                    if (monitorInfo != null)
                        continue;
                    if (info.Extensions.Contains(extension))
                        monitorInfo = info;
                }
                else
                {
                    string monitorPath = info.Watcher.WatchPath;
                    int index = s_compareLastIndex(assetPath, monitorPath);
                    if (index == -1)
                        continue;
                    if (index > lastIndex && index >= monitorPath.Length - 1)
                    {
                        monitorInfo = info;
                        lastIndex = index;
                    }
                }
            }
            return monitorInfo;
        }

        /// <summary>
        /// 判断一个资源路径是否匹配当前的观察者
        /// </summary>
        /// <param name="watcherInfo"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        internal static bool IsMatchWatcherInfo(WatcherInfo watcherInfo, string assetPath)
        {
            if (!watcherInfo.IsEnabled)
                return false;
            return GetWatcherInfoByAssetPath(assetPath) == watcherInfo;
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
                if (item.Value.Searcher.SearchPrefixes.Contains(option))
                    return item.Value;
            }
            return default;
        }
        /// <summary>
        /// 根据参数获取资源验证器
        /// </summary>
        /// <param name="opts"></param>
        /// <returns></returns>
        internal static List<VerifierInfo> GetVerifierInfoByAssetPath(string assetPath)
        {
            if (!CheckAssetPath(assetPath))
                return default;
            string extension = Path.GetExtension(assetPath).ToLower();
            s_verifierInfoList.Clear();
            foreach (var item in AssetMonitorConfig.Instance.VerifierInfoDict)
            {
                var info = item.Value;
                if (!info.IsEnabled)
                    continue;
                if (info.Verifier == null)
                    continue;
                if (info.IsExtension)
                {
                    if (info.Extensions.Contains(extension))
                        s_verifierInfoList.Add(info);
                }
                else
                {
                    string verifyPath = info.Verifier.VerifyPath;
                    int index = s_compareLastIndex(assetPath, verifyPath);
                    if (index == -1)
                        continue;
                    if (index < verifyPath.Length - 1) //没有匹配上
                        continue;
                    s_verifierInfoList.Add(info);
                }
            }
            return s_verifierInfoList;


        }

        /// <summary>
        /// 根据类型获取资源验证器
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static VerifierInfo GetVerifierInfoByType(string typeName)
        {
            if (AssetMonitorConfig.Instance.VerifierInfoDict.TryGetValue(typeName, out VerifierInfo info))
                return info;
            return null;
        }
        /// <summary>
        /// 判断一个资源路径是否匹配当前的验证者
        /// </summary>
        /// <param name="watcherInfo"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        internal static bool IsMatchVerifierInfo(VerifierInfo verifierInfo, string assetPath)
        {
            if (!verifierInfo.IsEnabled)
                return false;

            if (verifierInfo.IsExtension)
            {
                return verifierInfo.Extensions.Contains(Path.GetExtension(assetPath).ToLower());
            }
            else
            {
                return s_compareLastIndex(assetPath, verifierInfo.Verifier.VerifyPath) >= verifierInfo.Verifier.VerifyPath.Length - 1;
            }
        }
        /// <summary>
        /// 获取两个字符串的相同部分最后一个字符的索引
        /// </summary>
        /// <param name="originPath"></param>
        /// <param name="targetPath"></param>
        /// <returns>最后一个字符的索引</returns>
        private static int s_compareLastIndex(string originPath, string targetPath)
        {
            int originLength = originPath.Length;
            int targetLength = targetPath.Length;
            if (originLength < targetLength)
                return -1;
            int index = 0;
            for (int i = 0; i < targetLength; i++)
            {
                index = i;
                if (originPath[i] != targetPath[i])
                    break;
            }
            return index;
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
        public static string AssetPathToGuid(string path)
        {
            string guid = string.Empty;
            if (!CheckAssetPath(path))
                return string.Empty;
            path = FormatAssetPath(path);
            if (path == SPECIAL_FOLDER)
                return SPECIAL_GUID;
            return AssetDatabase.AssetPathToGUID(path);
        }

        /// <summary>
        /// 根据GUID获取Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GuidToAssetPath(string guid)
        {
            if (guid == SPECIAL_GUID)
                return SPECIAL_FOLDER;
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        public static bool IsMissingGUID(string guid)
        {
            string path = GuidToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
                return true;

            if (File.Exists(path) || Directory.Exists(path))
                return false;

            if (s_specialFiles.Contains(path))
                return false;
            return true;
        }

        #endregion

        #region Path

        /// <summary>
        /// 检查路径是否合规
        /// </summary>
        /// <returns></returns>
        public static bool CheckAssetPath(string path)
        {
            // 特殊路径直接返回true
            if (s_specialFiles.Contains(path))
                return true;

            path = FormatAssetPath(path);
            return path.StartsWith(ASSET_FOLDER) || path.StartsWith(SPECIAL_FOLDER) || path.StartsWith(LIBRARY_FOLDER);
        }

        /// <summary>
        /// 格式化AssetDatabse路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FormatAssetPath(string path)
        {
            if (s_specialFiles.Contains(path))
                return path;

            return string.IsNullOrWhiteSpace(path) ? path : path.Replace('\\', '/');
        }

        public static Texture2D GetIconByAssetPath(string path)
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

        public static Texture2D GetIconByGuid(string guid) => GetIconByAssetPath(GuidToAssetPath(guid));

        /// <summary>
        /// 格式化成磁盘相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string s_formatDiskPath(string path)
        {
            if (s_specialFiles.Contains(path))
                return path;

            return string.IsNullOrWhiteSpace(path) ? path : FileUtil.GetProjectRelativePath(Path.GetFullPath(path).Replace('\\', '/'));
        }
        #endregion

        #region 资源处理

        internal static void OnPostprocessAllAssets(string[] importedAssets = null, string[] deletedAssets = null, string[] movedAssets = null, string[] movedFromAssetPaths = null)
        {
            if (!AssetMonitorConfig.Instance.IsInitialized)
                return;
            importedAssets = importedAssets ?? StringEmpty;
            deletedAssets = deletedAssets ?? StringEmpty;
            movedAssets = movedAssets ?? StringEmpty;
            //movedFromAssetPaths = movedFromAssetPaths ?? StringEmpty;
            ParseFiles(importedAssets);
            ParseFiles(deletedAssets);
            ParseFiles(movedAssets);
            //ParseFiles(movedFromAssetPaths);
            AssetMonitorConfig.Save();
        }
        internal static void InitFiles()
        {
            if (!AssetMonitorConfig.Instance.PathAssetRecordDict.ContainsKey(SPECIAL_FOLDER))
                AssetInfoRecord.Create(SPECIAL_GUID, SPECIAL_FOLDER, AssetKind.SpecialFolder);
            ParseFiles(AssetDatabase.GetAllAssetPaths());
            AssetMonitorConfig.Save();
        }

        internal static void ParseFiles(string[] files, bool isDelete = false)
        {
            if (files.Length == 0)
                return;
            try
            {
                int total = files.Length;
                for (int i = 0; i < total; i++)
                {
                    EditorUtility.DisplayProgressBar("资源监控", $"正在处理资源 {i + 1}/{total}", (float)(i + 1) / total);
                    s_parseAssetPath(files[i]);
                }
            }
            catch (Exception ex)
            {
                UnityDebug.LogError(ex.ToString());
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }


        /// <summary>
        /// 获取资源类型
        /// </summary>
        /// <param name="guid"></param>
        internal static string GetAssetTypeByGuid(string guid)
        {
            string path = GuidToAssetPath(guid);
            if (path == SPECIAL_FOLDER)
                return "Packages";
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return type != null ? type.Name : IsMissingGUID(guid) ? "Missing" : "Unity";
        }
        /// <summary>
        /// 获取资源类型
        /// </summary>
        /// <param name="path"></param>
        internal static string GetAssetTypeByPath(string path) => GetAssetTypeByGuid(AssetPathToGuid(path));

        private static void s_parseAssetPath(string assetPath, bool isDelete = false)
        {
            string diskPath = s_formatDiskPath(assetPath);
            foreach (var item in s_ignoreInitFolder)
            {
                if (diskPath.StartsWith(item))
                    return;
            }
            if (!CheckAssetPath(assetPath))
                return;
            string guid = AssetPathToGuid(assetPath);
            var record = GetRecordByGuid(guid, true);
            if (record != null)
                record.UpdateIfNeeded();
            //if (string.IsNullOrWhiteSpace(guid))
            //{
            //    //当前guid 被删除
            //    var record = GetRecordByAssetPath(assetPath);
            //    if (record != null)
            //        record.UpdateIfNeeded();
            //}
            //else
            //{
            //    var record = GetRecordByGuid(guid, true);
            //    if (record != null)
            //        record.UpdateIfNeeded();
            //}
        }

        private static AssetKind s_getAssetKind(string path)
        {
            string diskPath = s_formatDiskPath(path);
            var kind = AssetKind.NormalAsset;
            if (!AssetDatabase.IsValidFolder(diskPath))
            {
                kind = diskPath switch
                {
                    SPECIAL_FOLDER => AssetKind.SpecialFolder,
                    LIBRARY_FOLDER => AssetKind.LibraryFolder,
                    var p when s_specialFiles.Contains(p) => AssetKind.SpecialAsset,
                    var p when p.StartsWith(LIBRARY_FOLDER) => AssetKind.LibraryAsset,
                    _ => AssetKind.NormalAsset
                };
            }
            else
            {
                kind = diskPath switch
                {
                    SPECIAL_FOLDER => AssetKind.SpecialFolder,
                    var p when p.StartsWith(LIBRARY_FOLDER) => AssetKind.LibraryFolder,
                    _ => AssetKind.NormalFolder
                };
            }
            return kind;
        }
        #endregion

        #region AssetInfoRecord

        /// <summary>
        /// 根据guid获取AssetInfoRecord
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        internal static AssetInfoRecord GetRecordByGuid(string guid, bool isCreate = false)
        {
            AssetInfoRecord record = null;
            if (!CheckGuid(guid))
                return record;
            if (AssetMonitorConfig.Instance.GuidAssetRecordDict.TryGetValue(guid, out record))
                return record;
            if (!isCreate)
                return record;
            string path = GuidToAssetPath(guid);
            if (!CheckAssetPath(path))
                return record;
            record = AssetInfoRecord.Create(guid, FormatAssetPath(path), s_getAssetKind(path));
            return record;
        }

        /// <summary>
        /// 通过Path获取AssetInfoRecord
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        internal static AssetInfoRecord GetRecordByAssetPath(string assetPath, bool isCreate = false)
        {
            assetPath = FormatAssetPath(assetPath);
            AssetInfoRecord record = null;
            if (!CheckAssetPath(assetPath))
                return record;
            if (AssetMonitorConfig.Instance.PathAssetRecordDict.TryGetValue(assetPath, out record))
                return record;
            if (!isCreate)
                return record;
            string guid = AssetPathToGuid(assetPath);
            if (!CheckGuid(guid))
                return record;
            record = AssetInfoRecord.Create(guid, FormatAssetPath(assetPath), s_getAssetKind(assetPath));
            return record;
        }

        #endregion

    }
}
