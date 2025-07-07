using System;

namespace MFramework.Core
{
    /// <summary>
    /// 自定义运行时配置文件
    /// </summary>
    public interface IMicroRuntimeConfig : IConstructor
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init() { }
        /// <summary>
        /// 保存
        /// </summary>
        void Save() => MicroRuntimeConfig.CurrentConfig?.Save();
    }
}
