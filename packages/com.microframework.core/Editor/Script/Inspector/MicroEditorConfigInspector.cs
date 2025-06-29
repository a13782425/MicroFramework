using UnityEditor;

namespace MFramework.Core.Editor
{
    [CustomEditor(typeof(MicroEditorConfig))]
    internal class MicroEditorConfigInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}
