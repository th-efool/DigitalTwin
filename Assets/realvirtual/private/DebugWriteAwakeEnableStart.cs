// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    //! Debug component that logs Unity lifecycle events (Awake, OnEnable, Start) for troubleshooting script execution order
    public class DebugWriteAwakeEnableStart : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("<color=yellow>----- Awake: " + this.name);
        }

        private void OnEnable()
        {
            Debug.Log("<color=yellow>----- OnEnable: " + this.name);
        }

        private void Start()
        {
            Debug.Log("<color=yellow>----- Start: " + this.name);
        }
    }
}