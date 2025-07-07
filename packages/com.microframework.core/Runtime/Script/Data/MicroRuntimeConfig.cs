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
        /// <summary>
        /// 需要加载的所有模块模块
        /// </summary>
        [SerializeReference]
        public List<MicroClassSerializer> InitModules = new List<MicroClassSerializer>();
        [SerializeReference]
        public List<IMicroRuntimeConfig> Configs = new List<IMicroRuntimeConfig>();

        private static MicroRuntimeConfig _currentConfig;
        /// <summary>
        /// 当前运行时配置
        /// </summary>
        public static MicroRuntimeConfig CurrentConfig
        {
            get => _currentConfig; internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "运行时配置不能为空，请先创建一个运行时配置文件。");

                if (_currentConfig != value)
                {
                    _currentConfig = value;
                    foreach (var item in value.Configs)
                        item.Init();
                }
            }
        }

        public MicroRuntimeConfig()
        {
            if (Configs.RemoveAll(a => a == null) > 0)
                Save();
        }

        /// <summary>
        /// 获取运行时配置数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRuntimeConfig<T>() where T : class, IMicroRuntimeConfig, new()
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
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

    }
}
