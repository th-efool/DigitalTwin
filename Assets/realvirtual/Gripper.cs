// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS3003
namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/gripper")]
    #region doc
    //! Gripper component providing complete gripping functionality for robotic pick-and-place operations.
    
    //! The Gripper component is a sophisticated tool for simulating industrial gripping mechanisms in realvirtual,
    //! offering realistic finger movement control and intelligent collision-based gripping. It provides a complete
    //! solution for pick-and-place operations, automatically managing finger positions, detecting contact with MUs,
    //! and maintaining secure grips during transport operations. The component simulates real-world gripper behavior
    //! including opening/closing times, maximum aperture, and contact-based stopping for safe material handling.
    //!
    //! Key Features:
    //! - Automatic finger movement with configurable opening/closing speeds
    //! - Intelligent collision detection for contact-based gripping
    //! - Dual finger control with synchronized movement
    //! - Raycast-based MU detection for precise gripping
    //! - Automatic stop-on-contact to prevent over-gripping
    //! - Support for fingerless grippers using raycast detection
    //! - Real-time status monitoring (opening, closing, fully opened, fully closed)
    //! - Integrated Grip component for MU attachment management
    //!
    //! Common Applications:
    //! - Robotic end-of-arm tooling (EOAT) simulation
    //! - Pneumatic and servo-electric gripper modeling
    //! - Pick-and-place robot systems
    //! - Automated assembly operations
    //! - Material handling and packaging systems
    //! - Quality inspection and sorting stations
    //! - Collaborative robot (cobot) applications
    //!
    //! Operating Principles:
    //! The Gripper operates by controlling two finger GameObjects that move symmetrically along defined
    //! direction vectors. When closing, the fingers move inward until they contact an MU or reach their
    //! fully closed position. Upon contact detection via raycast sensing, the gripper automatically stops
    //! closing and secures the MU using the integrated Grip component. The opening process releases any
    //! gripped MUs and returns the fingers to their fully open position. The component supports both
    //! manual control through Inspector properties and automated control via PLC signals.
    //!
    //! Finger Configuration:
    //! The component automatically locates finger GameObjects named "Left" and "Right" in its children,
    //! or custom finger objects can be assigned. For special cases without physical fingers, the gripper
    //! can operate in raycast-only mode, simulating magnetic or vacuum grippers. The finger movement
    //! is calculated based on the GripperWidth parameter and follows the DirectionClosing vector for
    //! precise control over gripper geometry.
    //!
    //! Integration Points:
    //! The Gripper seamlessly integrates with realvirtual's automation ecosystem through PLC signal
    //! interfaces (PLCOutputBool for commands, PLCInputBool for status), the Sensor system for MU
    //! detection, and the Grip component for attachment management. It can be controlled by robot
    //! controllers, PLC programs, or scripted automation sequences, providing flexible integration
    //! options for various simulation scenarios.
    //!
    //! Performance Considerations:
    //! The component uses efficient raycast detection limited to specific layers (rvMU, rvMUSensor)
    //! to minimize performance impact. Finger movements are calculated in FixedUpdate for smooth,
    //! deterministic motion synchronized with Unity's physics system. The automatic stop-on-contact
    //! feature prevents unnecessary calculations once gripping is achieved, optimizing runtime performance.
    //!
    //! Status and Feedback:
    //! The Gripper provides comprehensive status information including current state (opening, closing),
    //! position status (fully opened, fully closed), and grip status (MUIsGripped). These status flags
    //! can be monitored through Inspector properties or transmitted to PLC systems via signal interfaces,
    //! enabling closed-loop control and monitoring in automated systems.
    //!
    //! For detailed documentation and examples, visit:
    //! https://doc.realvirtual.io/components-and-scripts/gripper
    #endregion
    public class Gripper : BehaviorInterface
    {
        [Tooltip("Keep empty if it is named Left")]
        public GameObject LeftFinger; //!< Reference to the left finger GameObject

        [Tooltip("Keep empty if it is named Right")]
        public GameObject RightFinger; //!< Reference to the right finger GameObject

        [Tooltip("Time in seconds to fully open the gripper")]
        public float TimeOpening; //!< Time in seconds for opening the gripper
        [Tooltip("Time in seconds to fully close the gripper")]
        public float TimeClosing; //!< Time in seconds for closing the gripper
        
        [Tooltip("Maximum opening width of the gripper in mm")]
        public float GripperWidth; //!< Maximum gripper width in millimeters
        [Tooltip("Additional offset in mm when gripper is fully open")]
        public float OpenPosOffset; //!< Offset position in millimeters when gripper is open
        [Tooltip("Local direction vector for finger movement")]
        public Vector3 DirectionFinger = new Vector3(1, 0, 0); //!< Direction vector for finger movement
        [Tooltip("Local direction vector for closing movement")]
        public Vector3 DirectionClosing = new Vector3(1, 0, 0); //!< Direction vector for closing movement

        [Header("Status")] [ReadOnly] public bool FullyClosed; //!< True when gripper is fully closed
        public bool Close; //!< Set to true to close the gripper
        [ReadOnly] public bool Closing; //!< True while gripper is closing
        [ReadOnly] public bool FullyOpened; //!< True when gripper is fully opened
        public bool Open; //!< Set to true to open the gripper
        [ReadOnly] public bool Opening; //!< True while gripper is opening
        [ReadOnly] public bool MUIsGripped; //!< True when an MU is gripped
        [ReadOnly] public MU GrippedMU; //!< Reference to the currently gripped MU


        [Header("PLC IOs")] public PLCOutputBool CloseGripper; //!< PLC output signal to close the gripper
        public PLCOutputBool OpenGripper; //!< PLC output signal to open the gripper
        public PLCInputBool IsClosing; //!< PLC input signal indicating gripper is closing
        public PLCInputBool IsOpening; //!< PLC input signal indicating gripper is opening
        public PLCInputBool IsFullyOpened; //!< PLC input signal indicating gripper is fully opened
        public PLCInputBool IsFullyClosed; //!< PLC input signal indicating gripper is fully closed


        private GameObject leftfinger;
        private GameObject rightfinger;
        private Sensor RayCastSensor;
        private bool nofingers;
        private Rigidbody rbright, rbleft;
        private bool isclosingnotnull;
        private bool isopeningnotnull;
        private bool isfullyopenednotnull;
        private bool isfullyclosednotnull;
        private bool isclosegrippernotnull;
        private bool isopengrippernotnull;
        private bool grippedonclosing;
        private Vector3 leftstartpos;
        private Vector3 rightstartpos;
        private float posrel;
        private float posabs;
        private Grip grip;
        private float gripdistancerel;
        public bool usefingers;
        private float fingerposright, fingerposleft;
        private MU muingripper;
    

        new void Awake()
        {
            // Create and Reference Gripper Components
            if (LeftFinger == null)
            {
                leftfinger = GetChildByName("Left");
            }
            else
            {
                leftfinger = LeftFinger;
            }

            if (RightFinger == null)
            {
                rightfinger = GetChildByName("Right");
            }
            else
            {
                rightfinger = RightFinger;
            }

            nofingers = leftfinger == null;
            this.gameObject.layer = LayerMask.NameToLayer("rvMU");
            if (nofingers)
            {
                RayCastSensor = gameObject.AddComponent<Sensor>();
                RayCastSensor.UseRaycast = true;
                RayCastSensor.RayCastDirection = DirectionFinger;
                RayCastSensor.RayCastLength = GripperWidth;
            }
            else
            {
                RayCastSensor = leftfinger.AddComponent<Sensor>();
                RayCastSensor.UseRaycast = true;
                var globdir = transform.TransformDirection(DirectionClosing);
                RayCastSensor.RayCastDirection = RayCastSensor.transform.InverseTransformDirection(globdir);
                RayCastSensor.RayCastLength = GripperWidth / 5;
            }

            if (rightfinger != null)
            {
                rbright = Global.AddComponentIfNotExisting<Rigidbody>(rightfinger);
                rbright.isKinematic = true;
                rbright.useGravity = false;
            }

            if (leftfinger != null)
            {
                rbleft = Global.AddComponentIfNotExisting<Rigidbody>(leftfinger);
                rbleft.isKinematic = true;
                rbleft.useGravity = false;
            }
            
            RayCastSensor.ShowSensorLinerenderer = false;
            RayCastSensor.AdditionalRayCastLayers = new List<string>();
            RayCastSensor.AdditionalRayCastLayers.Add("rvMUSensor");
            RayCastSensor.AdditionalRayCastLayers.Add("rvMU");
            isclosingnotnull = IsClosing != null;
            isopeningnotnull = IsOpening != null;
            isfullyopenednotnull = IsFullyOpened != null;
            isfullyclosednotnull = IsFullyClosed != null;
            isclosegrippernotnull = CloseGripper != null;
            isopengrippernotnull = OpenGripper != null;

            if (leftfinger != null && rightfinger != null)
            {
                leftstartpos = leftfinger.transform.localPosition;
                rightstartpos = rightfinger.transform.localPosition;
                usefingers = true;
            }
            else
            {
                usefingers = false;
            }

            DirectionClosing = Vector3.Normalize(DirectionClosing);
            
            RayCastSensor.EventEnter += RayCastSensorOnEventEnter;
            RayCastSensor.EventExit += RayCastSensorOnEventExit;
            
            // Grip
            grip = Global.AddComponentIfNotExisting<Grip>(gameObject);
            grip.PartToGrip = RayCastSensor;
            base.Awake();
        }

        private void RayCastSensorOnEventExit(GameObject obj)
        {
            var mu = obj.GetComponent<MU>();
            if (mu == null) return;
            if (mu == muingripper)
                muingripper = null;
        }
        

        private void RayCastSensorOnEventEnter(GameObject obj)
        {
            var mu = obj.GetComponent<MU>();
            if (mu == null) return;
            
            if (usefingers)
            {
                if (Close == true)
                {
                    grippedonclosing = true;
                    GrippedMU = mu;
                    grip.Fix(mu);
                    gripdistancerel = (posabs + RayCastSensor.RayCastDistance) / (GripperWidth / 2);
                }
            }
            else
            {
                muingripper = mu;
                gripdistancerel = 1;
            }
           
        }
        
        
        [Button("Close")]
        public void GripperClose()
        {
            Close = true;
            Open = false;
        }

        public void Stop()
        {
            Close = false;
            Open = false;
        }

        [Button("Open")]
        public void GripperOpen()
        {
            Open = true;
            Close = false;
        }

        
        // Start is called before the first frame update
        void Start()
        {
            FullyOpened = true;
            FullyClosed = false;
            Opening = false;
            Closing = false;
            posrel = 0;
        }

        void UpdateGripper()
        {

            bool isstopped = false;

            if (Open && Close)
                return;
            if (usefingers && Close && grippedonclosing && posrel >= gripdistancerel)
            {
                posrel = gripdistancerel;
                Closing = false;
                isstopped = true;
             
            }

            if (!usefingers && Close && posrel >= gripdistancerel)
            {
                if (muingripper != null && GrippedMU == null)
                {
                    GrippedMU = muingripper;
                    grip.Fix(muingripper);
                    muingripper = null;
                }
            }

            if (Open && GrippedMU != null)
            {
                grip.Unfix(GrippedMU);
                GrippedMU = null;
            }

            if (Open)
            {
                grippedonclosing = false;
                posrel = posrel - ((GripperWidth / 2) / TimeClosing * Time.fixedDeltaTime) / 2;
            }
            
            if (Close && !isstopped)
            {
                posrel = posrel + ((GripperWidth / 2) / TimeClosing * Time.fixedDeltaTime) / 2;
            }

            if (Close && !isstopped)
                Closing = true;
            
            if (Open && posrel <= 0)
            {
                Opening = false;
            }

            if (Close && posrel >= 1)
            {
                Closing = false;
            }

            if (posrel > 1)
                posrel = 1;
            if (posrel < 0)
                posrel = 0;


            if (posrel == 0)
                FullyOpened = true;
            else
                FullyOpened = false;

            if (posrel == 1)
                FullyClosed = true;
            else
                FullyClosed = false;

            // Update Positions
            posabs = (GripperWidth / 2) * posrel;
            if (usefingers)
            {
                leftfinger.transform.localPosition =
                    leftstartpos + DirectionClosing * (posabs + OpenPosOffset) / realvirtualController.Scale;
                rightfinger.transform.localPosition =
                    rightstartpos - DirectionClosing * (posabs + OpenPosOffset) / realvirtualController.Scale;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Get PLCOutput Signals
            if (isclosegrippernotnull)
                Close = CloseGripper.Value;

            if (isopengrippernotnull)
                Open = OpenGripper.Value;

            // Calculate Positions, Status and so on
            UpdateGripper();

            // Set PLCInput Signals
            if (isclosingnotnull)
                IsClosing.Value = Closing;

            if (isopeningnotnull)
                IsOpening.Value = Opening;

            if (isfullyclosednotnull)
                IsFullyClosed.Value = FullyClosed;

            if (isfullyopenednotnull)
                IsFullyOpened.Value = FullyOpened;
            
        }
    }
}