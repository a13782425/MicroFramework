using MFramework.Core;
using System;
using System.Collections.Generic;
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

        private IResourcePathProcessor _pathProcessor;
        public IResourcePathProcessor PathProcessor
        {
            get
            {
                if (_pathProcessor == null)
                {
                    m_createPathProcessor();
                }
                else
                {
                    if (_pathProcessor.GetType() != PathProcessorSerializer?.CurrentType)
                    {
                        m_createPathProcessor();
                    }
                }
                return _pathProcessor;
            }
        }
        private IResourcePatcher _patch;
        public IResourcePatcher ResourcePatcher
        {
            get
            {
                if (_patch == null)
                {
                    m_createResourcePatcher();
                }
                else
                {
                    if (_patch.GetType() != ResourcePatchSerializer?.CurrentType)
                    {
                        m_createResourcePatcher();
                    }
                }
                return _patch;
            }
        }


        private void m_createPathProcessor()
        {
            Type tempType = PathProcessorSerializer?.CurrentType;
            if (tempType == null)
            {
                _pathProcessor = new DefaultResourcePathProcessor();
                PathProcessorSerializer = new MicroClassSerializer()
                {
                    AssemblyName = typeof(DefaultResourcePathProcessor).Assembly.FullName,
                    TypeName = typeof(DefaultResourcePathProcessor).FullName
                };
                MicroLogger.LogWarning($"{nameof(ResourceRuntimeConfig)}:{nameof(PathProcessorSerializer)}为空，使用默认值");
            }
            else
                _pathProcessor = Activator.CreateInstance(tempType) as IResourcePathProcessor;
        }
        private void m_createResourcePatcher()
        {
            Type tempType = ResourcePatchSerializer?.CurrentType;
            if (tempType == null)
            {
                _patch = new DefaultResourcePatch();
                ResourcePatchSerializer = new MicroClassSerializer()
                {
                    AssemblyName = typeof(DefaultResourcePatch).Assembly.FullName,
                    TypeName = typeof(DefaultResourcePatch).FullName
                };
                MicroLogger.LogWarning($"{nameof(ResourceRuntimeConfig)}:{nameof(ResourcePatchSerializer)}为空，使用默认值");
            }
            else
                _patch = Activator.CreateInstance(tempType) as IResourcePatcher;
        }
    }
}
