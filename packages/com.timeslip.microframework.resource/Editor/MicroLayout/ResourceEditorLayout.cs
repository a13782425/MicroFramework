using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Editor
{
    internal class ResourceEditorLayout : BaseMicroLayout
    {
        public override string Title => "资源管理/基础/编辑器配置";
        public override int Priority => int.MinValue + 1;
    }
}
