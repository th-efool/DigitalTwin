using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! Automatically stops a drive when a sensor detects an object.
    //! This logic controller monitors a sensor and stops the associated drive when the sensor becomes occupied,
    //! then restarts the drive when the sensor is clear or when a restart signal is received.
    public class LogicStopDriveOnSensor : MonoBehaviour
    {
        [InfoBox("This script stops the drive when the sensor is getting to occupied. ")]
        public Drive Drive; //!< The drive to control based on sensor state

        public Sensor Sensor; //!< The sensor that triggers drive stop when occupied
        public PLCInputBool ConveyorStopped; //!< Input signal indicating the conveyor has stopped
        public PLCOutputBool Restart; //!< Output signal to restart the conveyor until next sensor trigger
      
        private bool sensoroccupiedbefore = false;

        private bool _isSensorNotNull;
        private bool _isDriveNotNull;
        private bool _isConveyorStoppedNotNull;
        private bool _isRestartNotNull;
        private bool first;

        // Start is called before the first frame update
        private void Awake()
        {
            _isConveyorStoppedNotNull = ConveyorStopped != null;
            _isDriveNotNull = Drive != null;
            _isSensorNotNull = Sensor != null;
            _isRestartNotNull = Restart != null;
            first = true;
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if (_isSensorNotNull && _isDriveNotNull)
            {
                if (Sensor.Occupied && (!sensoroccupiedbefore || first))
                {
                    Drive.JogForward = false;
                    if (_isConveyorStoppedNotNull) ConveyorStopped.Value = true;
                }
                if ((!Sensor.Occupied && (sensoroccupiedbefore || first) || (_isRestartNotNull && Restart.Value)))
                {
                    Drive.JogForward = true;
                }
            }

            first = false;
            sensoroccupiedbefore = Sensor.Occupied;
        }
    }
}