// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
using UnityEngine.UI;

namespace realvirtual
{
    [ExecuteAlways]
    public class rvUIAutoRowHeight : MonoBehaviour
    {
        public LayoutElement row;
        public RectTransform content;
        
        private void Update()
        {
            row.preferredHeight = content.rect.height;
            row.flexibleHeight = 0;
        }

    }
}

