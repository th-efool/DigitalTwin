// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
	[RequireComponent(typeof(Drive))]
	//! This drive is only for test purposes. It is moving constantly two erratic positions between MinPos and MaxPos. 
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
	public class Drive_ErraticPosition : BehaviorInterface, IDriveBehavior
	{
		[Tooltip("Minimum position in millimeters for random movement range")]
		public float MinPos = 0; //!< Minimum position of the range where the drive is allowed to move to.
		[Tooltip("Maximum position in millimeters for random movement range")]
		public float MaxPos = 100; //!< Maximum position of the range where the drive is allowed to move to.
		[Tooltip("Drive speed in mm/s for erratic movements")]
		public float Speed = 100; //!< Speed of the drive in millimeter / second.
		[Tooltip("Enable drive to move to random positions")]
		public bool Driving = false; //!< Set to true if Drive should drive to erratic positions.
		[Tooltip("Toggle between min/max only (true) or random positions (false)")]
		public bool IterateBetweenMaxAndMin = false; //!< If true, the drive will only iterate between MinPos and MaxPos. If false, the drive will iterate between random positions
		[Tooltip("PLC signal to enable erratic movement (always enabled if empty)")]
		public PLCOutputBool SignalEnable; //!< If this signal is true, the drive will drive to erratic positions. If the signal is void it is always enabled
		private Drive Drive;
		private float _destpos;
		private bool signalenablenotnull;
		private bool moveto0;
		private bool waitingForZero;
		private float positionTolerance = 0.01f; // tolerance for position comparison in mm
		void Reset()
		{
			Drive = GetComponent<Drive>();
			if (Drive.UseLimits)
			{
				MinPos = Drive.LowerLimit;
				MaxPos = Drive.UpperLimit;
			}
		}
		
		// Use this for initialization
		protected override void OnStartSim()
		{
			Drive = GetComponent<Drive>();
			signalenablenotnull = SignalEnable != null;
			moveto0 = false;
			waitingForZero = false;
		}

		// Update is called once per frame
		public void CalcFixedUpdate()
		{
			if (ForceStop || !this.enabled) 
				return;

			// Handle return to zero when signal is disabled
			if (signalenablenotnull)
			{
				if (!SignalEnable.Value)
				{
					// Need to return to zero position
					if (Mathf.Abs(Drive.CurrentPosition) > positionTolerance && !moveto0 && !waitingForZero)
					{
						// Stop any current movement first
						Drive.Stop();
						Drive.TargetStartMove = false;
						moveto0 = true;
						waitingForZero = true;
						Driving = false;
						// Start move to zero
						Drive.TargetPosition = 0;
						Drive.TargetSpeed = Speed;
						Drive.TargetStartMove = true;
						return;
					}
					
					// Check if we're moving to zero
					if (moveto0)
					{
						// Keep the move to zero active
						if (!Drive.IsRunning && Mathf.Abs(Drive.CurrentPosition) > positionTolerance)
						{
							// Restart move if stopped prematurely
							Drive.TargetPosition = 0;
							Drive.TargetStartMove = true;
						}
						
						// Check if reached zero
						if (Mathf.Abs(Drive.CurrentPosition) <= positionTolerance)
						{
							// Successfully reached zero
							moveto0 = false;
							waitingForZero = false;
							Driving = false;
							Drive.TargetStartMove = false;
							Drive.Stop();
							// Force position to exactly zero to avoid drift
							Drive.CurrentPosition = 0;
						}
					}
					return; // Don't do erratic movement when signal is off
				}
				else
				{
					// Signal is enabled - check if we were returning to zero
					if (moveto0 || waitingForZero)
					{
						// Cancel return to zero if signal re-enabled
						moveto0 = false;
						waitingForZero = false;
						Driving = false;
						Drive.Stop();
						Drive.TargetStartMove = false;
					}
				}
			}

			// Continue with erratic movement only if enabled
			if (!signalenablenotnull || SignalEnable.Value)
			{
				if (Driving && !Drive.IsRunning && Mathf.Abs(Drive.CurrentPosition - _destpos) > positionTolerance)
				{
					Drive.TargetPosition = _destpos;
					Drive.TargetStartMove = true;
				}    

				if (!Driving)
				{
					Drive.TargetSpeed = Speed;
					if (!IterateBetweenMaxAndMin)
						Drive.TargetPosition = Random.Range(MinPos, MaxPos);
					else
					{
						if (Mathf.Abs(Drive.CurrentPosition - MaxPos) <= positionTolerance)
							Drive.TargetPosition = MinPos;
						else
							Drive.TargetPosition = MaxPos;
					}
					Drive.TargetStartMove = true;
					Driving = true;
					_destpos = Drive.TargetPosition;
				}
				else if (Drive.IsRunning && Driving)
				{
					Drive.TargetStartMove = false;
				}

				if (Mathf.Abs(Drive.CurrentPosition - _destpos) <= positionTolerance && Driving)
				{
					Driving = false;
				}
			}
		}
	}
}
