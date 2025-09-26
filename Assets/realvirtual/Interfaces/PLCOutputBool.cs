﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using System;
using System.Text;
using UnityEngine;

 namespace realvirtual
{
	[System.Serializable]
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
	#region doc
	//! PLC Boolean Output Signal - Represents a boolean output signal from simulation to PLC.
	
	//! PLCOutputBool is a core signal type for sending boolean (true/false) data from the Unity simulation
	//! environment to external PLC systems. This component acts as a digital output point that simulation
	//! components can write to, providing feedback and status information to PLC control programs.
	//!
	//! Key Features:
	//! - Sends boolean values to connected PLC systems through various industrial protocols
	//! - Provides real-time value updates with automatic change detection
	//! - Supports edge detection for state transitions within the simulation
	//! - Includes override functionality for testing and manual control
	//! - Tracks connection status for reliable communication monitoring
	//!
	//! Signal Direction: Simulation → PLC (Output from PLC perspective)
	//! The signal flows from Unity simulation to the PLC controller, enabling the simulation to provide
	//! feedback about component states, sensor readings, and operation status to PLC programs.
	//!
	//! Common Applications:
	//! - Sending simulated sensor states to PLC (photoelectric sensors, limit switches)
	//! - Providing component status feedback (motor running, valve open/closed)
	//! - Signaling operation completion or error states
	//! - Implementing virtual buttons and switches for HMI functionality
	//! - Transmitting safety-relevant information like emergency stop acknowledgment
	//!
	//! Interface Integration:
	//! PLCOutputBool supports all major industrial communication protocols:
	//! - OPC UA: Writes to OPC UA boolean nodes with configurable update rates
	//! - S7 TCP/IP: Direct writing to Siemens S7 PLC data blocks
	//! - Modbus TCP/RTU: Updates Modbus coil or holding registers
	//! - TwinCAT ADS: Writes to Beckhoff TwinCAT boolean variables
	//! - MQTT: Publishes to MQTT topics for cloud and IoT connectivity
	//! - Shared Memory: Provides high-performance local data exchange
	//!
	//! Update Mechanism:
	//! The component monitors value changes in FixedUpdate() for physics-synchronized updates.
	//! When values change, the EventSignalChanged event notifies all connected interfaces to
	//! transmit the new state to the PLC. This ensures minimal latency between simulation
	//! state changes and PLC program reactions, critical for real-time control applications.
	//!
	//! Virtual Commissioning:
	//! In virtual commissioning scenarios, PLCOutputBool signals replace physical sensors and
	//! actuator feedback, allowing complete PLC program testing without hardware. The override
	//! feature enables manual manipulation of signals for comprehensive test coverage.
	#endregion
	public class PLCOutputBool : Signal
	{

		public StatusBool Status;
		[HideInInspector] public bool ChangedToTrue;
		[HideInInspector] public bool ChangedToFalse;
		
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
			{
				var oldvalue = Status.Value;
				Status.Value = value;
				if (oldvalue != value)
				{
					SignalChangedEvent(this);
				}

			}
		}

		public override void OnToggleHierarchy()
		{
			if (Settings.Override == false)
				Settings.Override = true;
			Status.ValueOverride = !Status.ValueOverride;
			EventSignalChanged.Invoke(this);
		}

		public override void SetStatusConnected(bool status)
		{
			Status.Connected = status;
		}

		public override bool GetStatusConnected()
		{
			return Status.Connected;
		}

		// When Script is added or reset ist pushed
		private void Reset()
		{
			Settings.Active = true;
			Settings.Override = false;
			Status.Value  = false;
		}
	

		public override string GetVisuText()
		{
			return Value.ToString();
		}
		
		public override byte[] GetByteValue()
		{
			return BitConverter.GetBytes(Value);
		}
		
		public override int GetByteSize()
		{
			return 1;
		}

		// Sets the value as a string
		public override void SetValue(string value)
		{
			if (value != "")
			{
				if (value == "0")
					Value = false;
				else if (value == "1")
					Value = true;
				else
					Value = bool.Parse(value);
			}
			else
				Value = false;
		}

		public override void SetValue(object value)
		{
			if (value != null)
				Value = (bool)value;
		}
		
		public override object GetValue()
		{
			return Value;
		}

		// Sets the value as a bool
		public void SetValue(bool value)
		{
			Value = value;
		}
		
		public override void SetValue(byte[] value)
		{
			Value = System.BitConverter.ToBoolean(value, 0);
		}
		
		public void FixedUpdate()
		{
			ChangedToTrue = false;
			ChangedToFalse = false;
			if (Status.OldValue != Status.Value)
			{
				if (Status.OldValue == false && Value == true)
					ChangedToTrue = true;
				if (Status.OldValue == true && Value == false)
					ChangedToFalse = true;
				EventSignalChanged.Invoke(this);
				Status.OldValue = Status.Value;
			}		
		}

	}
}