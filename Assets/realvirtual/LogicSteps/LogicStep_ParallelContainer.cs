// // realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// // (c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Collections;
using UnityEngine;

namespace realvirtual
{
    //! Logic step container that executes all child logic steps in parallel.
    //! This container starts all direct child logic steps simultaneously and waits for all to complete before proceeding.
    //! Progress is displayed as the minimum progress of all parallel steps. Useful for concurrent automation tasks.
    public class LogicStep_ParallelContainer : LogicStep
    {
        [Header("Parallel Container Settings")]
        public bool DebugMode; //!< Enables debug logging for parallel step execution
        
        Hashtable parallelSteps = new Hashtable();
        
        protected override void OnStartInit()
        {
            IsContainer = true;
            // get all logic steps in this container but only those exactly one level below
            var steps = GetComponentsInChildren<LogicStep>(false);
            foreach (var step in steps)
            {
                if (step.transform.parent == transform)
                {
                    parallelSteps.Add(step, false);
                }
            }
            
         
        }

        protected override void OnStarted()
        {
            // Start all steps in parallel
            var steps = new ArrayList(parallelSteps.Keys); // Kopie der Keys
            foreach (LogicStep step in steps)
            {
                if (DebugMode) 
                {
                    Debug.Log($"Starting parallel step: {step.gameObject.name}  / {step.Name}");
                }
                step.StartStep();
            }
            
        }
        
        public void OnParallelStepFinished(LogicStep step)
        {
            if (parallelSteps.ContainsKey(step))
            {
                parallelSteps[step] = true;
                bool allFinished = true;
                foreach (var value in parallelSteps.Values)
                {
                    if (!(bool)value)
                    {
                        allFinished = false;
                        break;
                    }
                }

                if (allFinished)
                {
                    if (DebugMode) 
                    {
                        Debug.Log("All parallel steps finished.");
                    }
                    OnAllFinished();
                }
                else
                {
                    if (DebugMode) 
                    {
                        Debug.Log($"One parallel step finished: {step.gameObject.name}  / {step.Name}");
                    }
                }
            }
            else
            {
                Debug.LogError("Step not found in parallel steps: " + step.Name);
            }
        }
        
        private void OnAllFinished()
        {
            // Reset all steps to not active
            var keys = new ArrayList(parallelSteps.Keys);
            foreach (LogicStep step in keys)
            {
                parallelSteps[step] = false; // Set all steps to not active
            }
            NextStep();
        }
        
        
        public void FixedUpdate()
        {
        
            // Minimum aller parallelSteps berechnen, aber 0, wenn alle 0 sind
            float min = float.MaxValue;
            bool anyActive = false;
            foreach (LogicStep step in parallelSteps.Keys)
            {
                if (step.State > 0)
                {
                    if (step.State < min)
                        min = step.State;
                    anyActive = true;
                }
            }
            State = anyActive ? min : 0f;

        }
    }
}