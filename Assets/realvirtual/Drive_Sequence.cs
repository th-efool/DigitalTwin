// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
   
    //! Defines sequentially movement of drives which can set signals or be started by signals
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
    public class Drive_Sequence : BehaviorInterface
    {
        [System.Serializable]
        public class DriveSequence
        {
            [Tooltip("Description of this sequence step")]
            public string Description;
            [Tooltip("Signal to set to false when this step starts")]
            public Signal SignalToFalseOnStart;
            [Tooltip("Wait for this signal to become true before starting drive")]
            public Signal WaitForSignal;
            [Tooltip("Drive to control in this step (uses this GameObject's drive if empty)")]
            public Drive Drive;
            [Tooltip("Target position in millimeters for this step")]
            public float Destination;
            [Tooltip("Don't wait for drive to reach destination, proceed immediately")]
            public bool NoWait;
            [Tooltip("Drive speed in mm/s for this step (0 uses drive's default)")]
            public float Speed;
            [Tooltip("Time to wait in seconds after step completes")]
            public float WaitAfterStep;
            [Tooltip("Signal to set true when this step finishes")]
            public Signal FinishedSignal;
    
        }

        [Tooltip("Start sequence automatically when scene plays")]
        public bool StartAtBeginning = true;
        [Tooltip("Reset wait signals to false after they trigger")]
        public bool ResetWaitForSignals = true;
        [Tooltip("Pause after each step (requires manual continuation)")]
        public bool StopAfterEachStep = false;
        
        [ReadOnly] public int CurrentStep = -1;
        [ReadOnly] public Drive CurrentDrive;
        [ReadOnly] public float CurrentDestination;
        [ReadOnly] public Signal CurrentWaitForSignal;
        
        [SerializeField]
        [Tooltip("List of sequential drive movements to execute")]
        public List<DriveSequence> Sequence = new List<DriveSequence>();

     

        private bool waitforsignal = false;
        private bool waitfornextstepbutton = false;
        private bool waitafterstep = false;
        
        // Start is called before the first frame update
        void StartSequzence()
        {
            CurrentStep = -1;
            NextStep();
        }

        void NextStep()
        {
            waitafterstep = false;
            CurrentStep++;
            waitforsignal = false;
            if (CurrentStep > Sequence.Count-1)
            {
                CurrentStep = 0;
            }

            CurrentDrive = Sequence[CurrentStep].Drive;
          
            if (CurrentDrive == null)
                CurrentDrive = this.GetComponent<Drive>();
         
            CurrentDestination = Sequence[CurrentStep].Destination;
            
            if (Sequence[CurrentStep].SignalToFalseOnStart!=null)
            {
                Sequence[CurrentStep].SignalToFalseOnStart.SetValue(false);
            }   
            
            if (Sequence[CurrentStep].FinishedSignal!=null)
            {
                Sequence[CurrentStep].FinishedSignal.SetValue(false);
            }   
            
    
            if (Sequence[CurrentStep].WaitForSignal != null)
            {
                waitforsignal = true;
                CurrentWaitForSignal = Sequence[CurrentStep].WaitForSignal;
            }
            else
            {
                StartDrive();
                if (Sequence[CurrentStep].NoWait)
                    StepFinished();
            }
    
            
        }

        void StartDrive()
        {
            if (CurrentDrive == null)
                return;
            if (Sequence[CurrentStep].Speed!=0)
                CurrentDrive.TargetSpeed =  Sequence[CurrentStep].Speed;
            if (!(CurrentDestination==0 && Sequence[CurrentStep].Speed==0))
                 CurrentDrive.DriveTo(CurrentDestination);
        }
        
        void Start()
        {
            if (StartAtBeginning)
                StartSequzence();
        }
        
        void ButtonNextStep()
        {
            if (waitfornextstepbutton)
            {
                NextStep();
                waitfornextstepbutton = false;
            }

        }

        void StepFinished()
        {
            if (StopAfterEachStep != true)
            {
                NextStep();
            }
            else
                waitfornextstepbutton = true;
        }

        // Update is called once per frame
        public void FixedUpdate()
        {
            if (ForceStop) return;
            
            if (CurrentStep > -1)
            {
                if (waitforsignal)
                {
                    if ((bool)CurrentWaitForSignal.GetValue() == true)
                    {
                        waitforsignal = false;
                        StartDrive();
                        if (ResetWaitForSignals)
                            CurrentWaitForSignal.SetValue(false);
                        return;
                    }
                }

                if (ReferenceEquals(CurrentDrive,null) )
                {
                    if (!waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        waitafterstep = true;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        Invoke("StepFinished", wait);
                    }
                }
                else
                {
                    if  (CurrentDrive.CurrentPosition == CurrentDestination && !waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        waitafterstep = true;
                        Invoke("StepFinished", wait);
                    }
                }
           
            }
        }
    }
}

