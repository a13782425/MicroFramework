using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 查看GUI编辑器样式
    /// </summary>
    internal class EditorStyleMicroLayout : BaseMicroLayout
    {
        public override string Title => "Tool/编辑器内置样式预览器";
        public override int Priority => int.MaxValue;

        public override bool Init()
        {
            IMGUIContainer iMGUIContainer = new IMGUIContainer(m_onGui);
            panel.Add(iMGUIContainer);
            return base.Init();
        }
        Vector2 scrollPosition = Vector2.zero;
        string searchStr = "";
        private void m_onGui()
        {
            GUILayout.BeginHorizontal("helpbox");
            GUILayout.Label("查找内置样式：");
            searchStr = GUILayout.TextField(searchStr, "SearchTextField");
            if (GUILayout.Button("", "SearchCancelButton"))
            {
                searchStr = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
            foreach (GUIStyle style in GUI.skin)
            {
                if (style.name.ToLower().Contains(searchStr.ToLower()))
                {
                    DrawStyle(style);
                }
            }
            GUILayout.EndScrollView();
        }
        void DrawStyle(GUIStyle style)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Button(style.name, style.name);
            GUILayout.FlexibleSpace();
            EditorGUILayout.SelectableLabel(style.name);
            if (GUILayout.Button("复制样式名称"))
            {
                EditorGUIUtility.systemCopyBuffer = style.name;
                window.ShowNotification(new GUIContent("复制样式名成功"), 1);
            }
            GUILayout.EndHorizontal();
        }
    }
}
