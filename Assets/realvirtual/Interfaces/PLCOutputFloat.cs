﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace realvirtual
{
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
	[System.Serializable]

	#region doc
	//! PLC Float Output Signal - Represents a floating-point output signal from simulation to PLC.
	
	//! PLCOutputFloat is a critical signal type for sending analog and continuous numerical data from
	//! the Unity simulation to external PLC systems. This component transmits 32-bit floating-point
	//! values representing measured values, calculated results, and analog feedback from the simulation.
	//!
	//! Key Features:
	//! - Sends 32-bit IEEE 754 floating-point values to PLC systems
	//! - Provides real-time value updates with automatic change notification
	//! - Supports high-precision analog signal transmission
	//! - Includes override functionality for testing and calibration
	//! - Tracks connection status for reliable communication
	//! - Handles culture-invariant number formatting for international compatibility
	//!
	//! Signal Direction: Simulation → PLC (Output from PLC perspective)
	//! The signal flows from Unity simulation to PLC controllers, providing analog feedback about
	//! simulation states, measurements, and calculated values that PLC programs use for control decisions.
	//!
	//! Common Applications:
	//! - Sending simulated analog sensor readings (temperature, pressure, level)
	//! - Providing actual speed feedback from motors and drives
	//! - Transmitting position data from encoders and measuring systems
	//! - Reporting calculated values (flow rates, power consumption, efficiency)
	//! - Sending distance measurements from ultrasonic or laser sensors
	//! - Providing weight and force measurements from load cells
	//!
	//! Interface Integration:
	//! PLCOutputFloat supports all major industrial protocols:
	//! - OPC UA: Writes to OPC UA Float/Double nodes with engineering units
	//! - S7 TCP/IP: Direct writing to Siemens S7 REAL data blocks
	//! - Modbus TCP/RTU: Updates holding registers (2 registers for float)
	//! - TwinCAT ADS: Writes to Beckhoff REAL variables
	//! - MQTT: Publishes JSON payloads with float values
	//! - Shared Memory: High-performance float array updates
	//!
	//! Data Format and Conversion:
	//! The component handles various data type conversions:
	//! - Automatic conversion from double to float when needed
	//! - Culture-invariant string parsing for consistent decimal notation
	//! - Byte array conversion for protocol-specific transmission
	//! - NaN and Infinity handling for error state communication
	//!
	//! Update Mechanism:
	//! Value changes are detected in Update() for smooth visual updates. The EventSignalChanged
	//! event ensures immediate transmission to connected interfaces. The system maintains
	//! previous values for change detection, minimizing unnecessary network traffic while
	//! ensuring timely updates for control-critical applications.
	//!
	//! Virtual Commissioning:
	//! PLCOutputFloat signals are essential for hardware-in-the-loop testing, replacing physical
	//! analog sensors with simulated values. This enables complete validation of PLC analog
	//! processing, scaling, and control algorithms without physical equipment.
	#endregion
	public class PLCOutputFloat : Signal
	{

		public StatusFloat Status;
		private float _value;

		public float Value
		{
			get
			{
				if (Settings.Override)
				{
					return Status.ValueOverride;
				}
				else
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
			Status.Value = 0;
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

		public override void SetValue(string value)
		{
			if (value != "")
				Value = float.Parse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
			else
				Value = 0;
		}
		
		public override void SetValue(byte[] value)
		{
			Value = System.BitConverter.ToSingle(value, 0);
		}
		
		public override void SetValue(object  value)
		{
			if (value != null)
			{
				Type t = value.GetType();
				if (t == typeof(double))
				{

					Value = Convert.ToSingle(value);
				}
				else
				{
					Value = (float) value;
				}
			}
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
			if (Status.OldValue != Status.Value)
			{
				if (EventSignalChanged!=null)
					EventSignalChanged.Invoke(this);
				Status.OldValue = Status.Value;
			}		
		}
	}
}
