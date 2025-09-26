// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if REALVIRTUAL_PROFESSIONAL
using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_StatEndCycle : LogicStep
    {
        public StatCycleTime StatCycleTimeComponent;

        protected new bool NonBlocking()
        {
            return true;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (StatCycleTimeComponent != null)
                StatCycleTimeComponent.StopCycle();
            NextStep();
        }
        
        protected new void Start()
        {
            base.Start();
        }
    }

}
#endif
