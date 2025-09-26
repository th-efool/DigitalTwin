// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(Drive))]
    //! Drive_Cylinder simulates pneumatic or hydraulic cylinder behavior for Drive components.
    //! Provides two-position actuation with configurable extension/retraction times and stroke limits.
    //! Supports both single-bit and dual-bit control modes with position feedback and optional sensor-based stopping.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
    public class Drive_Cylinder : BehaviorInterface, IDriveBehavior
    {

        [Header("Settings")] 
        [Tooltip("Uses single signal for control. When false, cylinder moves in")]
        public bool OneBitCylinder = false; //!< if set to true only one bit is needed to controll the cylinder. If Out=false the cylinder moves in
        [Tooltip("Minimum position in millimeters")]
        public float MinPos = 0; //!< Minimum position in millimeters of the cylinder.
        [Tooltip("Maximum position in millimeters")]
        public float MaxPos = 100; //!< Maximumposition in millimeters of the cylinder.
        [Tooltip("Time to move from minimum to maximum position in seconds")]
        public float TimeOut = 1; //!< Time for moving out from minimum position to maximum position in seconds.
        [Tooltip("Time to move from maximum to minimum position in seconds")]
        public float TimeIn = 1;  //!< Time for moving in from maximum position to minimum position in seconds.
        [Tooltip("Optional sensor to stop cylinder before reaching minimum position")]
        public Sensor StopWhenDrivingToMin; //!< Sensor for stopping the cylinder before reaching the min position (optional)
        [Tooltip("Optional sensor to stop cylinder before reaching maximum position")]
        public Sensor StopWhenDrivingToMax; //!< Sensor for stopping the cylinder before reaching the max position (optional)
            
        [Header("Behavior Signals")] 
        [Tooltip("Moves cylinder to maximum position when true")]
        public bool _out = false; //!< true for moving the cylinder out.
        [HideIf("OneBitCylinder")] 
        [Tooltip("Moves cylinder to minimum position when true")]
        public bool _in = false; //!< true for moving the cylinder in.
        [Tooltip("True when cylinder is at maximum position or stopped by sensor")]
        public bool _isOut = false; //!< is true when cylinder is out or stopped by Max sensor.
        [Tooltip("True when cylinder is at minimum position or stopped by sensor")]
        public bool _isIn = false; //!<  is true when cylinder is in or stopped by Min sensor.
        [Tooltip("True when cylinder is currently moving to maximum position")]
        public bool _movingOut = false; //!<  is true when cylinder is currently moving out
        [Tooltip("True when cylinder is currently moving to minimum position")]
        public bool  _movingIn = false; //!<  is true when cylinder is currently moving in
        [Tooltip("True when cylinder is exactly at maximum position")]
        public bool _isMax = false; //!< is true when cylinder is at maximum position.
        [Tooltip("True when cylinder is exactly at minimum position")]
        public bool _isMin = false; //!< is true when cylinder is at minimum position.
    
        [Header("PLC IOs")] 
        [Tooltip("PLC output signal to move cylinder to maximum position")]
        public PLCOutputBool Out; //!< Signal for moving the cylinder out
        [HideIf("OneBitCylinder")]
        [Tooltip("PLC output signal to move cylinder to minimum position")]
        public PLCOutputBool In; //!< Signal for moving the cylinder in
        [Tooltip("PLC input signal when cylinder is at maximum position")]
        public PLCInputBool IsOut; //!<  Signal when the cylinder is out or stopped by Max sensor.
        [Tooltip("PLC input signal when cylinder is at minimum position")]
        public PLCInputBool IsIn; //!<  Signal when the cylinder is in or stopped by Max sensor.
        [Tooltip("PLC input signal when cylinder is exactly at maximum position")]
        public PLCInputBool IsMax; //!< Signal is true when the cylinder is at Max position.
        [Tooltip("PLC input signal when cylinder is exactly at minimum position")]
        public PLCInputBool IsMin; //!< Signal is true when the cylinder is at Min position.
        [Tooltip("PLC input signal when cylinder is moving to maximum position")]
        public PLCInputBool IsMovingOut; //!<  Signals is true when the cylinder is moving in.
        [Tooltip("PLC input signal when cylinder is moving to minimum position")]
        public PLCInputBool IsMovingIn; //!<  Signal is true when the cylinder is moving out.

        // Event Cylinder Reached Min Position
        public delegate void OnMinDelegate();   //!< Delegate function which is called when cylinder is at Min 
        public event OnMinDelegate EventOnMin;
        // Event Cylinder Reached Max Position
        public delegate void OnMaxDelegate();    //!< Delegate function which is called when cylinder is at Max.
        public event OnMaxDelegate EventOnMax;
        
        private float _oldposition;
        private Drive Cylinder;  
        private bool _oldin, _oldout;
        private bool _isIsInNotNull;
        private bool _isIsOutNotNull;
        private bool _isIsMinNotNull;
        private bool _isIsMaxNotNull;
        private bool _isIsMovingInNotNull;
        private bool _isIsMovingOutNotNull;
        private bool _isStopWhenDrivingToMaxNotNull;
        private bool _isStopWhenDrivingToMinNotNull;
        private bool _isInNotNull;
        private bool _isOutNotNull;

        // Use this for initialization
        protected override void OnStartSim() 
        {
            _isOutNotNull = Out != null;
            _isInNotNull = In != null;
            _isStopWhenDrivingToMinNotNull = StopWhenDrivingToMin != null;
            _isStopWhenDrivingToMaxNotNull = StopWhenDrivingToMax != null;
            _isIsMovingOutNotNull = IsMovingOut != null;
            _isIsMovingInNotNull = IsMovingIn != null;
            _isIsMaxNotNull = IsMax != null;
            _isIsMinNotNull = IsMin != null;
            _isIsOutNotNull = IsOut != null;
            _isIsInNotNull = IsIn != null;
         
        }

        private void Start()
        {
            Cylinder = GetComponent<Drive>();
            Cylinder.CurrentPosition = MinPos;
            _isMin = false;
            _isMax = false;
            _isIn = false;
            _isOut = false;
            _movingIn = false;
            _movingOut = false;
        }

        // Update is called once per frame
        public void CalcFixedUpdate()
        {
            if (ForceStop || !this.enabled)
                return;
            // Get external Signals
            if (_isInNotNull)
                _in = In.Value;
            if (_isOutNotNull)
                _out = Out.Value;
   
            // Moving Stopped at Min or Maxpos
            if (_movingOut && Cylinder.IsPosition == MaxPos)
            {
                _movingOut = false;
                _movingIn = false;
                _isOut = true;
            }
            if (_movingIn && Cylinder.IsPosition == MinPos)
            {
                _movingIn = false;
                _movingOut = false;
                _isIn = true;
            }
        
            // Stop on Collision
            if (_isStopWhenDrivingToMinNotNull && _movingIn)
            {
                if (StopWhenDrivingToMin.Occupied)
                {
                    _movingIn = false;
                    _movingOut = false;
                    _isIn = true;
                    Cylinder.Stop();
                }
            }
            if (_isStopWhenDrivingToMaxNotNull && _movingOut)
            {
                if (StopWhenDrivingToMax.Occupied)
                {
                    _movingIn = false;
                    _movingOut = false;
                    _isOut = true;
                    Cylinder.Stop();
                }
            }
        
            // At Maxpos
            if (Cylinder.CurrentPosition == MaxPos)
                _isMax = true;
            else
                _isMax = false;

            // At Minpos
            if (Cylinder.CurrentPosition == MinPos)
                _isMin = true;
            else
                _isMin = false;
        
            // EventMaxPos
            if (Cylinder.IsPosition == MaxPos && _oldposition != Cylinder.IsPosition && EventOnMax != null)
                EventOnMax();
        
            // EventMinPos
            if (Cylinder.IsPosition == MinPos && _oldposition != Cylinder.IsPosition && EventOnMin != null)
                EventOnMin();

        
            // Start to Move Cylinder
            if (!(_out && _in) && !OneBitCylinder)
            {
                if (_out && !_isOut && !_movingOut)
                {
                    Cylinder.TargetSpeed = Math.Abs(MaxPos - MinPos) / TimeOut;
                    Cylinder.DriveTo(MaxPos);
                    _movingOut = true;
                    _movingIn = false;
                    _isIn = false;
                }
                if (_in && !_isIn && !_movingIn)
                {
                    Cylinder.TargetSpeed = Math.Abs(MaxPos - MinPos) / TimeIn;
                    Cylinder.DriveTo(MinPos);
                    _isOut = false;
                    _movingIn = true;
                    _movingOut = false;
                }
            }
            else
            {
                if (_out && !_isOut && !_movingOut)
                {
                    Cylinder.TargetSpeed = Math.Abs(MaxPos - MinPos) / TimeOut;
                    Cylinder.DriveTo(MaxPos);
                    _movingOut = true;
                    _movingIn = false;
                    _isIn = false;
                }
                
                if (!_out && !_isIn && !_movingIn)
                {
                    Cylinder.TargetSpeed = Math.Abs(MaxPos - MinPos) / TimeIn;
                    Cylinder.DriveTo(MinPos);
                    _isOut = false;
                    _movingIn = true;
                    _movingOut = false;
                }
            }
            
        
            // Set external Signals
            if (_isIsInNotNull)
                IsIn.Value = _isIn;
            if (_isIsOutNotNull)
                IsOut.Value = _isOut;
            if (_isIsMinNotNull)
                IsMin.Value = _isMin;
            if (_isIsMaxNotNull)
                IsMax.Value = _isMax;
            if (_isIsMovingInNotNull)
                IsMovingIn.Value = _movingIn;
            if (_isIsMovingOutNotNull)
                IsMovingOut.Value = _movingOut;

            _oldposition = Cylinder.IsPosition;
            _oldout = _out;
            _oldin = _in;
        }
    }
}