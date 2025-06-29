using System;
using UnityEditor;
using UnityEngine;

namespace MFramework.Core.Editor
{
    //[Serializable]
    //public abstract class CustomMicroEditorConfig : ICustomMicroEditorConfig
    //{

    //    public void Save() => MicroEditorConfig.Instance.Save();
    //}

    public interface IMicroEditorConfig : IConstructor
    {
        void Save() => MicroEditorConfig.Instance.Save();
    }
}
