// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    //! StartDriveOnCondition automatically starts a drive based on monitoring other drive positions or sensor states.
    //! It enables synchronization between drives by starting movement when specific conditions are met,
    //! such as another drive reaching a certain position or a sensor being triggered.
    [RequireComponent(typeof(Drive))]
    public class StartDriveOnCondition : BehaviorInterface
    {
        public enum Condition {Greater, Smaller};
        [Header("Conditions")]
        [Tooltip("Enable starting this drive based on another drive's position")]
        public bool StartBasedOnOtherDrivesPositon; //!< Enables starting this drive based on another drive's position
        [ShowIf("StartBasedOnOtherDrivesPositon")]
        [Tooltip("Drive whose position is monitored for triggering conditions")]
        public Drive MonitoredDrive; //!< The drive whose position is monitored for triggering conditions
        [ShowIf("StartBasedOnOtherDrivesPositon")]
        [Tooltip("Condition type (Greater or Smaller) that triggers the drive start")]
        public Condition ConditionToStartDrive; //!< Condition type (Greater or Smaller) that triggers the drive start
        [ShowIf("StartBasedOnOtherDrivesPositon")]
        [Tooltip("Position value in millimeters that triggers the drive start")]
        public float StartOnPosition; //!< Position value in millimeters that triggers the drive start
        [ShowIf("StartBasedOnOtherDrivesPositon")]
        [Tooltip("Value in millimeters to increment the trigger position after each activation")]
        public float IncrementStartOnPositon; //!< Value in millimeters to increment the trigger position after each activation    
        [ShowIf("StartBasedOnOtherDrivesPositon")][ReadOnly]public float CurrentStartOnPositon; //!< Current trigger position in millimeters after increments    
        
        [Tooltip("Enable starting this drive based on a sensor state")]
        public bool StartBasedOnSensor; //!< Enables starting this drive based on a sensor state
        [ShowIf("StartBasedOnSensor")]
        [Tooltip("Sensor whose state is monitored for triggering conditions")]
        public Sensor MonitoredSensor; //!< The sensor whose state is monitored for triggering conditions
        [ShowIf("StartBasedOnSensor")]
        [Tooltip("Start when sensor is occupied (true) or free (false)")]
        public bool StartOnSensorHigh; //!< If set to true, starts when sensor is occupied; if false, starts when sensor is free
        
        [Header("Destination")]
        [Tooltip("Target position in millimeters to move the drive to when triggered")]
        public float MoveThisDriveTo; //!< Target position in millimeters to move the drive to when triggered
        [Tooltip("Move relative to current position (true) or absolute position (false)")]
        public bool MoveIncremental; //!< If set to true, moves relative to current position; if false, moves to absolute position
        [ReadOnly] public float CurrentTarget; //!< Calculated target position in millimeters for the next movement
    
            
        private bool monitoredDriveNotNull;
        private bool sensorNotNull;
        private bool lastsensor;
        private float lastdrivepos;
        private Drive thisdrive;
        
        // Start is called before the first frame update
        void Start()
        {
            thisdrive = GetComponent<Drive>();
            monitoredDriveNotNull = MonitoredDrive != null;
            sensorNotNull = MonitoredSensor != null;
            if (monitoredDriveNotNull)
                lastdrivepos = MonitoredDrive.CurrentPosition;
            if (sensorNotNull)
                lastsensor = MonitoredSensor.Occupied;
            CurrentStartOnPositon = StartOnPosition;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (MoveIncremental)
                CurrentTarget = thisdrive.CurrentPosition + MoveThisDriveTo;
            else
                CurrentTarget = MoveThisDriveTo;
            
            if (StartBasedOnOtherDrivesPositon && monitoredDriveNotNull)
            {
               
                
                if (ConditionToStartDrive == Condition.Greater)
                {
                    if (MonitoredDrive.CurrentPosition >= CurrentStartOnPositon &&
                        lastdrivepos < CurrentStartOnPositon)
                    {
                   
                        thisdrive.DriveTo(CurrentTarget);
                        if (IncrementStartOnPositon!=0)
                        {
                            CurrentStartOnPositon += IncrementStartOnPositon;
                        }
                    }
                }
                
                if (ConditionToStartDrive == Condition.Smaller)
                {
                    if (MonitoredDrive.CurrentPosition <= CurrentStartOnPositon &&
                        lastdrivepos > CurrentStartOnPositon)
                    {
          
                        thisdrive.DriveTo(CurrentTarget);
                        if (IncrementStartOnPositon!=0)
                        {
                            CurrentStartOnPositon += IncrementStartOnPositon;
                        }
                    }
                }
                lastdrivepos = MonitoredDrive.CurrentPosition;
            }


            if (StartBasedOnSensor && sensorNotNull)
            {

                if (MonitoredSensor.Occupied == StartOnSensorHigh && lastsensor != StartOnSensorHigh)
                {
                    thisdrive.DriveTo(CurrentTarget);
                }

                lastsensor = MonitoredSensor.Occupied;
            }
        }
    }

}

