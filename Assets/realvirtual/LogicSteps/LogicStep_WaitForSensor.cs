// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! Logic step that waits for a sensor to reach a specific occupation state before proceeding.
    //! This step blocks execution until the specified sensor is either occupied or not occupied.
    //! Useful for synchronizing automation sequences with physical object detection.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_WaitForSensor: LogicStep
    {
        [Header("Sensor Configuration")]
        [Required("Sensor is required")]
        public Sensor Sensor; //!< The sensor to monitor for occupation state changes
        
        [Label("Wait For")]
        [Dropdown("OccupiedNotOccupied")]
        public bool WaitForOccupied; //!< If true, waits for sensor to be occupied; if false, waits for sensor to be not occupied
        
        private string[] OccupiedNotOccupied = new string[] { "Not Occupied", "Occupied" };

        private bool sensornotnull = false;
        
        protected new bool NonBlocking()
        {
            return false;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (sensornotnull==false)
                NextStep();
        }

     
        protected new void Start()
        {
            sensornotnull = Sensor != null;
            base.Start();
        }
        
        private void FixedUpdate()
        {
           if (sensornotnull && StepActive)
               if (Sensor.Occupied==WaitForOccupied)
                   NextStep();
        }
    }

}

