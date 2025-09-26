// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System.Collections.Generic ;
using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_WaitForDrivesAtTarget: LogicStep
    {
        [ReorderableList] public List<Drive> Drives;
      
        
        protected new bool NonBlocking()
        {
            return false;
        }
        
        protected override void OnStarted()
        {
            IsWaiting = true;
            State = 50;
            if (Drives == null )
                NextStep();
        }
        
        
        private void FixedUpdate()
        {
            if (!StepActive)
                return;
            
            bool nextstep = true;
            foreach (var drive in Drives)
            {
                if (!drive.IsAtTarget)
                    nextstep = false;
            }
            if (nextstep)
                NextStep();
        }
    }

}

