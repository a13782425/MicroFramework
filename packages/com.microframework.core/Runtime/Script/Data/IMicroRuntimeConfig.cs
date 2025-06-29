using System;

namespace MFramework.Core
{
    /// <summary>
    /// 自定义运行时配置文件
    /// </summary>
    public interface IMicroRuntimeConfig : IConstructor
    {
        void Save() => MicroRuntimeConfig.CurrentConfig?.Save();
    }
}
