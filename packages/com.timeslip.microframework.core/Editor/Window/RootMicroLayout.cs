using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static MFramework.Core.MicroRuntimeConfig;
using static UnityEditor.GenericMenu;

namespace MFramework.Core.Editor
{
    internal class RootMicroLayout : BaseMicroLayout
    {
        public override string Title => "微框架";

        public override int Priority => int.MinValue;

        private MicroRuntimeConfig _config;
        private ReorderableList _reorderableList;
        //TODO: 下拉列表
        private List<MicroClassSerializer> _allModuleClassList = new List<MicroClassSerializer>();
        public override bool Init()
        {
            _config = MicroRuntimeConfig.CurrentConfig;
            _reorderableList = new ReorderableList(_config.InitModules, typeof(MicroClassSerializer), true, true, true, true);
            _reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "自定义加载的模块:");
            };
            _reorderableList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _config.InitModules[index];
                    rect.y += 2;
                    if (EditorGUI.DropdownButton(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                         new GUIContent(element.ToString()), FocusType.Passive
                         ))
                    {
                        // 计算弹出菜单的位置和大小
                        Rect popupPosition = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                        m_buildPopupList(m_popupSelect).DropDown(popupPosition);
                    }
                };
            _reorderableList.onAddDropdownCallback = m_addDropdownCallback;
            _reorderableList.onAddCallback = m_addCallback;
            _reorderableList.onCanAddCallback = m_canAddCallback;
            _reorderableList.onRemoveCallback = m_removeCallback;
            Label label = new Label();
            label.text = "微框架配置";
            label.AddToClassList(MicroStyles.H1);
            IMGUIContainer container = new IMGUIContainer(m_onGui);
            container.style.marginTop = 4;
            container.style.marginBottom = 4;
            container.style.marginLeft = 4;
            container.style.marginRight = 4;
            panel.Add(label);
            panel.Add(container);
            Button generateButton = new Button();
            generateButton.text = "生成类映射文件";
            generateButton.clicked += m_generateButtonClick;
            panel.Add(generateButton);
            return true;
        }

        [MenuItem("MFramework/生成器/生成类映射文件")]
        private static void m_generateButtonClick()
        {
            string filePath = EditorUtility.SaveFilePanelInProject("生成类映射文件", "MicroTypeMapper", "cs", "");
            if (string.IsNullOrWhiteSpace(filePath))
            {
                EditorUtility.DisplayDialog("错误", "生成路径为空", "关闭");
                return;
            }
            const string ONE_TAB = "    ";
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("//------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("//-------------------------------------------- generate file -------------------------------------------------");
            sb.AppendLine("//------------------------------------------------------------------------------------------------------------");
            sb.AppendLine($"public class {Path.GetFileNameWithoutExtension(filePath)} : MFramework.Core.IMicroTypeMapper");
            sb.AppendLine($"{{");
            sb.AppendLine($"{ONE_TAB}public System.Type GetType(string typeFullName)");
            sb.AppendLine($"{ONE_TAB}{{");
            sb.AppendLine($"{ONE_TAB}{ONE_TAB}return null;");
            sb.AppendLine($"{ONE_TAB}}}");

            sb.AppendLine($"{ONE_TAB}public System.Type GetType(MFramework.Core.MicroClassSerializer classSerializer)");
            sb.AppendLine($"{ONE_TAB}{{");
            sb.AppendLine($"{ONE_TAB}{ONE_TAB}return GetType(classSerializer.TypeName);");
            sb.AppendLine($"{ONE_TAB}}}");

            sb.AppendLine($"}}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        private void m_addDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            m_buildPopupList(m_addPopupSelect).DropDown(buttonRect);
        }

        private GenericMenu m_buildPopupList(MenuFunction2 popupSelect)
        {
            GenericMenu genericMenu = new GenericMenu();
            foreach (var item in _allModuleClassList)
            {
                genericMenu.AddItem(new GUIContent(item.TypeName), false, popupSelect, item);
            }
            return genericMenu;
        }
        private void m_addPopupSelect(object userData)
        {
            if (userData is MicroClassSerializer moduleClass)
            {
                _config.InitModules.Add(moduleClass);
                _allModuleClassList.Remove(moduleClass);
            }
        }
        private void m_popupSelect(object userData)
        {
            if (userData is MicroClassSerializer moduleClass)
            {
                if (_reorderableList.index >= 0 && _reorderableList.index < _config.InitModules.Count)
                {
                    MicroClassSerializer temp = _config.InitModules[_reorderableList.index];
                    _config.InitModules[_reorderableList.index] = moduleClass;
                    _allModuleClassList.Remove(moduleClass);
                    _allModuleClassList.Add(temp);
                }
            }
        }

        private bool m_canAddCallback(ReorderableList list)
        {
            return _allModuleClassList.Count > 0;
        }

        private void m_removeCallback(ReorderableList list)
        {
            if (list.index >= 0 && list.index < _config.InitModules.Count)
            {
                MicroClassSerializer moduleClass = _config.InitModules[list.index];
                _config.InitModules.RemoveAt(list.index);
                _allModuleClassList.Add(moduleClass);
            }
        }

        private void m_addCallback(ReorderableList list)
        {
            MicroClassSerializer moduleClass = _allModuleClassList[0];
            _allModuleClassList.RemoveAt(0);
            _config.InitModules.Add(moduleClass);
        }
        private void m_resetList()
        {
            _allModuleClassList.Clear();
            var modules = TypeCache.GetTypesDerivedFrom<IMicroModule>();
            foreach (var module in modules)
            {
                if (module.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                if (module.IsAbstract || module.IsInterface || !module.IsClass)
                    continue;
                if (_config.InitModules.FirstOrDefault(a => a.AssemblyName == module.Assembly.FullName && a.TypeName == module.FullName) != null)
                {
                    //已经有了
                    continue;
                }
                //还没有
                _allModuleClassList.Add(new MicroClassSerializer() { AssemblyName = module.Assembly.FullName, TypeName = module.FullName });
            }
        }
        public override void ShowUI()
        {
            m_resetList();
        }



        private void m_onGui()
        {
            _config.AutoRegisterModule = EditorGUILayout.ToggleLeft("是否自动加载全部模块", _config.AutoRegisterModule);
            if (!_config.AutoRegisterModule)
            {
                _reorderableList.DoLayoutList();
            }
        }
    }
}
