

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Object = UnityEngine.Object;


namespace realvirtual
{
    //! PartChanger component for dynamically switching between different visual representations of parts.
    //! Enables runtime variation of MU appearances for simulating product variants or configuration changes.
    //! Supports sensor-triggered changes and PLC control for automated appearance switching in production simulations.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/partchanger")]
    public class PartChanger : BehaviorInterface
    {
    
        [Tooltip("Index of the appearance in MUAppearances list to change to")]
        public int PartNumber; //!<  The number in the list MUAppearances the part should be changed to
        [BoxGroup("Change detected MU on Sensor")] [Label("Enable")]
        [Tooltip("Enable changing parts detected by sensor")]
        public bool ChangeSensorMU; //!<  True if a collision with a Sensor should change the part
        [BoxGroup("Change detected MU on Sensor")] [ShowIf("ChangeSensorMU")]
        [Tooltip("Sensor that triggers part changes when MUs are detected")]
        public Sensor ChangeOnSensor; //!<  The connected Sensor

        [BoxGroup("Change this part")] [Label("Enable")]
        [Tooltip("Enable changing this GameObject's appearance directly")]
        public bool ChangeThisPart; //!<  True if this object should be changed (does not needs to be an MU)
        [BoxGroup("Change this part")] [ShowIf("ChangeThisPart")][ReorderableList]
        [Tooltip("List of prefab appearances to switch between for this GameObject")]
        public List<GameObject> MUAppearences; //!<  MUApprearances if this part (not an MU on the Sensor) should be changed

        [BoxGroup("Condition")]
        [Tooltip("Change part when sensor detects an object")]
        public bool ConditionSensorIsTouched=true; //! Changes the Part if Sensor is touched
        [BoxGroup("Condition")]
        [Tooltip("Change part when PartNumber property changes")]
        public bool ConditionPartNumberIsChanging=true; //! Changes the Part if Partnumber is changing
        [BoxGroup("Condition")]
        [Tooltip("Change part when PLCChangePartNow signal triggers")]
        public bool ConditionPLCSignalChangeNow=true; //!Changes the Part on PLCSignal Change Now
        
        
        [BoxGroup("Signals")]
        [Tooltip("PLC signal that sets the current part number index")]
        public PLCOutputInt PLCCourrentPartNumber; //! PLC output for current part number
        [BoxGroup("Signals")]
        [Tooltip("PLC signal to trigger immediate part change")]
        public PLCOutputBool PLCChangePartNow; //! PLC outpout for changing the part
       
        private int _oldpartnumber=0;
        private bool _notnullchangeonsensor=false;
        private bool _notnullPLCOutCurrentPartNumber = false;
        private bool _notnullchangenow=false;
        private bool _oldPLCChangePartNow = false;
  

        //! Unity Event On Sensor Enter end Exit
        public void OnSensor(MU mu,bool enter)
        {
            if (ConditionSensorIsTouched && enter)
            {
                ChangePart(mu);
            }
            
        }

        
        private void ChangePart(GameObject OldParent, GameObject New)
        {
            // Delete all gamobjects in parent
            var themu = OldParent.GetComponent<MU>();
            
            // Inform Sensor about parts which are going to be deleted
            var allchildren = themu.GetComponentsInChildren<Collider>();
            foreach (var child in allchildren)
            {
                foreach (var sensor in themu.CollidedWithSensors)
                {
                    sensor.OnMUPartsDestroyed(child.gameObject);
                }
            }
            
            // Destroy objects
            foreach(Transform child in OldParent.transform) {
                Destroy(child.gameObject);
            }
            
            // Make a copy of the new part
            var newcopy = Object.Instantiate(New, OldParent.transform);
            newcopy.transform.localPosition = Vector3.zero;
            newcopy.transform.localRotation = Quaternion.identity;
            newcopy.SetActive(true);
        }
        
        //! Change the Part mu
        public void ChangePart(MU mu)
        {
            // Was collission enter so get Part from MU
            if (!ReferenceEquals(mu, null))
            {
                GameObject newpart=null;
                // get new Appearance from MU
                if (PartNumber <= mu.MUAppearences.Count)
                {
                    newpart = mu.MUAppearences[PartNumber];
                }
                
                if (newpart != null)
                {
                    ChangePart(mu.gameObject,newpart);
                }
            }

        }

        //! Change this Part
        public void ChangePart()
        {
            // Change this part
            GameObject newpart=null;
            if (PartNumber <= MUAppearences.Count)
            {
                newpart = MUAppearences[PartNumber];
                if (newpart != null)
                {
                    ChangePart(this.gameObject,newpart);
                }
            }
        }
  
        void Reset()
        {
            ChangeOnSensor = GetComponent<Sensor>();
            if (ChangeOnSensor != null)
            {
                ChangeSensorMU = true;
                ChangeThisPart = false;
            }
        }

      
        // Start is called before the first frame update
        void Start()
        {
            if (ChangeOnSensor!=null)
                _notnullchangeonsensor = true;
            if (PLCCourrentPartNumber!=null)
               _notnullPLCOutCurrentPartNumber = true;
            if (PLCChangePartNow!=null)
                _notnullchangenow = true;

            if (_notnullchangeonsensor)
            {
                if (ChangeOnSensor.EventMUSensor == null)
                    ChangeOnSensor.EventMUSensor = new Game4AutomationEventMUSensor();
                ChangeOnSensor.EventMUSensor.AddListener(OnSensor);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_notnullPLCOutCurrentPartNumber)
            {
                PartNumber = PLCCourrentPartNumber.Value;
            }

            if (ConditionPartNumberIsChanging)
            {
                if (PartNumber != _oldpartnumber)
                {
                    if (ChangeSensorMU) // sensor detected, change part(s) in sensor
                    {
                        if (_notnullchangeonsensor)
                        {
                            foreach (var mu in ChangeOnSensor.CollidingMus)
                            {
                                // Get Parts from MU
                                ChangePart(mu);
                            }
                        }
                    }
                    else // not sensor detected, this part
                    {
                        ChangePart();
                    }
                }
            }

            if (_notnullchangenow)
            {
                if (PLCChangePartNow.Value != _oldPLCChangePartNow)
                {
                    if (ChangeSensorMU) // sensor detected, change part(s) in sensor
                    {
                        if (_notnullchangeonsensor)
                        {
                            foreach (var mu in ChangeOnSensor.CollidingMus)
                            {
                                // Get Parts from MU
                                ChangePart(mu);
                            }
                        }
                    }
                    else // not sensor detected, this part
                    {
                        ChangePart();
                    }
                }
                _oldPLCChangePartNow = PLCChangePartNow.Value;
            }

           
            _oldpartnumber = PartNumber;
        }
    }


}
