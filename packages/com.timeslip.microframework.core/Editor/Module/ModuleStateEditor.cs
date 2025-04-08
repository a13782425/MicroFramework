using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 模块编辑器
    /// </summary>
    internal static class ModuleStateEditor
    {
        private const float CHECK_INTERVAL = 1f;
        private static float temp_time = 0;
        private static Dictionary<int, EditorModuleMono> objs = new Dictionary<int, EditorModuleMono>();

        [PlayModeChanged]
        private static void OnPlayChanged(PlayModeStateChange state)
        {
            temp_time = CHECK_INTERVAL;
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    MicroContextEditor.onUpdate += temp_onUpdate;
                    EditorApplication.hierarchyWindowItemOnGUI += m_hierarchyWindowItemOnGUI;
                    break;
                default:
                    MicroContextEditor.onUpdate -= temp_onUpdate;
                    EditorApplication.hierarchyWindowItemOnGUI -= m_hierarchyWindowItemOnGUI;
                    break;
            }
        }
        private static void temp_onUpdate()
        {
            if (temp_time > 0)
            {
                temp_time -= Time.deltaTime;
                return;
            }
            if (object.ReferenceEquals(MicroContext.gameObject, null))
                return;
            temp_time = CHECK_INTERVAL;

            objs.Clear();
            foreach (var item in MicroContext.transform.GetComponentsInChildren<EditorModuleMono>())
            {
                objs.Add(item.gameObject.GetInstanceID(), item);
            }
        }
        private static void m_hierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            try
            {
                if (objs.TryGetValue(instanceID, out var mono))
                {
                    Rect rect = new Rect();
                    rect.x = selectionRect.x + selectionRect.width;
                    rect.y = selectionRect.y;
                    rect.height = selectionRect.height;
                    rect.width = 32;
                    string icon = "sv_icon_dot0_pix16_gizmo";
                    switch (mono.module.GetState())
                    {
                        case ModuleState.None:
                        case ModuleState.Destory:
                            icon = "sv_icon_dot0_pix16_gizmo";
                            break;
                        case ModuleState.Initializing:
                        case ModuleState.Suspended:
                            icon = "sv_icon_dot5_pix16_gizmo";
                            break;
                        case ModuleState.Running:
                            icon = "sv_icon_dot3_pix16_gizmo";
                            break;
                    }
                    EditorGUI.LabelField(rect, EditorGUIUtility.IconContent(icon));
                }

            }
            catch (Exception ex)
            {
                MicroContextEditor.logger.LogError(ex.Message);
            }
        }
    }

}
