using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    public class MeasurePLCCycleTime : MonoBehaviour
    {
        [Tooltip("PLC output signal that should be incremented by the PLC each cycle")]
        public PLCOutputInt SignalPLCCycleCounter;
        
        [Tooltip("PLC input signal for Unity's cycle counter")]
        public PLCInputInt SignalUnityCylceCounter;
        
        [Tooltip("PLC input signal for Unity's time in milliseconds")]
        public PLCInputInt SignalUnityTimeMs;
        [ReadOnly]public int UnityCycleCounter;
        [InfoBox("The PLC Cycle Counter which should be incremented in the PLC each cycle by 1")]
        [ReadOnly]public int PLCCycleCounter;
        [InfoBox("The PLC Cycle measured by the medium time between 100 increments of the cycle counter")]
        [ReadOnly]public float PLCCycleTimeMs;
        [InfoBox("The time measured between two changes of the PLC cycle counter in Unity Time")]
        [ReadOnly]public float CommCycleTimeMs;
        [InfoBox("The minimum time measured between two changes of the PLC cycle counter in Unity Time over 1000 cycles ")]
        [ReadOnly]public float MinCommCycleTimeMs;
        [InfoBox("The medium time measured between two changes of the PLC cycle counter in Unity Time over 1000 cycles")]
        [ReadOnly]public float MedCommCycleTimeMs;  
        [InfoBox("The maximum time measured between two changes of the PLC cycle counter in Unity Time over 1000 cycles")]
        [ReadOnly]public float MaxCommCycleTimeMs;  
        [ReadOnly]public int UnityTimeMs;
        
        
        [HideInInspector] public float NumMeasures = 1000;
    
     

        private float lastcyclenum;
        private float startmeasure;
        private float lastchangecyclenum;
        private float startmeasuretime;
        private float sumcycletime;
        private float nummeasures;

        // Update is called once per frame
        void FixedUpdate()
        {
            UnityCycleCounter++;
            
            if (SignalUnityCylceCounter != null)
            {
                SignalUnityCylceCounter.Value = UnityCycleCounter;
            }
            
            if (SignalUnityTimeMs != null)
            {
                SignalUnityTimeMs.Value = (int)(Time.fixedTime * 1000);
                UnityTimeMs = SignalUnityTimeMs.Value;
            }

            if (SignalPLCCycleCounter != null)
            {
                var currycle = SignalPLCCycleCounter.Value;
                PLCCycleCounter = SignalPLCCycleCounter.Value;
                
                if (startmeasure == 0)
                {
                    startmeasure = PLCCycleCounter;
                    startmeasuretime = Time.fixedTime;
                }

                if (currycle - startmeasure > NumMeasures)
                {
                    var deltatime = Time.fixedTime - startmeasuretime;
                    var numcycles = currycle - startmeasure;
                    PLCCycleTimeMs = deltatime / numcycles * 1000;
                    startmeasure = currycle;
                    startmeasuretime = Time.fixedTime;
                    MinCommCycleTimeMs = 999999;
                    MaxCommCycleTimeMs = 0;
                    sumcycletime = 0;
                    nummeasures = 0;
                }

                // check value changed
                if (currycle != lastcyclenum)
                {
                    nummeasures++;
                    var deltatime = Time.fixedTime - lastchangecyclenum;
                    CommCycleTimeMs = deltatime * 1000;
                    if (CommCycleTimeMs < MinCommCycleTimeMs)
                    {
                        MinCommCycleTimeMs = CommCycleTimeMs;
                    }
                    if (CommCycleTimeMs > MaxCommCycleTimeMs)
                    {
                        MaxCommCycleTimeMs = CommCycleTimeMs;
                    }
                    sumcycletime = sumcycletime + CommCycleTimeMs;
                    MedCommCycleTimeMs = sumcycletime / nummeasures;
                    lastchangecyclenum = Time.fixedTime;
                }

                lastcyclenum = currycle;
            }
        }
    }
}

