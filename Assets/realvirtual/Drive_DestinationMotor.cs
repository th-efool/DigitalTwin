// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
	[RequireComponent(typeof(Drive))]
	//! Drive_DestinationMotor provides position-controlled movement for Drive components.
	//! Implements servo-like behavior with target position control, speed regulation, and acceleration management.
	//! Ideal for precise positioning applications like NC axes, robotics, and automated positioning systems.
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
	public class Drive_DestinationMotor: BehaviorInterface, IDriveBehavior
	{
		private Drive Drive;

		[Header("Settings")]
		[Tooltip("Scale factor for current position feedback value")]
		public float CurrentPositionScale = 1; //!< Scale factor for the current position feedback

		private new void Awake()
		{
			_isIsAtPositionNotNull = IsAtPosition!=null;
		}

		[Header("PLC IOs")] 
		[Tooltip("PLC output for current drive speed in mm/s")]
		public PLCOutputFloat Speed; //!< Current Speed of the drive in millimeters / second
		[Tooltip("PLC output signal to start drive movement to destination")]
		public PLCOutputBool StartDrive; //!< Start to drive Signal
		[Tooltip("PLC output for target destination position in millimeters")]
		public PLCOutputFloat Destination; //!<  Destination position of the drive in millimeters
		[Tooltip("PLC output for drive acceleration in mm/s²")]
		public PLCOutputFloat Acceleration; //!< Acceleration of the drive in millimeters / second
		[Tooltip("PLC output for maximum drive speed in mm/s")]
		public PLCOutputFloat TargetSpeed; //!< Target (maximum) speed of the drive in mm/ second
		
		[Tooltip("PLC input for current drive position in millimeters")]
		public PLCInputFloat IsAtPosition; //!< Signal is true if Drive is at destination position
		[Tooltip("PLC input for current drive speed in mm/s")]
		public PLCInputFloat IsAtSpeed; //!<  Signal for current Drive speed in mm / second
		[Tooltip("PLC input signal indicating drive has reached destination")]
		public PLCInputBool IsAtDestination; //!<  Signal if Drive is at Destination
		[Tooltip("PLC input signal indicating if drive is currently moving")]
		public PLCInputBool IsDriving; //!<  Signal is true if Drive is currently driving.
		private bool _isStartDriveNotNull;
		private bool _isDestinationNotNull;
		private bool _isTargetSpeedNotNull;
		private bool _isAccelerationNotNull;
		private bool _isIsAtPositionNotNull;
		private bool _isIsAtDestinationNotNull;
		private bool _isIsDrivingNotNull;
		private bool _isIsAtSpeedNotNull;


		// Use this for initialization
		protected override void OnStartSim() 
		{
			_isIsAtSpeedNotNull = IsAtSpeed!=null;
			_isIsDrivingNotNull = IsDriving!=null;
			_isIsAtDestinationNotNull = IsAtDestination!=null;
			_isAccelerationNotNull = Acceleration!=null;
			_isTargetSpeedNotNull = TargetSpeed!=null;
			_isDestinationNotNull = Destination!=null;
			_isStartDriveNotNull = StartDrive!=null;
			Drive = GetComponent<Drive>();
		}

		// Update is called once per frame
		public void CalcFixedUpdate()
		{
			if (ForceStop || !this.enabled)
				return;
			// PLC Outputs
			if (_isStartDriveNotNull)
				Drive.TargetStartMove = StartDrive.Value;
			if (_isDestinationNotNull)
				Drive.TargetPosition = Destination.Value;
			if (_isTargetSpeedNotNull)
				Drive.TargetSpeed = TargetSpeed.Value;
			if (_isAccelerationNotNull)
				Drive.Acceleration = Acceleration.Value;
		
			// PLC Inputs
			if (_isIsAtPositionNotNull)
				IsAtPosition.Value = Drive.CurrentPosition * CurrentPositionScale;
			if (_isIsAtDestinationNotNull)
				IsAtDestination.Value = Drive.IsAtTarget;
			if (_isIsDrivingNotNull)
				IsDriving.Value = Drive.IsRunning;
			if (_isIsAtSpeedNotNull)
				IsAtSpeed.Value = Drive.CurrentSpeed;
		}
	
	}
}
