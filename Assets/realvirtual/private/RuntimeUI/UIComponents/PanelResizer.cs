// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
using UnityEngine.EventSystems;

namespace realvirtual
{
#pragma warning disable CS3009, CS3003, CS3001
    //! Allows resizing of UI panels by dragging on edges or corners
    public class PanelResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
        IPointerExitHandler
    {
        [Tooltip("Which edges to resize from. For example:\n" +
                 "(1,0) = Right edge only,\n" +
                 "(-1,0) = Left edge only,\n" +
                 "(0,1) = Top edge only,\n" +
                 "(0,-1) = Bottom edge only.\n" +
                 "Or combinations for corners.")]
        public Vector2 modifier = Vector2.one;

        [Tooltip("The panel RectTransform to resize")]
        public RectTransform windowTransform;        // The panel to resize
        
        [Tooltip("Optional cursor texture to show when resizing")]
        public Texture2D resizeCursor;               // Optional: cursor image when resizing

        private Vector2 originalSize;
        private Vector2 originalPosition;
        private Vector2 originalMousePos;
        private bool isDragging;

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            if (resizeCursor)
            {
                // Show a custom cursor while dragging
                Cursor.SetCursor(resizeCursor, new Vector2(resizeCursor.width * 0.5f, resizeCursor.height * 0.5f), CursorMode.Auto);
            }

            // Record current size & position
            originalSize = windowTransform.sizeDelta;
            originalPosition = windowTransform.anchoredPosition;

            // Convert mouse position to the PARENT's local space
            RectTransform parentRect = windowTransform.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                windowTransform.GetComponentInParent<Canvas>().transform as RectTransform, eventData.position, eventData.pressEventCamera, out originalMousePos
            );
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            RectTransform parentRect = windowTransform.parent as RectTransform;
            Vector2 currentMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                windowTransform.GetComponentInParent<Canvas>().transform as RectTransform, eventData.position, eventData.pressEventCamera, out currentMousePos
            );

            // Delta in parent's local coordinates
            Vector2 delta = currentMousePos - originalMousePos;
            
            Vector3 scale = windowTransform.localScale;
            

            // We'll compute new size and new position
            Vector2 newSize = originalSize;
            Vector2 newPos  = originalPosition;

            // --- Horizontal resize ---
            if (Mathf.Abs(modifier.x) > 0.01f)
            {
                if (modifier.x > 0)
                {
                    // Resizing from the RIGHT side
                    // Increase width by delta.x
                    newSize.x = originalSize.x + delta.x / scale.x;
                    newPos.x = originalPosition.x + delta.x * 0.5f;
                }
                else
                {
                    // Resizing from the LEFT side
                    // Increase width by -delta.x (if user drags left, delta.x is negative => bigger width)
                    newSize.x = originalSize.x - delta.x / scale.x;
                    // Move the panel's X position to keep the right side where it was
                    newPos.x = originalPosition.x + delta.x * 0.5f;
                }
            }

            // --- Vertical resize ---
            if (Mathf.Abs(modifier.y) > 0.01f)
            {
                if (modifier.y > 0)
                {
                    // Resizing from the TOP side
                    // Increase height by delta.y
                    newSize.y = originalSize.y + delta.y / scale.y;
                    newPos.y = originalPosition.y + delta.y * 0.5f;
                }
                else
                {
                    // Resizing from the BOTTOM side
                    // Increase height by -delta.y
                    newSize.y = originalSize.y - delta.y / scale.y;
                    // Move the panel's Y position to keep the top side where it was
                    newPos.y = originalPosition.y + delta.y * 0.5f;
                }
                
            }

            // Optionally clamp so size doesn't go negative
            if (newSize.x < 0) newSize.x = 0;
            if (newSize.y < 0) newSize.y = 0;

            // Apply final position & size
            windowTransform.anchoredPosition = newPos;
            windowTransform.sizeDelta        = newSize;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            // Restore default cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            Cursor.SetCursor(resizeCursor, Vector2.one * resizeCursor.width * 0.5f, CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isDragging)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Revert to the previous cursor
            }
        }
    }
#pragma warning restore CS3009, CS3003, CS3001
}