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
        private string _title;
        public override string Title => _title;
        private int _priority = 0;
        public override int Priority => _priority;

        internal void SetTitle(string title) => this._title = title;
        internal void SetPriority(int priority) => this._priority = priority;
    }
}
