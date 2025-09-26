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
	public class Drive_ContinousDestination: BehaviorInterface {
		private Drive Drive;

		private new void Awake()
		{
			_isIsAtPositionNotNull = SignalIsAtPosition!=null;
		}

		[Header("Continous Destination IO's")] 
		[Tooltip("Target destination position in millimeters")]
		public float Destination = 0;
		[Tooltip("Drive acceleration in mm/s²")]
		public float Acceleration = 100;
		[Tooltip("Maximum drive speed in mm/s")]
		public float TargetSpeed = 100;
		[Tooltip("Scale factor for current position feedback value")]
		public float CurrentPositionScale = 1; //!< Scale factor for the current position feedback

		[Header("PLC IO's")] 
		[Tooltip("PLC output for destination position in millimeters")]
		public PLCOutputFloat SignalDestination; //!<  Destination position of the drive in millimeters
		[Tooltip("PLC output for drive acceleration in mm/s²")]
		public PLCOutputFloat SignalAcceleration; //!< Acceleration of the drive in millimeters / second
		[Tooltip("PLC output for maximum drive speed in mm/s")]
		public PLCOutputFloat SignalTargetSpeed; //!< Target (maximum) speed of the drive in mm/ second
		
		[Tooltip("PLC input for current drive position in millimeters")]
		public PLCInputFloat SignalIsAtPosition; //!< Signal is true if Drive is at destination position
		[Tooltip("PLC input for current drive speed in mm/s")]
		public PLCInputFloat SignalIsAtSpeed; //!<  Signal for current Drive speed in mm / second
		[Tooltip("PLC input signal indicating drive has reached destination")]
		public PLCInputBool SignalIsAtDestination; //!<  Signal if Drive is at Destination
		[Tooltip("PLC input signal indicating if drive is currently moving")]
		public PLCInputBool SignalIsDriving; //!<  Signal is true if Drive is currently driving.
		
		private bool _isStartDriveNotNull;
		private bool _isDestinationNotNull;
		private bool _isTargetSpeedNotNull;
		private bool _isAccelerationNotNull;
		private bool _isIsAtPositionNotNull;
		private bool _isIsAtDestinationNotNull;
		private bool _isIsDrivingNotNull;
		private bool _isIsAtSpeedNotNull;
		private float destinationbefore=0;

		// Use this for initialization
		protected override void OnStartSim()
		{
			_isIsAtSpeedNotNull = SignalIsAtSpeed!=null;
			_isIsDrivingNotNull = SignalIsDriving!=null;
			_isIsAtDestinationNotNull = SignalIsAtDestination!=null;
			_isAccelerationNotNull = SignalAcceleration!=null;
			_isTargetSpeedNotNull = SignalTargetSpeed!=null;
			_isDestinationNotNull = SignalDestination!=null;
			Drive = GetComponent<Drive>();
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			if (ForceStop || !this.enabled)
				return;
			
			// PLC Outputs
			if (_isDestinationNotNull)
				Destination = SignalDestination.Value;
			if (_isTargetSpeedNotNull)
				TargetSpeed = SignalTargetSpeed.Value;
			if (_isAccelerationNotNull)
				Acceleration= SignalAcceleration.Value;

		
			if (Destination!=destinationbefore)
				Drive.DriveTo(Destination);
			Drive.TargetSpeed = TargetSpeed;
			Drive.Acceleration = Acceleration;
			
			
			// PLC Inputs
			if (_isIsAtPositionNotNull)
				SignalIsAtPosition.Value = Drive.CurrentPosition * CurrentPositionScale;
			if (_isIsAtDestinationNotNull)
				SignalIsAtDestination.Value = Drive.IsAtTarget;
			if (_isIsDrivingNotNull)
				SignalIsDriving.Value = Drive.IsRunning;
			if (_isIsAtSpeedNotNull)
				SignalIsAtSpeed.Value = Drive.CurrentSpeed;

			destinationbefore = Destination;
		}
	
	}
}
