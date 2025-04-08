using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MFramework.Editor
{
    internal static class UIModuleEditor
    {
        private const string ASSETS_ROOT = "Assets";
        private const string RESOURCE_ROOT = "Resource";
        private static string _assetPath = Application.dataPath + "/";
        private static UIEditorConfig _config = default;
        //[MenuItem("MFramework/界面/生成界面", false, 0)]
        internal static void GenerateView()
        {
            try
            {
                _config = MicroEditorConfig.Instance.GetEditorConfig<UIEditorConfig>();
                s_checkPath();
                List<ViewDto> viewList = new List<ViewDto>();
                s_loadPrefab(ref viewList);
                StringBuilder uiCodeGen = new StringBuilder();
                //m_initTitle(uiCodeGen);
                foreach (var item in viewList)
                {
                    uiCodeGen.Clear();
                    s_genComponent(item);
                    if (!File.Exists(item.ViewCodeFile) && !string.IsNullOrWhiteSpace(item.ViewCode))
                    {
                        File.WriteAllText(item.ViewCodeFile, item.ViewCode, new UTF8Encoding());
                    }
                    if (!File.Exists(item.ViewModelCodeFile) && !string.IsNullOrWhiteSpace(item.ViewModelCode))
                    {
                        File.WriteAllText(item.ViewModelCodeFile, item.ViewModelCode, new UTF8Encoding());
                    }
                    m_initTitle(uiCodeGen);
                    if (!string.IsNullOrWhiteSpace(_config.Namespace))
                    {
                        uiCodeGen.AppendLine($"{s_getTab(0)}namespace {_config.Namespace}");
                        uiCodeGen.AppendLine($"{s_getTab(0)}{{");
                    }
                    uiCodeGen.Append(item.ViewGenCode);
                    uiCodeGen.AppendLine();
                    uiCodeGen.Append(item.ViewModelGenCode);
                    if (!string.IsNullOrWhiteSpace(_config.Namespace))
                    {
                        uiCodeGen.AppendLine($"{s_getTab(0)}}}");
                    }
                    File.WriteAllText(Path.Combine(_config.CodeGenFileRoot, item.IsPanel ? "Panel" : "Widget", $"{item.ViewName}.g.cs"), uiCodeGen.ToString(), new UTF8Encoding());
                }
                //File.WriteAllText(_config.CodeGenFile, uiCodeGen.ToString(), new UTF8Encoding());
                EditorUtility.DisplayDialog("Successful", "生成成功succeed", "确定");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
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
            //sb.AppendLine("using MFramework.Runtime;");
            //sb.AppendLine("using TMPro;");
            //sb.AppendLine("using UnityEngine;");
            //sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("#pragma warning disable IDE0049");
            sb.AppendLine("#pragma warning disable IDE0001");
            sb.AppendLine();
        }
        private static void s_genComponent(ViewDto item)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(item.AssetPath);
            List<TranDto> trans = new List<TranDto>();
            s_getTrans(obj.transform, "", trans);
            s_getComponent(trans, item);
            s_createComponent(trans, item);
            s_generateViewModel(item);
            obj = null;
        }
        private static void s_generateViewModel(ViewDto viewDto)
        {
            StringBuilder genString = new StringBuilder();

            genString.AppendLine($"{s_getTab(1)}partial class {viewDto.ViewModelName} : MFramework.Runtime.IViewModel");
            genString.AppendLine($"{s_getTab(1)}{{");
            genString.AppendLine($"{s_getTab(2)}private {viewDto.ViewName} view;");

            genString.AppendLine($"{s_getTab(2)}public {viewDto.ViewModelName}({viewDto.ViewName} view)");
            genString.AppendLine($"{s_getTab(2)}{{");
            genString.AppendLine($"{s_getTab(3)}this.view = view;");
            genString.AppendLine($"{s_getTab(2)}}}");
            genString.AppendLine($"{s_getTab(1)}}}");

            viewDto.ViewModelGenCode = genString.ToString();
        }
        private static void s_createComponent(List<TranDto> trans, ViewDto viewDto)
        {
            //trans.Reverse();
            StringBuilder genString = new StringBuilder();
            string tab = "    ";
            string firstTab = string.IsNullOrWhiteSpace(_config.Namespace) ? "" : tab;
            //todo 路径需要用IResourcePathProcessor处理
            genString.AppendLine($"{s_getTab(0)}[MFramework.Runtime.UIConfigAttribute(\"{viewDto.ResourcePath}\")]");
            genString.AppendLine($"{s_getTab(0)}[MFramework.Core.IgnoreAttribute]");
            genString.AppendLine($"{s_getTab(0)}partial class {viewDto.ViewName} : {viewDto.BaseName}");
            genString.AppendLine($"{s_getTab(0)}{{");
            viewDto.Components.Reverse();
            genString.AppendLine($"{s_getTab(2)}private {viewDto.ViewModelName} viewModel;");
            foreach (var item in viewDto.Components)
            {
                if (UIEditorConfig.MVVMTypeDic.TryGetValue(item.ComType, out var mvvmType))
                {
                    genString.AppendLine($"{s_getTab(1)}private {item.ComType} raw_{item.ComName} = null;");
                    genString.AppendLine($"{s_getTab(1)}private {mvvmType} {item.ComName} = null;");
                }
                else
                {
                    genString.AppendLine($"{s_getTab(1)}private {item.ComType} {item.ComName};");
                }
            }
            genString.AppendLine($"{s_getTab(2)}protected override UniversalScript.Runtime.IViewModel GetViewModel() => viewModel;");
            genString.AppendLine($"{s_getTab(1)}protected override void InitializeElement()");
            genString.AppendLine($"{s_getTab(1)}{{");
            genString.AppendLine($"{s_getTab(3)}viewModel = new {viewDto.ViewModelName}(this);");
            foreach (var item in viewDto.Components)
            {
                string mvvmType = "";
                UIEditorConfig.MVVMTypeDic.TryGetValue(item.ComType, out mvvmType);
                if (item.ComName.StartsWith(UIEditorConfig.WIDGET_HEAD))
                {
                    genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = this.AddWidget<{item.ComType}>(this.transform.Find(\"{item.Path}\").gameObject);");
                }
                else
                {
                    switch (item.ComType)
                    {
                        case string str when str == typeof(GameObject).Name:
                            if (string.IsNullOrWhiteSpace(mvvmType))
                            {
                                genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = this.transform.Find(\"{item.Path}\").gameObject;");
                            }
                            else
                            {
                                genString.AppendLine($"{s_getTab(2)}this.raw_{item.ComName} = this.transform.Find(\"{item.Path}\").gameObject;");
                                genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = new {mvvmType}(this, raw_{item.ComName});");
                            }
                            break;
                        case string str when str == typeof(Transform).Name:
                            if (string.IsNullOrWhiteSpace(mvvmType))
                            {
                                genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = this.transform.Find(\"{item.Path}\");");
                            }
                            else
                            {
                                genString.AppendLine($"{s_getTab(2)}this.raw_{item.ComName} = this.transform.Find(\"{item.Path}\");");
                                genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = new {mvvmType}(this, raw_{item.ComName});");
                            }
                            break;
                        default:
                            if (string.IsNullOrWhiteSpace(mvvmType))
                            {
                                genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = this.transform.Find(\"{item.Path}\").GetComponent<{item.ComType}>();");
                            }
                            else
                            {
                                genString.AppendLine($"{s_getTab(2)}this.raw_{item.ComName} = this.transform.Find(\"{item.Path}\").GetComponent<{item.ComType}>();");
                                genString.AppendLine($"{s_getTab(2)}this.{item.ComName} = new {mvvmType}(this, raw_{item.ComName});");
                            }
                            break;
                    }
                }
            }
            genString.AppendLine($"{s_getTab(1)}}}");
            genString.AppendLine($"{s_getTab(0)}}}");
            viewDto.ViewGenCode = genString.ToString();
        }

        private static string s_getTab(int lvl, bool checkNamespace = true)
        {
            if (lvl <= 0)
                return "";
            if (string.IsNullOrWhiteSpace(_config.Namespace) && checkNamespace)
                return s_getTab(lvl - 1, false);
            string tab = "    ";
            string firstTab = string.IsNullOrWhiteSpace(_config.Namespace) ? "" : tab;
            string res = "";
            while (lvl > 0)
            {
                res += tab;
                lvl--;
            }
            return res;
        }
        private static void s_getComponent(List<TranDto> trans, ViewDto viewDto)
        {
            foreach (var item in trans)
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

                        viewDto.Components.Add(new ComponentDto() { ComName = tranName, ComType = typeof(GameObject).Name, Path = item.ParentPath + tranName, IsWidget = true });
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
                        comName = UIEditorConfig.UIExportDic.FirstOrDefault(a => a.Value == typeName).Key + comName;
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
                if (UIEditorConfig.UIExportDic.ContainsKey(exportName))
                {
                    typeNames[i] = UIEditorConfig.UIExportDic[exportName];
                }
                else
                {
                    typeNames[i] = typeof(Transform).Name;
                }
            }
            return typeNames;
        }

        private static void s_getTrans(Transform transform, string parentPath, List<TranDto> trans)
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

                        s_getTrans(tran, path, trans);
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
                trans.Add(tranDto);
            }
        }

        private static void s_loadPrefab(ref List<ViewDto> viewList)
        {
            //true代表Panel,false代表Widget
            List<bool> rootPathList = new List<bool>() { true, false };
            foreach (var item in rootPathList)
            {
                string rootPath = (item ? _config.PanelPrefabRoot : _config.WidgetPrefabRoot);
                string scriptPath = (item ? _config.PanelCodeRoot : _config.WidgetCodeRoot);
                string baseName = item ? "UIPanel" : "UIWidget";
                string[] prefabFiles = Directory.GetFiles(rootPath, "*.prefab", SearchOption.AllDirectories);
                string[] scriptFiles = Directory.GetFiles(scriptPath, "*.cs", SearchOption.AllDirectories);
                foreach (var prefabFile in prefabFiles)
                {
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
                        viewDto.ViewCodeFile = $"{scriptPath}\\View\\{viewDto.ViewName}.cs";
                        s_createViewCode(viewDto);
                    }
                    if (string.IsNullOrWhiteSpace(viewDto.ViewModelCodeFile))
                    {
                        viewDto.ViewModelCodeFile = $"{scriptPath}\\ViewModel\\{viewDto.ViewModelName}.cs";
                        s_createViewModelCode(viewDto);
                    }
                    int num = prefabFile.LastIndexOf(ASSETS_ROOT);
                    string path = prefabFile.Substring(num, prefabFile.Length - num);
                    viewDto.AssetPath = path;

                    path = Path.GetDirectoryName(prefabFile);
                    num = path.LastIndexOf(RESOURCE_ROOT) + RESOURCE_ROOT.Length + 2;
                    path = path.Substring(num, path.Length - num).Replace("\\", "/");
                    viewDto.ResourcePath = path + "/" + viewDto.Name;
                    viewList.Add(viewDto);
                }
            }
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
            if (!string.IsNullOrWhiteSpace(_config.Namespace))
            {
                sb.AppendLine($"{s_getTab(0)}namespace {_config.Namespace}");
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
            if (!string.IsNullOrWhiteSpace(_config.Namespace))
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
            if (!string.IsNullOrWhiteSpace(_config.Namespace))
            {
                sb.AppendLine($"{s_getTab(0)}namespace {_config.Namespace}");
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
            if (!string.IsNullOrWhiteSpace(_config.Namespace))
            {
                sb.AppendLine($"{s_getTab(0)}}}");
            }
            viewDto.ViewModelCode = sb.ToString();
        }


        private static void s_checkPath()
        {
            string panelViewPath = _config.PanelPrefabRoot;
            string panelScriptPath = Path.Combine(_config.PanelCodeRoot, "View");
            string panelVMScriptPath = Path.Combine(_config.PanelCodeRoot, "ViewModel");
            string widgetViewPath = _config.WidgetPrefabRoot;
            string widgetScriptPath = Path.Combine(_config.WidgetCodeRoot, "View");
            string widgetVMScriptPath = Path.Combine(_config.WidgetCodeRoot, "ViewModel");
            string panelGenScriptPath = Path.Combine(_config.CodeGenFileRoot, "Panel");
            string widgetGenScriptPath = Path.Combine(_config.CodeGenFileRoot, "Widget");

            if (!Directory.Exists(panelViewPath))
            {
                Directory.CreateDirectory(panelViewPath);
            }
            if (!Directory.Exists(widgetViewPath))
            {
                Directory.CreateDirectory(widgetViewPath);
            }
            if (!Directory.Exists(panelGenScriptPath))
            {
                Directory.CreateDirectory(panelGenScriptPath);
            }
            if (!Directory.Exists(widgetGenScriptPath))
            {
                Directory.CreateDirectory(widgetGenScriptPath);
            }
            if (!Directory.Exists(panelScriptPath))
            {
                Directory.CreateDirectory(panelScriptPath);
            }
            if (!Directory.Exists(panelVMScriptPath))
            {
                Directory.CreateDirectory(panelVMScriptPath);
            }
            if (!Directory.Exists(widgetScriptPath))
            {
                Directory.CreateDirectory(widgetScriptPath);
            }
            if (!Directory.Exists(widgetVMScriptPath))
            {
                Directory.CreateDirectory(widgetVMScriptPath);
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
            /// <summary>
            /// Resource下的路径
            /// </summary>
            public string ResourcePath;

            public string ViewCode;
            public string ViewGenCode;
            public string ViewCodeFile;
            public string ViewGenCodeFile;

            public string ViewModelCode;
            public string ViewModelGenCode;
            public string ViewModelCodeFile;
            public string ViewModelGenCodeFile;

            public List<ComponentDto> Components;

            public ViewDto()
            {
                Components = new List<ComponentDto>();
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
