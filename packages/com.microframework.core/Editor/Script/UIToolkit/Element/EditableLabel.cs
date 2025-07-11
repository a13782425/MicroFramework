﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary> A label that transforms into a field for editing its text. <c>UXML support</c> </summary>
    /// <remarks>
    /// By default, it becomes editable with a double click. Use <see cref="BeginEditing"/> to enter edit mode from code.
    /// Set <see cref="emptyTextLabel"/> to show a placeholder text when the label is empty.
    /// </remarks>
    /// <example>
    /// Here's an example for customizing an editable label.
    /// <code language="csharp"><![CDATA[
    /// public class MyCustomEditor : Editor
    /// {
    ///     public override VisualElement CreateInspectorGUI()
    ///     {
    ///         var root = new VisualElement();
    ///         var editableLabel = new EditableLabel();
    ///         root.Add(editableLabel);
    /// 
    ///         // We can disable editing on double click this way:
    ///         editableLabel.editOnDoubleClick = false;
    /// 
    ///         // EditableLabels can enter edit mode from code. 
    ///         // For example, we can add a listener to edit it on Alt+Click:
    ///         editableLabel.RegisterCallback<MouseDownEvent>(e =>
    ///         {
    ///             if (e.altKey && e.button == 0)
    ///                 editableLabel.BeginEditing();
    ///         });
    /// 
    ///         // We can also make it editable from a context menu action:
    ///         var menuManipulator = new ContextualMenuManipulator(e =>
    ///         {
    ///             e.menu.AppendAction("Edit label's text", a => editableLabel.BeginEditing());
    ///         });
    ///         root.AddManipulator(menuManipulator);
    /// 
    ///         // We can add a placeholder text for when the label is empty:
    ///         editableLabel.emptyTextLabel = "Alt+Click to edit this label";
    ///         // Or we can set the label's actual text like this:
    ///         editableLabel.value = "Initial text";
    /// 
    ///         // EditableLabels can be bound to any string property, just like any
    ///         // TextField. We can do it by setting it's bindingPath:
    ///         editableLabel.bindingPath = "m_Name";
    /// 
    ///         return root;
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public class EditableLabel : BindableElement, INotifyValueChanged<string>
    {
#if !REMOVE_UXML_FACTORIES
        public new class UxmlFactory : UxmlFactory<EditableLabel, UxmlTraits> { }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value" };
            UxmlStringAttributeDescription m_EmptyTextLabel = new UxmlStringAttributeDescription { name = "empty-text-label" };
            UxmlBoolAttributeDescription m_IsDelayed = new UxmlBoolAttributeDescription
            {
                name = "is-delayed",
                obsoleteNames = new[] { "delayed" },
                defaultValue = true
            };
            UxmlBoolAttributeDescription m_Multiline = new UxmlBoolAttributeDescription { name = "multiline" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var label = ve as EditableLabel;
                label.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
                label.emptyTextLabel = m_EmptyTextLabel.GetValueFromBag(bag, cc);
                label.isDelayed = m_IsDelayed.GetValueFromBag(bag, cc);
                label.multiline = m_Multiline.GetValueFromBag(bag, cc);
            }
        }
