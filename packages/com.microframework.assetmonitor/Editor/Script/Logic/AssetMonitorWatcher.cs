using System.Collections.Generic;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 自定义资产监控
    /// </summary>
    public interface IAssetMonitorWatcher
    {
        /// <summary>
        /// 自定义资源监控的名字
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 自定义资源监控的描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 资产监控目录,文件,拓展名(颗粒度越小优先级越高)
        /// 以AssetDatabase可读为基准
        /// <para>Assets/CustomAssetMonitor</para>
        /// <para>Assets/CustomAssetMonitor/Data.bytes</para>
        /// <para>*.png|*.fbx</para>
        /// <para>优先级: 文件 > 文件夹 > 拓展名 </para>
        /// </summary>
        /// <example>Assets/CustomAssetMonitor</example>
        /// <example>Assets/CustomAssetMonitor/Data.bytes</example>
        /// <example>Packages/CustomAssetMonitor</example>
        /// <example>*.png</example>
        /// <example>*.png|*.fbx</example>
        string WatchPath { get; }

        /// <summary>
        /// 文件监控有变化
        /// </summary>
        /// <param name="record">资源文件的记录(克隆值)</param>
        /// <returns>当前文件包含的资源,以AssetDatabase可读的路径为准</returns>
        IEnumerable<string> OnAssetChanged(AssetInfoRecord record);

    }
}
