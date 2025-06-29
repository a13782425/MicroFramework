using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 自定义资产监控
    /// </summary>
    public interface ICustomAssetMonitor
    {
        /// <summary>
        /// 资产监控目录或者文件(颗粒度越小优先级越高)
        /// 以AssetDatabase可读为基准
        /// <para>Assets/CustomAssetMonitor</para>
        /// <para>Assets/CustomAssetMonitor/Data.bytes</para>
        /// <para>同时出现上面两个配置,如果是Data.bytes发生变化,则调用下面的实现,如果是其下的其他文件则调用上面的</para>
        /// </summary>
        /// <example>Assets/CustomAssetMonitor</example>
        /// <example>Assets/CustomAssetMonitor/Data.bytes</example>
        /// <example>Packages/CustomAssetMonitor</example>
        string Path { get; }

        /// <summary>
        /// 文件监控有变化
        /// </summary>
        /// <param name="filePath">基于AssetDatabase可读的路径,变化的文件,如果监控的文件夹会调用多次</param>
        /// <returns>当前文件包含的资源,以AssetDatabase可读的路径为准</returns>
        IEnumerable<string> OnAssetMonitorChanged(string filePath);

    }
}
