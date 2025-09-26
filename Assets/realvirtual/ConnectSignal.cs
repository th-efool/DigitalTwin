// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
    //! Behavior model which is just connecting an PLCOutput to an PLCInput
    public class ConnectSignal : BehaviorInterface
    {
        [Tooltip("Signal to connect to this signal (value will be copied from connected signal)")]
        public Signal ConnectedSignal;
     
        private bool connectedsignalnotnull;

        private Signal thissignal;
    
        // Start is called before the first frame update
        void Start()
        {
            thissignal = GetComponent<Signal>();
            connectedsignalnotnull = thissignal != null;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (connectedsignalnotnull)
            {
                object value = ConnectedSignal.GetValue();
                var fromtype = value.GetType();
                var totypeval = thissignal.GetValue();
                var totype = totypeval.GetType();
                /// Special Type Conversions
                if (fromtype==typeof(int) && totype == typeof(bool))
                {
                    if ((int)value > 0)
                        thissignal.SetValue(true);
                    else
                        thissignal.SetValue(false);
                    return;
                }
                if (fromtype==typeof(bool) && totype == typeof(int))
                {
                    if ((bool)value)
                        thissignal.SetValue(1);
                    else
                        thissignal.SetValue(0);
                    return;
                } 
                // General Type Conversion
                thissignal.SetValue(ConnectedSignal.GetValue());
            }
        }
    }
}

