// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace realvirtual
{
    [System.Serializable]
    public class EventMUGrip : UnityEvent<MU, bool>
    {
    }

    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    #region doc
    //! Grip component for attaching and transporting MUs with moving mechanisms like robots or grippers.
    
    //! The Grip component is a fundamental part of realvirtual's material handling system, enabling dynamic
    //! pick-and-place operations in industrial automation simulations. It provides flexible attachment mechanisms
    //! for securely gripping MUs (Material Units) and transporting them through the production system.
    //! The component works by detecting MUs through a sensor, fixing them kinematically or with physics joints,
    //! and maintaining the attachment while the parent object moves through space.
    //!
    //! Key Features:
    //! - Sensor-based MU detection for automatic or controlled gripping
    //! - Kinematic attachment for stable, physics-free transportation
    //! - Optional physics joint connection for dynamic simulations
    //! - Alignment control for precise positioning during pick and place operations
    //! - Direct gripping mode for immediate attachment on sensor detection
    //! - Support for loading MUs as subcomponents onto other MUs
    //! - Single-bit or dual-bit PLC control modes
    //! - Unity events for grip and ungrip notifications
    //!
    //! Common Applications:
    //! - Robotic end effectors and tool changers
    //! - Conveyor transfer mechanisms
    //! - AGV loading/unloading systems
    //! - Palletizing and depalletizing operations
    //! - Assembly line pick-and-place stations
    //! - Material sorting and distribution systems
    //!
    //! Integration Points:
    //! The Grip component integrates seamlessly with other realvirtual components through the sensor system
    //! for MU detection, the MU system for material tracking, and Drive_Cylinder for automated gripping
    //! based on cylinder positions. It can be controlled through PLC signals (PLCOutputBool) for industrial
    //! control system integration or directly through Unity Inspector properties for simulation control.
    //!
    //! Performance Considerations:
    //! The component uses kinematic attachment by default, which is more performant than physics-based
    //! joints. When using physics joints (ConnectToJoint), ensure proper joint configuration to avoid
    //! unstable simulations. The component efficiently manages multiple gripped objects through list-based
    //! tracking and provides immediate response to control signals in FixedUpdate for deterministic behavior.
    //!
    //! Events and Signals:
    //! The EventMUGrip Unity event provides real-time notifications of grip operations, passing the MU
    //! reference and grip state (true for grip, false for ungrip). This enables custom logic execution
    //! during material handling operations, such as updating production tracking systems or triggering
    //! dependent automation sequences.
    //!
    //! For detailed documentation and examples, visit:
    //! https://doc.realvirtual.io/components-and-scripts/grip
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/grip")]
    public class Grip : BaseGrip, IFix
    {
        [Header("Kinematic")] 
        [Tooltip("Sensor that identifies which MUs should be gripped")]
        public Sensor PartToGrip; //!< Identifies the MU to be gripped.

        [Tooltip("Automatically grip MUs when detected by PartToGrip sensor")]
        public bool
            DirectlyGrip = false; //!< If set to true the MU is directly gripped when Sensor PartToGrip detects a Part

        [Tooltip("GameObject to align MUs with before picking (optional)")]
        public GameObject PickAlignWithObject;
        [Tooltip("Align rotation with PickAlignWithObject when picking")]
        public bool AlignRotation = true; //!<  If not null the MUs are aligned with this object before picking.
        [Tooltip("GameObject to align MUs with when placing (optional)")]
        public GameObject PlaceAlignWithObject; //!<  If not null the MUs are aligned with this object after placing.

        [Tooltip("Should be usually kept empty, for very special cases where joint should be used for gripping")]
        public UnityEngine.Joint
            ConnectToJoint; //< Should be usually kept empty, for very special cases where joint should be used for gripping

        [Tooltip("Trigger picking when this sensor is occupied (optional)")]
        public Sensor PickBasedOnSensor; //!< Picking is started when this sensor is occupied (optional)
        [Tooltip("Trigger picking based on cylinder position (optional)")]
        public Drive_Cylinder PickBasedOnCylinder; //!< Picking is stared when Cylinder is Max or Min (optional)
        [Tooltip("Pick when cylinder reaches maximum position, otherwise pick at minimum")]
        public bool PickOnCylinderMax; //!< Picking is started when Cylinderis Max
        [Tooltip("Keep objects kinematic (no physics) after placing")]
        public bool NoPhysicsWhenPlaced = false; //!< Object remains kinematic (no phyisics) when placed

        [Tooltip("Load placed components onto another MU as subcomponents")]
        public bool
            PlaceLoadOnMU = false; //!<  When placing the components they should be loaded onto an MU as subcomponent.

        [Tooltip("Sensor that identifies the target MU for loading placed components")]
        public Sensor PlaceLoadOnMUSensor; //!<  Sensor defining the MU where the picked MUs should be loaded to.

        [Header("Pick & Place Control")]
        [Tooltip("Enable picking of MUs identified by the sensor")]
        public bool PickObjects = false; //!< true for picking MUs identified by the sensor.

        [Tooltip("Enable placing of currently gripped MUs")]
        public bool PlaceObjects = false; //!< //!< true for placing the loaded MUs.

        [Header("Events")] 
        [Tooltip("Unity event triggered on grip (true) and ungrip (false) with MU reference")]
        public EventMUGrip
            EventMUGrip; //!<  Unity event which is called for MU grip and ungrip. On grip it passes MU and true. On ungrip it passes MU and false.

        [Header("PLC IOs")]
        [Tooltip("Use single bit control (true) or two separate bits for pick/place (false)")]
        public bool
            OneBitControl =
                false; //!< If true the grip is controlled by one bit. If false the grip is controlled by two bits.

        [Tooltip("PLC signal to control picking operation")]
        public PLCOutputBool SignalPick;
        [HideIf("OneBitControl")] 
        [Tooltip("PLC signal to control placing operation (when using two-bit control)")]
        public PLCOutputBool SignalPlace;

        [HideInInspector] public List<GameObject> PickedMUs;

        private bool _issignalpicknotnull;
        private bool _issignalplacenotnull;
        private bool Deactivated = false;
        private bool _pickobjectsbefore = false;
        private bool _placeobjectsbefore = false;
        private List<FixedJoint> _fixedjoints;

        //! Picks the GameObject obj
        public void DeActivate(bool activate)
        {
            Deactivated = activate;
        }

        //! Picks the GameObject obj
        public void Fix(MU mu)
        {
            if (Deactivated)
                return;

            var obj = mu.gameObject;
            if (PickedMUs.Contains(obj) == false)
            {
                if (mu == null)
                {
                    ErrorMessage("MUs which should be picked need to have the MU script attached!");
                    return;
                }

                if (ConnectToJoint == null)
                    mu.Fix(this.gameObject);

                if (PickAlignWithObject != null)
                {
                    obj.transform.position = PickAlignWithObject.transform.position;
                    if (AlignRotation)
                          obj.transform.rotation = PickAlignWithObject.transform.rotation;
                }

                if (ConnectToJoint != null)
                    ConnectToJoint.connectedBody = mu.Rigidbody;

                PickedMUs.Add(obj);
                if (EventMUGrip != null)
                    EventMUGrip.Invoke(mu, true);
            }
        }

        //! Places the GameObject obj
        public void Unfix(MU mu)
        {
            if (Deactivated)
                return;

            var obj = mu.gameObject;
            var tmpfixedjoints = _fixedjoints;
            var rb = mu.Rigidbody;
            if (EventMUGrip != null)
                EventMUGrip.Invoke(mu, false);

            if (PlaceAlignWithObject != null)
            {
                obj.transform.position = PlaceAlignWithObject.transform.position;
                obj.transform.rotation = PlaceAlignWithObject.transform.rotation;
            }

            if (ConnectToJoint == null)
                mu.Unfix();

            if (ConnectToJoint != null)
                ConnectToJoint.connectedBody = null;

            if (PlaceLoadOnMUSensor == null)
            {
                if (!NoPhysicsWhenPlaced)
                    if (rb != null)
                        rb.isKinematic = false;
                    else
                        Warning("No Rigidbody for MU which is unfixed", this);
            }

            if (PlaceLoadOnMUSensor != null)
            {
                if (PlaceLoadOnMUSensor.LastTriggeredBy != null)
                {
                    var loadmu = PlaceLoadOnMUSensor.LastTriggeredBy.GetComponent<MU>();
                    if (loadmu == null)
                    {
                        ErrorMessage("You can only load parts on parts which are of type MU, please add to part [" +
                                     PlaceLoadOnMUSensor.LastTriggeredBy.name + "] MU script");
                    }

                    loadmu.LoadMu(mu);
                }
            }

            PickedMUs.Remove(obj);
        }

        //! Picks al objects collding with the Sensor
        public void Pick()
        {
            if (Deactivated)
                return;

            if (PartToGrip != null)
            {
                // Attach all objects with fixed joint - if not already attached
                foreach (GameObject obj in PartToGrip.CollidingObjects)
                {
                    var pickobj = GetTopOfMu(obj);
                    if (pickobj == null)
                        Warning("No MU on object for gripping detected", obj);
                    else
                        Fix(pickobj);
                }
            }
            else
            {
                ErrorMessage(
                    "Grip needs to define with a Sensor which parts to grip - no [Part to Grip] Sensor is defined");
            }
        }

        //! Places all objects
        public void Place()
        {
            if (Deactivated)
                return;

            var tmppicked = PickedMUs.ToArray();
            foreach (var mu in tmppicked)
            {
                if (mu != null)
                    Unfix(mu.GetComponent<MU>());
            }
        }

        private void Reset()
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        // Use this for initialization
        private void Start()
        {
            PickedMUs = new List<GameObject>();
            _issignalpicknotnull = SignalPick != null;
            _issignalplacenotnull = SignalPlace != null;
            if (PartToGrip == null)
            {
                Error("Grip Object needs to be connected with a sensor to identify objects to pick", this);
            }

            _fixedjoints = new List<FixedJoint>();
            GetComponent<Rigidbody>().isKinematic = true;

            if (PickBasedOnSensor != null)
            {
                PickBasedOnSensor.EventEnter += PickBasedOnSensorOnEventEnter;
            }

            if (DirectlyGrip == true)
            {
                PartToGrip.EventEnter += PickBasedOnSensorOnEventEnter;
            }

            if (PickBasedOnSensor != null)
            {
                PickBasedOnSensor.EventExit += PickBasedOnSensorOnEventExit;
            }


            if (PickBasedOnCylinder != null)
            {
                if (PickOnCylinderMax)
                {
                    PickBasedOnCylinder.EventOnMin += Place;
                    PickBasedOnCylinder.EventOnMax += Pick;
                }
                else
                {
                    PickBasedOnCylinder.EventOnMin += Pick;
                    PickBasedOnCylinder.EventOnMax += Place;
                }
            }
        }

        private void PickBasedOnSensorOnEventExit(GameObject obj)
        {
            var mu = obj.GetComponent<MU>();
            if (mu != null)
                Unfix(mu);
        }

        private void PickBasedOnSensorOnEventEnter(GameObject obj)
        {
            var mu = obj.GetComponent<MU>();
            if (mu != null)
                Fix(mu);
        }


        private void FixedUpdate()
        {
            if (Deactivated)
                return;

            if (_issignalpicknotnull)
            {
                PickObjects = SignalPick.Value;
            }

            if (_issignalplacenotnull)
            {
                PlaceObjects = SignalPlace.Value;
            }

            if (OneBitControl)
                PlaceObjects = !PickObjects;

            if (_pickobjectsbefore == false && PickObjects)
            {
                Pick();
            }

            if (_placeobjectsbefore == false && PlaceObjects)
            {
                Place();
            }

            _pickobjectsbefore = PickObjects;
            _placeobjectsbefore = PlaceObjects;
        }
    }
}