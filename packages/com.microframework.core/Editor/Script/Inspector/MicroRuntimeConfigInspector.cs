using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    [CustomEditor(typeof(MicroRuntimeConfig))]
    internal class MicroRuntimeConfigInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }

        //public override VisualElement CreateInspectorGUI()
        //{
        //    var visualElement = new InspectorElement(this.serializedObject);
        //    visualElement.SetEnabled(false);
        //    return visualElement;
        //}
    }
}
