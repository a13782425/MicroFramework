using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    [Ignore]
    public class UIRectTransform : UITransform
    {
        public UIRectTransform(UIView view, RectTransform rectTransform) : base(view, rectTransform) { }
    }
}
