using MFramework.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Editor
{
    internal class UIConfigMicroLayout : BaseMicroLayout
    {
        public override string Title => "UI(MVVM)/编辑器配置";

        private UIEditorConfig _config = default;

        public override bool Init()
        {
            _config = MicroEditorConfig.Instance.GetEditorConfig<UIEditorConfig>();
            Label label = new Label("UI(MVVM)配置");
            label.AddToClassList(MicroStyles.H2);
            panel.Add(label);
            IMGUIContainer container = new IMGUIContainer(m_onGui);
            container.style.marginTop = 4;
            container.style.marginBottom = 4;
            container.style.marginLeft = 4;
            container.style.marginRight = 4;
            panel.Add(container);
            panel.Add(_config.DrawUI());
            return base.Init();
        }

        private void m_onGui()
        {
            EditorGUILayout.LabelField($"导出组件Tag为：{UIEditorConfig.TAG_NAME}", GUILayout.MinWidth(48));
            EditorGUILayout.LabelField($"导出关联的Widget前缀为：{UIEditorConfig.WIDGET_HEAD}", GUILayout.MinWidth(48));
            EditorGUILayout.LabelField($"\t既{UIEditorConfig.WIDGET_HEAD}xxxx，它会与已存在的Widget预制进行关联", GUILayout.MinWidth(48));
            _config.Namespace = EditorGUILayout.TextField("脚本命名空间：", _config.Namespace);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Panel预制体文件夹：", _config.PanelPrefabRoot);
                if (GUILayout.Button("选择文件夹", GUILayout.MaxWidth(128)))
                {
                    string str = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                    string path = UnityEditor.FileUtil.GetProjectRelativePath(str);//获取基于Assets的路径
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        _config.PanelPrefabRoot = path;
                        _config.Save();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Widget预制体文件夹：", _config.WidgetPrefabRoot);
                if (GUILayout.Button("选择文件夹", GUILayout.MaxWidth(128)))
                {
                    string str = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                    string path = UnityEditor.FileUtil.GetProjectRelativePath(str);//获取基于Assets的路径
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        _config.WidgetPrefabRoot = path;
                        _config.Save();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Panel代码文件夹：", _config.PanelCodeRoot);
                if (GUILayout.Button("选择文件夹", GUILayout.MaxWidth(128)))
                {
                    string str = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                    string path = UnityEditor.FileUtil.GetProjectRelativePath(str);//获取基于Assets的路径
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        _config.PanelCodeRoot = path;
                        _config.Save();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Widget代码文件夹：", _config.WidgetCodeRoot);
                if (GUILayout.Button("选择文件夹", GUILayout.MaxWidth(128)))
                {
                    string str = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                    string path = UnityEditor.FileUtil.GetProjectRelativePath(str);//获取基于Assets的路径
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        _config.WidgetCodeRoot = path;
                        _config.Save();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("生成代码保存文件：", _config.CodeGenFileRoot);
                if (GUILayout.Button("选择文件夹", GUILayout.MaxWidth(128)))
                {
                    string str = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                    string path = UnityEditor.FileUtil.GetProjectRelativePath(str);//获取基于Assets的路径
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        _config.CodeGenFileRoot = path;
                        _config.Save();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
