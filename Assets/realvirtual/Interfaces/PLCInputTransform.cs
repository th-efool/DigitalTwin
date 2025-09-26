﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


 using System;
 using UnityEngine;
 
namespace realvirtual
{
    //! PLC INT INPUT Signal
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
    [SelectionBase]
    public class PLCInputTransform : Signal
    {
        public StatusTransform Status;
       
        public Pose Value
        {
            get
            {
                if (Settings.Override)
                {
                    return Status.ValueOverride;;
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
	
        // When Script is added or reset ist pushed
        private void Reset()
        {
            UpdateEnable = true;
            Settings.Active= true;
            Settings.Override = false;
            Status.Value =  new Pose(transform.position, transform.rotation);
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
            var val = new byte[27];
            // write value.position.x into byte array
            BitConverter.GetBytes(Value.position.x).CopyTo(val, 0);
            BitConverter.GetBytes(Value.position.y).CopyTo(val, 4);
            BitConverter.GetBytes(Value.position.z).CopyTo(val, 8);
            BitConverter.GetBytes(Value.rotation.x).CopyTo(val, 12);
            BitConverter.GetBytes(Value.rotation.y).CopyTo(val, 16);
            BitConverter.GetBytes(Value.rotation.z).CopyTo(val, 20);
            BitConverter.GetBytes(Value.rotation.w).CopyTo(val, 24);
            return val;
        }
        
        public override int GetByteSize()
        {
            return 28;
        }


        public override string GetVisuText()
        {
            
                return Value.position.x.ToString("0.0") + " " + Value.position.y.ToString("0.0") + " " +
                       Value.position.z.ToString("0.0");
        }
	
        public override bool IsInput()
        {
            return true;
        }


        public override void SetValue(string value)
        {
            throw new NotImplementedException();
        }
        
        public override void SetValue(byte[] value)
        {
            Pose tmppose = new Pose();
            tmppose.position.x = System.BitConverter.ToSingle(value, 0);
            tmppose.position.y = System.BitConverter.ToSingle(value, 4);
            tmppose.position.z = System.BitConverter.ToSingle(value, 8);
            tmppose.rotation.x = System.BitConverter.ToSingle(value, 12);
            tmppose.rotation.y = System.BitConverter.ToSingle(value, 16);
            tmppose.rotation.z = System.BitConverter.ToSingle(value, 20);
            tmppose.rotation.w = System.BitConverter.ToSingle(value, 24);  
            Value = tmppose;
        }

        public override void SetValue(object value)
        {
            Value = (Pose)value;
        }
        
        public override object GetValue()
        {
            return Value;
        }
        
     
        public void SetValue(Pose value)
        {
            Value = value;
        }

        public void SetValueToTransform()
        {
            Value = new Pose(transform.localPosition, transform.localRotation);
        }
        
        public void SetTransformFromValue()
        {
            transform.localPosition= Value.position;
            transform.localRotation = Value.rotation;
        }

        private void FixedUpdate()
        {
           SetValueToTransform();
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
