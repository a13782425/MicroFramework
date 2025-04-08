using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MFramework.Core.Editor
{
    internal static class MicroTypeCache
    {
        static Dictionary<Type, List<FieldInfo>> _typeToFields = new Dictionary<Type, List<FieldInfo>>();
        /// <summary>
        /// 获取序列化字段
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static List<FieldInfo> GetSerializedFields(object obj) => GetSerializedFields(obj.GetType());
        /// <summary>
        /// 获取序列化字段
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static List<FieldInfo> GetSerializedFields(Type type)
        {
            if (_typeToFields.ContainsKey(type))
                return _typeToFields[type];
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<FieldInfo> result = new List<FieldInfo>();
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
                    continue;
                if (field.GetCustomAttribute<HideInInspector>() != null)
                    continue;
                if (field.GetCustomAttribute<SerializeField>() != null || field.IsPublic || field.GetCustomAttribute<SerializeReference>() != null)
                {
                    if (field.FieldType.IsAssignableFrom(typeof(IDictionary)))
                        if (!field.FieldType.IsSubclassOf(typeof(SerializableDictionaryBase)))
                            continue;
                    result.Add(field);
                }
            }
            return result;
        }
    }
}
