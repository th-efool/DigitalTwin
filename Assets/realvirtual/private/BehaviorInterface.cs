// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace realvirtual
{
	//! Class for saving the connection data - the signal and the name of the property where the signal is attached to
	public class BehaviorInterfaceConnection
	{
		public Signal Signal;
		public string Name;
	}

	//! Base class for all behavior models with connection to PLC signals. 
	public class BehaviorInterface : realvirtualBehavior, ISignalInterface
	{

	
		public List<BehaviorInterfaceConnection> ConnectionInfo = new List<BehaviorInterfaceConnection>();

	
		//! Returns a list of behavior interface connections for this component.
		//! IMPLEMENTS ISignalInterface::GetConnections
		public new List<BehaviorInterfaceConnection> GetConnections()
		{
			ConnectionInfo = UpdateConnectionInfo();
			return ConnectionInfo;
		}

		//! Returns a list of connected signals for this component.
		//! IMPLEMENTS ISignalInterface::GetSignals
		public new List<Signal> GetSignals()
		{
			return GetConnectedSignals();
		}
	}
}
