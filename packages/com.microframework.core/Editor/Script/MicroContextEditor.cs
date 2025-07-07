using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using MFramework.Core;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 微游戏编辑器上下文
    /// </summary>
    [InitializeOnLoad]
    public static class MicroContextEditor
    {
        /// <summary>
        /// 必须包含的包
        /// </summary>
        internal readonly static string[] RequirePackages = new string[] { "com.unity.nuget.newtonsoft-json" };
        /// <summary>
        /// 编辑器根目录
        /// </summary>
        internal const string EDITOR_ROOT_PATH = "Packages/com.microframework.core/Editor";
        /// <summary>
        /// 编辑器资源位置
        /// </summary>
        internal const string EDITOR_ASSET_PATH = EDITOR_ROOT_PATH + "/Script/__MicroEditor";

        /// <summary>
        /// 默认配置文件名称
        /// </summary>
        internal readonly static string DEFAULT_CONFIG_NAME = "默认配置";
        /// <summary>
        /// 最后一次选择的树节点
        /// </summary>
        internal const string MICRO_EDITOR_LAST_SELECT_KEY = "__MicroEditorLastSelect";

        /// <summary>
        /// 导入了微框架的宏定义
        /// </summary>
        internal const string MICRO_FRAMEWORK_DEFINE = "MICRO_CORE";

        /// <summary>
        /// 编辑器模式变化的方法字典
        /// </summary>
        internal readonly static List<Action<PlayModeStateChange>> PlayModeChangedDic = new();
        /// <summary>
        /// 编辑器更新的方法字典
        /// </summary>
        internal readonly static List<Action> EditorUpdateDic = new List<Action>();
        /// <summary>
        /// 日志打印
        /// </summary>
        internal static IMicroLogger logger;
        public static event Action onUpdate;

        //private static SerializedObject _editorSerializedObject;
        ///// <summary>
        ///// 编辑器配置序列化对象
        ///// </summary>
        //internal static SerializedObject EditorSerializedObject
        //{
        //    get
        //    {
        //        if (_editorSerializedObject == null)
        //        {
        //            _editorSerializedObject = new SerializedObject(MicroEditorConfig.Instance);
        //        }
        //        if (_editorSerializedObject.targetObject != MicroEditorConfig.Instance)
        //        {
        //            _editorSerializedObject = new SerializedObject(MicroEditorConfig.Instance);
        //        }
        //        return _editorSerializedObject;
        //    }
        //}
        //private static SerializedObject _runtimeSerializedObject;
        ///// <summary>
        ///// 运行时配置序列化对象
        ///// </summary>
        //internal static SerializedObject RuntimeSerializedObject
        //{
        //    get
        //    {
        //        if (_runtimeSerializedObject == null)
        //        {
        //            _runtimeSerializedObject = new SerializedObject(MicroRuntimeConfig.CurrentConfig);
        //        }
        //        if (_runtimeSerializedObject.targetObject != MicroRuntimeConfig.CurrentConfig)
        //        {
        //            _runtimeSerializedObject = new SerializedObject(MicroRuntimeConfig.CurrentConfig);
        //        }
        //        return _runtimeSerializedObject;
        //    }
        //}

        /// <summary>
        /// 自定义绘制器字典
        /// </summary>
        internal readonly static Dictionary<Type, ICustomDrawer> CustomDrawers = new Dictionary<Type, ICustomDrawer>();

        static MicroContextEditor()
        {
            UnityOnLoad();
        }

        private static void UnityOnLoad()
        {
            m_checkDefine();
            EditorApplication.delayCall += m_delayLoad;
            logger = MicroLogger.GetMicroLogger("MicroEditor");
            EditorApplication.update -= m_update;
            EditorApplication.update += m_update;
            EditorApplication.playModeStateChanged -= m_playModeStateChanged;
            EditorApplication.playModeStateChanged += m_playModeStateChanged;
            m_initMethodInfo();
            m_initCustomDrawer();
            m_checkPackage();
            m_initConfig();
        }

        /// <summary>
        /// 获取某个脚本的目录
        /// <para>确保脚本名和类名一致</para>
        /// </summary>
        /// <param name="scriptType">脚本类型</param>
        /// <returns></returns>
        public static string GetScriptPath(Type scriptType)
        {
            string[] guids = AssetDatabase.FindAssets(scriptType.Name + " t:Script");
            string str = string.Empty;
            if (guids.Length > 0)
            {
                foreach (var item in guids)
                {
                    string filePath = AssetDatabase.GUIDToAssetPath(item);
                    var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                    if (mono == null)
                        continue;
                    if (mono.GetClass().FullName == scriptType.FullName)
                    {
                        str = Path.GetDirectoryName(filePath);
                        break;
                    }
                }
            }
            return str;
        }
        /// <summary>
        /// 获取最后一次选择的树节点
        /// </summary>
        /// <returns></returns>
        public static string GetLastSelectTreeNode() => EditorPrefs.GetString(MICRO_EDITOR_LAST_SELECT_KEY, "");
        /// <summary>
        /// 设置最后一次选择的树节点
        /// </summary>
        /// <param name="path"></param>
        public static void SetLastSelectTreeNode(string path) => EditorPrefs.SetString(MICRO_EDITOR_LAST_SELECT_KEY, path);

        /// <summary>
        /// 获取所有运行时配置文件名称
        /// </summary>
        /// <returns></returns>
        public static List<MicroDropdownContent.ValueItem> GetRuntimeConfigNames()
        {
            List<MicroDropdownContent.ValueItem> names = new List<MicroDropdownContent.ValueItem>();
            string[] guids = AssetDatabase.FindAssets("t:MicroRuntimeConfig");
            if (guids.Length > 0)
            {
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MicroRuntimeConfig config = AssetDatabase.LoadAssetAtPath<MicroRuntimeConfig>(path);
                    if (config == null)
                        continue;
                    names.Add(new MicroDropdownContent.ValueItem { value = path, displayName = config.ConfigName });
                }
            }
            return names;
        }

        /// <summary>
        /// 加载资源文件夹下的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadRes<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(Path.Combine(EDITOR_ASSET_PATH, path));
        }

        /// <summary>
        /// 获取当前的窗口
        /// </summary>
        /// <returns></returns>
        internal static MicroEditorWindow GetMicroEditorWindow()
        {
            return EditorWindow.GetWindow<MicroEditorWindow>(true);
        }
        /// <summary>
        /// 获取所有微框架布局
        /// </summary>
        /// <returns></returns>
        internal static List<MicroLayoutModel> GetMicroLayouts()
        {
            List<MicroLayoutModel> models = new List<MicroLayoutModel>();
            TypeCache.TypeCollection list = TypeCache.GetTypesDerivedFrom<BaseMicroLayout>();
            Type defautType = typeof(DefaultMicroLayout);
            foreach (var item in list)
            {
                if (!item.IsAbstract && !item.IsInterface && item != defautType)
                {
                    var constructors = item.GetConstructors();
                    if (constructors.FirstOrDefault(a => a.GetParameters().Length == 0) != null)
                    {
                        BaseMicroLayout layout = Activator.CreateInstance(item) as BaseMicroLayout;
                        models.Add(new MicroLayoutModel(layout));
                    }
                    else
                    {
                        logger.LogError($"类型:{item.Name},没有无参构造函数");
                    }
                }
            }
            return models;
        }

        /// <summary>
        /// 打开设置
        /// </summary>
        [MenuItem("MFramework/Preferences", priority = -99)]
        private static void m_openPreferences()
        {
            GetMicroEditorWindow();
        }

        private static void m_delayLoad()
        {
            EditorApplication.delayCall -= m_delayLoad;
        }

        private static void m_playModeStateChanged(PlayModeStateChange state)
        {
            try
            {
                foreach (var item in PlayModeChangedDic)
                {
                    item.Invoke(state);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// 编辑器下更新
        /// </summary>
        private static void m_update()
        {
            try
            {
                onUpdate?.Invoke();
                foreach (var item in EditorUpdateDic)
                {
                    item.Invoke();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// 获取运行时配置
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static MicroRuntimeConfig GetRuntimeConfig(string filePath = "Assets/Resources/MicroRuntimeConfig.asset")
        {
            MicroRuntimeConfig runtimeConfig = AssetDatabase.LoadAssetAtPath<MicroRuntimeConfig>(filePath);
            if (runtimeConfig == null)
            {
                runtimeConfig = ScriptableObject.CreateInstance<MicroRuntimeConfig>();
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                AssetDatabase.CreateAsset(runtimeConfig, filePath);
                AssetDatabase.SetMainObject(runtimeConfig, filePath);
                AssetDatabase.Refresh();
            }
            else
            {
                if (runtimeConfig.Configs.RemoveAll(a => a == null) > 0)
                    runtimeConfig.Save();
            }
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IMicroRuntimeConfig>();
            foreach (var type in types)
            {
                if (type.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                if (type.IsAbstract || type.IsValueType || type.IsGenericType)
                    continue;
                if (runtimeConfig.Configs.FirstOrDefault(a => a.GetType() == type) != null)
                    continue;
                IMicroRuntimeConfig config = (IMicroRuntimeConfig)Activator.CreateInstance(type);
                runtimeConfig.Configs.Add(config);
            }
            return runtimeConfig;
        }

        /// <summary>
        /// 检测宏定义
        /// </summary>
        private static void m_checkDefine()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (defines.Contains(MICRO_FRAMEWORK_DEFINE))
                return;
            if (!string.IsNullOrEmpty(defines))
            {
                defines += $";{MICRO_FRAMEWORK_DEFINE}";
            }
            else
            {
                defines = MICRO_FRAMEWORK_DEFINE;
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }

        /// <summary>
        /// 检查Package包
        /// </summary>
        private static void m_checkPackage()
        {
            PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();
            List<string> tempList = new List<string>();
            foreach (var item in RequirePackages)
            {
                if (packages.FirstOrDefault(a => a.name == item) == null)
                {
                    tempList.Add(item);
                }
            }
            foreach (string packageName in tempList)
            {
                AddRequest add = Client.Add(packageName);
            }
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        private static void m_initConfig()
        {
            try
            {
                string selectConfigName = MicroEditorConfig.Instance.SelectConfigName;
                if (string.IsNullOrWhiteSpace(selectConfigName))
                {
                    //没有运行时配置
                    MicroRuntimeConfig.CurrentConfig = GetRuntimeConfig();
                    MicroEditorConfig.Instance.SelectConfigName = MicroRuntimeConfig.CurrentConfig.ConfigName;
                    MicroRuntimeConfig.CurrentConfig.Save();
                    MicroEditorConfig.Instance.Save();
                    AssetDatabase.Refresh();
                }
                else
                {
                    //有运行时配置
                    var runtimeNames = GetRuntimeConfigNames();
                    foreach (var item in runtimeNames)
                    {
                        if (item.displayName == selectConfigName)
                        {
                            MicroRuntimeConfig.CurrentConfig = GetRuntimeConfig(item.value);
                            break;
                        }
                    }
                    if (MicroRuntimeConfig.CurrentConfig != null)
                        return;
                    MicroRuntimeConfig.CurrentConfig = GetRuntimeConfig();
                    MicroEditorConfig.Instance.SelectConfigName = MicroRuntimeConfig.CurrentConfig.ConfigName;
                    MicroRuntimeConfig.CurrentConfig.Save();
                    MicroEditorConfig.Instance.Save();
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// 初始化自定义绘制器
        /// </summary>
        private static void m_initCustomDrawer()
        {
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<ICustomDrawer>())
            {
                if (drawerType.IsAbstract || !drawerType.IsClass || drawerType.IsGenericType)
                    continue;
                if (drawerType.GetCustomAttribute<IgnoreAttribute>() != null)
                    return;
                CustomDrawerAttribute customDrawer = drawerType.GetCustomAttribute<CustomDrawerAttribute>();
                if (customDrawer == null)
                    continue;
                Type targetType = customDrawer.TargetType;
                if (targetType == null)
                    continue;
                if (!CustomDrawers.ContainsKey(targetType))
                {
                    CustomDrawers.Add(targetType, (ICustomDrawer)Activator.CreateInstance(drawerType));
                    continue;
                }
                if (drawerType.Assembly == typeof(MicroContextEditor).Assembly)
                    continue;
                CustomDrawers.Add(targetType, (ICustomDrawer)Activator.CreateInstance(drawerType));
                logger.LogWarning($"类型:{targetType.Name} 的绘制器已经存在于类型:{CustomDrawers[targetType].GetType().Name}中，但被{drawerType.Name}覆盖。");
            }
        }
        
        /// <summary>
        /// 初始化编辑器变化和Update的方法
        /// </summary>
        private static void m_initMethodInfo()
        {
            EditorUpdateDic.Clear();
            PlayModeChangedDic.Clear();
            foreach (var item in TypeCache.GetMethodsWithAttribute<PlayModeChangedAttribute>())
            {
                if (!item.IsStatic)
                    continue;
                var @params = item.GetParameters();
                if (@params.Length != 1)
                    continue;
                if (@params[0].ParameterType != typeof(PlayModeStateChange))
                    continue;
                PlayModeChangedDic.Add((Action<PlayModeStateChange>)item.CreateDelegate(typeof(Action<PlayModeStateChange>)));
            }
            foreach (var item in TypeCache.GetMethodsWithAttribute<EditorUpdateAttribute>())
            {
                if (!item.IsStatic)
                    continue;
                var @params = item.GetParameters();
                if (@params.Length != 0)
                    continue;
                EditorUpdateDic.Add((Action)item.CreateDelegate(typeof(Action)));
            }
        }
    }
}
