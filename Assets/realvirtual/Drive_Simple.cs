// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(Drive))]
    //! Drive_Simple provides basic bidirectional control for Drive components with PLC integration.
    //! Enables forward/backward movement control with speed and acceleration settings through PLC signals.
    //! Includes position and speed feedback for closed-loop control in automation systems.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
    public class Drive_Simple : BehaviorInterface, IDriveBehavior
    {
       
        [Header("Settings")] 
        [Tooltip("Scale factor for input/output speed and acceleration values")]
        public float ScaleSpeed = 1;  //!< Scale factor for the input and output speed and acceleration
        [Tooltip("Scale factor for current position feedback value")]
        public float CurrentPositionScale = 1; //!< Scale factor for the current position feedback

        [Header("PLC IOs")] 
        [Tooltip("PLC output for drive speed in mm/s (affected by ScaleSpeed)")]
        public PLCOutputFloat Speed; //!< PLCOutput for the speed of the drive in millimeter / second, can be scaled by Scale factor.
        [Tooltip("PLC output for drive acceleration in mm/s² (affected by ScaleSpeed)")]
        public PLCOutputFloat Accelaration; //!< PLCOutput for the speed of the drive in millimeter / second, can be scaled by Scale factor.
        [Tooltip("PLC output signal to move drive forward")]
        public PLCOutputBool Forward; //!< Signal to move the drive forward
        [Tooltip("PLC output signal to move drive backward")]
        public PLCOutputBool Backward; //!< Signal to move the drive backward
        [Tooltip("PLC input for current drive position in millimeters")]
        public PLCInputFloat IsAtPosition; //!< Signal for current position of the drive (in millimeter).
        [Tooltip("PLC input for current drive speed in mm/s")]
        public PLCInputFloat IsAtSpeed; //!< Signal for current speed of the drive (in millimeter/s).
        [Tooltip("PLC input signal indicating if drive is currently moving")]
        public PLCInputBool IsDriving; //!< Signal is true if Drive is driving.

        private Drive Drive;
        private bool _isSpeedNotNull;
        private bool _isIsAtPositionNotNull;
        private bool _isForwardNotNull;
        private bool _isBackwardNotNull;
        private bool _isIsDrivingNotNull;
        private bool _isIsAtSpeedNotNull;
        private bool _isAccelerationNotNull;
        
        
        //!  is called when simulation starts
        protected override void OnStartSim() 
        {
            _isIsDrivingNotNull = IsDriving != null;
            _isBackwardNotNull = Backward != null;
            _isForwardNotNull = Forward != null;
            _isIsAtPositionNotNull = IsAtPosition != null;
            _isIsAtSpeedNotNull = IsAtSpeed != null;
            _isSpeedNotNull = Speed != null;
            _isAccelerationNotNull = Accelaration != null;
            Drive = GetComponent<Drive>();
        }

        // Update is called once per frame
        public void CalcFixedUpdate()
        {
            if (ForceStop || !this.enabled)
                return;
            // Get external PLC Outputs
            if (_isSpeedNotNull)
                Drive.TargetSpeed  = Speed.Value* ScaleSpeed;
            if (_isForwardNotNull)
                Drive.JogForward = Forward.Value;
            if (_isBackwardNotNull)
                Drive.JogBackward = Backward.Value;
            if (_isAccelerationNotNull)
                Drive.Acceleration = Accelaration.Value*ScaleSpeed;
        
            // Set external PLC Outpits
            if (_isIsAtPositionNotNull)
                IsAtPosition.Value = (Drive.CurrentPosition/ ScaleSpeed) * CurrentPositionScale;
            if (_isIsAtSpeedNotNull)
                IsAtSpeed.Value = Drive.CurrentSpeed/ ScaleSpeed ;
            if (_isIsDrivingNotNull)
                IsDriving.Value = Drive.IsRunning;
        }
    }
}