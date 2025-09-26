// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(Drive))]
    //! Behavior model of a drive which is connected to another drive with a gear.
    //! All positions of the master drive are directly transfered with offset and gear factor to this drive.
    //! The formula is CurrentPosition = MasterDrive.CurrentPosition * GearFactor+Offset;
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive-behavior")]
    public class Drive_Gear : BehaviorInterface, IDriveBehavior
    {
        [Header("Settings")]
        [Tooltip("Master drive that controls this drive's position")]
        public Drive MasterDrive; //!< Master drive which is defining the position of this drive
        [Tooltip("Gear ratio multiplier applied to master drive position")]
        public float GearFactor = 1; //!< Gear factor of the gear
        [Tooltip("Position offset in millimeters added to calculated position")]
        public float Offset = 0; //!< Offset of the gear in millimeter
		
        private Drive _thisdrive; 
        private Drive Drive;
		
		
        // Use this for initialization
        void Start()
        {
            _thisdrive = GetComponent<Drive>();
            if (MasterDrive != null)
                MasterDrive.AddSubDrive(_thisdrive); //! Add this drive as a subdrive to the master drive for guaranteed fixed update sequence
        }

        public  void CalcFixedUpdate()
        {
            if (ForceStop || !this.enabled)
                return;
            
            var posi = MasterDrive.CurrentPosition * GearFactor+Offset;
            _thisdrive.CurrentPosition = posi;
            _thisdrive.CurrentSpeed = MasterDrive.CurrentSpeed * GearFactor;
        }
		
    }
}