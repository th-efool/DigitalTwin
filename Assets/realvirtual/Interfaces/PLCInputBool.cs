﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using NaughtyAttributes;
using UnityEngine;

 namespace realvirtual
{
	
	#region doc
	//! PLC Boolean Input Signal - Represents a boolean input signal from PLC to simulation.
	
	//! PLCInputBool is a fundamental signal type for receiving boolean (true/false) data from external PLC systems
	//! into the Unity simulation environment. This component serves as a digital input point that can be read and
	//! monitored by simulation components to react to PLC control logic.
	//!
	//! Key Features:
	//! - Receives boolean values from connected PLC systems via various industrial protocols
	//! - Supports real-time value changes with event notification system
	//! - Provides edge detection for rising (false to true) and falling (true to false) transitions
	//! - Implements override capability for manual testing and debugging without PLC connection
	//! - Maintains connection status to indicate communication health with PLC
	//!
	//! Signal Direction: PLC → Simulation (Input from PLC perspective)
	//! The signal flows from the PLC controller into the Unity simulation, allowing PLC programs to control
	//! simulation behavior through boolean states such as start/stop commands, sensor states, or status flags.
	//!
	//! Common Applications:
	//! - Receiving start/stop commands from PLC programs
	//! - Reading digital sensor states (proximity sensors, light barriers, limit switches)
	//! - Monitoring PLC status flags and operation modes
	//! - Triggering simulation events based on PLC conditions
	//! - Implementing safety interlocks and emergency stop signals
	//!
	//! Interface Integration:
	//! PLCInputBool seamlessly integrates with multiple industrial communication protocols including:
	//! - OPC UA: Maps to OPC UA boolean nodes for standardized communication
	//! - S7 TCP/IP: Direct integration with Siemens S7 PLCs via TCP/IP
	//! - Modbus TCP/RTU: Maps to Modbus coil or discrete input registers
	//! - TwinCAT ADS: Connects to Beckhoff TwinCAT boolean variables
	//! - MQTT: Subscribes to MQTT topics for IoT integration
	//! - Shared Memory: High-speed local communication for virtual commissioning
	//!
	//! Update Mechanism:
	//! The signal value is updated in FixedUpdate() to ensure consistent timing with Unity's physics system.
	//! Value changes trigger the EventSignalChanged event, allowing other components to react immediately.
	//! Edge detection (ChangedToTrue/ChangedToFalse) provides single-frame flags for transition handling,
	//! essential for implementing one-shot logic and state machines in automation sequences.
	#endregion
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
	public class PLCInputBool : Signal
	{
		//! Status struct of the bool
	
		public StatusBool Status;

		[HideInInspector] public bool ChangedToTrue;
		[HideInInspector] public bool ChangedToFalse;
		
		//! Sets and gets the value
		public bool Value
		{
			get
			{
				if (Settings.Override)
				{
					return Status.ValueOverride;
				} else
				{
					return Status.Value;
				}
			}
			set
			{   var oldvalue = Status.Value;
				Status.Value = value;
				if (oldvalue != value)
				{
					SignalChangedEvent(this);
				}
				
			}
		}
		

	
		// When Script is added or reset ist pushed
		private void Reset()
		{	
			Settings.Active = true;
			Settings.Override = false;
			Status.Value = false;
			Status.OldValue = false;
		}
		

		public override void OnToggleHierarchy()
		{
			if (Settings.Override == false)
				Settings.Override = true;
			Status.ValueOverride = !Status.ValueOverride;
			EventSignalChanged.Invoke(this);
			SignalChangedEvent(this);
		}
	
		//! Sets the Status connected
		public override void SetStatusConnected(bool status)
		{
			Status.Connected = status;
		}

		//! Gets the status connected
		public override bool GetStatusConnected()
		{
			return Status.Connected;
		}
		
		public override void SetValue(byte[] value)
		{
			Value = BitConverter.ToBoolean(value, 0);
		}
		
		public override byte[] GetByteValue()
		{
			return BitConverter.GetBytes(Value);
		}

		
		public override int GetByteSize()
		{
			return 1;
		}


		//! Gets the text for displaying it in the hierarchy view
		public override string GetVisuText()
		{
			return Value.ToString();
		}
	
		//! True if signal is input
		public override bool IsInput()
		{
			return true;
		}

		//! Sets the Value as a string
		public override void SetValue(string value)
		{
			if (value != "")
			{
				if (value == "0")
				{
					Value = false;
					return;
				}

				if (value == "1")
				{
					Value = true;
					return;
				}
				Value = bool.Parse(value);
			}
			else
				Value = false;
		}
	
		public override void SetValue(object value)
		{
			if (value != null )
				Value = (bool)value;
		}
		
		public override object GetValue()
		{
			return Value;
		}
		
		//! Sets the Value as a bool
		public void SetValue(bool value)
		{
			Value = value;
		}


		
		public void FixedUpdate()
		{
			ChangedToTrue = false;
			ChangedToFalse = false;
			if (Status.OldValue != Value)
			{
				if (Status.OldValue == false && Value == true)
					ChangedToTrue = true;
				if (Status.OldValue == true && Value == false)
					ChangedToFalse = true;
				if (EventSignalChanged!=null)
					EventSignalChanged.Invoke(this);
				Status.OldValue = Value;
			}		
		}

	}
}
