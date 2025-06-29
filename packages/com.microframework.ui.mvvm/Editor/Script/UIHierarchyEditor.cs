using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MFramework.Editor
{
    internal static class UIHierarchyEditor
    {
        [InitializeOnLoadMethod]
        private static void UnityOnLoad()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= m_hierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += m_hierarchyWindowItemOnGUI;
        }
        private static Dictionary<string, Type> s_typeCache = new Dictionary<string, Type>();
        public static Texture2D GetScriptIcon(MonoScript script)
        {
            Type editorGUIUtilityType = typeof(EditorGUIUtility);
            MethodInfo getIconForObjectMethod = editorGUIUtilityType.GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            if (getIconForObjectMethod != null)
            {
                Texture2D icon = (Texture2D)getIconForObjectMethod.Invoke(null, new object[] { script });
                return icon;
            }
            return null;
        }
        private static void m_hierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
                return;
            RectTransform rectTran = go.GetComponent<RectTransform>();
            if (rectTran == null)
                return;
            if (go.tag != UIEditorConfig.TAG_NAME)
                return;
            string[] names = s_getExportType(rectTran.name);
            if (names == null)
                return;
            Rect rect = new Rect();
            rect.x = selectionRect.x + selectionRect.width - 4;
            rect.y = selectionRect.y;
            rect.height = selectionRect.height;
            rect.width = 32;
            Texture2D iconTexture = null;
            foreach (var comName in names)
            {
                Type comType = GetTypeFromString(comName);
                if (comType != null)
                {
                    iconTexture = AssetPreview.GetMiniTypeThumbnail(comType);
                    if (iconTexture == null)
                    {
                        if (go.GetComponent(comType) != null)
                            iconTexture = EditorGUIUtility.GetIconForObject(go.GetComponent(comType));
                    }
                    if (iconTexture != null)
                    {
                        GUIContent content = new GUIContent(iconTexture);
                        EditorGUI.LabelField(rect, content);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("CollabConflict Icon")); //0-15
                    }
                }
                rect = new Rect(rect.x - 18, rect.y, rect.width, rect.height);
            }
        }

        private static string[] s_getExportType(string name)
        {
            int index = name.IndexOf("_");
            string str = name;
            if (index > 0)
            {
                str = name.Substring(0, index);
            }
            else
            {
                return null;
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
                    typeNames[i] = typeof(Transform).FullName;
                }
            }
            return typeNames;
        }
        public static Type GetTypeFromString(string typeName)
        {
            if (s_typeCache.ContainsKey(typeName))
                return s_typeCache[typeName];
            // 尝试直接获取类型
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                s_typeCache.Add(typeName, type);
                return type;
            }

            // 如果直接获取失败，可以选择遍历已加载的程序集中查找类型
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null)
                {
                    s_typeCache.Add(typeName, type);
                    return type;
                }
            }

            // 如果仍然找不到，返回null或抛出异常
            return null;
        }
    }
}
