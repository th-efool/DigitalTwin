// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using UnityEngine;
using UnityEngine.EventSystems;

namespace realvirtual
{
    public class UIButtonClick : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler
    {
        public bool pressed = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

    }
}