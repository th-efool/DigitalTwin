// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
    //! Records signal values over time into an AnimationCurve for visualization and analysis.
    //! Automatically captures signal changes at each fixed update and stores them in an editable AnimationCurve.
    //! Useful for debugging signal behavior, creating test patterns, or analyzing system dynamics.
    public class SignalChart : BehaviorInterface
    {
        [Tooltip("AnimationCurve storing the recorded signal values over time")]
        public AnimationCurve Chart; //!< AnimationCurve storing the recorded signal values over time
        [Tooltip("Delay in seconds before recording starts")]
        public float RecordAfterSeconds = 2; //!< Delay in seconds before recording starts
        [Tooltip("Enable or disable signal recording")]
        public bool Record = true; //!< Enables or disables signal recording
        private Signal thissignal;
        private bool signalnotnull;
        
        // Start is called before the first frame update
        void Start()
        {
           

            thissignal = GetComponent<Signal>();
            signalnotnull = thissignal != null;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (signalnotnull)
            {
                if (Record && Time.fixedTime>= RecordAfterSeconds )
                Chart.AddKey(Time.fixedTime, (float) thissignal.GetValue());
            }
            
        }
    }
}

