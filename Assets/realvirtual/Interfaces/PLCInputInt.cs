﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using UnityEngine;

namespace realvirtual
{
    #region doc
    //! PLC Integer Input Signal - Represents a 32-bit integer input signal from PLC to simulation.
    
    //! PLCInputInt is a fundamental signal type for receiving integer data from external PLC systems
    //! into the Unity simulation. This component handles 32-bit signed integer values, essential for
    //! discrete counting, indexing, state codes, and parameter transmission from PLC control logic.
    //!
    //! Key Features:
    //! - Receives 32-bit signed integer values (-2,147,483,648 to 2,147,483,647)
    //! - Provides real-time value monitoring with change detection
    //! - Supports discrete state representation and counting operations
    //! - Includes override capability for testing and debugging
    //! - Maintains connection status for communication reliability
    //! - Automatic type conversion from various numeric formats
    //!
    //! Signal Direction: PLC → Simulation (Input from PLC perspective)
    //! The signal transfers integer values from PLC controllers to Unity simulation, enabling PLC
    //! programs to communicate counts, indices, state codes, and discrete parameters.
    //!
    //! Common Applications:
    //! - Receiving production counters and batch quantities
    //! - Reading state machine codes and operation modes
    //! - Transferring array indices and recipe numbers
    //! - Implementing discrete position values (encoder counts)
    //! - Receiving error codes and diagnostic information
    //! - Reading timer and counter values from PLC
    //! - Transferring part types and product identifiers
    //!
    //! Interface Integration:
    //! PLCInputInt integrates with all major industrial protocols:
    //! - OPC UA: Maps to OPC UA Int32 nodes with proper data typing
    //! - S7 TCP/IP: Direct reading from Siemens S7 DINT data types
    //! - Modbus TCP/RTU: Maps to holding/input registers (2 registers for 32-bit)
    //! - TwinCAT ADS: Connects to Beckhoff DINT variables
    //! - MQTT: Subscribes to integer values in JSON payloads
    //! - Shared Memory: Direct integer array access for high-speed transfer
    //!
    //! Data Range and Precision:
    //! Supports full 32-bit signed integer range:
    //! - Minimum: -2,147,483,648
    //! - Maximum: 2,147,483,647
    //! - No decimal places (whole numbers only)
    //! - Ideal for counters, indices, and discrete values
    //!
    //! Update Mechanism:
    //! The component uses FixedUpdate() for physics-synchronized value monitoring, ensuring
    //! consistent timing with Unity's simulation cycle. Value changes trigger EventSignalChanged
    //! events, allowing dependent components to react to new integer values immediately.
    //! This is particularly important for state machines and sequence control logic.
    //!
    //! Type Conversion:
    //! Automatically handles conversion from various sources:
    //! - String parsing for text-based protocols
    //! - Byte array conversion for binary protocols
    //! - Object casting with proper type checking
    //! - Safe conversion from floating-point values when needed
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
    public class PLCInputInt : Signal
    {
        public StatusInt Status;
	
        public int Value
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
            return Value.ToString("0");
        }
	
        public override bool IsInput()
        {
            return true;
        }
        
        public override void SetValue(string value)
        {
            if (value != "")
                Value = int.Parse(value);
            else
                Value = 0;
        }
        
        public override void SetValue(byte[] value)
        {
            Value =System.BitConverter.ToInt32(value, 0);
        }
		
        public override void SetValue(object value)
        {
            Value = System.Convert.ToInt32(value);
        }
        
        public override object GetValue()
        {
            return Value;
        }
        
        //! Sets the value as an int
        public void SetValue(int value)
        {
            Value = value;
        }

        public void FixedUpdate()
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
