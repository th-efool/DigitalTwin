using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace realvirtual
{
    //! Records and traces signal values over time for analysis and debugging purposes.
    //! Supports tracing of bool, int, and float signals with automatic value recording at each fixed update.
    //! Traces can be saved as references and compared for testing or optimization purposes.
    public class SignalTracer : MonoBehaviour
    {
        [Tooltip("List of signals to trace and record over time")]
        public List<Signal> TracedSignals = new List<Signal>(); //!< List of signals to trace and record
        [Tooltip("ScriptableObject asset for storing the recorded trace data")]
        public SignalTrace SignalTrace; //!< ScriptableObject asset for storing the recorded trace data
        [Tooltip("Automatically start tracing when the component starts")]
        public bool TraceAtStart = true; //!< If true, automatically starts tracing when the component starts


        private InterfaceSignal signaltype;
        private Hashtable signalValueTable = new Hashtable();
       
        
        private bool traceActive = false;

        void Start()
        {
            if (TraceAtStart)
                StartTrace();
        }
        
        
        //! Initializes a trace for a named value. Must be called before tracing the value.
        public void InitValue(string valuename)
        {
            if (SignalTrace == null)
            {
                Debug.LogError(
                    "SignalTracer is not able to trace the signal because no Scriptable Object (SignalTrace) for recording is assinged");
                return;
            }
            SignalTrace.InitValue(valuename);
        }

        //! Records a value at the current time in the signal trace.
        public void TraceValue(string valuename, float value)
        {
            // trace the value
            SignalTrace.TraceValue(valuename, value);
        }
        
        //! Records a value at a specific time in the signal trace.
        public void TraceValue(string valuename, float time, float value)
        {
            // trace the value
            SignalTrace.TraceValue(valuename, time, value);
        }
        
        //! Saves the current traces as a reference for later comparison.
        public void CopyTracesToReference(string reference)
        {
            if (SignalTrace == null)
            {
                Debug.LogError(
                    "SignalTracer is not able to trace the signal because no Scriptable Object (SignalTrace) for recording is assinged");
                return;
            }
            SignalTrace.CopyTracesTo( reference);
        }
        
        //! Compares two signal curves and returns the difference.
        public float CompareCurves(string curve1, string curve2, float tolerance)
        {
            return SignalTrace.CompareCurves(curve1, curve2);
        }


        //! Starts recording signal values for all signals in the TracedSignals list.
        [Button("Start Trace")] //call method ShowCurves
        public void StartTrace()
        {
            if (SignalTrace == null)
            {
                Debug.LogError(
                    "SignalTracer is not able to trace the signal because no Scriptable Object (SignalTrace) for recording is assinged");
                return;
            }
            
            if (TracedSignals.Count > 0)
            {
                foreach (Signal signal in TracedSignals)
                {
                    signalValueTable.Add(signal.gameObject,Global.GetPath(signal.gameObject));
                }
                traceActive = true;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (SignalTrace.TracedSignals.Count > 0 && traceActive)
            {
                foreach (var signal in TracedSignals)
                {
                    signaltype = signal.GetInterfaceSignal();

                    float value = 0f;
                    switch (signaltype.Type)
                    {
                        case InterfaceSignal.TYPE.INT:
                        {
                            value = (float) signal.GetValue();
                            break;
                        }
                        case InterfaceSignal.TYPE.BOOL:
                        {
                            if ((bool) signal.GetValue())
                            {
                                value = 1f;
                            }
                            else
                            {
                                value = 0f;
                            }

                            break;
                        }
                        case InterfaceSignal.TYPE.REAL:
                        {
                            value = (float) signal.GetValue();
                            break;
                        }
                    }
                    var signalname = signalValueTable[signal].ToString();
                    TraceValue(signalname, value);
                }
            }
        }
    }
}