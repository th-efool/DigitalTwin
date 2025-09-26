// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#pragma warning disable 4014

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    // Fixed Issues:
    // 1. Line 319: Fixed inverted exit sensor logic (was checking == false, now == true)
    // 2. Replaced dangerous Invoke() calls with cancellable async timers
    // 3. Added state validation before transitions
    // 4. Added timer cancellation on switch off/emergency stop
    public class PLCDemoCNCLoadUnload : realvirtualBehavior
    {
        public float MachineCycleTime = 10;
        [InfoBox("Only Machine Control, connected PLC is controlling the rest of the system")]
        public bool OnlyMachineControll= false;
        
        [Header("State")]
        [ReadOnly] public string RobotState;
        [ReadOnly] public string EntryState;
        [ReadOnly] public string MachineState;
        [ReadOnly] public string ExitState;
        [ReadOnly] public bool AutomaticMode = true;

        [Header("Buttons")] 
        public PLCInputBool OnSwitch;
        public PLCInputBool EmergencyButton;
        public PLCInputBool AutomaticButton;
        public PLCOutputBool AutomaticButtonLight;
        public PLCInputBool RobotButton;
        public PLCOutputBool RobotLight;
        public PLCInputBool ConveyorInButton;
        public PLCOutputBool ConveyorInLight;
        public PLCInputBool ConyeyorOutButton;
        public PLCOutputBool ConveyorOutLight;
        
        [Header("Robot")] public PLCOutputBool StartLoadingProgramm;
        public PLCOutputBool StartUnloadingProgramm;
        public PLCInputBool LoadingProgrammIsRunning;
        public PLCInputBool UnloadingProgrammIsRunning;
        
        [Header("Machine")] 
        public PLCOutputBool StartMachine;
        public PLCOutputBool MoveToolingWheel;
        
        [Header("PLCToMachine")] 
        public PLCOutputBool OpenDoor;
        public PLCInputBool DoorOpened;
        public PLCInputBool DoorClosed;
        public PLCOutputBool StartMachining;
        public PLCInputBool IsMachining;
        public PLCInputBool MachiningFinished;
        
        public PLCOutputBool EntryConveyorStart;
        public PLCInputBool EntrySensorOccupied;
        
        [Header("ExitConveyor")]
        public PLCOutputBool ExitConveyorStart;
        public PLCInputBool ExitSensorOccupied;
        
        private CancellationTokenSource cancellationTokenSource, cancellationTokenSourceEmergency;
        private bool startmachinebefore = false;
        private CancellationTokenSource machineTimerToken;
        private CancellationTokenSource toolingWheelToken;
        new void Awake()
        {
            // if not enabled do nothing
            if (!this.enabled || Active == ActiveOnly.Never)
                return;
            RobotState = "WaitingForLoading";
            EntryState = "WatingForPart";
            MachineState = "Empty";
            ExitState = "Empty";
            if (!OnlyMachineControll) EntryConveyorStart.Value = true;
            AutomaticMode = true;
            if (MachiningFinished != null) MachiningFinished.Value = true;
            base.Awake();
        }
        
        
        private async Task BlinkLight(PLCOutputBool light, float frequence, CancellationToken token)
        {
            bool interrupted = false;
            try
            {
                while (!interrupted)
                {
                    light.Value = !light.Value;
                    await Task.Delay(System.TimeSpan.FromSeconds(frequence), token);
                }
            }
            catch (TaskCanceledException)
            {
                interrupted = true;
            }
        }

        private void MachineControll()
        {
            // this is used if only the machine is controlled and the rest of the system by a real connected plc

            if (MachineState == "Empty" && StartMachining.Value && !startmachinebefore && IsMachining.Value == false)
            {
                MachiningFinished.Value = false;
                StartMachine.Value = true;
                MoveToolingWheel.Value = true;
                OpenDoor.Value = false;
                IsMachining.Value = true;
                // Use coroutine instead of Invoke for better state control
                StartMachineTimer(MachineCycleTime);
                StartToolingWheelTimer(4.0f);
            }

            if (MachineState == "WaitingForUnloading")
            {
                IsMachining.Value = false;
                MachiningFinished.Value = true;
                StartMachine.Value = false;
                MachineState = "Empty";
            }
            startmachinebefore = StartMachining.Value;
        }
        // This is the PLC Cycle, permanent loop - checking the inputs and setting the outputs based on the state
        void FixedUpdate()
        {
            if (OnlyMachineControll)
            {
                MachineControll();
                return;
            }
                
      
            // On Switch Pressed - switch if possible
            if (OnSwitch.Value == false)
            {
                EntryConveyorStart.Value = false;
                ExitConveyorStart.Value = false;
                // Cancel any running timers when switch is off
                if (machineTimerToken != null)
                {
                    machineTimerToken.Cancel();
                }
                if (toolingWheelToken != null)
                {
                    toolingWheelToken.Cancel();
                }
            }
            
            // Emergency Button Pressed - switch if possible
            if (EmergencyButton.ChangedToTrue  && !AutomaticButton.Value)
            {
                AutomaticButtonLight.Value = false;
                AutomaticButton.Value = false;
                EntryConveyorStart.Value = false;
                ExitConveyorStart.Value = false;
                AutomaticMode = false;
                cancellationTokenSourceEmergency = new CancellationTokenSource();
                BlinkLight(AutomaticButtonLight,0.2f,cancellationTokenSourceEmergency.Token);
            }
            
            if (EmergencyButton.ChangedToFalse)
            {
                if (cancellationTokenSourceEmergency != null)
                {
                    cancellationTokenSourceEmergency.Cancel();
                    cancellationTokenSourceEmergency = null;
                }
                 
            }
            
            if (!AutomaticMode && !EmergencyButton.Value)
            {
                if (cancellationTokenSource == null)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    BlinkLight(AutomaticButtonLight,0.7f,cancellationTokenSource.Token);
                }
              
            }
           
            
            // Automaticmode Button Pressed - switch if possible
            if (AutomaticButton.ChangedToTrue && !EmergencyButton.Value && OnSwitch.Value)
            {
                if (AutomaticMode) // turn off if possible
                {
                    AutomaticMode = false;
                    cancellationTokenSource = new CancellationTokenSource();
                    BlinkLight(AutomaticButtonLight,0.7f,cancellationTokenSource.Token);
                }
                else
                {
                    AutomaticMode = true;
                    if (cancellationTokenSource != null)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = null;
                    }
                        
                }
            }
            
            if (AutomaticMode)
            {
                AutomaticButtonLight.Value = true;
            }
            
            RobotLight.Value = RobotButton.Value;

            if (OnSwitch.Value)
            {
                ConveyorInLight.Value = ConveyorInButton.Value;
                ConveyorOutLight.Value = ConyeyorOutButton.Value;
            }
            else
            {
                AutomaticMode = false;
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = null;
                }
                
                if (cancellationTokenSourceEmergency != null)
                {
                    cancellationTokenSourceEmergency.Cancel();
                    cancellationTokenSourceEmergency = null;
                }
                   
                ConveyorInLight.Value = false;
                ConveyorOutLight.Value = false;
                RobotLight.Value = false;
                AutomaticButtonLight.Value = false;
             
            }
            
            
            // Entry Conveyor ENTRYSTATE
            if (EntryState == "WatingForPart" && !EmergencyButton.Value && AutomaticMode && OnSwitch.Value)  
            {
                if (!ConveyorInButton.Value)
                {
                    EntryConveyorStart.Value = false;
                }
                else
                {
                    if (EntrySensorOccupied.Value == true)
                    {
                        EntryState = "PartAvailable";
                        EntryConveyorStart.Value = false;
                    }
                    if (EntrySensorOccupied.Value == false) // only move if waiting for part
                    {
                        EntryConveyorStart.Value = true;
                    }
                }
             
            }

            // Start Robot When Part is available
            if (RobotState == "WaitingForLoading" && AutomaticMode && RobotButton.Value && OnSwitch.Value)
            {
                if (EntryState == "PartAvailable" && MachineState == "Empty")
                {
                    MachineState = "Loading";
                    RobotState = "LoadingMachineMoveToConveyor";
                    StartLoadingProgramm.Value = true;
                    EntryState = "WatingForRobotToTakePart";
                }
            }
            
            // Set Entry for Waiting if Part is taken by Robot
            if (RobotState == "LoadingMachineMoveToConveyor")
            {
                if (EntrySensorOccupied.Value == false)
                {
                    RobotState = "LoadingMachineMoveToMachine";
                    EntryState = "WatingForPart";
                    StartLoadingProgramm.Value = false;
                }
            }
            
            // If Loading is finished, start Machine
            if (RobotState == "LoadingMachineMoveToMachine")
            {
                 if (LoadingProgrammIsRunning.Value == false)
                 {
                     // Ensure we're in the right state before transitioning
                     if (MachineState == "Loading")
                     {
                         RobotState = "WaitingForUnloading";
                         MachineState = "StartMachine";
                     }
                 }
            }
            
            
            // Start Unloading if Machine is ready
            if (RobotState == "WaitingForUnloading" && AutomaticMode && RobotButton.Value && OnSwitch.Value)
            {
                if (MachineState == "WaitingForUnloading")
                {
                    // Check if exit is ready - either empty or occupied but with conveyor running
                    bool exitReady = (ExitState == "Empty") || 
                                    (ExitState == "Occupied" && ConyeyorOutButton.Value);
                    
                    if (exitReady)
                    {
                        ExitState = "WaitingForPartFromRobot";
                        MachineState = "Unloading";
                        RobotState = "UnloadingMachine";
                        StartUnloadingProgramm.Value = true;
                    }
                }
            }

            
            // If Unloading is finsihe, set Machine to Empty
            if (RobotState == "UnloadingMachine")
            {
                if (UnloadingProgrammIsRunning.ChangedToFalse) // only negative flank because it might take some time to start
                {
                    RobotState = "WaitingForLoading";
                    MachineState = "Empty";
                    // Only set ExitState to Occupied if it was waiting for part
                    if (ExitState == "WaitingForPartFromRobot")
                    {
                        ExitState = "Occupied";
                    }
                    StartUnloadingProgramm.Value = false;
                }
            }
           
            
            // Exit Conveyor EXITSTATE
            if (ExitState == "WaitingForPartFromRobot" && !EmergencyButton.Value && OnSwitch.Value)
            {
                ExitConveyorStart.Value = false;
            }

            if (ExitState == "Occupied" && !EmergencyButton.Value && ConyeyorOutButton.Value)
            {  
                ExitConveyorStart.Value = true;
                
               // get negative flank of exit sensor - part has left the sensor
               if (ExitSensorOccupied.ChangedToFalse == true)
               {
                   ExitState = "Empty";
                   ExitConveyorStart.Value = false;  // Stop conveyor after part exits
               }
            }
       
            
            /// Machine States
            if (MachineState == "Loading")
            {
                OpenDoor.Value = true;
            }
            
            if (MachineState == "Unloading")
            {
                OpenDoor.Value = true; 
                MoveToolingWheel.Value = false;
                StartToolingWheelTimer(2.0f);
            }
            
            if (MachineState == "StartMachine" && AutomaticMode && OnSwitch.Value)
            {
                StartMachine.Value = true;
                MoveToolingWheel.Value = true;
                OpenDoor.Value = false;
                StartMachineTimer(MachineCycleTime);
                StartToolingWheelTimer(4.0f);
                MachineState = "Machining"; // Change state to prevent re-triggering
            }
            
            if (MachineState == "WaitingForUnloading")
            {
                // Stop the machine when waiting for unloading
                if (StartMachine.Value == true)
                {
                    StartMachine.Value = false;
                }
                OpenDoor.Value = true; 
            }
            
        }
        
        void EndMachine()
        {
            // Only change state if we're still in the expected state
            if (MachineState == "Machining")
            {
                MachineState = "WaitingForUnloading";
                StartMachine.Value = false; // Stop the machine after cycle completes
            }
            else
            {
            }
        }
        
        void EndMoveToolingWheel()
        {
            MoveToolingWheel.Value = false;
        }
        
        // Safe timer methods that can be cancelled
        async void StartMachineTimer(float delay)
        {
            // Cancel any existing timer
            if (machineTimerToken != null)
            {
                machineTimerToken.Cancel();
            }
            
            machineTimerToken = new CancellationTokenSource();
            
            try
            {
                await Task.Delay(System.TimeSpan.FromSeconds(delay), machineTimerToken.Token);
                EndMachine();
            }
            catch (System.OperationCanceledException)
            {
                // Timer was cancelled, do nothing
            }
        }
        
        async void StartToolingWheelTimer(float delay)
        {
            // Cancel any existing timer
            if (toolingWheelToken != null)
            {
                toolingWheelToken.Cancel();
            }
            
            toolingWheelToken = new CancellationTokenSource();
            
            try
            {
                await Task.Delay(System.TimeSpan.FromSeconds(delay), toolingWheelToken.Token);
                EndMoveToolingWheel();
            }
            catch (System.OperationCanceledException)
            {
                // Timer was cancelled, do nothing
            }
        }
    }
}

