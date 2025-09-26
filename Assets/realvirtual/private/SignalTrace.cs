// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace realvirtual
{
    [CreateAssetMenu(fileName = "SignalTrace", menuName = "realvirtual/Add SignalTrace", order = 1)]
    //! Scriptable object for saving camera positions (user views)
    public class SignalTrace : ScriptableObject
    {
        [FormerlySerializedAs("Lock")] public bool DontSave = false;
        public List<string> TracedSignals=new List<string>();
        public List<AnimationCurve> TracedCurves = new List<AnimationCurve>();
            
        // adding a signal to the traced signals
        public AnimationCurve InitValue(string signalname)
        {
            AnimationCurve curve = null;
            if (!TracedSignals.Contains(signalname))
            {
                TracedSignals.Add(signalname);
                curve = new AnimationCurve();
                TracedCurves.Add(curve);
            }
            else
            {
                var index = TracedSignals.IndexOf(signalname);
                curve = TracedCurves[index];
                curve.ClearKeys();
            }
#if UNITY_EDITOR
            if (!DontSave)
                EditorUtility.SetDirty(this);
#endif
            return curve;
        }
        
        
        public void TraceValue(string signalname, float value)
        {
             TraceValue(signalname, Time.fixedTime, value);
        }
        
        // adding a value to the signal
        public void TraceValue(string signalname, float time, float value)
        {
            if (TracedSignals.Contains(signalname))
            {
                var index = TracedSignals.IndexOf(signalname);
                // write an error if it is not existing
                if (index < 0)
                {
                    Debug.LogWarning("SignalTrace: Signal " + signalname + " not found");
                }
              
                
                // check if curve has already 1 key
                var curve = TracedCurves[index];
                curve.AddKey(time,value);
                if (curve.length > 2)
                {
                    #if UNITY_EDITOR
                    AnimationUtility.SetKeyLeftTangentMode(curve, curve.length-2, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, curve.length-2, AnimationUtility.TangentMode.Linear);
                    #endif
                }
                
            }
            else
            {
                Debug.LogWarning("SignalTrace: Signal " + signalname + " not found");
            }
            #if UNITY_EDITOR
            if (!DontSave)
                EditorUtility.SetDirty(this);
            #endif
        }
        
        // compare two curves with each other
        public float CompareCurves(string curve1, string curve2)
        {
            var index1 = TracedSignals.IndexOf(curve1);
            var index2 = TracedSignals.IndexOf(curve2);
            if (index1 < 0 || index2 < 0)
            {
                Debug.LogWarning("SignalTrace: Signal " + curve1 + " or " + curve2 + " not found");
                return 0;
            }
            var curve1keys = TracedCurves[index1].keys;
            var curve2keys = TracedCurves[index2].keys;
           
            float error = 0;
            for (int i = 0; i < curve1keys.Length; i++)
            {
                if (i<curve2keys.Length)
                     error += Mathf.Abs(curve1keys[i].value - curve2keys[i].value);
            }
            return error;
        }
        
        // Saves all traces to a reference
        public void CopyTracesTo(string reference)
        {
            foreach (var signal in TracedSignals.ToArray())
            {
                // check if reference is at the end of the signal name
                if (!signal.EndsWith(reference))
                {
                    // copy this signal to a reference
                    var originalcurve = TracedCurves[TracedSignals.IndexOf(signal)];
                    var referencecurve = InitValue(signal+reference);
                    referencecurve.ClearKeys();
                    // copy the content of the curve to the reference curve
                    foreach (var key in originalcurve.keys)
                    {
                        referencecurve.AddKey(key);
                    }
                    
                }
            }
        }
    }

}

