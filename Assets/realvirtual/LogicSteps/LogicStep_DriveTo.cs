// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! Logic step that moves a drive to a specified target position and waits for completion.
    //! This step can perform absolute or relative movements and optionally read the target position from a signal.
    //! The step automatically proceeds to the next step when the drive reaches its destination.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_DriveTo : LogicStep
    {
        [Header("Drive Configuration")]
        [Required("Drive component is required")]
        public Drive drive; //!< The drive component to control
        
        [Header("Destination")]
        [OnValueChanged("EditorPosition")]
        [Label("Target Position")]
        public float Destination; //!< Target position in millimeters for the drive movement
        
        [Label("Get Position from Signal")]
        public PLCOutputFloat DestinationFromPLCOutput; //!< Optional signal to read the destination position from
        
        [Label("Relative Movement")]
        public bool Relative = false; //!< If true, the destination is relative to current position
        
        [Space]
        [OnValueChanged("LiveEditStart")]
        [Label("Live Preview")]
        public bool LiveEdit = false; //!< Enables live preview of the target position in editor
        private float startpos = 0;
        private float delta = 0;

        private float des = 0;
        
        private bool ValueBySignal= false;

        private void Awake()
        {
            if (drive != null && DestinationFromPLCOutput != null)
            {
                // take value from PLCOutputFloat if not 0
                if (DestinationFromPLCOutput.Value != 0)
                {
                    Destination = DestinationFromPLCOutput.Value;
                    ValueBySignal = true;
                }
            }
        }

        protected override void OnStarted()
        {
            LiveEdit = false;
            State = 0;
            if (drive != null)
            {
                drive.OnAtPosition += DriveOnOnAtPosition; 
                des = Destination;
                startpos = drive.CurrentPosition;
                if (Relative)
                    des = drive.CurrentPosition + Destination;
                 delta = Mathf.Abs(drive.CurrentPosition - Destination);
                drive.DriveTo(des);
            }
            else
            {
                NextStep();
            }
        }

        private void OnDestroy()
        {
            
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

        public void FixedUpdate()
        {
            if (StepActive)
            {
                var currdelta =  Mathf.Abs(drive.CurrentPosition - des);
                State = ((delta-currdelta) / delta * 100);
            }

            if (ValueBySignal) // Update Destination from PLCOutputFloat if it is set and has changed
            {
                if(Destination!=DestinationFromPLCOutput.Value)
                    Destination = DestinationFromPLCOutput.Value;
            }
        }

        private void DriveOnOnAtPosition(Drive drive1)
        {
            drive.OnAtPosition -= DriveOnOnAtPosition;
            NextStep();
        }
    }

}

