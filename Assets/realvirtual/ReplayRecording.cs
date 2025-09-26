
using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(DrivesRecorder))]
    public class ReplayRecording : realvirtualBehavior,ISignalInterface
    {
        public string Sequence;
        public PLCOutputBool StartOnSignal;
        public PLCInputBool IsReplayingSignal;
        
        [ReadOnly] public bool IsReplaying = false;
        private DrivesRecorder _drivesRecorder;
        private bool oldStartOnSignal;
        // Start is called before the first frame update
        void Start()
        {
            _drivesRecorder = GetComponent<DrivesRecorder>();
        }




        // Update is called once per frame
        void FixedUpdate()
        {
            if (!IsReplaying && StartOnSignal != null && StartOnSignal.Value && !oldStartOnSignal) // Only Start on Positive Flank
            {
                _drivesRecorder.StartReplay(Sequence);
                IsReplaying = true;
            }
            
            if (IsReplaying)
            {
                if (!_drivesRecorder.Replaying)
                {
                    IsReplaying = false;
                }
            }
     
            if (IsReplayingSignal != null)
            {
                    IsReplayingSignal.Value = IsReplaying;
            }
            
            if (StartOnSignal != null)
                    oldStartOnSignal = StartOnSignal.Value;
            
        }
    }

}
