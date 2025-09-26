﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using UnityEngine;

 namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
    [System.Serializable]

    #region doc
    //! PLC Integer Output Signal - Represents a 32-bit integer output signal from simulation to PLC.
    
    //! PLCOutputInt is an essential signal type for sending integer data from the Unity simulation
    //! environment to external PLC systems. This component transmits 32-bit signed integer values,
    //! providing discrete feedback, counts, state information, and calculated integer results to PLC programs.
    //!
    //! Key Features:
    //! - Sends 32-bit signed integer values to connected PLC systems
    //! - Provides automatic change detection and event notification
    //! - Supports discrete state and counter value transmission
    //! - Includes override functionality for testing and manual control
    //! - Maintains connection status for communication monitoring
    //! - Robust type conversion with error handling
    //!
    //! Signal Direction: Simulation → PLC (Output from PLC perspective)
    //! The signal flows from Unity simulation to PLC controllers, enabling the simulation to provide
    //! integer feedback such as counts, states, positions, and diagnostic codes to PLC control programs.
    //!
    //! Common Applications:
    //! - Sending simulated encoder positions and pulse counts
    //! - Providing part counting and production statistics
    //! - Transmitting state machine status codes
    //! - Reporting error codes and diagnostic information
    //! - Sending calculated integer results (total counts, averages)
    //! - Providing array indices and selection values
    //! - Transmitting quality codes and inspection results
    //!
    //! Interface Integration:
    //! PLCOutputInt supports all major industrial communication protocols:
    //! - OPC UA: Writes to OPC UA Int32 nodes with proper typing
    //! - S7 TCP/IP: Direct writing to Siemens S7 DINT data blocks
    //! - Modbus TCP/RTU: Updates holding registers (2 registers for 32-bit)
    //! - TwinCAT ADS: Writes to Beckhoff DINT variables
    //! - MQTT: Publishes integer values in JSON format
    //! - Shared Memory: High-speed integer array updates
    //!
    //! Data Integrity and Conversion:
    //! The component ensures reliable data transmission:
    //! - Safe conversion from various numeric types (float, double, long)
    //! - Overflow protection with Convert.ToInt32()
    //! - Null value handling to prevent exceptions
    //! - Byte array conversion for binary protocols
    //! - String formatting for text-based communication
    //!
    //! Update Mechanism:
    //! Value changes are monitored in Update() for responsive updates. When the value changes,
    //! EventSignalChanged is invoked to notify all connected interfaces. This ensures that
    //! PLC programs receive timely updates for counter values, state changes, and other
    //! discrete information critical for sequence control and decision making.
    //!
    //! Virtual Commissioning:
    //! In virtual commissioning scenarios, PLCOutputInt signals simulate discrete sensors,
    //! encoders, and counting devices. This allows complete testing of PLC counting logic,
    //! state machines, and discrete control algorithms without physical hardware.
    //! The override feature enables manual value injection for comprehensive test coverage.
    //!
    //! Performance Considerations:
    //! Integer signals are highly efficient for discrete value transmission, using only
    //! 4 bytes per value. They are ideal for high-frequency updates like encoder feedback
    //! and counter values, providing optimal network bandwidth usage compared to float types.
    #endregion
    public class PLCOutputInt : Signal
    {
        public StatusInt Status;
        private float _value;
        public int Value
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

        public override void SetValue(string value)
        {
            if (value != "")
                Status.Value = int.Parse(value);
            else
                Status.Value = 0;

     
        }

        //! Sets the value as an int
        public void SetValue(int value)
        {
            Value = value;
         
        }

        //! Sets the value as an int
        public override void SetValue(object value)
        {
            if (value != null)
            {
                Type t = value.GetType();
                try
                {
                    Value = Convert.ToInt32(value);
                } catch
                {}
            }
        }
        
        public override void SetValue(byte[] value)
        {
            Value = System.BitConverter.ToInt32(value, 0);
        }

        public override object GetValue()
        {
            return Value;
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
            return Value.ToString("0");
        }
        
        public void Update()
        {
            if (Status.OldValue != Status.Value)
            {
                EventSignalChanged.Invoke(this);
                Status.OldValue = Status.Value;
            }		
        }
    }
}