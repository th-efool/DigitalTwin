
// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
    public class StationSensor : MonoBehaviour
    {
        private BaseStation station;

        private void Start()
        {
            station = GetComponentInParent<BaseStation>();

        }

        private void OnTriggerEnter(Collider other)
        {
            if (station != null)
                station.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (station != null)
                station.OnTriggerExit(other);

        }


    }
}
