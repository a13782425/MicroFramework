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
    public class UIGameObject : UIComponent
    {
        public UIGameObject(UIView view, GameObject obj)
        {
            this.gameObject = obj;
            view.RegisterComponent(this);
        }
    }
}
