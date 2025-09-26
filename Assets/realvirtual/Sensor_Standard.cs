// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(Sensor))]
    //! The Sensor_Standard component is providing the Sensor behavior and connection to the PLC inputs and outputs.
    public class Sensor_Standard : BehaviorInterface
    {
       
        [Header("Settings")] 
        [Tooltip("Inverts sensor signal logic. When false, signal is true if occupied. When true, signal is false if occupied")]
        public bool NormallyClosed = false;  //!< Defines if sensor signal is *true* if occupied (*NormallyClosed=false*) of if signal is *false* if occupied (*NormallyClosed=true*)
        
        [Header("Interface Connection")] 
        [Tooltip("PLC input signal that reflects the sensor's occupied state")]
        public PLCInputBool Occupied; //! Boolean PLC input for the Sensor signal.

        private Sensor Sensor;
        private bool _isOccupiedNotNull;

        // Use this for initialization
        void Start()
        {
            _isOccupiedNotNull = Occupied != null;
            Sensor = GetComponent<Sensor>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            bool occupied = false;

            // Set Behavior Outputs
            if (NormallyClosed)
            {
                occupied = !Sensor.Occupied;
            }
            else
            {
                occupied = Sensor.Occupied;
            }

            // Set external PLC Outputs
            if (_isOccupiedNotNull)
                Occupied.Value = occupied;

        }
    }
}