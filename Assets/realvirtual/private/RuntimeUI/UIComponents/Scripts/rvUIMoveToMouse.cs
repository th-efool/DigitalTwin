// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    //! Moves a UI element to follow the mouse position with an optional offset
    public class rvUIMoveToMouse : MonoBehaviour
    {
        [Tooltip("Offset from mouse position in screen pixels")]
        public Vector2 offset;
        
        void Update()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 anchoredPos = mousePos - offset;
            transform.position = anchoredPos;
        }
    }
}
