using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MFramework.Core
{

    /// <summary>
    /// 框架运行时数据
    /// </summary>
    public sealed class MicroRuntimeConfig : ScriptableObject
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string ConfigName = "默认配置";

        [DisplayName("是否自动加载")]
        [NonSerialized]
        public bool AutoRegisterModule = false;
        /// <summary>
        /// 需要加载的所有模块模块
        /// </summary>
        [SerializeReference]
        public List<MicroClassSerializer> InitModules = new List<MicroClassSerializer>();
        [SerializeReference]
        public List<ICustomMicroRuntimeConfig> Configs = new List<ICustomMicroRuntimeConfig>();

        private static MicroRuntimeConfig _currentConfig;
        /// <summary>
        /// 当前运行时配置
        /// </summary>
        public static MicroRuntimeConfig CurrentConfig { get => _currentConfig; internal set => _currentConfig = value; }

        public T GetRuntimeConfig<T>() where T : class, ICustomMicroRuntimeConfig, new()
        {
            Type dataType = typeof(T);
            foreach (var item in Configs)
            {
                if (item.GetType() == dataType)
                    return item as T;
            }
            T data = new T();
            Configs.Add(data);
#if UNITY_EDITOR
            Save();
#endif
            return data;
        }
#if UNITY_EDITOR
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif

    }
}
