// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    //! Logic step that sets a boolean signal to a specified value and immediately proceeds.
    //! This non-blocking step is used to control signals during automation sequences.
    //! Commonly used for triggering outputs or setting control flags.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_SetSignalBool : LogicStep
    {
        [Header("Signal Configuration")]
        public Signal Signal; //!< The boolean signal to set
        public bool SetToTrue; //!< The value to set the signal to (true or false)

        private bool signalnotnull = false;
        
        protected new bool NonBlocking()
        {
            return true;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (signalnotnull)
                Signal.SetValue((bool)SetToTrue);
            NextStep();
        }
        
        protected new void Start()
        {
            signalnotnull = Signal != null;
            base.Start();
        }
        
      
    }

}

