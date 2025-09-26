// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    

using UnityEngine;

namespace realvirtual
{
    public class DeactivateAllOtherInterfaces : MonoBehaviour,IBeforeAwake
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void OnBeforeAwake()
        {
            if (!enabled) return;
           
            InterfaceBaseClass[] interfaces = FindObjectsByType<InterfaceBaseClass>(FindObjectsInactive.Include,FindObjectsSortMode.None);
      
            foreach (InterfaceBaseClass intface in interfaces)
            {
                if (intface.gameObject == this.gameObject) continue;       // make sure that it is not the current interface
                
                intface.Active = realvirtualBehavior.ActiveOnly.Never;
                intface.ChangeConnectionMode(false);
                Debug.Log("DeactivateAllOtherInterfaces: Deactivated " + intface.gameObject.name);
            }
        }
        
    }
}
