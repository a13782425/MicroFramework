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
    public class UITransform : UIComponent
    {
        public UITransform(UIView view, Transform tran)
        {
            this.gameObject = tran.gameObject;
            view.RegisterComponent(this);
        }
    }
}
