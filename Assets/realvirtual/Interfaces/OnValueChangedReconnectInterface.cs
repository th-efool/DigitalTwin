// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_6000_0_OR_NEWER

#pragma warning disable 4014

using System.Threading.Tasks;

namespace realvirtual
{
    
    using UnityEngine;
    public class OnValueChangedReconnectInterface : MonoBehaviour
    {
        public int ReconnectDelayMs = 1000;
        public bool DebugMode = false;
        // used for reconnecting an interface when connection properties are changed and interface is already connected
        public async Awaitable  OnInterfaceValueChanged()
        {
            var interf = GetComponent<InterfaceBaseClass>();
            if (interf == null) return;
            interf.CloseInterface();
            await Task.Delay(ReconnectDelayMs);
            interf.OpenInterface();
        }

        public void OnValueChanged()
        {
            if (DebugMode) Debug.Log("OnValueChanged");
            OnInterfaceValueChanged();
        }
    }
  
}

#endif // UNITY_6000_0_OR_NEWER
