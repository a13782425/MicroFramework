namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源校验者接口
    /// </summary>
    public interface IAssetMonitorVerifier
    {
        /// <summary>
        /// 资源校验者名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 资源校验者描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 资源验证的目录,文件,拓展名
        /// 以AssetDatabase可读为基准
        /// <para>Assets/CustomAssetMonitor</para>
        /// <para>Assets/CustomAssetMonitor/Data.bytes</para>
        /// <para>*.png|*.fbx</para>
        /// <para>所有匹配上的都会执行</para>
        /// </summary>
        /// <example>Assets/CustomAssetMonitor</example>
        /// <example>Assets/CustomAssetMonitor/Data.bytes</example>
        /// <example>Packages/CustomAssetMonitor</example>
        /// <example>*.png</example>
        /// <example>*.png|*.fbx</example>
        string VerifyPath { get; }

        /// <summary>
        /// 验证资源
        /// </summary>
        /// <param name="record">资源记录</param>
        /// <returns>验证是否通过</returns>
        bool Verify(AssetInfoRecord record);
    }
}
