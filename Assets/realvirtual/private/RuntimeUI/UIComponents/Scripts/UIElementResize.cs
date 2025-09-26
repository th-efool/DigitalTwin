using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual

{
    public class UIElementResize : MonoBehaviour
    {
        internal RectTransform rectTransform;
        private RectTransform windowTR;
        public GameObject ContentWindow;
        [SerializeField] internal int minWidth = 380;
        [SerializeField] internal int minHeight = 300;

        public void Awake()
        {
            
            rectTransform = GetComponent<RectTransform>();
            windowTR = ContentWindow.GetComponent<RectTransform>();
            ContentWindow.GetComponent<UIWindowMovement>().Initialize(this);
        }
        internal void EnsureWindowIsWithinBounds()
        {
            Vector2 canvasSize = rectTransform.sizeDelta;
            Vector2 windowSize = windowTR.sizeDelta;

            if( windowSize.x < minWidth )
                windowSize.x = minWidth;
            if( windowSize.y < minHeight )
                windowSize.y = minHeight;

            if( windowSize.x > canvasSize.x )
                windowSize.x = canvasSize.x;
            if( windowSize.y > canvasSize.y )
                windowSize.y = canvasSize.y;

            Vector2 windowPos = windowTR.anchoredPosition;
            Vector2 canvasHalfSize = canvasSize * 0.5f;
            Vector2 windowHalfSize = windowSize * 0.5f;
            Vector2 windowBottomLeft = windowPos - windowHalfSize + canvasHalfSize;
            Vector2 windowTopRight = windowPos + windowHalfSize + canvasHalfSize;

            if( windowBottomLeft.x < 0f )
                windowPos.x -= windowBottomLeft.x;
            else if( windowTopRight.x > canvasSize.x )
                windowPos.x -= windowTopRight.x - canvasSize.x;

            if( windowBottomLeft.y < 0f )
                windowPos.y -= windowBottomLeft.y;
            else if( windowTopRight.y > canvasSize.y )
                windowPos.y -= windowTopRight.y - canvasSize.y;

            windowTR.anchoredPosition = windowPos;
            windowTR.sizeDelta = windowSize;
        }
        
        internal void OnWindowDimensionsChanged( Vector2 size )
        {
            windowTR.sizeDelta = size;
            EnsureWindowIsWithinBounds();
  
        }
    }
}
