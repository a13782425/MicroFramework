using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core
{
    internal sealed class DefaultMicroLayout : BaseMicroLayout
    {
        public override string Title { get; }
        private int _priority = 0;
        public override int Priority => _priority;
        internal void SetPriority(int priority) => this._priority = priority;
    }
}
