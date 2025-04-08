using System;
using System.Collections.Generic;
using System.IO;
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
                {
                    s_initConfig();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 当前选择的配置名称
        /// </summary>
        [SerializeField]
        internal string SelectConfigName = "";
        [SerializeReference]
        public List<ICustomMicroEditorConfig> Configs = new List<ICustomMicroEditorConfig>();

        public T GetEditorConfig<T>() where T : class, ICustomMicroEditorConfig, new()
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
            if (guids.Length != 0)
            {
                //加载配置
                if (guids.Length > 1)
                {
                    MicroLogger.LogWarning("编辑器配置文件存在多个，只会加载其中一个");
                }
                string configGuid = guids[0];
                _instance = AssetDatabase.LoadAssetAtPath<MicroEditorConfig>(AssetDatabase.GUIDToAssetPath(configGuid));
                _instance.SelectConfigName = string.IsNullOrWhiteSpace(_instance.SelectConfigName) ? MicroContextEditor.DEFAULT_CONFIG_NAME : _instance.SelectConfigName;
                if (_instance.Configs != null)
                {
                    _instance.Configs.RemoveAll(a => a == null);
                }
            }
            else
            {
                MicroLogger.LogError("编辑器配置文件被删除");
            }
        }
    }
}
