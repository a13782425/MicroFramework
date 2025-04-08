using MFramework.Core;
using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MFramework.Editor
{
    [CustomPreview(typeof(Object))]
    public class ResourceInspectorExtension : ObjectPreview
    {
        public override void Initialize(Object[] targets)
        {
            base.Initialize(targets);
            //resPath = AssetDatabase.GetAssetPath(target);
        }
        public override bool HasPreviewGUI()
        {

            bool isShow = !Application.isPlaying && Selection.assetGUIDs.Length == 1; //showPreview;
            if (isShow)
            {
              
            }
            return isShow;
        }
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("资源路径拓展");
        }



    }
}
