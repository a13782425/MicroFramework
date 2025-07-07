using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 定义资源的类型，用于标识不同来源或用途的资源。
    /// </summary>
    [Flags]
    [Serializable]
    public enum AssetKind : byte
    {
        /// <summary>
        /// 占位符
        /// </summary>
        None = 0,
        /// <summary>
        /// 常规资源（默认类型）。
        /// </summary>
        NormalAsset = 1 << 1,
        /// <summary>
        /// 文件夹资源。
        /// </summary>
        NormalFolder = 1 << 2,
        /// <summary>
        /// 特殊资源
        /// 有GUID 但是在Unity安装目录的
        /// </summary>
        SpecialAsset = 1 << 3,
        /// <summary>
        /// 特殊文件夹
        /// 没有GUID的 特指Packages
        /// </summary>
        SpecialFolder = 1 << 4,
        /// <summary>
        /// 来自Library的资源，表示该资源来源于Unity的Library文件夹。
        /// </summary>
        LibraryAsset = 1 << 5,
        /// <summary>
        /// 来自Library的文件夹，表示该资源来源于Unity的Library文件夹。
        /// </summary>
        LibraryFolder = 1 << 6,


        /// <summary>
        /// 特殊标识
        /// </summary>
        Special = SpecialAsset | SpecialFolder,
        /// <summary>
        /// 该资源来源于Unity的Library文件夹。
        /// </summary>
        Library = LibraryAsset | LibraryFolder,
        /// <summary>
        /// 文件夹
        /// </summary>
        Folder = NormalFolder | LibraryFolder | SpecialFolder,
        /// <summary>
        /// 文件
        /// </summary>
        Asset = NormalAsset | LibraryAsset | SpecialAsset,

        /// <summary>
        /// 忽略引用计算
        /// </summary>
        IgnoreReference = Library | Special

    }

    /// <summary>
    /// 可序列化
    /// </summary>
    internal interface ISerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    /// <summary>
    /// 资源监控配置类
    /// </summary>
    [Serializable]
    internal sealed class AssetMonitorConfig : ISerializable
    {
        /// <summary>
        /// 配置版本号
        /// </summary>
        internal const string CONFIG_VERSION = "1.0.0.0";
        private static AssetMonitorConfig _instance = default;
        public static AssetMonitorConfig Instance
        {
            get
            {
                if (_instance == null)
                    s_load();
                return _instance;
            }
        }

        #region 序列化

        /// <summary>
        /// 项目页签固定大小
        /// </summary>
        internal float ProjectFiexdSize { get; set; } = 120;

        /// <summary>
        /// 使用Unity下的资源大小
        /// 需要序列化
        /// </summary>
        internal bool UseUnitySize { get; set; } = true;

        /// <summary>
        /// 是否显示空引用
        /// 需要序列化
        /// </summary>
        internal bool ShowEmptyReference { get; set; } = false;

        /// <summary>
        /// 是否在项目文件夹中显示
        /// 需要序列化
        /// </summary>
        internal bool ShowInProject { get; set; } = true;

        /// <summary>
        /// 是否在项目文件夹中选中
        /// 需要序列化
        /// </summary>
        internal bool SelectInProject { get; set; } = true;

        /// <summary>
        /// 是否自动展开树
        /// 需要序列化
        /// </summary>
        internal bool AutoExpandedTree { get; set; } = true;
        /// <summary>
        /// 当前所有资源信息
        /// 需要序列化
        /// </summary>
        private readonly List<AssetInfoRecord> _assetRecords = new List<AssetInfoRecord>();
        /// <summary>
        /// 所有右键指令
        /// 需要序列化
        /// </summary>
        private readonly List<CommandInfo> _commandInfos = new List<CommandInfo>();
        /// <summary>
        /// 所有搜索器
        /// 需要序列化
        /// </summary>
        private readonly List<SearcherInfo> _searcherInfos = new List<SearcherInfo>();
        /// <summary>
        /// 所有检测器
        /// 需要序列化
        /// </summary>
        private readonly List<WatcherInfo> _watcherInfos = new List<WatcherInfo>();
        /// <summary>
        /// 所有资源验证器
        /// 需要序列化
        /// </summary>
        private readonly List<VerifierInfo> _verifierInfos = new List<VerifierInfo>();

        #endregion

        #region 不序列化

        /// <summary>
        /// 是否初始化
        /// 不序列化
        /// </summary>
        internal bool IsInitialized => _assetRecords.Count > 0;

        /// <summary>
        /// 以GUID为Key的资源信息字典
        /// 不序列化
        /// </summary>Tundra
        internal readonly Dictionary<string, AssetInfoRecord> GuidAssetRecordDict = new Dictionary<string, AssetInfoRecord>();
        /// <summary>
        /// 以路径为Key的资源信息字典
        /// 不序列化
        /// </summary>
        internal readonly Dictionary<string, AssetInfoRecord> PathAssetRecordDict = new Dictionary<string, AssetInfoRecord>();
        /// <summary>
        /// 右键指令信息字典
        /// 不序列化
        /// </summary>
        internal readonly Dictionary<string, CommandInfo> CommandInfoDict = new Dictionary<string, CommandInfo>();
        /// <summary>
        /// 搜索器信息字典
        /// 不序列化
        /// </summary>
        internal readonly Dictionary<string, SearcherInfo> SearcherInfoDict = new Dictionary<string, SearcherInfo>();
        /// <summary>
        /// 检测器信息字典
        /// 不序列化
        /// </summary>
        internal readonly Dictionary<string, WatcherInfo> WatcherInfoDict = new Dictionary<string, WatcherInfo>();
        /// <summary>
        /// 验证器信息字典
        /// 不序列化
        /// </summary>
        internal readonly Dictionary<string, VerifierInfo> VerifierInfoDict = new Dictionary<string, VerifierInfo>();

        /// <summary>
        /// 自定义右键指令类型
        /// 不序列化
        /// key: 类型全称 value: Type
        /// </summary>
        internal readonly Dictionary<string, Type> CommandTypeDict = new Dictionary<string, Type>();
        /// <summary>
        /// 搜索器类型信息
        /// 不序列化
        /// key: 类型全称 value: Type
        /// </summary>
        internal readonly Dictionary<string, Type> SearcherTypeDict = new Dictionary<string, Type>();
        /// <summary>
        /// 自定义监视器类型
        /// 不序列化
        /// key: 类型全称 value: Type
        /// </summary>
        internal readonly Dictionary<string, Type> WatcherTypeDict = new Dictionary<string, Type>();
        /// <summary>
        /// 自定义资源校验类型
        /// 不序列化
        /// key: 类型全称 value: Type
        /// </summary>
        internal readonly Dictionary<string, Type> VerifierTypeDict = new Dictionary<string, Type>();

        #endregion

        /// <summary>
        /// 保存资源监控配置
        /// </summary>
        internal static void Save()
        {
            if (!File.Exists(CONFIG_CACHE_PATH))
                File.Create(CONFIG_CACHE_PATH);
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            try
            {
                Instance.Serialize(writer);
                File.WriteAllBytes(CONFIG_CACHE_PATH, stream.ToArray());
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                EditorUtility.DisplayDialog("提示", $"资源监控缓存文件保存错误，请联系作者", "确定");
                File.WriteAllBytes(CONFIG_CACHE_PATH, new byte[0]);
            }
        }

        /// <summary>
        /// 添加一个资源记录
        /// </summary>
        /// <param name="record"></param>
        internal void AddRecord(AssetInfoRecord record)
        {
            if (!_assetRecords.Contains(record))
                _assetRecords.Add(record);
            if (!GuidAssetRecordDict.ContainsKey(record.Guid))
                GuidAssetRecordDict.Add(record.Guid, record);
            if (!PathAssetRecordDict.ContainsKey(record.AssetPath))
                PathAssetRecordDict.Add(record.AssetPath, record);
        }

        /// <summary>
        /// 删除一个资源记录
        /// </summary>
        /// <param name="record"></param>
        internal void RemoveRecord(AssetInfoRecord record)
        {
            if (GuidAssetRecordDict.ContainsKey(record.Guid))
                GuidAssetRecordDict.Remove(record.Guid);
            if (PathAssetRecordDict.ContainsKey(record.AssetPath))
                PathAssetRecordDict.Remove(record.AssetPath);
            _assetRecords.Remove(record);
        }

        /// <summary>
        /// 清空所有记录
        /// </summary>
        internal void ClearAllRecords()
        {
            _assetRecords.Clear();
            GuidAssetRecordDict.Clear();
            PathAssetRecordDict.Clear();
            Save();
        }

        /// <summary>
        /// 添加一个搜索器
        /// </summary>
        /// <param name="info"></param>
        internal void AddSearcher(SearcherInfo info)
        {
            if (!_searcherInfos.Contains(info))
                _searcherInfos.Add(info);
            if (!SearcherInfoDict.ContainsKey(info.TypeName))
                SearcherInfoDict.Add(info.TypeName, info);
        }

        /// <summary>
        /// 添加一个检测器
        /// </summary>
        /// <param name="info"></param>
        internal void AddWatcher(WatcherInfo info)
        {
            if (!_watcherInfos.Contains(info))
                _watcherInfos.Add(info);
            if (!WatcherInfoDict.ContainsKey(info.TypeName))
                WatcherInfoDict.Add(info.TypeName, info);
        }

        /// <summary>
        /// 添加一个右键指令
        /// </summary>
        /// <param name="info"></param>
        internal void AddCommand(CommandInfo info)
        {
            if (!_commandInfos.Contains(info))
                _commandInfos.Add(info);
            if (!CommandInfoDict.ContainsKey(info.TypeName))
                CommandInfoDict.Add(info.TypeName, info);
        }
        /// <summary>
        /// 添加一个资源验证器
        /// </summary>
        /// <param name="info"></param>
        internal void AddVerifier(VerifierInfo info)
        {
            if (!_verifierInfos.Contains(info))
                _verifierInfos.Add(info);
            if (!VerifierInfoDict.ContainsKey(info.TypeName))
                VerifierInfoDict.Add(info.TypeName, info);
        }

        public void Deserialize(BinaryReader reader)
        {
            reader.ReadString(); // 读取配置版本
            ProjectFiexdSize = reader.ReadSingle();
            UseUnitySize = reader.ReadBoolean();
            ShowInProject = reader.ReadBoolean();
            ShowEmptyReference = reader.ReadBoolean();
            SelectInProject = reader.ReadBoolean();
            AutoExpandedTree = reader.ReadBoolean();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                AssetInfoRecord record = new AssetInfoRecord();
                record.Deserialize(reader);
                _assetRecords.Add(record);
                if (!GuidAssetRecordDict.ContainsKey(record.Guid))
                    GuidAssetRecordDict.Add(record.Guid, record);
                if (!PathAssetRecordDict.ContainsKey(record.AssetPath))
                    PathAssetRecordDict.Add(record.AssetPath, record);
            }

            #region 右键菜单
            //右键菜单
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                CommandInfo command = new CommandInfo();
                command.Deserialize(reader);
                _commandInfos.Add(command);
                if (!CommandInfoDict.ContainsKey(command.TypeName))
                    CommandInfoDict.Add(command.TypeName, command);
            }
            #endregion

            #region 搜索器
            //搜索器
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                SearcherInfo searcher = new SearcherInfo();
                searcher.Deserialize(reader);
                _searcherInfos.Add(searcher);
                if (!SearcherInfoDict.ContainsKey(searcher.TypeName))
                    SearcherInfoDict.Add(searcher.TypeName, searcher);
            }
            #endregion

            #region 观察者
            //观察者
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                WatcherInfo watcher = new WatcherInfo();
                watcher.Deserialize(reader);
                _watcherInfos.Add(watcher);
                if (!WatcherInfoDict.ContainsKey(watcher.TypeName))
                    WatcherInfoDict.Add(watcher.TypeName, watcher);
            }
            #endregion

            #region 资源验证
            //资源验证
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                VerifierInfo verifier = new VerifierInfo();
                verifier.Deserialize(reader);
                _verifierInfos.Add(verifier);
                if (!VerifierInfoDict.ContainsKey(verifier.TypeName))
                    VerifierInfoDict.Add(verifier.TypeName, verifier);
            }
            #endregion

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(CONFIG_VERSION); // 版本号
            writer.Write(ProjectFiexdSize);
            writer.Write(UseUnitySize);
            writer.Write(ShowInProject);
            writer.Write(ShowEmptyReference);
            writer.Write(SelectInProject);
            writer.Write(AutoExpandedTree);
            writer.Write(_assetRecords.Count);
            foreach (var record in _assetRecords)
                record.Serialize(writer);
            //右键菜单
            writer.Write(_commandInfos.Count);
            foreach (var commandInfo in _commandInfos)
                commandInfo.Serialize(writer);
            //搜索器
            writer.Write(_searcherInfos.Count);
            foreach (var info in _searcherInfos)
                info.Serialize(writer);
            //观察者
            writer.Write(_watcherInfos.Count);
            foreach (var info in _watcherInfos)
                info.Serialize(writer);
            //资源验证
            writer.Write(_verifierInfos.Count);
            foreach (var info in _verifierInfos)
                info.Serialize(writer);
        }

        /// <summary>
        /// 刷新配置
        /// </summary>
        internal void RefreshConfig()
        {

            #region 右键菜单
            //搜索器
            foreach (var item in CommandTypeDict)
            {
                if (CommandInfoDict.ContainsKey(item.Key))
                    continue;
                CommandInfo.Create(item.Key);
            }
            for (int i = _commandInfos.Count - 1; i >= 0; i--)
            {
                var item = _commandInfos[i];
                if (!CommandTypeDict.ContainsKey(item.TypeName))
                {
                    CommandInfoDict.Remove(item.TypeName);
                    _commandInfos.RemoveAt(i);
                }
            }
            #endregion

            #region 搜索器
            //搜索器
            foreach (var item in SearcherTypeDict)
            {
                if (SearcherInfoDict.ContainsKey(item.Key))
                    continue;
                SearcherInfo.Create(item.Key);
            }
            for (int i = _searcherInfos.Count - 1; i >= 0; i--)
            {
                var item = _searcherInfos[i];
                if (!SearcherTypeDict.ContainsKey(item.TypeName))
                {
                    SearcherInfoDict.Remove(item.TypeName);
                    _searcherInfos.RemoveAt(i);
                }
            }
            #endregion

            #region 观察者
            //观察者
            foreach (var item in WatcherTypeDict)
            {
                if (WatcherInfoDict.ContainsKey(item.Key))
                    continue;
                WatcherInfo info = WatcherInfo.Create(item.Key);
                for (int j = _assetRecords.Count - 1; j >= 0; j--)
                    _assetRecords[j].RefreshWatcherRelation(info);
            }
            for (int i = _watcherInfos.Count - 1; i >= 0; i--)
            {
                WatcherInfo item = _watcherInfos[i];
                if (!WatcherTypeDict.ContainsKey(item.TypeName))
                {
                    WatcherInfoDict.Remove(item.TypeName);
                    _watcherInfos.RemoveAt(i);
                    for (int j = _assetRecords.Count - 1; j >= 0; j--)
                        _assetRecords[j].RemoveWatcherRelation(item);
                }
            }
            #endregion

            #region 资源验证
            // 资源验证
            foreach (var item in VerifierTypeDict)
            {
                if (VerifierInfoDict.ContainsKey(item.Key))
                    continue;
                VerifierInfo info = VerifierInfo.Create(item.Key);
                for (int j = _assetRecords.Count - 1; j >= 0; j--)
                    _assetRecords[j].RefreshVerifyResult(info);
            }
            for (int i = _verifierInfos.Count - 1; i >= 0; i--)
            {
                VerifierInfo item = _verifierInfos[i];
                if (!VerifierTypeDict.ContainsKey(item.TypeName))
                {
                    VerifierInfoDict.Remove(item.TypeName);
                    _verifierInfos.RemoveAt(i);
                    for (int j = _assetRecords.Count - 1; j >= 0; j--)
                        _assetRecords[j].RemoveVerifyResult(item);
                }
            }
            #endregion

            for (int i = _assetRecords.Count - 1; i >= 0; i--)
                _assetRecords[i].RefreshFileSize();
        }

        /// <summary>
        /// 加载资源监控配置
        /// </summary>
        /// <returns></returns>
        private static void s_load()
        {
            if (_instance != null)
                return;
            if (!File.Exists(CONFIG_CACHE_PATH))
                File.Create(CONFIG_CACHE_PATH).Close();
            _instance = new AssetMonitorConfig();
            try
            {
                byte[] bytes = File.ReadAllBytes(CONFIG_CACHE_PATH);
                if (bytes.Length == 0)
                {
                    _instance.m_firstInit();
                    return;
                }
                using var stream = new MemoryStream(bytes);
                using var reader = new BinaryReader(stream);
                string configVersion = reader.ReadString(); // 读取配置版本
                if (configVersion != CONFIG_VERSION)
                {
                    //EditorUtility.DisplayDialog("提示", $"资源监控缓存文件版本错误，请手动刷新资源控制缓存", "确定");
                    _instance.m_firstInit();
                    return;
                }
                reader.BaseStream.Seek(0, SeekOrigin.Begin); // 重置流位置到开头
                _instance.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                EditorUtility.DisplayDialog("提示", $"资源监控缓存文件加载错误，请手动刷新资源控制缓存", "确定");
                _instance = new AssetMonitorConfig();
                _instance.m_firstInit();
            }

        }
        private void m_firstInit()
        {
            ProjectFiexdSize = 120;
            UseUnitySize = true;
            ShowInProject = true;
            ShowEmptyReference = false;
            SelectInProject = true;
            AutoExpandedTree = true;
            _assetRecords.Clear();
            _commandInfos.Clear();
            _searcherInfos.Clear();
            _watcherInfos.Clear();
            _verifierInfos.Clear();
            CommandInfoDict.Clear();
            SearcherInfoDict.Clear();
            WatcherInfoDict.Clear();
            VerifierInfoDict.Clear();
            GuidAssetRecordDict.Clear();
            PathAssetRecordDict.Clear();

            Save();

        }

    }

    /// <summary>
    /// 资源记录
    /// </summary>
    public sealed class AssetInfoRecord : IEquatable<AssetInfoRecord>, ISerializable, ICloneable
    {
        private readonly static string[] UnitySizeTypes = new string[]
        {
            "Texture2D",
        };
        /// <summary>
        /// 资源的唯一标识符
        /// 需要序列化
        /// </summary>
        public string Guid { get; private set; } = "";

        /// <summary>
        /// 资源的完整路径(基于AssetDatabase)
        /// 也是唯一的标识符
        /// 需要序列化
        /// </summary>
        public string AssetPath { get; private set; } = "";

        /// <summary>
        /// 是否处于AB包中
        /// 需要序列化
        /// </summary>
        public string AbName { get; private set; } = "";

        /// <summary>
        /// 资源类型
        /// 需要序列化
        /// </summary>
        public string AssetType { get; private set; } = "";

        /// <summary>
        /// 资源类型
        /// 需要序列化
        /// </summary>
        public AssetKind Kind { get; private set; } = AssetKind.NormalAsset;

        /// <summary>
        /// 资源大小
        /// Byte
        /// 需要序列化
        /// </summary>
        public long DiskSize { get; private set; } = 0;

        /// <summary>
        /// Unity大小
        /// 需要序列化
        /// </summary>
        public long UnitySize { get; private set; } = 0;

        /// <summary>
        /// 获取资源最后修改的哈希值。
        /// 需要序列化
        /// </summary>
        public long LastAssetHash { get; private set; } = 0;

        /// <summary>
        /// 获取资源Meta文件最后修改的哈希值。
        /// 需要序列化
        /// </summary>
        public long LastMetaHash { get; private set; } = 0;

        /// <summary>
        /// 依赖于【此资源】的其它资源的GUID列表。
        /// (关系: 其它资源 -> 此资源)
        /// 需要序列化
        /// </summary>
        public readonly List<RelationInfo> DependencyRelations = new List<RelationInfo>();
        /// <summary>
        /// 依赖的guid缓存
        /// 不序列化
        /// </summary>
        private readonly HashSet<string> _dependencyGuidCache = new HashSet<string>();

        /// <summary>
        /// 此资源引用的【其它资源】的GUID列表。
        /// (关系: 此资源 -> 其它资源)
        /// 需要序列化
        /// </summary>
        public readonly List<RelationInfo> ReferenceRelations = new List<RelationInfo>();
        /// <summary>
        /// 引用的guid缓存
        /// 不序列化
        /// </summary>
        private readonly HashSet<string> _referenceGuidCache = new HashSet<string>();

        /// <summary>
        /// 验证器的结果
        /// 需要序列化
        /// </summary>
        internal readonly List<VerifyResult> VerifyResults = new List<VerifyResult>();


        private string _assetName = "";
        /// <summary>
        /// 资源名
        /// 不序列化
        /// </summary>
        public string AssetName => _assetName;

        private string _parentPath = "";
        /// <summary>
        /// 父路径
        /// 不序列化
        /// </summary>
        public string ParentPath => _parentPath;
        /// <summary>
        /// 是否是根节点
        /// 不序列化
        /// </summary>
        public bool IsRoot => string.IsNullOrWhiteSpace(ParentPath) || _isClone;

        /// <summary>
        /// 是否是目录
        /// 不序列化
        /// </summary>
        public bool IsFolder => Kind.IsFolder();

        /// <summary>
        /// 资源大小
        /// </summary>
        public long Size => IsFolder ? DiskSize : AssetMonitorConfig.Instance.UseUnitySize ? (UnitySizeTypes.Contains(AssetType) ? UnitySize : DiskSize) : DiskSize;

        /// <summary>
        /// 包含资源数量
        /// 不序列化
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// 是否是克隆
        /// 不序列化
        /// </summary>
        private bool _isClone = false;

        private AssetInfoRecord _parent = null;
        /// <summary>
        /// 父节点
        /// 不序列化
        /// </summary>
        public AssetInfoRecord Parent
        {
            get
            {
                if (IsRoot)
                    return _parent;
                if (_parent == null && !_isClone)
                    _parent = AssetMonitorTools.GetRecordByAssetPath(ParentPath, true);
                return _parent;
            }
            internal set
            {
                _parent = value;
                if (value == null)
                    _parentPath = "";
                else
                    _parentPath = value.AssetPath;
            }
        }
        /// <summary>
        /// 子节点
        /// 不序列化
        /// </summary>
        public List<AssetInfoRecord> Childs { get; } = new List<AssetInfoRecord>();

        /// <summary>
        /// 文件信息
        /// </summary>
        private FileSystemInfo _fileInfo;
        /// <summary>
        /// meta文件信息
        /// </summary>
        private FileInfo _metaFileInfo;
        /// <summary>
        /// 创建AssetInfoRecord
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="filePath"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        internal static AssetInfoRecord Create(string guid, string assetPath, AssetKind kind = AssetKind.NormalAsset)
        {
            AssetInfoRecord infoRecord = new AssetInfoRecord();
            infoRecord.Guid = guid;
            infoRecord.AssetPath = assetPath;
            infoRecord.Kind = kind;

            infoRecord._parentPath = kind switch
            {
                AssetKind.NormalFolder => System.IO.Path.GetDirectoryName(assetPath),
                AssetKind.NormalAsset => System.IO.Path.GetDirectoryName(assetPath),
                _ => ""
            };
            infoRecord.AssetType = AssetMonitorTools.GetAssetTypeByGuid(guid);
            infoRecord._assetName = System.IO.Path.GetFileName(assetPath);
            AssetMonitorConfig.Instance.AddRecord(infoRecord);
            return infoRecord;
        }

        /// <summary>
        /// 刷新依赖关系,如果需要的话
        /// </summary>
        internal void UpdateIfNeeded()
        {
            if (_isClone)
                return;
            if (Kind.IsIngoreReference())
                return;
            if (string.IsNullOrWhiteSpace(AssetPath))
            {
                Debug.LogWarning("无法更新依赖, 路径非法");
                return;
            }


            // 更新资源地址
            string path = AssetMonitorTools.GuidToAssetPath(this.Guid);
            if (path != AssetPath)
            {
                m_clearFileSize();
                // 资源路径被换, 只用更新路径即可
                AssetPath = path;
                _parentPath = Kind switch
                {
                    AssetKind.NormalFolder => System.IO.Path.GetDirectoryName(AssetPath),
                    AssetKind.NormalAsset => System.IO.Path.GetDirectoryName(AssetPath),
                    _ => ""
                };
                _parent = null;
                return;
            }

            if (!File.Exists(AssetPath) && !Directory.Exists(AssetPath))
            {
                //文件被删除
                AssetMonitorConfig.Instance.RemoveRecord(this);
                m_clearRelation();
                m_clearFileSize();
                return;
            }


            if (_fileInfo == null)
            {
                if (IsFolder)
                    _fileInfo = new DirectoryInfo(AssetPath);
                else
                    _fileInfo = new FileInfo(AssetPath);
            }
            else
                _fileInfo.Refresh();

            if (_metaFileInfo == null)
                _metaFileInfo = new FileInfo(AssetPath + ".meta");
            else
                _metaFileInfo.Refresh();
            long currentAssetHash = _fileInfo.LastWriteTimeUtc.Ticks;
            long currentMetaHash = 0;
            if (_metaFileInfo != null)
                currentMetaHash = _metaFileInfo.LastWriteTimeUtc.Ticks;
            if (LastAssetHash == currentAssetHash && LastMetaHash == currentMetaHash)
                return;
            bool isFirst = LastAssetHash == 0 && LastMetaHash == 0;
            LastAssetHash = currentAssetHash;
            LastMetaHash = currentMetaHash;
            if (!IsFolder)
            {
                DiskSize = ((FileInfo)_fileInfo).Length;
                UnitySize = AssetMonitorTools.GetAssetUnitySize(this);
            }
            m_refreshRelation();
            if (!isFirst)
                m_clearFileSize();
            RefreshFileSize();
        }

        /// <summary>
        /// 刷新文件树
        /// </summary>
        internal void RefreshFileSize()
        {
            if (_isClone)
                return;
            if (IsRoot || Kind.IsIngoreReference())
                return;
            if (Parent != null)
            {
                Parent.Childs.Add(this);
                bool shouldUpdateSize = this.Kind == AssetKind.NormalAsset;
                for (var ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
                {
                    ancestor.Count++;
                    if (shouldUpdateSize)
                        ancestor.DiskSize += this.Size;
                }
            }
            else
                Debug.LogError($"Parent is null  {ParentPath} -- {AssetPath}");
        }

        /// <summary>
        /// 删除的文件需要清除关系
        /// </summary>
        private void m_clearRelation()
        {
            if (_isClone)
                return;
            foreach (var item in ReferenceRelations)
            {
                AssetInfoRecord record = AssetMonitorTools.GetRecordByGuid(item.Guid);
                if (record == null)
                    continue;
                record.DependencyRelations.RemoveAll(a => a.Guid == Guid);
                record._dependencyGuidCache.Remove(Guid);
            }
            this.ReferenceRelations.Clear();
            this._referenceGuidCache.Clear();
        }

        /// <summary>
        /// 删除的文件需要清除关系
        /// </summary>
        private void m_clearFileSize()
        {
            if (_isClone)
                return;
            if (IsRoot || Kind.IsIngoreReference())
                return;
            if (Parent != null)
            {
                Parent.Childs.Remove(this);
                bool shouldUpdateSize = this.Kind == AssetKind.NormalAsset;
                for (var ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
                {
                    ancestor.Count--;
                    if (shouldUpdateSize)
                        ancestor.DiskSize -= this.Size;
                }
            }
        }

        private void m_refreshRelation()
        {
            if (_isClone)
                return;
            _referenceGuidCache.Clear();
            ReferenceRelations.Clear();
            string[] references = AssetDatabase.GetDependencies(this.AssetPath, false);
            foreach (var reference in references)
            {
                var guid = AssetDatabase.AssetPathToGUID(reference);
                if (AssetMonitorTools.CheckGuid(guid) && !_referenceGuidCache.Contains(guid))
                {
                    _referenceGuidCache.Add(guid);
                    ReferenceRelations.Add(new RelationInfo() { Guid = guid, Relation = UNITY_RELATION });
                    AssetInfoRecord dependency = AssetMonitorTools.GetRecordByGuid(guid, true);
                    if (dependency == null)
                        continue;
                    string[] dependencies = AssetDatabase.GetDependencies(dependency.AssetPath, false);
                    if (dependencies.Contains(this.Guid) && !dependency._dependencyGuidCache.Contains(this.Guid))
                    {
                        dependency._dependencyGuidCache.Add(this.Guid);
                        dependency.DependencyRelations.Add(new RelationInfo() { Guid = this.Guid, Relation = UNITY_RELATION });
                    }
                }
            }
            foreach (var guid in AssetMonitorTools.FindDependenciesByYaml(this.AssetPath))
            {
                if (AssetMonitorTools.CheckGuid(guid) && !_referenceGuidCache.Contains(guid))
                {
                    _referenceGuidCache.Add(guid);
                    ReferenceRelations.Add(new RelationInfo() { Guid = guid, Relation = UNITY_RELATION });
                    AssetInfoRecord dependency = AssetMonitorTools.GetRecordByGuid(guid, true);
                    if (dependency == null)
                        continue;
                    string[] dependencies = AssetDatabase.GetDependencies(dependency.AssetPath, false);
                    if (dependencies.Contains(this.Guid) && !dependency._dependencyGuidCache.Contains(this.Guid))
                    {
                        dependency._dependencyGuidCache.Add(this.Guid);
                        dependency.DependencyRelations.Add(new RelationInfo() { Guid = this.Guid, Relation = UNITY_RELATION });
                    }
                }
            }
            m_refreshWatcher();
            m_refreshVerifier();
        }

        /// <summary>
        /// 刷新资源验证情况
        /// </summary>
        private void m_refreshVerifier()
        {
            if (_isClone)
                return;
            var verifierInfos = AssetMonitorTools.GetVerifierInfoByAssetPath(AssetPath);
            if (verifierInfos.Count <= 0)
                return;
            foreach (var verifierInfo in verifierInfos)
                RefreshVerifyResult(verifierInfo);
        }

        /// <summary>
        /// 刷新自定义观察者的引用关系
        /// </summary>
        private void m_refreshWatcher()
        {
            if (_isClone)
                return;
            var watcherInfo = AssetMonitorTools.GetWatcherInfoByAssetPath(this.AssetPath);
            if (watcherInfo == null)
                return;
            RefreshWatcherRelation(watcherInfo);
        }

        public bool Equals(AssetInfoRecord other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;
            return Guid == other.Guid;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((AssetInfoRecord)obj);
        }
        public override int GetHashCode()
        {
            return Guid != null ? Guid.GetHashCode() : 0;
        }
        public void Deserialize(BinaryReader reader)
        {
            Guid = reader.ReadString();
            AssetPath = reader.ReadString();
            AbName = reader.ReadString();
            AssetType = reader.ReadString();
            Kind = (AssetKind)reader.ReadByte();
            DiskSize = reader.ReadInt64();
            UnitySize = reader.ReadInt64();
            LastAssetHash = reader.ReadInt64();
            LastMetaHash = reader.ReadInt64();
            int hashCount = reader.ReadInt32();
            DependencyRelations.Clear();
            for (int i = 0; i < hashCount; i++)
            {
                RelationInfo info = new RelationInfo();
                info.Deserialize(reader);
                _dependencyGuidCache.Add(info.Guid);
                DependencyRelations.Add(info);
            }
            hashCount = reader.ReadInt32();
            ReferenceRelations.Clear();
            for (int i = 0; i < hashCount; i++)
            {
                RelationInfo info = new RelationInfo();
                info.Deserialize(reader);
                _referenceGuidCache.Add(info.Guid);
                ReferenceRelations.Add(info);
            }
            hashCount = reader.ReadInt32();
            VerifyResults.Clear();
            for (int i = 0; i < hashCount; i++)
            {
                VerifyResult info = new VerifyResult();
                info.Deserialize(reader);
                info.Guid = this.Guid;
                VerifyResults.Add(info);
            }

            //处理一些信息
            if (IsFolder)
                DiskSize = 0;
            _parentPath = Kind switch
            {
                AssetKind.NormalFolder => System.IO.Path.GetDirectoryName(AssetPath),
                AssetKind.NormalAsset => System.IO.Path.GetDirectoryName(AssetPath),
                _ => ""
            };
            _assetName = System.IO.Path.GetFileName(AssetPath);
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Guid);
            writer.Write(AssetPath);
            writer.Write(AbName);
            writer.Write(AssetType);
            writer.Write((byte)Kind);
            writer.Write(DiskSize);
            writer.Write(UnitySize);
            writer.Write(LastAssetHash);
            writer.Write(LastMetaHash);

            writer.Write(DependencyRelations.Count);
            foreach (var dependency in DependencyRelations)
                dependency.Serialize(writer);

            writer.Write(ReferenceRelations.Count);
            foreach (var reference in ReferenceRelations)
                reference.Serialize(writer);

            writer.Write(VerifyResults.Count);
            foreach (var verifyResult in VerifyResults)
                verifyResult.Serialize(writer);
        }

        /// <summary>
        /// 刷新一个观察者的结果
        /// 会先删除旧结果，再添加新的结果
        /// </summary>
        /// <param name="verifierInfo"></param>
        internal void RefreshWatcherRelation(WatcherInfo watcherInfo)
        {
            if (_isClone)
                return;
            RemoveWatcherRelation(watcherInfo);
            if (!watcherInfo.IsEnabled)
                return;
            var clone = (AssetInfoRecord)this.Clone();
            var list = watcherInfo.Watcher.OnAssetChanged(clone);
            PoolReturn(clone);
            if (list != null)
            {
                string relation = watcherInfo.Watcher.Name;
                foreach (var item in list)
                {
                    var guid = AssetDatabase.AssetPathToGUID(item);
                    if (AssetMonitorTools.CheckGuid(guid) && !_referenceGuidCache.Contains(guid))
                    {
                        _referenceGuidCache.Add(guid);
                        ReferenceRelations.Add(new RelationInfo() { Guid = guid, Relation = relation });
                        AssetInfoRecord reference = AssetMonitorTools.GetRecordByGuid(guid, true);
                        if (reference != null && !reference._dependencyGuidCache.Contains(this.Guid))
                        {
                            reference._dependencyGuidCache.Add(this.Guid);
                            reference.DependencyRelations.Add(new RelationInfo() { Guid = this.Guid, Relation = relation });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 移除该观察者的所有关系
        /// </summary>
        /// <param name="watcherInfo"></param>
        internal void RemoveWatcherRelation(WatcherInfo watcherInfo)
        {
            if (_isClone)
                return;
            string relation = watcherInfo.Watcher.Name;
            for (int i = ReferenceRelations.Count - 1; i >= 0; i--)
            {
                var relationInfo = ReferenceRelations[i];
                // 先移除该观察者的旧依赖
                if (relationInfo.Relation != relation)
                    continue;

                AssetInfoRecord reference = AssetMonitorTools.GetRecordByGuid(relationInfo.Guid, true);
                if (reference != null)
                {
                    reference.DependencyRelations.RemoveAll(item => item.Guid == Guid);
                    reference._dependencyGuidCache.Remove(this.Guid);
                }
                _referenceGuidCache.Remove(relationInfo.Guid);
                ReferenceRelations.RemoveAt(i);
            }
        }

        /// <summary>
        /// 刷新一个验证器的结果
        /// 会先删除旧结果，再添加新的结果
        /// </summary>
        /// <param name="verifierInfo"></param>
        internal void RefreshVerifyResult(VerifierInfo verifierInfo)
        {
            if (_isClone)
                return;
            RemoveVerifyResult(verifierInfo);
            if (!verifierInfo.IsEnabled)
                return;
            var clone = (AssetInfoRecord)this.Clone();
            bool result = verifierInfo.Verifier.Verify(clone);
            PoolReturn(clone);
            VerifyResults.Add(new VerifyResult()
            {
                Guid = this.Guid,
                TypeName = verifierInfo.TypeName,
                IsValid = result
            });
        }
        /// <summary>
        /// 移除某个验证器的结果
        /// </summary>
        /// <param name="verifierInfo"></param>
        internal void RemoveVerifyResult(VerifierInfo verifierInfo)
        {
            if (_isClone)
                return;
            VerifyResults.RemoveAll(item => item.TypeName == verifierInfo.TypeName);
        }

        public object Clone()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(memoryStream);
            this.Serialize(writer);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using BinaryReader reader = new BinaryReader(memoryStream);
            var assetInfoRecord = PoolGet();
            assetInfoRecord._isClone = true;
            assetInfoRecord.Deserialize(reader);
            return assetInfoRecord;
        }


        #region pool

        private static readonly Queue<AssetInfoRecord> _pool = new Queue<AssetInfoRecord>();

        /// <summary>
        /// 通过对象池中获取一个AssetInfoRecord对象,但他是克隆对象
        /// </summary>
        /// <returns></returns>
        internal static AssetInfoRecord PoolGet()
        {
            var record = _pool.Count > 0 ? _pool.Dequeue() : new AssetInfoRecord();
            record._isClone = true;
            return record;
        }
        /// <summary>
        /// 归还一个AssetInfoRecord对象
        /// </summary>
        /// <param name="record"></param>
        internal static void PoolReturn(AssetInfoRecord record)
        {
            if (record == null)
                return;
            record._isClone = true;
            record.Guid = "";
            record.AssetPath = "";
            record.AbName = "";
            record.AssetType = "";
            record.Kind = AssetKind.NormalAsset;
            record.DiskSize = 0;
            record.UnitySize = 0;
            record.LastAssetHash = 0;
            record.LastMetaHash = 0;
            record.DependencyRelations.Clear();
            record._dependencyGuidCache.Clear();
            record.ReferenceRelations.Clear();
            record._referenceGuidCache.Clear();
            record.VerifyResults.Clear();
            record._assetName = "";
            record._parentPath = "";
            record._parent = null;
            foreach (var item in record.Childs)
            {
                PoolReturn(item);
            }
            record.Childs.Clear();
            _pool.Enqueue(record);
        }
        #endregion
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    internal sealed class VerifyResult : ISerializable
    {
        /// <summary>
        /// 资源的guid
        /// 不序列化
        /// </summary>
        public string Guid { get; set; } = "";

        /// <summary>
        /// 验证器的类型名
        /// </summary>
        public string TypeName { get; set; } = "";
        /// <summary>
        /// 是否验证成功
        /// </summary>
        public bool IsValid { get; set; } = true;
        public void Deserialize(BinaryReader reader)
        {
            TypeName = reader.ReadString();
            IsValid = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(IsValid);
        }
    }

    /// <summary>
    /// 资源间的关系
    /// </summary>
    public sealed class RelationInfo : IEquatable<RelationInfo>, IEquatable<string>, ISerializable
    {
        /// <summary>
        /// 资源的guid
        /// </summary>
        public string Guid { get; internal set; } = "";

        /// <summary>
        /// 关系类型
        /// 默认为 Unity
        /// </summary>
        public string Relation { get; internal set; } = "Unity";

        public void Deserialize(BinaryReader reader)
        {
            this.Guid = reader.ReadString();
            this.Relation = reader.ReadString();
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Relation); // 将枚举转换为字节存储
        }
        public bool Equals(RelationInfo other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Guid == other.Guid;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((RelationInfo)obj);
        }
        public override int GetHashCode()
        {
            return Guid != null ? Guid.GetHashCode() : 0;
        }

        public bool Equals(string other)
        {
            if (ReferenceEquals(null, other))
                return false;
            return this.Guid == other;
        }

        public static bool operator ==(RelationInfo info, string guid)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(info, guid))
                return true;
            if (System.Object.ReferenceEquals(null, info))
                return false;
            return info.Guid == guid;
        }
        public static bool operator !=(RelationInfo info, string guid)
        {
            return !(info == guid);
        }
        public static bool operator ==(string guid, RelationInfo info)
        {
            return info == guid;
        }
        public static bool operator !=(string guid, RelationInfo info)
        {
            return info != guid;
        }
    }

    /// <summary>
    /// 拓展相关
    /// </summary>
    internal interface IConfigExtension
    {
        bool IsEnabled { get; set; }
        string GetDisplayName();
        string GetDescription();
    }

    /// <summary>
    /// 搜索器信息
    /// </summary>
    internal class SearcherInfo : ISerializable, IConfigExtension
    {

        #region 序列化

        /// <summary>
        /// 搜索器类全名
        /// 需要序列化
        /// </summary>
        public string TypeName { get; private set; } = "";
        /// <summary>
        /// 搜索器是否启用
        /// 需要序列化
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion


        private IAssetMonitorSearcher _searcher;
        /// <summary>
        /// 搜索器对象
        /// 不序列化
        /// </summary>
        public IAssetMonitorSearcher Searcher
        {
            get
            {
                if (_searcher == null)
                {
                    if (AssetMonitorConfig.Instance.SearcherTypeDict.TryGetValue(TypeName, out Type type))
                        _searcher = (IAssetMonitorSearcher)Activator.CreateInstance(type);
                }
                return _searcher;
            }
        }


        public string GetDisplayName()
        {
            return Searcher?.Name;
        }

        public string GetDescription()
        {
            return Searcher?.Description;
        }

        public void Deserialize(BinaryReader reader)
        {
            TypeName = reader.ReadString();
            IsEnabled = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(IsEnabled);
        }

        /// <summary>
        /// 创建搜索器
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static SearcherInfo Create(string typeName)
        {
            SearcherInfo info = new SearcherInfo();
            info.TypeName = typeName;
            AssetMonitorConfig.Instance.AddSearcher(info);
            return info;
        }
    }

    /// <summary>
    /// 右键指令信息
    /// </summary>
    internal class CommandInfo : ISerializable, IConfigExtension
    {
        #region 序列化

        /// <summary>
        /// 右键指令类全名
        /// 需要序列化
        /// </summary>
        public string TypeName { get; private set; } = "";
        /// <summary>
        /// 右键指令是否启用
        /// 需要序列化
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion

        private IAssetMonitorCommand _command;
        /// <summary>
        /// 搜索器对象
        /// 不序列化
        /// </summary>
        public IAssetMonitorCommand Command
        {
            get
            {
                if (_command == null)
                {
                    if (AssetMonitorConfig.Instance.CommandTypeDict.TryGetValue(TypeName, out Type type))
                        _command = (IAssetMonitorCommand)Activator.CreateInstance(type);
                }
                return _command;
            }
        }

        /// <summary>
        /// 创建右键指令
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static CommandInfo Create(string typeName)
        {
            CommandInfo info = new CommandInfo();
            info.TypeName = typeName;
            AssetMonitorConfig.Instance.AddCommand(info);
            return info;
        }
        public void Deserialize(BinaryReader reader)
        {
            TypeName = reader.ReadString();
            IsEnabled = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(IsEnabled);
        }

        public string GetDisplayName()
        {
            return Command?.Name;
        }

        public string GetDescription()
        {
            return Command?.Description;
        }
    }

    /// <summary>
    /// 检测器观察者信息
    /// </summary>
    internal class WatcherInfo : ISerializable, IConfigExtension
    {

        #region 序列化

        /// <summary>
        /// 观察者类全名
        /// 需要序列化
        /// </summary>
        public string TypeName { get; private set; } = "";
        /// <summary>
        /// 观察者是否启用
        /// 需要序列化
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion


        private IAssetMonitorWatcher _watcher;
        /// <summary>
        /// 搜索器对象
        /// 不序列化
        /// </summary>
        internal IAssetMonitorWatcher Watcher
        {
            get
            {
                if (_watcher == null)
                {
                    if (AssetMonitorConfig.Instance.WatcherTypeDict.TryGetValue(TypeName, out Type type))
                    {
                        _watcher = (IAssetMonitorWatcher)Activator.CreateInstance(type);
                        IsExtension = _watcher.WatchPath.TrimStart().StartsWith("*");
                        if (!IsExtension)
                            goto End;
                        Extensions = _watcher.WatchPath.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.TrimStart('*').ToLower()).ToArray();
                    }
                }
            End: return _watcher;
            }
        }
        /// <summary>
        /// 是否是针对拓展名
        /// </summary>
        public bool IsExtension { get; private set; }

        /// <summary>
        /// 当前的所有拓展名
        /// </summary>
        public string[] Extensions { get; private set; } = AssetMonitorTools.StringEmpty;

        /// <summary>
        /// 创建观察者
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static WatcherInfo Create(string typeName)
        {
            WatcherInfo info = new WatcherInfo();
            info.TypeName = typeName;
            AssetMonitorConfig.Instance.AddWatcher(info);
            return info;
        }

        public void Deserialize(BinaryReader reader)
        {
            TypeName = reader.ReadString();
            IsEnabled = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(IsEnabled);
        }

        public string GetDisplayName()
        {
            return Watcher?.Name;
        }

        public string GetDescription()
        {
            return Watcher?.Description;
        }
    }

    /// <summary>
    /// 资源校验者信息
    /// </summary>
    internal class VerifierInfo : ISerializable, IConfigExtension
    {
        #region 序列化

        /// <summary>
        /// 资源校验者类全名
        /// 需要序列化
        /// </summary>
        public string TypeName { get; private set; } = "";
        /// <summary>
        /// 资源校验者是否启用
        /// 需要序列化
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion

        private IAssetMonitorVerifier _verifier;
        /// <summary>
        /// 搜索器对象
        /// 不序列化
        /// </summary>
        public IAssetMonitorVerifier Verifier
        {
            get
            {
                if (_verifier == null)
                {
                    if (AssetMonitorConfig.Instance.VerifierTypeDict.TryGetValue(TypeName, out Type type))
                    {
                        _verifier = (IAssetMonitorVerifier)Activator.CreateInstance(type);
                        IsExtension = _verifier.VerifyPath.TrimStart().StartsWith("*");
                        if (!IsExtension)
                            goto End;
                        Extensions = _verifier.VerifyPath.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.TrimStart('*').ToLower()).ToArray();
                    }
                }
            End: return _verifier;
            }
        }

        /// <summary>
        /// 是否是针对拓展名
        /// </summary>
        public bool IsExtension { get; private set; }

        /// <summary>
        /// 当前的所有拓展名
        /// </summary>
        public string[] Extensions { get; private set; } = AssetMonitorTools.StringEmpty;

        /// <summary>
        /// 创建右键指令
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static VerifierInfo Create(string typeName)
        {
            VerifierInfo info = new VerifierInfo();
            info.TypeName = typeName;
            AssetMonitorConfig.Instance.AddVerifier(info);
            return info;
        }

        public void Deserialize(BinaryReader reader)
        {
            TypeName = reader.ReadString();
            IsEnabled = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(IsEnabled);
        }

        public string GetDisplayName()
        {
            return Verifier?.Name;
        }

        public string GetDescription()
        {
            return Verifier?.Description;
        }
    }
}