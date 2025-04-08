using System;

namespace MFramework.Core
{
    //    /// <summary>
    //    /// 运行时数据
    //    /// </summary>
    //    [Serializable]
    //    public abstract class CustomMicroRuntimeConfig
    //    {
    //#if UNITY_EDITOR
    //        public void Save() => MicroRuntimeConfig.Instance.Save();
    //#endif
    //    }

    public interface ICustomMicroRuntimeConfig : IConstructor
    {
#if UNITY_EDITOR
        void Save() => MicroRuntimeConfig.CurrentConfig?.Save();
#endif
    }
}
