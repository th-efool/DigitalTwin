// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if REALVIRTUAL_PROFESSIONAL
using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_StatState : LogicStep
    {
        public StatStates StatStatesComponent;
        public string SetState;

        protected new bool NonBlocking()
        {
            return true;
        }

        protected override void OnStarted()
        {
            if (StatStatesComponent != null)
            {
                StatStatesComponent.State(SetState);
            }
            NextStep();
        }

        protected new void Start()
        {
            base.Start();
        }
    }
}
#endif
