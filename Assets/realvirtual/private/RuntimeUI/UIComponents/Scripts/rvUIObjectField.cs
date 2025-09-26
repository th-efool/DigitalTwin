// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Reflection;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;


namespace realvirtual
{
#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class rvUIObjectField : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate bool OnObjectDrop(Object reference);

        public rvUIInputField inputField;

        public FieldInfo Field;

        public OnObjectDrop OnObjectDropped;
        public object Target;
        public Type BoundVariableType { get; set; }

        public void OnDrop(PointerEventData eventData)
        {
            var assignableObject =
                RuntimeInspectorUtils.GetAssignableObjectFromDraggedReferenceItem(eventData, BoundVariableType);

            if (assignableObject != null)
            {
                AssignObject(assignableObject);
                OnObjectDropped(assignableObject);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var assignableObject =
                RuntimeInspectorUtils.GetAssignableObjectFromDraggedReferenceItem(eventData, BoundVariableType);

            if (assignableObject != null)
                // make pink
                inputField.MakeSelected();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // make grey
            inputField.MakeUnselected();
        }

        public void SetText(string text)
        {
            inputField.ChangeValueText(text);
        }

        public void SetLabel(string text)
        {
            inputField.ChangeLabelText(text);
        }

        public void OpenObjectPickerWindow()
        {
            rvUIObjectPickerWindow.Open(AssignObject, BoundVariableType);
        }

        public void AssignObject(Object assignableObject)
        {
            Field.SetValue(Target, assignableObject);
            SetText(Field.GetValue(Target).ToString().Replace("realvirtual.", ""));
        }
    }
}
#pragma warning restore CS3009 // Base type is not CLS-compliant