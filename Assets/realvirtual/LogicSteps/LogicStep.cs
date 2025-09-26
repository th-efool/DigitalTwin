// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! Base class for all logic steps that define sequential or parallel automation logic in realvirtual.
    //! Logic steps can be chained together to create complex automation sequences.
    //! Steps are executed based on their order in the GameObject hierarchy.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public abstract class LogicStep : MonoBehaviour
    {
        [Header("Logic Step Configuration")]
        [InfoBox("Use the GameObject hierarchy to control execution order", EInfoBoxType.Normal)]
        public string Name; //!< Name identifier for this logic step, used for jumps and debugging
        
        [Header("Runtime State")]
        [ShowIf("IsShowingRuntimeInfo")]
        [ProgressBar("State", 100, EColor.Green)]
        public float State = 0; //!< Current execution state as percentage (0-100)
        
        [ShowIf("IsShowingRuntimeInfo")]
        [ReadOnly]
        [Label("Active")]
        public bool StepActive = false; //!< True when this step is currently executing
        
        [ShowIf("IsShowingRuntimeInfo")]
        [ReadOnly]
        [Label("Waiting")]
        public bool IsWaiting = false; //!< True when this step is waiting for a condition
        
        [HideInInspector] public bool IsContainer = false; //!< True if this step is a container for other steps
        
        // Helper property for ShowIf
        protected bool IsShowingRuntimeInfo => Application.isPlaying && (StepActive || IsWaiting);
        private LogicStep nextstep;
        private LogicStep_ParallelContainer parallelContainer;
        private LogicStep_SerialContainer serialContainer;
        
        // Needs to be implemented
        protected abstract void OnStarted();
        
        protected virtual void OnStartInit()
        {
            // Default implementation does nothing
        }
        
        // Is called to proceed to next step
        protected void NextStep()
        {
            IsWaiting = false;
            State = 0;
            StepActive = false;
            
            // special handling for parallel container
            if (parallelContainer != null)
            {
                parallelContainer.OnParallelStepFinished(this);
                return;
            }
            
            // special handling for serial container
            if (serialContainer != null)
            {
                serialContainer.OnSerialStepFinished(this);
                return;
            }
            
            // special handling for serial container
            
            
            if (nextstep!=null)
                nextstep.StartStep();
            else
                Invoke("NextStep",0.1f);
        }

        protected bool NonBlocking()
        {
            return false;
        }
        
        // Is called to proceed to next step with certain name (jump)
        protected void NextStep(string name)
        {
            var steps = GetComponents<LogicStep>();
            foreach (var step in steps)
            {
                if (step.Name == name)
                    step.StartStep();
                return;
            }
        }

        //! Starts the execution of this logic step.
        public void StartStep()
        {
            State = 100;
            StepActive = true;
            OnStarted();
        }

        protected void Start()
        {
            // in parent parallel or serial container in direct parent
            // get paren
            var parent = transform.parent;
            if (parent != null)
            {
                parallelContainer = parent.GetComponent<LogicStep_ParallelContainer>();
                serialContainer = parent.GetComponent<LogicStep_SerialContainer>();
            }
            
            StepActive = false;
            var tostart = false;
            if (parallelContainer != null || serialContainer != null)
            {
                // if not in parallel or serial container, get next step
                nextstep = null;
            }
            else
            {
                var steps = GetComponents<LogicStep>();
                for (int i = 0; i < steps.Length; i++)
                {
                    if (steps[i] == this)
                    {
                        if (i == 0)
                            tostart = true;
                        if (i < steps.Length - 1)
                        {
                            nextstep = steps[i + 1];

                        }
                        else
                        {
                            nextstep = steps[0];
                        }
                    }
                }
            }

            OnStartInit();
            
            if (tostart)
                StartStep();
            
        }
    }
}