#endif
        public static readonly string ussStyleFile = "UIToolkit/Element/EditableLabelStyle.uss";
        /// <summary> USS class name of elements of this type. </summary>
        public static readonly string ussClassName = "editor-editable-label";
        /// <summary> USS class name of the TextField inside this element. </summary>
        public static readonly string textFieldUssClassName = ussClassName + "__text-field";
        /// <summary> USS class name of the label used to show non-editable text. </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        private string m_Value;
        private string m_EmptyTextLabel;
        private readonly TextField m_TextField;
        private readonly Label m_Label;

        /// <summary> Whether to start editing by double clicking the label. See <see cref="BeginEditing"/> to start editing from code. </summary>
        public bool editOnDoubleClick { get; set; } = true;

        /// <summary> Whether to use multiline text. </summary>
        public bool multiline
        {
            get => m_TextField.multiline;
            set
            {
                m_TextField.multiline = value;
                m_TextField.style.whiteSpace = value ? WhiteSpace.Normal : WhiteSpace.NoWrap;
                m_Label.style.whiteSpace = value ? WhiteSpace.Normal : WhiteSpace.NoWrap;
            }
        }

        /// <summary> Whether the TextField inside this element is delayed. It's true by default. </summary>
        public bool isDelayed { get => m_TextField.isDelayed; set => m_TextField.isDelayed = value; }

        /// <summary> The maximum character length of this element's TextField. -1 means no limit and it's the default.  </summary>
        public int maxLength { get => m_TextField.maxLength; set => m_TextField.maxLength = value; }

        /// <summary> The string value of this element.</summary>
        public string value
        {
            get => m_Value;
            set
            {
                if (!EqualityComparer<string>.Default.Equals(m_Value, value))
                {
                    if (panel != null)
                    {
                        using (ChangeEvent<string> e = ChangeEvent<string>.GetPooled(m_Value, value))
                        {
                            e.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(e);
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }

        /// <summary> A text that appears when the EditableLabel's text is empty. </summary>
        public string emptyTextLabel
        {
            get => m_EmptyTextLabel;
            set
            {
                m_EmptyTextLabel = value;

                if (string.IsNullOrEmpty(m_Value))
                    (m_Label as INotifyValueChanged<string>).SetValueWithoutNotify(m_EmptyTextLabel);
            }
        }

        public EditableLabel()
        {
            this.AddStyleSheet(ussStyleFile);
            AddToClassList(ussClassName);
           

            m_TextField = new TextField { isDelayed = true, style = { display = DisplayStyle.None } };
            m_TextField.AddToClassList(textFieldUssClassName);
            m_TextField.RegisterValueChangedCallback(e =>
            {
                e.StopImmediatePropagation();
                value = e.newValue;
            });
            m_TextField.RegisterCallback<BlurEvent>(e => StopEditing());
            Add(m_TextField);

            m_Label = new Label { pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            Add(m_Label);
        }

#if UNITY_2022_2_OR_NEWER
        [EventInterest(typeof(MouseDownEvent))]
#endif
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (editOnDoubleClick
                && evt is MouseDownEvent mouseDown
                && mouseDown.button == 0
                && mouseDown.modifiers == EventModifiers.None
                && mouseDown.clickCount >= 2)
            {
                BeginEditing();
            }
        }

        /// <summary> Call this method to put the label in edit mode. </summary>
        public void BeginEditing()
        {
            m_Label.style.display = DisplayStyle.None;
            m_TextField.style.display = DisplayStyle.Flex;
            m_TextField.Focus();

            // Delay it to avoid unpredictable behavior from clicking inside a click event.
            EditorApplication.delayCall += SimulateClick;

            // In 2021 and newer, the first Click on a focused field selects the text, even if it was already selected.
            // 2022 adds a way to stop it with selectAllOnMouseUp, but we want to use the same code in all versions to
            // foster consistent behavior. So we solve this by simulating the first click on the field.
            void SimulateClick()
            {
                // UITK started using a different element to handle text in 2022.
#if UNITY_2022_1_OR_NEWER
                var textHandler = m_TextField.Q(null, TextElement.ussClassName);
#else
                var textHandler = m_TextField.Q(TextField.textInputUssName);
#endif
                var position = textHandler.worldBound.center;

                var systemEvent = new Event { type = EventType.MouseDown, mousePosition = position, button = 0 };
                using (var pointerDown = PointerDownEvent.GetPooled(systemEvent))
                {
                    pointerDown.target = textHandler;
                    textHandler.SendEvent(pointerDown);
                }

                systemEvent.type = EventType.MouseUp;
                using (var pointerUp = PointerUpEvent.GetPooled(systemEvent))
                {
                    pointerUp.target = textHandler;
                    textHandler.SendEvent(pointerUp);
                }

                // Select all text for Unity versions that don't select all in the first click (i.e. 2020).
                m_TextField.SelectAll();
            }
        }

        private void StopEditing()
        {
            m_Label.style.display = DisplayStyle.Flex;
            m_TextField.style.display = DisplayStyle.None;
        }

        /// <summary> Set the element's value without triggering a change event.</summary>
        /// <param name="newValue">The new value.</param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue;

            m_TextField.SetValueWithoutNotify(m_Value);
            (m_Label as INotifyValueChanged<string>).SetValueWithoutNotify(string.IsNullOrEmpty(m_Value) ? emptyTextLabel : m_Value);
        }
    }
}
