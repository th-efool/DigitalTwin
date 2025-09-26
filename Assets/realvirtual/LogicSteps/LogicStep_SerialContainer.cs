// // realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// // (c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    public class LogicStep_SerialContainer : LogicStep
    {
        [Header("Serial Container Settings")]
        [InfoBox("Executes child LogicSteps in sequence based on GameObject hierarchy order")]
        
        [Header("Runtime Info")]
        [ShowIf("IsShowingContainerInfo")]
        [ReadOnly]
        [Label("Active Step Index")]
        public int ActiveLogicStep = 0;
        
        [ShowIf("IsShowingContainerInfo")]
        [ReadOnly]
        [Label("Total Steps")]
        public int NumberLogicSteps;
        
        private bool IsShowingContainerInfo => Application.isPlaying;
        Hashtable serialSteps = new Hashtable();
        private List<LogicStep> stepOrder = new List<LogicStep>();
        private LogicStep currentStep;
        
        // Cycle time tracking
        private float cycleStartTime = 0f;
        private List<float> cycleTimes = new List<float>();
        private bool cycleInProgress = false;
        
        // Cycle time display fields for NaughtyAttributes
        [Header("Cycle Time Statistics")]
        [ShowIf("ShowCycleStats")]
        [ReadOnly]
        [Label("Min Cycle Time (s)")]
        [SerializeField] private float minCycleTimeDisplay = 0f;
        
        [ShowIf("ShowCycleStats")]
        [ReadOnly]
        [Label("Max Cycle Time (s)")]
        [SerializeField] private float maxCycleTimeDisplay = 0f;
        
        [ShowIf("ShowCycleStats")]
        [ReadOnly]
        [Label("Median Cycle Time (s)")]
        [SerializeField] private float medianCycleTimeDisplay = 0f;
        
        [ShowIf("ShowCycleStats")]
        [ReadOnly]
        [Label("Completed Cycles")]
        [SerializeField] private int completedCyclesDisplay = 0;
        
        private bool ShowCycleStats => Application.isPlaying && CompletedCycles > 0;
        
        // Public properties for UI display
        public float MinCycleTime { get; private set; } = 0f;
        public float MaxCycleTime { get; private set; } = 0f;
        public float MedianCycleTime { get; private set; } = 0f;
        public int CompletedCycles { get; private set; } = 0;

        protected override void OnStartInit()
        {
            IsContainer = true;
            NumberLogicSteps = 0;
            ActiveLogicStep = 0;
            stepOrder.Clear();
            serialSteps.Clear();
            
            // Reset cycle tracking state
            cycleInProgress = false;
            cycleStartTime = 0f;
            
            // loop through all direct children of this transform
            foreach (Transform child in transform)
            {
                // Get the LogicStep component if it exists
                LogicStep step = child.GetComponent<LogicStep>();
                if (step != null)
                {
                    stepOrder.Add(step);
                    serialSteps.Add(step, false);
                    NumberLogicSteps++;
                }
            }
        }
        
        // Public method to refresh child steps (useful for runtime creation)
        public void RefreshChildSteps()
        {
            OnStartInit();
        }

     
        
        protected override void OnStarted()
        {
            // Track cycle start time when starting a new cycle (ActiveLogicStep will be 0)
            if (ActiveLogicStep == 0)
            {
                if (!cycleInProgress)
                {
                    cycleStartTime = Time.time;
                    cycleInProgress = true;
                }
                else
                {
                    // Force start a new cycle if cycleStartTime is invalid
                    if (cycleStartTime <= 0)
                    {
                        cycleStartTime = Time.time;
                    }
                }
            }
            
            // Start the first step in the serial container
            foreach (LogicStep step in stepOrder)
            {
                if (!(bool)serialSteps[step])
                {
                    ActiveLogicStep++;
                    StartOneStep(step);
                    break; // Only start the first unstarted step
                   
                }
            }
        }

        private void StartOneStep(LogicStep step)
        {
            currentStep = step;
            serialSteps[step] = true;
            step.StartStep();
        }
        
        private void OnFinished()
        {
            // Track cycle completion
            if (cycleInProgress && cycleStartTime > 0)
            {
                float cycleTime = Time.time - cycleStartTime;
                cycleTimes.Add(cycleTime);
                CompletedCycles = cycleTimes.Count;
                UpdateCycleStatistics();
                cycleInProgress = false;
            }
            var keys = new ArrayList(serialSteps.Keys);
            foreach (LogicStep step in keys)
            {
                serialSteps[step] = false; // Setzt alle Schritte auf nicht aktiv zurück
            }
            ActiveLogicStep = 0; // Reset the active step counter
            
            // Normal behavior - proceed to next step after container
            NextStep();
            
        }
        
        public void OnSerialStepFinished(LogicStep step)
        {
            if (serialSteps.ContainsKey(step))
            {
                serialSteps[step] = true;
                bool allFinished = true;
                foreach (var value in serialSteps.Values)
                {
                    if (!(bool)value)
                    {
                        allFinished = false;
                        break;
                    }
                }

                if (allFinished)
                {
                    currentStep = null;
                    OnFinished();
                }
                else
                {
                    // Start the next step in the serial container
                    foreach (LogicStep nextStep in stepOrder)
                    {
                        if (!(bool)serialSteps[nextStep])
                        {
                            ActiveLogicStep++;
                            StartOneStep(nextStep);
                            break; // Only start the first unstarted step
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Step not found in serial container.");
            }
        }
        
        public void FixedUpdate()
        {
            // with per Step
          
            if (StepActive)
            {
                float with = 100 / NumberLogicSteps;
                State = (ActiveLogicStep-1) * with;
                if (currentStep != null)
                {
                    State = State + currentStep.State / 100 * with;
                }
            }
         
        }
        
        private void UpdateCycleStatistics()
        {
            if (cycleTimes.Count == 0) return;
            
            // Calculate min and max
            MinCycleTime = Mathf.Min(cycleTimes.ToArray());
            MaxCycleTime = Mathf.Max(cycleTimes.ToArray());
            
            // Update display fields for NaughtyAttributes
            minCycleTimeDisplay = MinCycleTime;
            maxCycleTimeDisplay = MaxCycleTime;
            
            // Calculate median
            var sortedTimes = new List<float>(cycleTimes);
            sortedTimes.Sort();
            int count = sortedTimes.Count;
            
            if (count % 2 == 0)
            {
                // Even number of elements - average of two middle elements
                MedianCycleTime = (sortedTimes[count / 2 - 1] + sortedTimes[count / 2]) / 2f;
            }
            else
            {
                // Odd number of elements - middle element
                MedianCycleTime = sortedTimes[count / 2];
            }
            
            // Update display fields
            medianCycleTimeDisplay = MedianCycleTime;
            completedCyclesDisplay = CompletedCycles;
        }
        
        // Public method to check if cycle timing is enabled (for UI)
        public bool IsCycleTimeTracking()
        {
            return cycleInProgress || CompletedCycles > 0;
        }
        
        // Public method to reset cycle statistics
        public void ResetCycleStatistics()
        {
            cycleTimes.Clear();
            MinCycleTime = 0f;
            MaxCycleTime = 0f;
            MedianCycleTime = 0f;
            CompletedCycles = 0;
            // Don't reset cycleInProgress or cycleStartTime if currently in a cycle
        }
    }
}