﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using UnityEngine;

 namespace realvirtual
{
	#region doc
	//! PLC Float Input Signal - Represents a floating-point input signal from PLC to simulation.
	
	//! PLCInputFloat is an essential signal type for receiving analog and continuous numerical data from
	//! external PLC systems into the Unity simulation. This component handles 32-bit floating-point values,
	//! enabling precise communication of analog measurements, setpoints, and control parameters.
	//!
	//! Key Features:
	//! - Receives 32-bit IEEE 754 floating-point values from PLC systems
	//! - Supports real-time value monitoring with change detection
	//! - Provides high precision for analog signal representation
	//! - Includes override capability for testing without PLC connection
	//! - Maintains connection status for communication health monitoring
	//! - Automatic byte-order conversion for cross-platform compatibility
	//!
	//! Signal Direction: PLC → Simulation (Input from PLC perspective)
	//! The signal transfers analog values from PLC controllers to Unity simulation, allowing PLC programs
	//! to control continuous parameters like speeds, positions, temperatures, and pressures.
	//!
	//! Common Applications:
	//! - Receiving speed setpoints for motors and drives (rpm, m/s)
	//! - Reading analog sensor values (pressure, temperature, flow rate)
	//! - Transferring position commands for servo axes
	//! - Implementing PID controller outputs and control values
	//! - Receiving scaling factors and calibration parameters
	//! - Transmitting measurement data from PLC calculations
	//!
	//! Interface Integration:
	//! PLCInputFloat seamlessly integrates with industrial protocols:
	//! - OPC UA: Maps to OPC UA Float/Double nodes with engineering units
	//! - S7 TCP/IP: Direct reading from Siemens S7 REAL data types
	//! - Modbus TCP/RTU: Maps to holding registers (2 registers for 32-bit float)
	//! - TwinCAT ADS: Connects to Beckhoff REAL variables
	//! - MQTT: Subscribes to JSON payloads with float values
	//! - Shared Memory: High-speed float array exchange
	//!
	//! Data Precision and Range:
	//! Uses standard 32-bit float representation providing:
	//! - Range: ±3.4 × 10^38
	//! - Precision: ~7 significant digits
	//! - Special values: NaN, Infinity support for error states
	//!
	//! Update Mechanism:
	//! Values are monitored in Update() for smooth animation compatibility. Changes trigger
	//! EventSignalChanged events for reactive component behavior. The system automatically
	//! handles byte-order conversion when receiving data from different PLC architectures,
	//! ensuring consistent value interpretation across platforms.
	#endregion
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
	public class PLCInputFloat : Signal
	{
		public StatusFloat Status;
	
		public float Value
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
	
		// When Script is added or reset ist pushed
		private void Reset()
		{
			Settings.Active= true;
			Settings.Override = false;
			Status.Value = 0;
			
		}
	
		public override void SetStatusConnected(bool status)
		{
			Status.Connected = status;
		}

		public override bool GetStatusConnected()
		{
			return Status.Connected;
		}
		
	

		public override byte[] GetByteValue()
		{
			return BitConverter.GetBytes(Value);
		}

		public override int GetByteSize()
		{
			return 4;
		}


		public override string GetVisuText()
		{
			return Value.ToString("0.0");
		}
	
		public override bool IsInput()
		{
			return true;
		}

		public override void SetValue(string value)
		{
	
			if (value != "")
				Status.Value = float.Parse(value);
			else
				Status.Value = 0;
			
		}
		
		public override void SetValue(byte[] value)
		{
			Value =System.BitConverter.ToSingle(value, 0);
		}
		
		//! Sets the value as an int
		public void SetValue(int value)
		{
			Value = value;
		}
		
		public override void SetValue(object value)
		{
			Value = (float)value;
		}
		
		public override object GetValue()
		{
			return Value;
		}
		
		//! Sets the value as a float
		public void SetValue(float value)
		{
			Value = value;
		}
		
		public void Update()
		{
			if (Status.OldValue != Value)
			{
				if (EventSignalChanged!=null)
					EventSignalChanged.Invoke(this);
				Status.OldValue = Value;
			}		
		}
	
	}
}
