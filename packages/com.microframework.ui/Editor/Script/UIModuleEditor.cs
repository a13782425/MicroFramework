using MFramework.Core;
using MFramework.Core.Editor;
using MFramework.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MFramework.UI.Editor
{
    internal static class UIModuleEditor
    {
        private static UIEditorConfig _config = default;
        private static ResourceRuntimeConfig _resourceConfig = default;

        private static string s_panelPrefabPath = string.Empty;
        private static string s_panelScriptPath = string.Empty;
        private static string s_panelVMScriptPath = string.Empty;

        private static string s_widgetPrefabPath = string.Empty;
        private static string s_widgetScriptPath = string.Empty;
        private static string s_widgetVMScriptPath = string.Empty;

        private static string s_panelGenScriptPath = string.Empty;
        private static string s_widgetGenScriptPath = string.Empty;

        [MenuItem("MFramework/UI/生成界面", false, 0)]
        internal static void GenerateView()
        {
            try
            {
                _config = MicroEditorConfig.Instance.GetEditorConfig<UIEditorConfig>();
                _resourceConfig = MicroRuntimeConfig.CurrentConfig.GetRuntimeConfig<ResourceRuntimeConfig>();
                s_checkPath();
                List<ViewDto> viewList = new List<ViewDto>();
                s_loadPrefab(ref viewList);
                StringBuilder uiCodeGen = new StringBuilder();
                //m_initTitle(uiCodeGen);
                foreach (var item in viewList)
                {
                    uiCodeGen.Clear();
                    s_generateView(item);
                    s_generateViewModel(item);
                    if (!File.Exists(item.ViewCodeFile) && !string.IsNullOrWhiteSpace(item.ViewCode))
                    {
                        File.WriteAllText(item.ViewCodeFile, item.ViewCode, new UTF8Encoding());
                    }
                    if (!File.Exists(item.ViewModelCodeFile) && !string.IsNullOrWhiteSpace(item.ViewModelCode))
                    {
                        File.WriteAllText(item.ViewModelCodeFile, item.ViewModelCode, new UTF8Encoding());
                    }
                    m_initTitle(uiCodeGen);
                    if (!string.IsNullOrWhiteSpace(_config.RootNamespace))
                    {
                        uiCodeGen.AppendLine($"{s_getTab(0)}namespace {_config.RootNamespace}");
                        uiCodeGen.AppendLine($"{s_getTab(0)}{{");
                    }
                    uiCodeGen.Append(item.ViewGenCode);
                    uiCodeGen.AppendLine();
                    uiCodeGen.Append(item.ViewModelGenCode);
                    if (!string.IsNullOrWhiteSpace(_config.RootNamespace))
                    {
                        uiCodeGen.AppendLine($"{s_getTab(0)}}}");
                    }
                    File.WriteAllText(Path.Combine(item.IsPanel ? s_panelGenScriptPath : s_widgetGenScriptPath, $"{item.ViewName}.g.cs"), uiCodeGen.ToString(), new UTF8Encoding());
                }
                //File.WriteAllText(_config.CodeGenFile, uiCodeGen.ToString(), new UTF8Encoding());
                EditorUtility.DisplayDialog("Successful", "生成成功succeed", "确定");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                MicroLogger.LogError(ex);
            }
            finally
            {
                //EditorUtility.ClearProgressBar();
            }
        }


        private static void m_initTitle(StringBuilder sb)
        {
            sb.AppendLine("//------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("//-------------------------------------------- generate file -------------------------------------------------");
            sb.AppendLine("//------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("#pragma warning disable IDE0049");
            sb.AppendLine("#pragma warning disable IDE0001");
            sb.AppendLine();
        }

        private static void s_loadPrefab(ref List<ViewDto> viewList)
        {
            //true代表Panel,false代表Widget
            List<bool> rootPathList = new List<bool>() { true, false };
            foreach (var item in rootPathList)
            {
                string rootPath = (item ? s_panelPrefabPath : s_widgetPrefabPath);
                rootPath = Path.GetRelativePath(Environment.CurrentDirectory, rootPath);
                string scriptPath = (item ? s_panelScriptPath : s_widgetScriptPath);
                scriptPath = Path.GetRelativePath(System.Environment.CurrentDirectory, scriptPath);

                string vmScriptPath = (item ? s_panelVMScriptPath : s_widgetVMScriptPath);
                vmScriptPath = Path.GetRelativePath(System.Environment.CurrentDirectory, vmScriptPath);


                string baseName = item ? "MFramework.UI.UIPanel" : "MFramework.UI.UIWidget";
                string[] prefabFiles = Directory.GetFiles(rootPath, "*.prefab", SearchOption.AllDirectories);
                string[] scriptFiles = Directory.GetFiles(scriptPath, "*.cs", SearchOption.AllDirectories);
                foreach (var prefabFile in prefabFiles)
                {
                    if (!_resourceConfig.PathProcessor.IsValid(prefabFile))
                        continue;//路径不合法
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);
                    if (obj == null)
                    {
                        MicroLogger.LogError($"{prefabFile} is not a valid prefab.");
                        continue;
                    }

                    ViewDto viewDto = new ViewDto();
                    viewDto.IsPanel = item;
                    viewDto.BaseName = baseName;
                    viewDto.Name = Path.GetFileNameWithoutExtension(prefabFile);
                    viewDto.ViewName = viewDto.Name + "View";
                    viewDto.ViewModelName = viewDto.Name + "ViewModel";
                    viewDto.ViewGenCode = "";
                    foreach (var script in scriptFiles)
                    {
                        string scriptName = Path.GetFileName(script);
                        if (scriptName == viewDto.ViewName + ".cs")
                            viewDto.ViewCodeFile = script;
                        if (scriptName == viewDto.ViewModelName + ".cs")
                            viewDto.ViewModelCodeFile = script;
                    }
                    if (string.IsNullOrWhiteSpace(viewDto.ViewCodeFile))
                    {
                        viewDto.ViewCodeFile = Path.Combine(scriptPath, $"{viewDto.ViewName}.cs");
                        s_createViewCode(viewDto);
                    }
                    if (string.IsNullOrWhiteSpace(viewDto.ViewModelCodeFile))
                    {
                        viewDto.ViewModelCodeFile = Path.Combine(vmScriptPath, $"{viewDto.ViewModelName}.cs");
                        s_createViewModelCode(viewDto);
                    }


                    s_getTrans(obj.transform, "", viewDto);
                    s_getComponent(viewDto);
                    obj = null;
                    viewDto.AssetPath = _resourceConfig.PathProcessor.GetAssetPath(prefabFile);

                    viewList.Add(viewDto);
                }
            }
        }

        private static void s_generateView(ViewDto viewDto)
        {
            //trans.Reverse();
            StringBuilder genString = new StringBuilder();
            genString.AppendLine($"{s_getTab(1)}[MFramework.UI.UIConfigAttribute(\"{viewDto.AssetPath}\")]");
            genString.AppendLine($"{s_getTab(1)}[MFramework.Core.IgnoreAttribute]");
            genString.AppendLine($"{s_getTab(1)}partial class {viewDto.ViewName} : {viewDto.BaseName}");
            genString.AppendLine($"{s_getTab(1)}{{");
            viewDto.Components.Reverse();
            genString.AppendLine($"{s_getTab(2)}private {viewDto.ViewModelName} viewModel;");
            foreach (var item in viewDto.Components)
            {
                genString.AppendLine($"{s_getTab(2)}private {item.ComType} {item.ComName};");
            }
            genString.AppendLine($"{s_getTab(2)}protected override MFramework.UI.IViewModel GetViewModel() => viewModel;");
            genString.AppendLine($"{s_getTab(2)}protected override void InitializeElement()");
            genString.AppendLine($"{s_getTab(2)}{{");
            genString.AppendLine($"{s_getTab(3)}viewModel = new {viewDto.ViewModelName}(this);");
            foreach (var item in viewDto.Components)
            {
                if (item.ComName.StartsWith(UIEditorConfig.WIDGET_HEAD))
                {
                    genString.AppendLine($"{s_getTab(3)}this.{item.ComName} = this.AddWidget<{item.ComType}>(this.Transform.Find(\"{item.Path}\").gameObject);");
                }
                else
                {
                    switch (item.ComType)
                    {
                        case string str when str == typeof(GameObject).Name:
                            genString.AppendLine($"{s_getTab(3)}this.{item.ComName} = this.Transform.Find(\"{item.Path}\").gameObject;");
                            break;
                        case string str when str == typeof(Transform).Name:
                            genString.AppendLine($"{s_getTab(3)}this.{item.ComName} = this.Transform.Find(\"{item.Path}\");");
                            break;
                        default:
                            genString.AppendLine($"{s_getTab(3)}this.{item.ComName} = this.Transform.Find(\"{item.Path}\").GetComponent<{item.ComType}>();");
                            break;
                    }
                }
            }
            genString.AppendLine($"{s_getTab(2)}}}");

            #region 索引器

            genString.AppendLine($"{s_getTab(2)}public System.Object this[string elementName] => elementName switch");
            genString.AppendLine($"{s_getTab(2)}{{");
            foreach (var item in viewDto.Components)
                genString.AppendLine($"{s_getTab(3)}\"{item.ComName}\" => this.{item.ComName},");
            genString.AppendLine($"{s_getTab(3)}_ => throw new System.NullReferenceException($\"{viewDto.ViewName}: {{elementName}} not found!\"),");
            genString.AppendLine($"{s_getTab(2)}}};");

            //genString.AppendLine($"{s_getTab(2)}public System.Object this[string elementName]");
            //genString.AppendLine($"{s_getTab(2)}{{");
            //genString.AppendLine($"{s_getTab(3)}get");
            //genString.AppendLine($"{s_getTab(3)}{{");
            //genString.AppendLine($"{s_getTab(4)}switch (elementName)");
            //genString.AppendLine($"{s_getTab(4)}{{");
            //foreach (var item in viewDto.Components)
            //{
            //    genString.AppendLine($"{s_getTab(5)}case \"{item.ComName}\":");
            //    genString.AppendLine($"{s_getTab(6)}return this.{item.ComName};");
            //}
            //genString.AppendLine($"{s_getTab(5)}default:");
            //genString.AppendLine($"{s_getTab(6)}throw new System.NullReferenceException($\"{viewDto.ViewName}: {{elementName}} not found!\");");
            //genString.AppendLine($"{s_getTab(4)}}}");
            //genString.AppendLine($"{s_getTab(3)}}}");
            //genString.AppendLine($"{s_getTab(2)}}}");

            #endregion

            #region 泛型方法

            genString.AppendLine($"{s_getTab(2)}public T GetElement<T>(string elementName) where T : class => elementName switch");
            genString.AppendLine($"{s_getTab(2)}{{");
            foreach (var item in viewDto.Components)
                genString.AppendLine($"{s_getTab(3)}\"{item.ComName}\" => this.{item.ComName} as T,");
            genString.AppendLine($"{s_getTab(3)}_ => throw new System.NullReferenceException($\"{viewDto.ViewName}: {{elementName}} not found!\"),");
            genString.AppendLine($"{s_getTab(2)}}};");

            //genString.AppendLine($"{s_getTab(2)}public T GetElement<T>(string elementName) where T : class");
            //genString.AppendLine($"{s_getTab(2)}{{");
            //genString.AppendLine($"{s_getTab(3)}switch (elementName)");
            //genString.AppendLine($"{s_getTab(3)}{{");
            //foreach (var item in viewDto.Components)
            //{
            //    genString.AppendLine($"{s_getTab(4)}case \"{item.ComName}\":");
            //    genString.AppendLine($"{s_getTab(5)}return this.{item.ComName} as T;");
            //}
            //genString.AppendLine($"{s_getTab(4)}default:");
            //genString.AppendLine($"{s_getTab(5)}throw new System.NullReferenceException($\"{viewDto.ViewName}: {{elementName}} not found!\");");
            //genString.AppendLine($"{s_getTab(3)}}}");
            //genString.AppendLine($"{s_getTab(2)}}}");

            #endregion

            genString.AppendLine($"{s_getTab(1)}}}");
            viewDto.ViewGenCode = genString.ToString();
        }
        private static void s_generateViewModel(ViewDto viewDto)
        {
            StringBuilder genString = new StringBuilder();

            genString.AppendLine($"{s_getTab(1)}partial class {viewDto.ViewModelName} : MFramework.UI.IViewModel");
            genString.AppendLine($"{s_getTab(1)}{{");
            genString.AppendLine($"{s_getTab(2)}private {viewDto.ViewName} view;");

            genString.AppendLine($"{s_getTab(2)}public {viewDto.ViewModelName}({viewDto.ViewName} view)");
            genString.AppendLine($"{s_getTab(2)}{{");
            genString.AppendLine($"{s_getTab(3)}this.view = view;");
            genString.AppendLine($"{s_getTab(2)}}}");
            genString.AppendLine($"{s_getTab(1)}}}");

            viewDto.ViewModelGenCode = genString.ToString();
        }
        private static void s_createViewCode(ViewDto viewDto)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine();
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(_config.RootNamespace))
            {
                sb.AppendLine($"{s_getTab(0)}namespace {_config.RootNamespace}");
                sb.AppendLine($"{s_getTab(0)}{{");
            }
            sb.AppendLine($"{s_getTab(1)}internal partial class {viewDto.ViewName}");
            sb.AppendLine($"{s_getTab(1)}{{");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}protected override void OnCreate()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//code");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}protected override void OnEnable()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//code");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}protected override void OnDisable()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//code");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}protected override void OnDestroy()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//code");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(1)}}}");
            if (!string.IsNullOrWhiteSpace(_config.RootNamespace))
            {
                sb.AppendLine($"{s_getTab(0)}}}");
            }
            viewDto.ViewCode = sb.ToString();
        }
        private static void s_createViewModelCode(ViewDto viewDto)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using MFramework.Core;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine();
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(_config.RootNamespace))
            {
                sb.AppendLine($"{s_getTab(0)}namespace {_config.RootNamespace}");
                sb.AppendLine($"{s_getTab(0)}{{");
            }
            sb.AppendLine($"{s_getTab(1)}internal partial class {viewDto.ViewModelName}");
            sb.AppendLine($"{s_getTab(1)}{{");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}public void OnCreate()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//接口方法，禁止删除");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}public void OnEnable()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//接口方法，禁止删除");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}public void OnDisable()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//接口方法，禁止删除");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(2)}public void OnDestroy()");
            sb.AppendLine($"{s_getTab(2)}{{");
            sb.AppendLine($"{s_getTab(3)}//接口方法，禁止删除");
            sb.AppendLine($"{s_getTab(2)}}}");
            sb.AppendLine();
            sb.AppendLine($"{s_getTab(1)}}}");
            if (!string.IsNullOrWhiteSpace(_config.RootNamespace))
            {
                sb.AppendLine($"{s_getTab(0)}}}");
            }
            viewDto.ViewModelCode = sb.ToString();
        }

        private static string s_getTab(int lvl, bool checkNamespace = true)
        {
            if (lvl <= 0)
                return "";
            if (string.IsNullOrWhiteSpace(_config.RootNamespace) && checkNamespace)
                return s_getTab(lvl - 1, false);
            string tab = "    ";
            string firstTab = string.IsNullOrWhiteSpace(_config.RootNamespace) ? "" : tab;
            string res = "";
            while (lvl > 0)
            {
                res += tab;
                lvl--;
            }
            return res;
        }

        private static void s_getComponent(ViewDto viewDto)
        {
            foreach (var item in viewDto.TranDtos)
            {
                string tranName = item.Tran.name;
                if (tranName.StartsWith(UIEditorConfig.WIDGET_HEAD))
                {
                    if (viewDto.Components.FirstOrDefault(com => com.ComName == tranName) != null)
                    {
                        throw new Exception($"界面:{viewDto.Name},存在重复命名组件:{tranName}");
                    }
                    GameObject obj = item.Tran.gameObject;
                    if (PrefabUtility.IsPartOfPrefabAsset(obj))
                    {
                        var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
                        string typeName = prefabAsset.name + "View";
                        viewDto.Components.Add(new ComponentDto() { ComName = tranName, ComType = typeName, Path = item.ParentPath + tranName });
                    }
                    else
                    {

                        viewDto.Components.Add(new ComponentDto() { ComName = tranName, ComType = typeof(GameObject).FullName, Path = item.ParentPath + tranName, IsWidget = true });
                    }
                }
                else
                {
                    string[] typeNames = s_getExportType(tranName, viewDto);
                    for (int i = 0; i < typeNames.Length; i++)
                    {
                        string typeName = typeNames[i];
                        string comName = item.ComOriginalName;
                        //if (!UIEditorConfig.MVVMTypeDic.ContainsKey(typeName))
                        //{
                        //    Debug.LogError($"界面:{viewDto.Name} 存在类型:{typeName},无法绑定,请查看UIEditorConfig的配置");
                        //    continue;
                        //}
                        var export = _config.Exports.FirstOrDefault(a => a.UIType.TypeName == typeName);
                        comName = export.UIPrefix + comName;
                        if (viewDto.Components.FirstOrDefault(com => com.ComName == comName) != null)
                        {
                            Debug.LogError($"界面:{viewDto.Name},存在重复命名组件:{tranName}");
                            continue;
                        }
                        viewDto.Components.Add(new ComponentDto() { ComName = comName, ComType = typeName, Path = item.ParentPath + tranName });
                    }
                }
            }
        }
        private static string[] s_getExportType(string name, ViewDto viewDto)
        {
            int index = name.IndexOf("_");
            string str = name;
            if (index > 0)
            {
                str = name.Substring(0, index);
            }
            else
            {
                Debug.LogError("没有对应的前缀:" + name + "  " + viewDto.Name);
            }

            string[] strs = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] typeNames = new string[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                string exportName = strs[i] + "_";
                var export = _config.Exports.FirstOrDefault(a => a.UIPrefix == exportName);
                if (export != null)
                {
                    typeNames[i] = export.UIType.TypeName;
                }
                else
                {
                    typeNames[i] = typeof(Transform).FullName;
                }
            }
            return typeNames;
        }

        private static void s_getTrans(Transform transform, string parentPath, ViewDto viewDto)
        {
            if (!transform.name.StartsWith(UIEditorConfig.WIDGET_HEAD))
            {
                if (transform.childCount > 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform tran = transform.GetChild(i);
                        string path = parentPath;
                        if (transform.parent != null)
                        {
                            path += (transform.name + "/");
                        }

                        s_getTrans(tran, path, viewDto);
                    }
                }
            }
            TranDto tranDto = null;
            if (transform.tag == UIEditorConfig.TAG_NAME)
            {
                tranDto = new TranDto();
                tranDto.Tran = transform;
                int startNum = transform.name.IndexOf('_');
                string str = transform.name.Substring(startNum + 1, transform.name.Length - startNum - 1);
                tranDto.ComOriginalName = str;
                tranDto.ParentPath = parentPath;
                viewDto.TranDtos.Add(tranDto);
            }
        }
        private static void s_checkPath()
        {
            s_panelPrefabPath = Path.Combine(_config.PrefabRootPath, "Panel"); ;
            s_panelScriptPath = Path.Combine(_config.CodeRootPath, "Panel/View");
            s_panelVMScriptPath = Path.Combine(_config.CodeRootPath, "Panel/ViewModel");

            s_widgetPrefabPath = Path.Combine(_config.PrefabRootPath, "Widget");
            s_widgetScriptPath = Path.Combine(_config.CodeRootPath, "Widget/View");
            s_widgetVMScriptPath = Path.Combine(_config.CodeRootPath, "Widget/ViewModel");

            s_panelGenScriptPath = Path.Combine(_config.CodeGenRootPath, "Panel");
            s_widgetGenScriptPath = Path.Combine(_config.CodeGenRootPath, "Widget");

            if (!Directory.Exists(s_panelPrefabPath))
            {
                Directory.CreateDirectory(s_panelPrefabPath);
            }
            if (!Directory.Exists(s_widgetPrefabPath))
            {
                Directory.CreateDirectory(s_widgetPrefabPath);
            }
            if (!Directory.Exists(s_panelGenScriptPath))
            {
                Directory.CreateDirectory(s_panelGenScriptPath);
            }
            if (!Directory.Exists(s_widgetGenScriptPath))
            {
                Directory.CreateDirectory(s_widgetGenScriptPath);
            }
            if (!Directory.Exists(s_panelScriptPath))
            {
                Directory.CreateDirectory(s_panelScriptPath);
            }
            if (!Directory.Exists(s_panelVMScriptPath))
            {
                Directory.CreateDirectory(s_panelVMScriptPath);
            }
            if (!Directory.Exists(s_widgetScriptPath))
            {
                Directory.CreateDirectory(s_widgetScriptPath);
            }
            if (!Directory.Exists(s_widgetVMScriptPath))
            {
                Directory.CreateDirectory(s_widgetVMScriptPath);
            }
        }

        [InitializeOnLoadMethod]
        private static void InitTag()
        {
            // Open tag manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            // Tags Property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            //Debug.Log("TagsPorp Size:" + tagsProp.arraySize);
            List<string> tags = new List<string>();
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                tags.Add(tagsProp.GetArrayElementAtIndex(i).stringValue);
            }

            if (tags.Contains(UIEditorConfig.TAG_NAME))
                return;
            tags.Add(UIEditorConfig.TAG_NAME);
            tagsProp.ClearArray();

            tagManager.ApplyModifiedProperties();


            for (int i = 0; i < tags.Count; i++)
            {
                // Insert new array element
                tagsProp.InsertArrayElementAtIndex(i);
                SerializedProperty sp = tagsProp.GetArrayElementAtIndex(i);
                // Set array element to tagName
                sp.stringValue = tags[i];

                tagManager.ApplyModifiedProperties();
            }
        }

        private class ViewDto
        {
            public bool IsPanel;
            /// <summary>
            /// 预制体名字
            /// </summary>
            public string Name;
            /// <summary>
            /// 视图类名
            /// </summary>
            public string ViewName;
            /// <summary>
            /// ViewModel类名
            /// </summary>
            public string ViewModelName;
            /// <summary>
            /// 视图父类名
            /// </summary>
            public string BaseName;
            /// <summary>
            /// Asset下的Path
            /// </summary>
            public string AssetPath;

            public string ViewCode;
            public string ViewGenCode;
            public string ViewCodeFile;
            public string ViewGenCodeFile;

            public string ViewModelCode;
            public string ViewModelGenCode;
            public string ViewModelCodeFile;
            public string ViewModelGenCodeFile;

            public List<ComponentDto> Components;

            public List<TranDto> TranDtos;

            public ViewDto()
            {
                Components = new List<ComponentDto>();
                TranDtos = new List<TranDto>();
            }
        }

        private class TranDto
        {
            public string ComOriginalName { get; set; }
            public string ParentPath { get; set; }
            public Transform Tran { get; set; }
        }
        private class ComponentDto
        {
            public string ComName { get; set; }
            public string BindComName { get; set; }
            public string ComType { get; set; }
            public string BindComType { get; set; }
            public string Path { get; set; }
            public bool IsWidget { get; set; }
        }
    }
}
