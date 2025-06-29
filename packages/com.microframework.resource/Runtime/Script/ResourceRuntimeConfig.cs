using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    [Serializable]
    public class ResourceRuntimeConfig : IMicroRuntimeConfig
    {
        [SerializeReference]
        internal MicroClassSerializer PathProcessorSerializer;
        [SerializeReference]
        internal MicroClassSerializer ResourcePatchSerializer;
        [SerializeReference]
        public List<IResourceCustomConfig> Configs = new List<IResourceCustomConfig>();

        public IResourcePathProcessor PathProcessor { get; private set; }
        public IResourcePatcher ResourcePatcher { get; private set; }
        public ResourceRuntimeConfig()
        {
            Type tempType = PathProcessorSerializer?.CurrentType;
            if (tempType == null)
            {
                PathProcessor = new DefaultResourcePathProcessor();
                PathProcessorSerializer = new MicroClassSerializer()
                {
                    AssemblyName = typeof(DefaultResourcePathProcessor).Assembly.FullName,
                    TypeName = typeof(DefaultResourcePathProcessor).FullName
                };
                MicroLogger.LogWarning($"{nameof(ResourceRuntimeConfig)}:{nameof(PathProcessorSerializer)}为空，使用默认值");
            }
            else
                PathProcessor = Activator.CreateInstance(tempType) as IResourcePathProcessor;
            tempType = ResourcePatchSerializer?.CurrentType;
            if (tempType == null)
            {
                ResourcePatcher = new DefaultResourcePatch();
                ResourcePatchSerializer = new MicroClassSerializer()
                {
                    AssemblyName = typeof(DefaultResourcePatch).Assembly.FullName,
                    TypeName = typeof(DefaultResourcePatch).FullName
                };
                MicroLogger.LogWarning($"{nameof(ResourceRuntimeConfig)}:{nameof(ResourcePatchSerializer)}为空，使用默认值");
            }
            else
                ResourcePatcher = Activator.CreateInstance(tempType) as IResourcePatcher;
        }

    
    }
}
