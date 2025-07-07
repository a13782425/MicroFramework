using MFramework.Core;
using MFramework.Core.Editor;
using MFramework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Editor
{
    internal class ResourceRuntimeLayout : BaseMicroLayout
    {
        public override string Title => "资源管理";
        public override int Priority => int.MinValue;

        private ResourceRuntimeConfig _config;

        private MicroClassElement _pathClassElement;
        private MicroClassElement _patchClassElement;

        public override bool Init()
        {
            _config = MicroRuntimeConfig.CurrentConfig.GetRuntimeConfig<ResourceRuntimeConfig>();
            _pathClassElement = new MicroClassElement("资源路径处理器", _config.PathProcessorSerializer, typeof(IResourcePathProcessor));
            _patchClassElement = new MicroClassElement("资源补丁配置", _config.ResourcePatchSerializer, typeof(IResourcePatcher));
            this.panel.Add(_pathClassElement);
            this.panel.Add(_patchClassElement);
            //var element = new MicroObjectField(new abc());
            //element.ShowFoldout = true;
            //this.Add(element);
            return base.Init();
        }
    }

    //public class abc
    //{
    //    public int i;
    //}

}
