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
    public class LogicStep_JumpOnSignal: LogicStep
    {
        [Header("Jump Configuration")]
        public string JumpToStep; //!< Name of the logic step to jump to when condition is met
        public Signal Signal; //!< The signal to evaluate for the jump condition
        public bool JumpOn; //!< Jump when signal equals this value (true or false)

        protected new bool NonBlocking()
        {
            return true;
        }

        protected override void OnStarted()
        {
            if (Signal != null && (bool)Signal.GetValue() == JumpOn)
                NextStep(JumpToStep);
            else
                NextStep();
        }
    }

}

