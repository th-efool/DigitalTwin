// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using UnityEngine;

namespace realvirtual
{
    public class rvUIWindow : MonoBehaviour
    {
        public GameObject window;
        private RectTransform _rect;
        private RectTransform canvasRectTransform;
        private float padding = 20f;
        private bool positionChecked = false;
        private rvUIScaler _scaler; 
        public void Awake()
        {
           _rect=GetComponent<RectTransform>();
           _scaler = GetComponent<rvUIScaler>();
           if (canvasRectTransform == null)
           {
               canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
           }

           positionChecked = false;
        }

        private void Update()
        {
            if (window.activeSelf && !positionChecked)
            {
                if (_scaler != null)
                    _scaler.UpdaterScale();
                EnsureVisible();
                positionChecked = true;
            }
            if(!window.activeSelf && positionChecked)
                positionChecked = false;
        }

        private void OnEnable()
        {
            EnsureVisible();
        }
        private void EnsureVisible()
        {
            if (_rect == null || canvasRectTransform == null) return;

            
            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);
            
            Vector3[] uiWindowCorners = new Vector3[4];
            _rect.GetWorldCorners(uiWindowCorners);
           
            Vector2 offset = Vector2.zero;
            
            float height = (uiWindowCorners[1].y - uiWindowCorners[0].y)/2;
            float width = (uiWindowCorners[2].x - uiWindowCorners[0].x)/2;
            
            if (uiWindowCorners[0].x < canvasCorners[0].x+padding) // Left
                offset.x = canvasCorners[0].x+padding+width - uiWindowCorners[0].x;
            else if (uiWindowCorners[2].x > canvasCorners[2].x-padding) // Right
                offset.x =  canvasCorners[2].x-padding-width - uiWindowCorners[2].x;

            
            if (uiWindowCorners[1].y > canvasCorners[1].y-padding) // Top
                offset.y = canvasCorners[1].y-padding-height - uiWindowCorners[1].y;
            else if (uiWindowCorners[0].y < canvasCorners[0].y+padding) // Bottom
                offset.y = canvasCorners[0].y +padding+height- uiWindowCorners[0].y;
            offset *= _rect.lossyScale;
            _rect.anchoredPosition += offset;
        }
    }
}

