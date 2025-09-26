// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using UnityEngine;

namespace realvirtual
{
    public class SignalConnection : MonoBehaviour
    {
        public Signal Signal;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void OnButtonClicked()
        {
            var signaltype = Signal.GetType();
            if (signaltype.ToString() == "realvirtual.PLCInputBool" || signaltype.ToString() == "realvirtual.PLCOutputBool" )
            {
                bool value = (bool)Signal.GetValue();
                Signal.SetValue(!value);
            }
            else
            {
                Debug.LogWarning("Signal type not supported");
            }

        }
    }
}
