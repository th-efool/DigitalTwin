// realvirtual.io (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

using UnityEngine;

namespace realvirtual
{
    //! Behavior model which is just connecting an PLCOutput to an PLCInput
    [RequireComponent(typeof(Signal))]
    public class ResetSignal : BehaviorInterface
    {
        [Tooltip("Value to monitor that triggers the reset (true=reset when signal becomes true, false=reset when signal becomes false)")]
        public bool SetToFalse = true;
        [Tooltip("Time delay in seconds before resetting the signal")]
        public float ResetAfterTime = 0.5f;
        private Signal thissignal;
        private bool valuebefore;

        private bool error = false;
        // Start is called before the first frame update
        void Start()
        {
            thissignal = GetComponent<Signal>();
            var thistype = thissignal.GetType();
            if (thistype != typeof(PLCOutputBool) && thistype != typeof(PLCInputBool))
            {
                Error("Reset Signal Component can be only used for Signals of type Bool");
                error = true;
            }

            this.valuebefore = (bool)thissignal.GetValue();
        }

        void ResetValue()
        {
            thissignal.SetValue(!SetToFalse);
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            if (error)
                return;
            
            bool current  = (bool)thissignal.GetValue();
            if (current != valuebefore)
            {
                if (current == SetToFalse)
                {
                    Invoke("ResetValue",ResetAfterTime);
                }
            }
            valuebefore = current;
        }
    }
}

