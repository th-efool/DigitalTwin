// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    //! Logic step that enables or disables a GameObject and immediately proceeds.
    //! This non-blocking step is used to control object visibility and activity during automation sequences.
    //! Useful for showing/hiding visual indicators, activating/deactivating components, or managing scene objects.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_Enable: LogicStep
    {
        [Header("GameObject Control")]
        public GameObject Gameobject; //!< The GameObject to enable or disable
        public bool Enable; //!< If true, enables the GameObject; if false, disables it

 
        
        protected new bool NonBlocking()
        {
            return true;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (Gameobject!=null)
                Gameobject.SetActive(Enable);
            NextStep();
        }

    }

}

