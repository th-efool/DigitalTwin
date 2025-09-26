// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    //! Logic step that conditionally jumps to another named step based on a signal value.
    //! This non-blocking step enables branching logic in automation sequences.
    //! If the signal matches the specified condition, execution jumps to the named step; otherwise continues normally.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_DestroyObject : LogicStep
    {

        [Header("Destruction Configuration")]
        [Required("Object to destroy is required")]
        [SerializeField] public Object ObjectToDestroy; //!< Name of the logic step to jump to when condition is met

        protected new bool NonBlocking()
        {
            return false;
        }


        protected override void OnStarted()
        {
            if (ObjectToDestroy != null) { Destroy(ObjectToDestroy); }
            else { NextStep(); }
        }
    }

}

