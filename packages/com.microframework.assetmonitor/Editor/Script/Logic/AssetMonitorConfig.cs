using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static MFramework.AssetMonitor.AssetMonitorConst;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源类型
    /// </summary>
    [Serializable]
    /// <summary>
    /// 定义资源的类型，用于标识不同来源或用途的资源。
    /// </summary>
    public enum AssetKind : byte
    {
        /// <summary>
        /// 常规资源（默认类型）。
        /// </summary>
        Regular = 0,
        /// <summary>
        /// 文件夹资源。
        /// </summary>
        Folder = 1,
        /// <summary>
        /// 特殊文件夹
        /// </summary>
        SpecialFolder = 2,
        /// <summary>
        /// 特殊资源
        /// </summary>
        SpecialRegular = 2,
        ///// <summary>
        ///// 设置类资源，通常用于配置文件等。
        ///// </summary>
        //Settings = 10,
        ///// <summary>
        ///// 来自包的资源，表示该资源来源于外部包。
        ///// </summary>
        //FromPackage = 20,
        ///// <summary>
        ///// 来自嵌入式包的资源，表示该资源来源于项目内部嵌入的包。
        ///// </summary>
        //FromEmbeddedPackage = 30,
        ///// <summary>
        ///// 不支持的资源类型，用于处理未知或不兼容的资源。
        ///// </summary>
        //Unsupported = 100
    }

    /// <summary>
    /// 关系类型
    /// </summary>
    public enum RelationType : byte
    {
        /// <summary>
        /// Unity引用
        /// </summary>
        Unity,
        /// <summary>
        /// 自定义引用
        /// </summary>
        Custom,
        /// <summary>
        /// 代码中引用
        /// </summary>
        Script,
    }

    /// <summary>
    /// 可序列化
    /// </summary>
    internal interface ISerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }


    //[Serializable]
    //internal class FileTreeData
    //{
    //    public string Name;
    //    public string FullPath;
    //    public bool IsDirectory;

    //    /// <summary>
    //    /// 文件大小或者文件夹大小
    //    /// </summary>
    //    public int Size;
    //    /// <summary>
    //    /// 文件数量或者文件夹数量
    //    /// </summary>
    //    public int FileCount;

    //    private AssetInfoRecord _assetInfo;
    //    public AssetInfoRecord AssetInfo
    //    {
    //        get
    //        {
    //            if (_assetInfo == null)
    //            {
    //                AssetMonitorTools.GetRecordByPath(AssetMonitorTools.FormatPath(FullPath), false);
    //            }
    //            return _assetInfo;
    //        }
    //    }

    //    public FileTreeData(string name, string fullPath, bool isDirectory)
    //    {
    //        this.Name = name;
    //        this.FullPath = fullPath;
    //        this.IsDirectory = isDirectory;
    //    }
    //}

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
        /// 是否开启
        /// 需要序列化
        /// </summary>
        internal bool IsOpen { get; set; } = false;
        /// <summary>
        /// 项目页签固定大小
        /// </summary>
        internal float ProjectFiexdSize { get; set; } = 120;

        /// <summary>
        /// 所有搜索器
        /// </summary>
        private readonly List<SearcherInfo> _searcherInfos = new List<SearcherInfo>();

        /// <summary>
        /// 当前所有资源信息
        /// 需要序列化
        /// </summary>
        private readonly List<AssetInfoRecord> _assetRecords = new List<AssetInfoRecord>();

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
        /// 搜索器信息字典
        /// 不序列化
        /// </summary>
        internal readonly Dictionary<string, SearcherInfo> SearcherInfoDict = new Dictionary<string, SearcherInfo>();

        /// <summary>
        /// 搜索器类型信息
        /// 不序列化
        /// key: 类型全称 value: Type
        /// </summary>
        internal readonly Dictionary<string, Type> SearcherTypes = new Dictionary<string, Type>();

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
        public void AddRecord(AssetInfoRecord record)
        {
            if (!_assetRecords.Contains(record))
                _assetRecords.Add(record);
            if (!GuidAssetRecordDict.ContainsKey(record.Guid))
                GuidAssetRecordDict.Add(record.Guid, record);
            if (!PathAssetRecordDict.ContainsKey(record.FilePath))
                PathAssetRecordDict.Add(record.FilePath, record);
        }

        /// <summary>
        /// 添加一个搜索器
        /// </summary>
        /// <param name="info"></param>
        public void AddSearcher(SearcherInfo info)
        {
            if (!_searcherInfos.Contains(info))
                _searcherInfos.Add(info);
        }

        public void Deserialize(BinaryReader reader)
        {
            reader.ReadString(); // 读取配置版本
            IsOpen = reader.ReadBoolean(); // 读取是否开启标志
            ProjectFiexdSize = reader.ReadSingle();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                AssetInfoRecord record = new AssetInfoRecord();
                record.Deserialize(reader);
                _assetRecords.Add(record);
                if (!GuidAssetRecordDict.ContainsKey(record.Guid))
                    GuidAssetRecordDict.Add(record.Guid, record);
                if (!PathAssetRecordDict.ContainsKey(record.FilePath))
                    PathAssetRecordDict.Add(record.FilePath, record);
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                SearcherInfo searcher = new SearcherInfo();
                searcher.Deserialize(reader);
                _searcherInfos.Add(searcher);
                if (!SearcherInfoDict.ContainsKey(searcher.SearcherTypeName))
                    SearcherInfoDict.Add(searcher.SearcherTypeName, searcher);
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(CONFIG_VERSION); // 版本号
            writer.Write(IsOpen);
            writer.Write(ProjectFiexdSize);
            writer.Write(_assetRecords.Count);
            foreach (var record in _assetRecords)
                record.Serialize(writer);
            writer.Write(_searcherInfos.Count);
            foreach (var info in _searcherInfos)
                info.Serialize(writer);
        }

        /// <summary>
        /// 刷新配置
        /// </summary>
        internal void RefreshConfig()
        {
            foreach (var item in SearcherTypes)
            {
                if (SearcherInfoDict.ContainsKey(item.Key))
                    continue;
                SearcherInfo.Create(item.Key);
            }
            for (int i = _searcherInfos.Count - 1; i >= 0; i--)
            {
                var item = _searcherInfos[i];
                if (!SearcherTypes.ContainsKey(item.SearcherTypeName))
                {
                    _searcherInfos.RemoveAt(i);
                    continue;
                }
            }

            foreach (var item in _assetRecords)
                item.RefreshTree();
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
            IsOpen = false;
            ProjectFiexdSize = 120;
            _assetRecords.Clear();
            _searcherInfos.Clear();
            SearcherInfoDict.Clear();
            GuidAssetRecordDict.Clear();
            PathAssetRecordDict.Clear();

            Save();
        }
    }

    /// <summary>
    /// 资源记录
    /// </summary>
    public sealed class AssetInfoRecord : IEquatable<AssetInfoRecord>, ISerializable
    {
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
        public string FilePath { get; private set; } = "";

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
        public AssetKind Kind { get; private set; } = AssetKind.Regular;

        /// <summary>
        /// 资源大小
        /// Byte
        /// 需要序列化
        /// </summary>
        public long Size { get; private set; } = 0;

        /// <summary>
        /// 获取资源最后修改的哈希值。
        /// 需要序列化
        /// </summary>
        public long LastHash { get; private set; } = 0;

        /// <summary>
        /// 依赖于【此资源】的其它资源的GUID列表。
        /// (关系: 其它资源 -> 此资源)
        /// 需要序列化
        /// </summary>
        public readonly HashSet<RelationInfo> DependencyRelations = new HashSet<RelationInfo>();
        /// <summary>
        /// 依赖的guid缓存
        /// </summary>
        private readonly HashSet<string> _dependencyGuidCache = new HashSet<string>();

        /// <summary>
        /// 此资源引用的【其它资源】的GUID列表。
        /// (关系: 此资源 -> 其它资源)
        /// 需要序列化
        /// </summary>
        public readonly HashSet<RelationInfo> ReferenceRelations = new HashSet<RelationInfo>();
        /// <summary>
        /// 引用的guid缓存
        /// </summary>
        public readonly HashSet<string> _referenceGuidCache = new HashSet<string>();

        private string _fieldName = "";
        /// <summary>
        /// 文件名
        /// 不序列化
        /// </summary>
        public string FileName => _fieldName;

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
        public bool IsRoot => string.IsNullOrWhiteSpace(ParentPath);

        /// <summary>
        /// 是否是目录
        /// 不序列化
        /// </summary>
        public bool IsDirectory => Kind != AssetKind.Regular;

        /// <summary>
        /// 包含资源数量
        /// 不序列化
        /// </summary>
        public int Count { get; private set; } = 0;

        private AssetInfoRecord _parent;
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
                if (_parent == null)
                    _parent = AssetMonitorTools.GetRecordByPath(ParentPath);
                return _parent;
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
        public static AssetInfoRecord Create(string guid, string filePath, AssetKind kind = AssetKind.Regular)
        {
            AssetInfoRecord infoRecord = new AssetInfoRecord();
            infoRecord.Guid = guid;
            infoRecord.FilePath = filePath;
            infoRecord.Kind = kind;
            infoRecord._parentPath = kind != AssetKind.SpecialRegular ? System.IO.Path.GetDirectoryName(filePath) : "";
            infoRecord.AssetType = AssetMonitorTools.GetAssetTypeByGuid(guid);
            infoRecord._fieldName = System.IO.Path.GetFileName(filePath);
            AssetMonitorConfig.Instance.AddRecord(infoRecord);
            return infoRecord;
        }

        /// <summary>
        /// 刷新依赖关系,如果需要的话
        /// </summary>
        public void UpdateDepIfNeeded()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                Debug.LogWarning("无法更新依赖, 路径非法");
                return;
            }

            if (_fileInfo == null)
            {
                if (IsDirectory)
                    _fileInfo = new DirectoryInfo(FilePath);
                else
                    _fileInfo = new FileInfo(FilePath);
            }
            else
                _fileInfo.Refresh();

            if (!_fileInfo.Exists)
            {
                Debug.LogWarning($"无法更新依赖, 路径不存在, 路径:{FilePath}");
                return;
            }

            if (_metaFileInfo == null)
                _metaFileInfo = new FileInfo(FilePath + ".meta");
            else
                _metaFileInfo.Refresh();
            if (!IsDirectory)
                Size = (long)((FileInfo)_fileInfo).Length;
            long currentHash = _fileInfo.LastWriteTimeUtc.Ticks;
            if (LastHash == currentHash)
                return;
            if (Kind == AssetKind.SpecialFolder)
                return;
            m_refreshDependencies();
            LastHash = currentHash;
        }

        /// <summary>
        /// 刷新文件树
        /// </summary>
        public void RefreshTree()
        {
            if (IsRoot)
                return;
            if (Parent != null)
            {
                Parent.Childs.Add(this);
                bool shouldUpdateSize = this.Kind == AssetKind.Regular || this.Kind == AssetKind.SpecialRegular;
                for (var ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
                {
                    ancestor.Count++;
                    if (shouldUpdateSize)
                        ancestor.Size += this.Size;
                }
            }
            else
                Debug.LogError($"Parent is null  {ParentPath}");
        }

        private void m_refreshDependencies()
        {
            _referenceGuidCache.Clear();
            ReferenceRelations.Clear();
            string[] dependencies = AssetDatabase.GetDependencies(this.FilePath, false);
            foreach (var dependency in dependencies)
            {
                var guid = AssetDatabase.AssetPathToGUID(dependency);
                if (AssetMonitorTools.CheckGuid(guid) && !_referenceGuidCache.Contains(guid))
                {
                    _referenceGuidCache.Add(guid);
                    ReferenceRelations.Add(new RelationInfo() { Guid = guid, Relation = RelationType.Unity });
                    AssetInfoRecord reference = AssetMonitorTools.GetRecordByGuid(guid);
                    if (reference != null && !reference._dependencyGuidCache.Contains(this.Guid))
                    {
                        reference._dependencyGuidCache.Add(this.Guid);
                        reference.DependencyRelations.Add(new RelationInfo() { Guid = this.Guid, Relation = RelationType.Unity });
                    }
                }
            }
            foreach (var guid in AssetMonitorTools.FindDependenciesByYaml(this.FilePath))
            {
                if (AssetMonitorTools.CheckGuid(guid) && !_referenceGuidCache.Contains(guid))
                {
                    _referenceGuidCache.Add(guid);
                    ReferenceRelations.Add(new RelationInfo() { Guid = guid, Relation = RelationType.Unity });
                    AssetInfoRecord reference = AssetMonitorTools.GetRecordByGuid(guid);
                    if (reference != null && !reference._dependencyGuidCache.Contains(this.Guid))
                    {
                        reference._dependencyGuidCache.Add(this.Guid);
                        reference.DependencyRelations.Add(new RelationInfo() { Guid = this.Guid, Relation = RelationType.Unity });
                    }
                }
            }
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
            FilePath = reader.ReadString();
            AbName = reader.ReadString();
            AssetType = reader.ReadString();
            Kind = (AssetKind)reader.ReadByte();
            Size = reader.ReadInt64();
            LastHash = reader.ReadInt64();
            int hashCount = reader.ReadInt32();
            DependencyRelations.Clear();
            for (int i = 0; i < hashCount; i++)
            {
                RelationInfo info = new RelationInfo();
                info.Deserialize(reader);
                DependencyRelations.Add(info);
            }
            hashCount = reader.ReadInt32();
            ReferenceRelations.Clear();
            for (int i = 0; i < hashCount; i++)
            {
                RelationInfo info = new RelationInfo();
                info.Deserialize(reader);
                ReferenceRelations.Add(info);
            }
            if (IsDirectory)
                Size = 0;
            _parentPath = Kind != AssetKind.SpecialRegular ? System.IO.Path.GetDirectoryName(FilePath) : "";
            _fieldName = System.IO.Path.GetFileName(FilePath);
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Guid);
            writer.Write(FilePath);
            writer.Write(AbName);
            writer.Write(AssetType);
            writer.Write((byte)Kind);
            writer.Write(Size);
            writer.Write(LastHash);
            writer.Write(DependencyRelations.Count);
            foreach (var guid in DependencyRelations)
                guid.Serialize(writer);
            writer.Write(ReferenceRelations.Count);
            foreach (var guid in ReferenceRelations)
                guid.Serialize(writer);
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
        public string Guid = "";

        /// <summary>
        /// 关系类型
        /// 用来区分 Unity 引用、脚本引用和自定义引用等不同类型的关系。
        /// </summary>
        public RelationType Relation = RelationType.Unity;

        public void Deserialize(BinaryReader reader)
        {
            this.Guid = reader.ReadString();
            this.Relation = (RelationType)reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Guid);
            writer.Write((byte)Relation); // 将枚举转换为字节存储
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
            if (System.Object.ReferenceEquals(null, info))
                return false;
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
    /// 搜索器信息
    /// </summary>
    internal class SearcherInfo : ISerializable
    {

        #region 序列化

        /// <summary>
        /// 搜索器类全名
        /// 需要序列化
        /// </summary>
        public string SearcherTypeName { get; private set; } = "";
        /// <summary>
        /// 搜索器是否启用
        /// 需要序列化
        /// </summary>
        public bool IsEnabled { get; private set; } = true;

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
                    if (AssetMonitorConfig.Instance.SearcherTypes.TryGetValue(SearcherTypeName, out Type type))
                        _searcher = (IAssetMonitorSearcher)Activator.CreateInstance(type);
                }
                return _searcher;
            }
        }


        /// <summary>
        /// 创建搜索器
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static SearcherInfo Create(string typeName)
        {
            SearcherInfo info = new SearcherInfo();
            info.SearcherTypeName = typeName;
            AssetMonitorConfig.Instance.AddSearcher(info);
            return info;
        }


        public void Deserialize(BinaryReader reader)
        {
            SearcherTypeName = reader.ReadString();
            IsEnabled = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(SearcherTypeName);
            writer.Write(IsEnabled);
        }
    }

    ///// <summary>
    ///// 资源监控信息类，用于记录和管理 Unity 项目中资源的元数据。
    ///// 实现了 IEquatable<AssetMonitorInfo> 接口以支持对象间的相等性比较。
    ///// </summary>
    //[Serializable]
    //internal class AssetMonitorInfo : IEquatable<AssetMonitorInfo>, ISerializable
    //{
    //    /// <summary>
    //    /// 获取资源的唯一标识符（GUID）。
    //    /// </summary>
    //    public string GUID { get; private set; }

    //    /// <summary>
    //    /// 获取资源在项目中的完整路径。
    //    /// </summary>
    //    public string Path { get; private set; }

    //    /// <summary>
    //    /// 获取资源的类型（如常规资源、设置类资源、包资源等）。
    //    /// </summary>
    //    public AssetKind Kind { get; private set; }

    //    /// <summary>
    //    /// 如果资源是设置类资源，则获取其具体类型（如音频管理器、输入管理器等）。
    //    /// </summary>
    //    public AssetSettingsKind SettingsKind { get; private set; }

    //    /// <summary>
    //    /// 获取资源的大小（单位：字节）。
    //    /// </summary>
    //    public long Size { get; private set; }

    //    /// <summary>
    //    /// 获取资源最后修改的哈希值。
    //    /// </summary>
    //    public ulong LastHash { get; private set; } = 0;

    //    /// <summary>
    //    /// 获取资源是否丢失。
    //    /// </summary>
    //    public bool IsMissing { get; private set; } = false;

    //    /// <summary>
    //    /// 此资源所依赖的【其它资源】的GUID列表。
    //    /// (关系: 此资源 -> 其它资源)
    //    /// </summary>
    //    public readonly HashSet<string> DependenciesGUIDs = new HashSet<string>();

    //    /// <summary>
    //    /// 依赖于【此资源】的其它资源的GUID列表。
    //    /// (关系: 其它资源 -> 此资源)
    //    /// </summary>
    //    public readonly HashSet<string> ReferencesGUIDs = new HashSet<string>();


    //    private FileInfo _fileInfo;
    //    private FileInfo _metaFileInfo;

    //    internal static AssetMonitorInfo Create(RawAssetMonitorInfo raw)
    //    {
    //        if (string.IsNullOrEmpty(raw.guid))
    //        {
    //            Debug.LogError("GUID非法, 无法创建资源监控信息, 资源地址:" + raw.path);
    //            return null;
    //        }
    //        AssetMonitorInfo info = new AssetMonitorInfo()
    //        {
    //            GUID = raw.guid,
    //            Path = raw.path,
    //            IsMissing = !string.IsNullOrWhiteSpace(raw.path),
    //            Kind = raw.kind
    //        };
    //        return info;
    //    }

    //    /// <summary>
    //    /// 创建内部资源的监控信息
    //    /// </summary>
    //    /// <param name="path"></param>
    //    /// <param name="guid"></param>
    //    /// <returns></returns>
    //    internal static AssetMonitorInfo Create(string guid, string path)
    //    {
    //        AssetMonitorInfo info = new AssetMonitorInfo()
    //        {
    //            GUID = guid,
    //            Path = path,
    //            IsMissing = !string.IsNullOrWhiteSpace(path),
    //            Kind = AssetKind.FromEmbeddedPackage,
    //        };
    //        return info;
    //    }

    //    /// <summary>
    //    /// 跟新依赖
    //    /// </summary>
    //    internal void UpdateIfNeeded()
    //    {
    //        if (string.IsNullOrWhiteSpace(Path))
    //        {
    //            Debug.LogWarning("无法更新依赖, 路径非法");
    //            return;
    //        }
    //        if (_fileInfo == null)
    //            _fileInfo = new FileInfo(Path);
    //        else
    //            _fileInfo.Refresh();

    //        if (!_fileInfo.Exists)
    //        {
    //            Debug.LogWarning($"无法更新依赖, 路径不存在, 路径:{Path}");
    //            return;
    //        }

    //        ulong currentHash = 0;

    //        if (_metaFileInfo == null)
    //            _metaFileInfo = new FileInfo(Path + ".meta");
    //        else
    //            _metaFileInfo.Refresh();

    //        currentHash = (ulong)_fileInfo.LastWriteTimeUtc.Ticks;

    //        Size = _fileInfo.Length;

    //        if (LastHash == currentHash)
    //        {
    //            // return;
    //            //for (var i = dependenciesGUIDs.Length - 1; i > -1; i--)
    //            //{
    //            //    var guid = dependenciesGUIDs[i];
    //            //    var path = AssetDatabase.GUIDToAssetPath(guid);
    //            //    path = CSPathTools.EnforceSlashes(path);
    //            //    if (!string.IsNullOrEmpty(path) && File.Exists(path)) continue;

    //            //    ArrayUtility.RemoveAt(ref dependenciesGUIDs, i);
    //            //    foreach (var referenceInfo in assetReferencesInfo)
    //            //    {
    //            //        if (referenceInfo.assetInfo.GUID != guid) continue;

    //            //        ArrayUtility.Remove(ref assetReferencesInfo, referenceInfo);
    //            //        break;
    //            //    }
    //            //}

    //            //if (!needToRebuildReferences) return;
    //        }

    //        //foreach (var referenceInfo in assetReferencesInfo)
    //        //{
    //        //    foreach (var info in referenceInfo.assetInfo.referencedAtInfoList)
    //        //    {
    //        //        if (!info.assetInfo.Equals(this)) continue;

    //        //        ArrayUtility.Remove(ref referenceInfo.assetInfo.referencedAtInfoList, info);
    //        //        break;
    //        //    }
    //        //}

    //        LastHash = currentHash;
    //        //needToRebuildReferences = true;

    //        //assetReferencesInfo = new AssetReferenceInfo[0];
    //        FindDependencies();
    //    }


    //    public void Deserialize(BinaryReader reader)
    //    {
    //        GUID = reader.ReadString(); //  GUID
    //        Path = reader.ReadString(); //  路径
    //        Kind = (AssetKind)reader.ReadByte(); //  资源类型
    //        SettingsKind = (AssetSettingsKind)reader.ReadByte(); //  设置类型
    //        LastHash = reader.ReadUInt64();
    //        Size = reader.ReadInt64(); //  大小
    //        int count = reader.ReadInt32(); //  依赖数量
    //        for (int i = 0; i < count; i++)
    //            DependenciesGUIDs.Add(reader.ReadString());
    //        count = reader.ReadInt32(); //  引用数量
    //        for (int i = 0; i < count; i++)
    //            ReferencesGUIDs.Add(reader.ReadString());
    //    }

    //    public void Serialize(BinaryWriter writer)
    //    {
    //        writer.Write(GUID);
    //        writer.Write(Path);
    //        writer.Write((byte)Kind);
    //        writer.Write((byte)SettingsKind);
    //        writer.Write(LastHash);
    //        writer.Write(Size);
    //        writer.Write(DependenciesGUIDs.Count); //  依赖数量
    //        foreach (var item in DependenciesGUIDs)
    //            writer.Write(item);
    //        writer.Write(ReferencesGUIDs.Count);
    //        foreach (var item in ReferencesGUIDs)
    //            writer.Write(item);
    //    }

    //    public bool Equals(AssetMonitorInfo other)
    //    {
    //        if (ReferenceEquals(null, other))
    //            return false;

    //        if (ReferenceEquals(this, other))
    //            return true;

    //        return GUID == other.GUID;
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (ReferenceEquals(null, obj))
    //            return false;

    //        if (ReferenceEquals(this, obj))
    //            return true;

    //        if (obj.GetType() != GetType())
    //            return false;

    //        return Equals((AssetMonitorInfo)obj);
    //    }
    //    public override int GetHashCode()
    //    {
    //        return GUID != null ? GUID.GetHashCode() : 0;
    //    }


    //    private void FindDependencies()
    //    {
    //        DependenciesGUIDs.Clear();
    //        if (this.Kind != AssetKind.Settings)
    //        {
    //            var dependencies = AssetDatabase.GetDependencies(this.Path, false);

    //            foreach (var path in dependencies)
    //            {
    //                var guid = AssetDatabase.AssetPathToGUID(path);
    //                if (AssetMonitorTools.CheckGuid(guid) && !DependenciesGUIDs.Contains(guid))
    //                {
    //                    DependenciesGUIDs.Add(guid);
    //                    AssetMonitorInfo reference = AssetMonitorTools.GetAssetMonitorInfoByGuid(guid);
    //                    if (reference != null && !reference.ReferencesGUIDs.Contains(GUID))
    //                        reference.ReferencesGUIDs.Add(GUID);
    //                }
    //            }
    //        }
    //        foreach (var guid in AssetMonitorTools.FindDependenciesByYaml(this.Path))
    //        {
    //            if (AssetMonitorTools.CheckGuid(guid) && !DependenciesGUIDs.Contains(guid))
    //            {
    //                DependenciesGUIDs.Add(guid);
    //                AssetMonitorInfo reference = AssetMonitorTools.GetAssetMonitorInfoByGuid(guid);
    //                if (reference != null && !reference.ReferencesGUIDs.Contains(GUID))
    //                    reference.ReferencesGUIDs.Add(GUID);
    //            }
    //        }
    //    }

    //    public static string[] GetAssetsGUIDs(string[] paths)
    //    {
    //        if (paths == null || paths.Length == 0)
    //        {
    //            return null;
    //        }

    //        var guids = new List<string>(paths.Length);
    //        foreach (var path in paths)
    //        {
    //            var guid = AssetDatabase.AssetPathToGUID(path);
    //            if (!string.IsNullOrEmpty(guid))
    //            {
    //                guids.Add(guid);
    //            }
    //        }

    //        return guids.ToArray();
    //    }
    //}

}