// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! Logic step that introduces a time delay before proceeding to the next step.
    //! Useful for creating pauses between automation actions or waiting for processes to complete.
    //! The step progress is displayed as a percentage during execution.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_Delay : LogicStep
    {
        [Header("Delay Settings")]
        [Label("Duration (seconds)")]
        [Min(0)]
        public float Duration; //!< Time to wait in seconds before proceeding to the next step

        private float starttime;
        
        protected override void OnStarted()
        {
            State = 0;
            starttime = Time.time;
        }
        

        private void Finished()
        {
            NextStep();
        }

        public void FixedUpdate()
        {
            if (StepActive)
            {
                var elapsed = Time.time - starttime;
                var delta = (elapsed / Duration) * 100;
                State = delta;
                if (elapsed >= Duration)
                {
                    NextStep();
                }
            }
        }

   
    }

}
