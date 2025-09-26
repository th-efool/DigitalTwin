﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Text;
using UnityEngine;

 namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
    [System.Serializable]

    //! PLC INT Output Signal
    public class PLCOutputText : Signal
    {
        public StatusText Status;
        private string _value;
        public string Value
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
            Status.Value = "";
        }

        public override void SetValue(string value)
        {
            Value = value;
        }

        public override void SetValue(object value)
        {
            Value = value.ToString();
        }
        
        public override void SetValue(byte[] value)
        {
            Value = System.Text.Encoding.UTF8.GetString(value);
        }
        
        public override object GetValue()
        {
            return Value;
        }
        
        public override byte[] GetByteValue()
        {
            return Encoding.UTF8.GetBytes(Value);
        }
        public override int GetByteSize()
        {
            return Status.Value.Length;
        }

        public override string GetVisuText()
        {
            return Value;
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