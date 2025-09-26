﻿// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Text;
using NaughtyAttributes;
using UnityEngine;

 namespace realvirtual
{
	//! PLC BOOL INPUT Signal
	[HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces")]
	public class PLCInputText : Signal
	{
		//! Status struct of the bool
	
		public StatusText Status;
		

		//! Sets and gets the value
		public string Value
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
			Settings.Active = true;
			Settings.Override = false;
			Status.Value = "";
			Status.OldValue = "";
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
		
		public override byte[] GetByteValue()
		{
			return Encoding.UTF8.GetBytes(Value);
		}
		
		public override int GetByteSize()
		{
			return Status.Value.Length;
		}


		//! Gets the text for displaying it in the hierarchy view
		public override string GetVisuText()
		{
			return Value;
		}
	
		//! True if signal is input
		public override bool IsInput()
		{
			return true;
		}

		//! Sets the Value as a string
		public override void SetValue(string value)
		{
			Value = value;
		}
		
		public override void SetValue(byte[] value)
		{
			Value = System.Text.Encoding.UTF8.GetString(value);
		}
	
		public override void SetValue(object value)
		{
			if (value != null)
				Value = value.ToString();
		}
		
		public override object GetValue()
		{
			return Value;
		}
		
		//! Sets the Value as a bool
		public void SetValue(bool value)
		{
			Value = value.ToString();
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
