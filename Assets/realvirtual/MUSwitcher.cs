// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    //! MUSwitcher dynamically toggles visibility of grouped GameObjects within MUs based on sensor detection.
    //! Enables runtime switching between different configurations or visual states of MUs when detected by sensors.
    //! Useful for simulating product variants, quality states, or processing stages in automation workflows.
    [RequireComponent(typeof(Sensor))]
    public class MUSwitcher : MonoBehaviour
    {

        [Tooltip("Name of the group to activate when MU is detected (other groups will be deactivated)")]
        public string SwitchToGroup;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // register to the sensor on the same gameobject
            var sensor = GetComponent<Sensor>();
            if (sensor != null)
            {
                sensor.EventMUSensor.AddListener(OnSensorEvent);
            }
            else
            {
                Debug.LogError(
                    "No Sensor found on the same GameObject. Please add a Sensor component to the GameObject");
            }

        }

       

        private void OnSensorEvent(MU mu, bool occupied)
        {
            if (occupied)
            {
                 // get all gameobject underneath mu which are in the defined group
                var toenable = new List<MU>();
                var todosable = new List<MU>();
                var muGroups = mu.GetComponentsInChildren<Group>(true);
                // check if the group is the same as defined
                foreach (var muGroup in muGroups)
                {
                    if (muGroup.GroupName == SwitchToGroup)
                    {
                         muGroup.gameObject.SetActive(true);
                         
                    }
                    else
                    {
                        muGroup.gameObject.SetActive(false);
                    }
                }
                
            }

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

