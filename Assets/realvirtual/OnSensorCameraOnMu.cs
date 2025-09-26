using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE
using Cinemachine;
#endif

namespace realvirtual
{
    public class OnSensorCameraOnMu : MonoBehaviour
    {
#if CINEMACHINE
        public Sensor sensor;

        private CinemachineVirtualCamera cinecam;
        
        // Start is called before the first frame update
        public void SetCameraOnMU()
        {
            sensor.EventEnter += SensorOnEventEnter;
        }

        private void SensorOnEventEnter(GameObject obj)
        {
            sensor.EventEnter -= SensorOnEventEnter;
            cinecam.Follow = obj.transform;
            cinecam.LookAt = obj.transform;

        }

        // Update is called once per frame
        void Start()
        {
            cinecam = GetComponent<CinemachineVirtualCamera>();
        }
#endif
    }
}


