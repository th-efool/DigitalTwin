// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;


namespace realvirtual
{
    public abstract class AbstractSelectionManager : MonoBehaviour
    {
        public abstract void Select(Renderer renderer);
        
        public abstract void Deselect(Renderer renderer);

        public abstract void DeselectAll();

    }
    
}
