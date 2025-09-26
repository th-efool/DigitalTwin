// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_StartDriveSpeed : LogicStep
    {
        public Drive drive;
        public float Speed;
        
        protected new bool NonBlocking()
        {
            return true;
        }

        protected override void OnStarted()
        {
            if (drive != null)
            {
                drive.TargetSpeed = Mathf.Abs(Speed);
                if (Speed > 0)
                {
                    drive.JogForward = true;
                    drive.JogBackward = false;
                }

                if (Speed == 0)
                {
                    drive.JogForward = false;
                    drive.JogBackward = false;
                }

                if (Speed < 0)
                {
                    drive.JogForward = false;
                    drive.JogBackward = true;
                }
            }

            NextStep();
            
        }
        
    }

}

