
﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using UnityEngine;

namespace realvirtual
{
	[RequireComponent(typeof(Drive))]
	//! Behavior model of a drive where the drive is exactly following the current given position of the PLC
	//! This is special useful for connecting motion controllers and robot controllers torealvirtual.
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
	public class Drive_FollowPosition : BehaviorInterface, IDriveBehavior
	{
		[Header("Settings")]
		[Tooltip("Position offset in millimeters added to input signal")]
		public float Offset = 0; //!< Offset in millimeter which is added to the position signal
		[Tooltip("Scale factor applied to position input signal")]
		public float Scale = 1; //!<  Scale factor which is scaling the position value
		[Tooltip("Scale factor for current position feedback value")]
		public float CurrentPositionScale = 1; //!< Scale factor for the current position feedback

		[Header("PLC IOs")] 
		[Tooltip("PLC output signal defining target drive position in millimeters")]
		public PLCOutputFloat Position; //!< Signal (PLCOutput) for the defined position of the drive
		[Tooltip("PLC input for current drive position in millimeters (before scale/offset)")]
		public PLCInputFloat CurrentPosition; //!< PLCInput for the current position of the drive (without offset and scaling)
		
		private Drive Drive;
		private bool _isPositionNotNull;
		private bool _isCurrentPositionNotNull;
		
		
		 
		protected override void OnStartSim()
		{
			Drive = GetComponent<Drive>();
			_isPositionNotNull = Position != null;
			_isCurrentPositionNotNull = CurrentPosition != null;
		}

		// Update is called once per frame
		public void CalcFixedUpdate()
		{
			if (ForceStop || !this.enabled)
				return;
			// Get external Signals
			if (_isPositionNotNull)
				Drive.SetPosition(Position.Value * Scale + Offset);
		
			// Set external Signals
			if (_isCurrentPositionNotNull)
				CurrentPosition.Value = Drive.CurrentPosition * CurrentPositionScale;
		}
		
	}
}
