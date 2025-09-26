// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using UnityEngine;

namespace realvirtual
{
    //! Copies the size of one RectTransform to another RectTransform
    [ExecuteInEditMode]
    public class rvUICopySize : MonoBehaviour
    {
        [Tooltip("The RectTransform to resize")]
        public RectTransform rectTransform;
        
        [Tooltip("The RectTransform to copy size from")]
        public RectTransform other;

        private void Update()
        {
            CopySize();
        }

        //! Copies the size from the other RectTransform to this one
        public void CopySize()
        {
            rectTransform.sizeDelta = other.sizeDelta;
        }
    }
}
