using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MFramework.Core.Editor
{
    public sealed class MicroEditorConfig : ScriptableObject
    {
        private static MicroEditorConfig _instance;
        public static MicroEditorConfig Instance
        {
            get
            {
                if (_instance == null)
                    s_initConfig();
                return _instance;
            }
        }
        /// <summary>
        /// 当前选择的配置名称
        /// </summary>
        [SerializeField]
        internal string SelectConfigName = "";
        [SerializeReference]
        public List<IMicroEditorConfig> Configs = new List<IMicroEditorConfig>();

        public T GetEditorConfig<T>() where T : class, IMicroEditorConfig, new()
        {
            Type dataType = typeof(T);
            foreach (var item in Configs)
            {
                if (item.GetType() == dataType)
                    return item as T;
            }
            T data = new T();
            Configs.Add(data);
            Save();
            return data;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void s_initConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:MicroEditorConfig");
            if (guids.Length == 0)
            {
                //创建配置
                string configFile = "Assets/Editor/MicroEditorConfig.asset";
                _instance = ScriptableObject.CreateInstance<MicroEditorConfig>();
                if (!Directory.Exists(Path.Combine(Application.dataPath, "Editor")))
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "Editor"));
                AssetDatabase.CreateAsset(_instance, configFile);
                AssetDatabase.SetMainObject(_instance, configFile);
                AssetDatabase.Refresh();
            }
            else
            {
                //加载配置
                if (guids.Length > 1)
                {
                    MicroLogger.LogWarning("编辑器配置存在多个，只会加载其中一个");
                }
                string configGuid = guids[0];
                _instance = AssetDatabase.LoadAssetAtPath<MicroEditorConfig>(AssetDatabase.GUIDToAssetPath(configGuid));
                if (_instance.Configs != null)
                    _instance.Configs.RemoveAll(a => a == null);
            }
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IMicroEditorConfig>();
            foreach (var type in types)
            {
                if (type.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                if (type.IsAbstract || type.IsValueType || type.IsGenericType)
                {
                    continue;
                }
                if (_instance.Configs.FirstOrDefault(a => a.GetType() == type) != null)
                    continue;
                IMicroEditorConfig config = (IMicroEditorConfig)Activator.CreateInstance(type);
                _instance.Configs.Add(config);
            }
        }
    }
}
