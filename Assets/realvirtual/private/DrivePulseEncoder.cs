using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{

    [RequireComponent(typeof(Drive))]
    public class DrivePulseEncoder : BehaviorInterface
    {
        public float PulseIntervall;

        public float Offset = 0;

        public PLCInputBool PulseEncoderSignal;

        private float intervall;
        private Drive parentDrive;
        // Start is called before the first frame update
        void Start()
        {
            intervall = PulseIntervall + Offset;
            parentDrive = GetComponentInParent<Drive>();
          
            PulseEncoderSignal.Value = false;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (parentDrive.CurrentPosition > intervall)
            {
                intervall = intervall + PulseIntervall;
                if (PulseEncoderSignal.Value == true)
                {
                    PulseEncoderSignal.Value = false;
                }
                else
                {
                    PulseEncoderSignal.Value = true;
                }
            }
        }
    }
}
