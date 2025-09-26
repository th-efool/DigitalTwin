// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{

    using UnityEngine;
    using UnityEngine.EventSystems;


    public class PanelReplacer : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public RectTransform windowTransform;

        private Vector2 originalMousePosition;
        private Vector2 originalPanelPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalMousePosition = eventData.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform.GetComponentInParent<Canvas>().transform as RectTransform, eventData.position,
                eventData.pressEventCamera, out originalMousePosition);
            originalPanelPosition = windowTransform.anchoredPosition; // Store the initial position of the panel
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localMousePosition = eventData.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform.GetComponentInParent<Canvas>().transform as RectTransform, eventData.position,
                eventData.pressEventCamera, out localMousePosition);
            Vector2 offset = localMousePosition - originalMousePosition;
            
            Vector2 scale = windowTransform.localScale;
            //offset = new Vector2(offset.x / scale.x, offset.y / scale.y);

            windowTransform.anchoredPosition = originalPanelPosition + offset;

            
        }
    }
}

