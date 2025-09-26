// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
	[RequireComponent(typeof(Drive))]
	//! Behavior model of an intelligent drive which is getting a destination and moving to the destination.
	//! This component needs to have as a basis a standard Drive.
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
	public class Drive_Speed: BehaviorInterface, IDriveBehavior 
	{
		private Drive Drive;

		[Header("Continous Destination IO's")] 
		[Tooltip("Target speed in mm/s (positive=forward, negative=backward)")]
		public float TargetSpeed = 100;
		[Tooltip("Drive acceleration in mm/s²")]
		public float Acceleration = 100;
		[Tooltip("Scale factor for current position feedback value")]
		public float CurrentPositionScale = 1; //!< Scale factor for the current position feedback
	

		[Header("PLC IO's")]
		[Tooltip("PLC output for drive acceleration in mm/s²")]
		public PLCOutputFloat SignalAcceleration; //!< Acceleration of the drive in millimeters / second
		[Tooltip("PLC output for target speed in mm/s")]
		public PLCOutputFloat SignalTargetSpeed; //!< Target (maximum) speed of the drive in mm/ second
		
		[Tooltip("PLC input for current drive speed in mm/s")]
		public PLCInputFloat SignalCurrentSpeed; //!<  Signal for current Drive speed in mm / second
		[Tooltip("PLC input for current drive position in millimeters")]
		public PLCInputFloat SignalCurrentPosition;  //!<  Signal for current Drive positon in mm 
		[Tooltip("PLC input signal indicating if drive is currently moving")]
		public PLCInputBool SignalIsDriving; //!<  Signal is true if Drive is currently driving.
		
		private bool _isStartDriveNotNull;
		private bool _isDestinationNotNull;
		private bool _isTargetSpeedNotNull;
		private bool _isAccelerationNotNull;
		private bool _isIsAtPositionNotNull;
		private bool _isIsAtDestinationNotNull;
		private bool _isCurrentPositionNotNull;
		private bool _isIsDrivingNotNull;
		private bool _isCurrentSpeedNotNull;

		// Use this for initialization
		protected override void OnStartSim()
		{
			_isCurrentSpeedNotNull = SignalCurrentSpeed!=null;
			_isIsDrivingNotNull = SignalIsDriving!=null;
			_isCurrentPositionNotNull = SignalCurrentPosition!=null;
			_isAccelerationNotNull = SignalAcceleration!=null;
			_isTargetSpeedNotNull = SignalTargetSpeed!=null;
			Drive = GetComponent<Drive>();
		}

		// Update is called once per frame
		public void CalcFixedUpdate()
		{
			if (ForceStop || !this.enabled)
				return;
			
			// PLC Outputs
			if (_isTargetSpeedNotNull)
				TargetSpeed = SignalTargetSpeed.Value;
			if (_isAccelerationNotNull)
				Acceleration= SignalAcceleration.Value;
			
			
			Drive.TargetSpeed = Mathf.Abs(TargetSpeed);
			if (TargetSpeed > 0)
			{
				Drive.JogForward = true;
				Drive.JogBackward = false;
			}

			if (TargetSpeed == 0)
			{
				Drive.JogForward = false;
				Drive.JogBackward = false;
			}
			
			if (TargetSpeed <0)
			{
				Drive.JogForward = false;
				Drive.JogBackward = true;
			}
				
			Drive.Acceleration = Acceleration;
			
			
			// PLC Inputs
			if (_isIsDrivingNotNull)
				SignalIsDriving.Value = Drive.IsRunning;
			if (_isCurrentSpeedNotNull)
				SignalCurrentSpeed.Value = Drive.CurrentSpeed;
			if (_isCurrentPositionNotNull)
				SignalCurrentPosition.Value = Drive.CurrentPosition * CurrentPositionScale;

		}
	
	}
}
