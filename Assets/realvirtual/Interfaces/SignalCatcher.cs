// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    


using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! Collects and organizes all signals in the scene under a single parent object.
    //! This component automatically gathers signals from throughout the scene hierarchy and reorganizes them
    //! as children of this object, making signal management and interface configuration more centralized.
    public class SignalCatcher : MonoBehaviour,IAllScenesLoaded
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        
        // a list of all gameobjects where children should be ignored
        [InfoBox("All signals in the scene will be collected and placed as children of this object. If you want to ignore children of specific objects, add them to the list below.")]
        public GameObject[] IgnoreThis; //!< List of GameObjects whose child signals should not be collected
        public bool OnlyInterfaceSignals = false; //!< If true, only collects signals that are part of interface components
        public bool DeactivateOtherInterfaces = false; //!< If true, deactivates all other interface components in the scene
        
        //! Collects all signals in the scene and reorganizes them under this object.
        public void CatchSignals()
        {
            if (!enabled) return;
            
            
            var collected = 0;
            // get all signals in whole model and place them here as a child object
            Signal[] signals = FindObjectsByType<Signal>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach (Signal signal in signals)
            {
                // check if this a child of an object that should be ignored
                // first check if the signal is already a child of this object
                if (signal.transform.IsChildOf(transform)) continue;
                
                // check if the signal is child of an object which is interfacebaseclass
                if (OnlyInterfaceSignals)
                {
                    if (signal.transform.GetComponentInParent<InterfaceBaseClass>() == null) continue;
                }
                
                foreach (GameObject go in IgnoreThis)
                {
                    if (signal.transform.IsChildOf(go.transform))
                    {
                        break;
                    }
                }
                signal.transform.SetParent(transform);
                collected++;
            }
            
            
            if (collected>0)
                 Debug.Log("Signal Catcher: Collected " + collected + " signals and copied them to [" + this.name + "]");
            
            // now find all interfaces which are not this interface
            if (DeactivateOtherInterfaces)
            {
                InterfaceBaseClass[] interfaces = FindObjectsByType<InterfaceBaseClass>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                foreach (InterfaceBaseClass intface in interfaces)
                {
                    if (intface.gameObject == this.gameObject) continue;       // make sure that it is not the current interface
                    intface.Active = realvirtualBehavior.ActiveOnly.Never;
                    intface.ChangeConnectionMode(true);
                    Debug.Log("Signal Catcher: Deactivated " + intface.gameObject.name);
                }
            }
        }

        //! Called after all scenes are loaded to automatically collect signals.
        public void AllScenesLoaded()
        {
            CatchSignals();
        }
    }
}
