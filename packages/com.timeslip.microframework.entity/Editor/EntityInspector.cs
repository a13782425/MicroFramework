using MFramework.Core;
using MFramework.Runtime;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MFramework.Editor
{

    [CustomEditor(typeof(EditorEntityMono))]
    [DisallowMultipleComponent]
    public class EntityInspector : UnityEditor.Editor
    {
        private EditorEntityMono mono;
        private static FieldInfo comsFieldinfo = typeof(Entity).GetField("_coms", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo argsFieldinfo = typeof(Entity).GetField("_args", BindingFlags.Instance | BindingFlags.NonPublic);
        private static PropertyInfo comStatesProp = typeof(EntityComponent).GetProperty("state", BindingFlags.Instance | BindingFlags.NonPublic);
        private DelayDictionary<Type, EntityComponent> _dic;
        private BindableDictionary<string, object> _args;
        private GUIStyle labelStyle;
        private void OnEnable()
        {
            mono = (EditorEntityMono)target;
            _dic = comsFieldinfo.GetValue(mono.Entity) as DelayDictionary<Type, EntityComponent>;
            _args = argsFieldinfo.GetValue(mono.Entity) as BindableDictionary<string, object>;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.richText = true;
            }
            GUILayout.BeginVertical("box");
            GUILayout.Label($"当前存在变量:{_args.Count}个");
            foreach (var item in _args)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label($"变量:{item.Key},类型:{item.Value.GetType().Name},值:{item.Value.ToString()}");
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label($"当前拥有组件：{_dic.Count}个");
            GUILayout.Space(5);
            foreach (var com in _dic)
            {
                GUILayout.BeginVertical("OL box");
                GUILayout.BeginHorizontal();
                string showGui = $"组件：{com.Key.Name}，状态：{comStatesProp.GetValue(com.Value)}";
                GUILayout.Space(10);
                GUILayout.Label(showGui);
                GUILayout.EndHorizontal();
                EntityArgsAttribute argsAttr = com.Key.GetCustomAttribute<EntityArgsAttribute>();
                if (argsAttr != null)
                {
                    foreach (var item in argsAttr.args)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        if (_args.TryGetValue(item, out object value))
                        {
                            GUILayout.Label($"变量:{item},值:{value.ToString()}");
                        }
                        else
                        {
                            GUILayout.Label($"变量:{item},<color=red>Entity不存在此变量</color>", labelStyle);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
            this.Repaint();
        }
    }
}
