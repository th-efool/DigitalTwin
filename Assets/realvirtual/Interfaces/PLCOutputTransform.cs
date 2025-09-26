﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

 namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
    [System.Serializable]
    [SelectionBase]
    public class PLCOutputTransform : Signal
    {
        public StatusTransform Status;
        [SerializeField] Pose _value;
        private bool settransform = false;
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
                settransform = true;
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
            UpdateEnable = true;
            Settings.Active = true;
            Settings.Override = false;
            // get gameobject
            Status.Value = new Pose(transform.position, transform.rotation);
        }
        
        public override void SetValue(String value)
        {
            Debug.Log("Not implemented to set value " + value);
        }
        
        public override void SetValue(System.Object value)
        {
            Value = (Pose) value;
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

        public void SetValue(Pose value)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
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

    
        public void Update()
        {
            if (Status.OldValue != Status.Value)
            {
                EventSignalChanged.Invoke(this);
                Status.OldValue = Status.Value;
            }		
        }

        public void SetValueToTransoform()
        {
            Value = new Pose(transform.localPosition, transform.localRotation);
        }
        
        public void SetTransformFromValue()
        {
            transform.localPosition= Value.position;
            transform.localRotation = Value.rotation;
        }

        public void FixedUpdate()
        {
            if (settransform)
            {
                settransform = false;
                SetTransformFromValue();
                SignalChangedEvent(this);
            }
            
        }
    }
}