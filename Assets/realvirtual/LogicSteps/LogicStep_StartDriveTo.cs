// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_StartDriveTo : LogicStep
    {
        public Drive drive;
        [OnValueChanged("EditorPosition")] public float Destination;
        public bool Relative = false;
        [OnValueChanged("LiveEditStart")] public bool LiveEdit = false;
        protected new bool NonBlocking()
        {
            return true;
        }
        
        private void LiveEditStart()
        {
            if (drive!=null)
                if (LiveEdit)
                {
               
                    drive.StartEditorMoveMode();
                    EditorPosition();
                }
                else
                    drive.EndEditorMoveMode();
        }
        

        private void EditorPosition()
        {
            if (drive != null)
            {
                if (LiveEdit)
                {
              
                    drive.SetPositionEditorMoveMode(Destination);
                }
            }
        }

        protected override void OnStarted()
        {
            State = 0;
            if (drive != null)
            {
                var des = Destination;
                if (Relative)
                    des = drive.CurrentPosition + Destination;
                drive.DriveTo(des);
            }
            
            NextStep();
          
        }
    }

}

