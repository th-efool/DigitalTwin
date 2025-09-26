// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Collections.Generic;
using UnityEngine;


namespace realvirtual
{
    public interface ISignalInterface
    {
        public List<BehaviorInterfaceConnection> GetConnections();
        public List<Signal> GetSignals();
        GameObject gameObject { get ; } 
    }
}

