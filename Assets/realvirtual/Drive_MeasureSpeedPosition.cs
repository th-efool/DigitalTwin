// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(Drive))]
    //! Behavior model of a cylinder movement which can be connected to a Drive.
    //! The cylinder is defined by a maximum (*MaxPos*) and minimum (*MinPos*) position in millimeter
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
    public class Drive_MeasureSpeedPosition : BehaviorInterface
    {
        [Header("Settings")]
        [Tooltip("Scale factor for current position feedback value")]
        public float CurrentPositionScale = 1; //!< Scale factor for the current position feedback
       
        [Header("PLC IOs")] 
        [Tooltip("PLC input for measured drive speed in mm/s")]
        public PLCInputFloat Speed; //!< PLCOutput for the speed of the drive in millimeter / second, can be scaled by Scale factor.
        [Tooltip("PLC input for measured drive acceleration in mm/s²")]
        public PLCInputFloat Accelaration; //!< PLCOutput for the speed of the drive in millimeter / second, can be scaled by Scale factor.
        [Tooltip("PLC input for measured drive position in millimeters")]
        public PLCInputFloat Position;
        private Drive Drive;
        private bool _isSpeedNotNull;
        private bool _isPositionNotNull;
        private bool _isAccelerationNotNull;

        // Use this for initialization
        void Start()
        {
            _isSpeedNotNull = Speed != null;
            _isAccelerationNotNull = Accelaration != null;
            _isPositionNotNull = Position != null;
            Drive = GetComponent<Drive>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Set PLCInputs
            if (_isSpeedNotNull)
                Speed.Value = Drive.CurrentSpeed;
            if (_isAccelerationNotNull)
                Accelaration.Value = Drive.Acceleration;
            if (_isPositionNotNull)
                Position.Value = Drive.CurrentPosition * CurrentPositionScale;
        }
    }
}