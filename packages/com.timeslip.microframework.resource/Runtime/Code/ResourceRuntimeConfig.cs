using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    [DisplayName("运行时配置")]
    public class ResourceRuntimeConfig : ICustomMicroRuntimeConfig
    {
        [DisplayName("路径处理器")]
        [SerializeField]
        public MicroClassSerializer PathProcessorSerializer;
        [SerializeField]
        [DisplayName("资源补丁")]
        public MicroClassSerializer ResourcePatchSerializer;

        public IResourcePathProcessor PathProcessor { get; private set; }
        public IResourcePatcher ResourcePatcher { get; private set; }
        public ResourceRuntimeConfig()
        {
            Type tempType = PathProcessorSerializer?.CurrentType;
            if (tempType == null)
                MicroLogger.LogError($"{nameof(ResourceRuntimeConfig)}:{nameof(PathProcessorSerializer)}为空");
            else
                PathProcessor = Activator.CreateInstance(tempType) as IResourcePathProcessor;
            tempType = ResourcePatchSerializer?.CurrentType;
            if (tempType == null)
                MicroLogger.LogError($"{nameof(ResourceRuntimeConfig)}:{nameof(ResourcePatchSerializer)}为空");
            else
                ResourcePatcher = Activator.CreateInstance(tempType) as IResourcePatcher;
        }

        [SerializeReference]
        public List<IResourceCustomConfig> Configs = new List<IResourceCustomConfig>();
    }
}
