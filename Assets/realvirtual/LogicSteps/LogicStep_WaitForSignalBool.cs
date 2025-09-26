// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_WaitForSignalBool: LogicStep
    {
        public Signal Signal;
        public bool WaitForTrue;

        private bool signalnotnull = false;
        
        protected new bool NonBlocking()
        {
            return false;
        }
        
        protected override void OnStarted()
        {
            IsWaiting = true;
            State = 50;
            if (signalnotnull==false)
                NextStep();
        }

     
        protected new void Start()
        {
            signalnotnull = Signal != null;
            base.Start();
        }
        
        private void FixedUpdate()
        {
           if (signalnotnull && StepActive)
               if ((bool)Signal.GetValue()==WaitForTrue)
                   NextStep();
        }
    }

}

