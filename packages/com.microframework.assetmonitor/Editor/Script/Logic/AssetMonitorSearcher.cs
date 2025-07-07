using System;
using System.IO;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 搜索器, 可实现资源属性的多维度匹配查询
    /// </summary>
    public interface IAssetMonitorSearcher
    {
        /// <summary>
        /// 搜索器名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 搜索器描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 搜索前缀
        /// 全部小写
        /// </summary>
        /// <example>
        /// //当输入e或者ext时候调用该类型,既e:txt
        /// public class ExtensionSearch : IAssetMonitorSearch
        /// {
        ///    public string[] SearchOptions => new string[] { "e", "ext" };
        /// }
        /// </example>
        string[] SearchPrefixes { get; }
        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="record">资源文件的记录(克隆值)</param>
        /// <param name="value">输入的参数</param>
        /// <returns>是否匹配</returns>
        bool Match(AssetInfoRecord record, string value);
    }

    /// <summary>
    /// 名称搜索
    /// </summary>
    internal sealed class NameAssetMonitorSearch : IAssetMonitorSearcher
    {
        public string Name => "内置名称搜索";
        public string Description => "";
        public string[] SearchPrefixes => new string[] { "", "n", "name" };


        public bool Match(AssetInfoRecord record, string value)
        {
            if (record == null)
                return false;
            if (value == null)
                return true;
            return record.AssetName.ToLower().Contains(value);
        }
    }
    /// <summary>
    /// Guid搜索
    /// </summary>
    internal sealed class GuidAssetMonitorSearch : IAssetMonitorSearcher
    {
        public string Name => "内置Guid搜索";
        public string Description => "";
        public string[] SearchPrefixes => new string[] { "g", "guid" };


        public bool Match(AssetInfoRecord record, string value)
        {
            if (record == null)
                return false;
            if (value == null)
                return true;
            return record.Guid.ToLower().Contains(value);
        }
    }
    /// <summary>
    /// 拓展名搜索
    /// </summary>
    internal sealed class ExtensionAssetMonitorSearch : IAssetMonitorSearcher
    {
        public string Name => "内置扩展名搜索";
        public string Description => "";
        public string[] SearchPrefixes => new string[] { "e", "ext" };

        public bool Match(AssetInfoRecord record, string value)
        {
            if (record == null)
                return false;
            if (record.IsFolder)
                return false;
            if (value == null)
                return true;
            var extension = Path.GetExtension(record.AssetName).ToLower().TrimStart('.');
            return extension == value.ToLower().TrimStart('.');
        }
    }
    /// <summary>
    /// 大小搜索
    /// </summary>
    internal sealed class SizeAssetMonitorSearch : IAssetMonitorSearcher
    {
        public string Name => "内置大小搜索";
        public string Description => "";
        public string[] SearchPrefixes => new string[] { "s", "size" };

        public bool Match(AssetInfoRecord record, string value)
        {
            if (record == null)
                return false;
            if (record.IsFolder)
                return false;
            if (value == null)
                return true;
            long targetSize = 0;
            long fileSize = record.Size;
            try
            {
                // 支持 >100kb, <1mb, =500b 等格式
                if (value.StartsWith(">"))
                {
                    targetSize = m_parseSizeString(value.Substring(1));
                    return fileSize > targetSize;
                }
                else if (value.StartsWith("<"))
                {
                    targetSize = m_parseSizeString(value.Substring(1));
                    return fileSize < targetSize;
                }
                else if (value.StartsWith("="))
                {
                    targetSize = m_parseSizeString(value.Substring(1));
                    return Math.Abs(fileSize - targetSize) < 1024; // 1KB 误差范围
                }
                else
                {
                    targetSize = m_parseSizeString(value);
                    return Math.Abs(fileSize - targetSize) < 1024;
                }
            }
            catch
            {
                return false;
            }

        }

        // 添加大小字符串解析方法
        private long m_parseSizeString(string sizeStr)
        {
            sizeStr = sizeStr.ToLower().Trim();

            long multiplier = 1;
            if (sizeStr.EndsWith("k"))
            {
                multiplier = 1024;
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
            }
            else if (sizeStr.EndsWith("kb"))
            {
                multiplier = 1024;
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 2);
            }
            if (sizeStr.EndsWith("m"))
            {
                multiplier = 1024 * 1024;
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
            }
            else if (sizeStr.EndsWith("mb"))
            {
                multiplier = 1024 * 1024;
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 2);
            }
            if (sizeStr.EndsWith("g"))
            {
                multiplier = 1024 * 1024 * 1024;
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
            }
            else if (sizeStr.EndsWith("gb"))
            {
                multiplier = 1024 * 1024 * 1024;
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 2);
            }
            else if (sizeStr.EndsWith("b"))
            {
                sizeStr = sizeStr.Substring(0, sizeStr.Length - 1);
            }

            if (double.TryParse(sizeStr, out double value))
                return (long)(value * multiplier);

            return 0;
        }
    }

    /// <summary>
    /// 类型搜索
    /// </summary>
    internal sealed class TypeAssetMonitorSearch : IAssetMonitorSearcher
    {
        public string Name => "内置类型搜索";

        public string Description => "";

        public string[] SearchPrefixes => new string[] { "t", "type" };

        public bool Match(AssetInfoRecord record, string value)
        {
            if (record.IsFolder)
                return value == "folder" || value == "directory" || value == "dir";
            var typeName = record.AssetType.ToLower();

            return typeName.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
