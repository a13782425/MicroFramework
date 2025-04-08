using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace MFramework.Core.Editor
{
    public class VisualSplitter : ImmediateModeElement
    {
        public new class UxmlFactory : UxmlFactory<VisualSplitter, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        private class SplitManipulator : MouseManipulator
        {
            private int m_ActiveVisualElementIndex = -1;

            private int m_NextVisualElementIndex = -1;

            private List<VisualElement> m_AffectedElements;

            private bool m_Active;

            public SplitManipulator()
            {
                base.activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse
                });
            }

            protected override void RegisterCallbacksOnTarget()
            {
                base.target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
                base.target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                base.target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
                base.target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            }

            protected void OnMouseDown(MouseDownEvent e)
            {
                if (!CanStartManipulation(e))
                {
                    return;
                }
                VisualSplitter visualSplitter = base.target as VisualSplitter;
                FlexDirection flexDirection = visualSplitter.resolvedStyle.flexDirection;
                if (m_AffectedElements != null)
                {
                    VisualElementListPool.Release(m_AffectedElements);
                }
                m_AffectedElements = visualSplitter.GetAffectedVisualElements();
                for (int i = 0; i < m_AffectedElements.Count - 1; i++)
                {
                    VisualElement visualElement = m_AffectedElements[i];
                    if (visualSplitter.GetSplitterRect(visualElement).Contains(e.localMousePosition))
                    {
                        if (flexDirection == FlexDirection.RowReverse || flexDirection == FlexDirection.ColumnReverse)
                        {
                            m_ActiveVisualElementIndex = i + 1;
                            m_NextVisualElementIndex = i;
                        }
                        else
                        {
                            m_ActiveVisualElementIndex = i;
                            m_NextVisualElementIndex = i + 1;
                        }
                        m_Active = true;
                        base.target.CaptureMouse();
                        e.StopPropagation();
                    }
                }
            }

            protected void OnMouseMove(MouseMoveEvent e)
            {
                if (m_Active)
                {
                    VisualSplitter visualSplitter = base.target as VisualSplitter;
                    VisualElement visualElement = m_AffectedElements[m_ActiveVisualElementIndex];
                    VisualElement visualElement2 = m_AffectedElements[m_NextVisualElementIndex];
                    FlexDirection flexDirection = visualSplitter.resolvedStyle.flexDirection;
                    float val;
                    if (flexDirection == FlexDirection.Column || flexDirection == FlexDirection.ColumnReverse)
                    {
                        float num = ((visualElement.resolvedStyle.minHeight == StyleKeyword.Auto) ? 0f : visualElement.resolvedStyle.minHeight.value);
                        float num2 = ((visualElement2.resolvedStyle.minHeight == StyleKeyword.Auto) ? 0f : visualElement2.resolvedStyle.minHeight.value);
                        float num3 = visualElement.layout.height + visualElement2.layout.height - num - num2;
                        float num4 = ((visualElement.resolvedStyle.maxHeight.value <= 0f) ? num3 : visualElement.resolvedStyle.maxHeight.value);
                        val = (Math.Min(e.localMousePosition.y, visualElement.layout.yMin + num4) - visualElement.layout.yMin - num) / num3;
                    }
                    else
                    {
                        float num5 = ((visualElement.resolvedStyle.minWidth == StyleKeyword.Auto) ? 0f : visualElement.resolvedStyle.minWidth.value);
                        float num6 = ((visualElement2.resolvedStyle.minWidth == StyleKeyword.Auto) ? 0f : visualElement2.resolvedStyle.minWidth.value);
                        float num7 = visualElement.layout.width + visualElement2.layout.width - num5 - num6;
                        float num8 = ((visualElement.resolvedStyle.maxWidth.value <= 0f) ? num7 : visualElement.resolvedStyle.maxWidth.value);
                        val = (Math.Min(e.localMousePosition.x, visualElement.layout.xMin + num8) - visualElement.layout.xMin - num5) / num7;
                    }
                    val = Math.Max(0f, Math.Min(0.999f, val));
                    float num9 = visualElement.resolvedStyle.flexGrow + visualElement2.resolvedStyle.flexGrow;
                    visualElement.style.flexGrow = val * num9;
                    visualElement2.style.flexGrow = (1f - val) * num9;
                    e.StopPropagation();
                }
            }

            protected void OnMouseUp(MouseUpEvent e)
            {
                if (m_Active && CanStopManipulation(e))
                {
                    m_Active = false;
                    base.target.ReleaseMouse();
                    e.StopPropagation();
                    m_ActiveVisualElementIndex = -1;
                    m_NextVisualElementIndex = -1;
                }
            }
        }

        private const int kDefaultSplitSize = 10;

        /// <summary>
        /// 拖拽范围
        /// </summary>
        public int splitSize = kDefaultSplitSize;

        public static readonly string ussClassName = "visual-splitter";

        /// <summary>
        /// 设置分割界面的方向
        /// </summary>
        public FlexDirection direction { get => this.style.flexDirection.value; set => this.style.flexDirection = value; }
        public VisualSplitter()
        {
            this.AddStyleSheet("UIToolkit\\Uss\\VisualSplitter");
            AddToClassList(ussClassName);
            this.AddManipulator(new SplitManipulator());
        }

        private List<VisualElement> GetAffectedVisualElements()
        {
            List<VisualElement> list = VisualElementListPool.Get();
            int num = base.hierarchy.childCount;
            for (int i = 0; i < num; i++)
            {
                VisualElement visualElement = base.hierarchy[i];
                if (visualElement.resolvedStyle.position == Position.Relative)
                {
                    list.Add(visualElement);
                }
            }
            return list;
        }
        protected override void ImmediateRepaint()
        {
            UpdateCursorRects();
        }

        private void UpdateCursorRects()
        {
            int num = base.hierarchy.childCount;
            for (int i = 0; i < num - 1; i++)
            {
                VisualElement visualElement = base.hierarchy[i];
                bool flag = base.resolvedStyle.flexDirection == FlexDirection.Column || base.resolvedStyle.flexDirection == FlexDirection.ColumnReverse;
                EditorGUIUtility.AddCursorRect(GetSplitterRect(visualElement), flag ? MouseCursor.ResizeVertical : MouseCursor.SplitResizeLeftRight);
            }
        }

        private Rect GetSplitterRect(VisualElement visualElement)
        {
            Rect result = visualElement.layout;
            if (base.resolvedStyle.flexDirection == FlexDirection.Row)
            {
                result.xMin = visualElement.layout.xMax - (float)splitSize * 0.5f;
                result.xMax = visualElement.layout.xMax + (float)splitSize * 0.5f;
            }
            else if (base.resolvedStyle.flexDirection == FlexDirection.RowReverse)
            {
                result.xMin = visualElement.layout.xMin - (float)splitSize * 0.5f;
                result.xMax = visualElement.layout.xMin + (float)splitSize * 0.5f;
            }
            else if (base.resolvedStyle.flexDirection == FlexDirection.Column)
            {
                result.yMin = visualElement.layout.yMax - (float)splitSize * 0.5f;
                result.yMax = visualElement.layout.yMax + (float)splitSize * 0.5f;
            }
            else if (base.resolvedStyle.flexDirection == FlexDirection.ColumnReverse)
            {
                result.yMin = visualElement.layout.yMin - (float)splitSize * 0.5f;
                result.yMax = visualElement.layout.yMin + (float)splitSize * 0.5f;
            }
            return result;
        }
    }
}
