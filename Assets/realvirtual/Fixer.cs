// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;


namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/fixer")]
    [SelectionBase]
    #region doc
    //! Fixer component for securing MUs to specific positions or surfaces in the automation system.
    
    //! The Fixer component is an essential tool for position management in realvirtual simulations, providing
    //! mechanisms to secure MUs at specific locations within the production system. It enables precise control
    //! over material positioning, whether for temporary holding, permanent placement, or transfer operations.
    //! The component operates through collision detection or raycast sensing, automatically fixing MUs when
    //! they enter the detection zone and managing their physics state for stable positioning.
    //!
    //! Key Features:
    //! - Dual detection modes: BoxCollider-based or Raycast-based sensing
    //! - Automatic fixing on contact with configurable release conditions
    //! - Alignment capabilities for precise MU positioning with offset control
    //! - Tag-based filtering to restrict fixing to specific MU types
    //! - Minimum distance detection for optimal fixing timing
    //! - Handover blocking to prevent unwanted transfers between fixers
    //! - Visual status indication for debugging and monitoring
    //! - Physics deactivation option for performance optimization
    //!
    //! Common Applications:
    //! - Workstation positioning and material holding
    //! - Conveyor stop gates and accumulation zones
    //! - Pallet positioning on transfer stations
    //! - Buffer storage locations in production lines
    //! - Quality inspection stations requiring stable MU positioning
    //! - Assembly fixtures and jigs
    //! - Loading/unloading positions for AGVs and robots
    //!
    //! Operation Modes:
    //! The Fixer supports multiple operation modes to accommodate different automation scenarios.
    //! In automatic mode (FixMU=true), MUs are fixed immediately upon detection. In signal-controlled
    //! mode, fixing and releasing are controlled through PLC signals, supporting both single-bit
    //! (toggle) and dual-bit (separate fix/release) control schemes. The AlignAndFixOnMinDistance
    //! mode enables intelligent fixing when the MU reaches its closest approach, ideal for moving
    //! conveyors and transfer operations.
    //!
    //! Integration Points:
    //! The Fixer integrates with the MU system for material tracking, supports handover protocols
    //! between multiple fixers, and can be controlled through PLC signals (PLCOutputBool) for
    //! industrial control system integration. It works seamlessly with conveyors, robots, and
    //! other transport systems, managing the transition between moving and stationary states.
    //!
    //! Performance Considerations:
    //! The component efficiently manages fixed MUs through list-based tracking and provides options
    //! for physics optimization. When DeactivatePhysicsWhenPlacing is enabled, released MUs have
    //! their physics disabled, reducing computational overhead in scenarios where movement is not
    //! required. The raycast mode offers precise detection with lower performance impact compared
    //! to collision-based detection for simple geometries.
    //!
    //! Visual Feedback:
    //! When ShowStatus is enabled, the Fixer provides visual feedback through color-coded indicators
    //! (red for empty, green for occupied) and raycast visualization, facilitating debugging and
    //! system monitoring during development and commissioning phases.
    //!
    //! For detailed documentation and examples, visit:
    //! https://doc.realvirtual.io/components-and-scripts/fixer
    #endregion
    [ExecuteAlways]
    public class Fixer : BehaviorInterface, IFix
    {
        [Tooltip("Use raycast detection instead of box collider for detecting MUs")]
        public bool UseRayCast; //!< Use Raycasts instead of Box Collider for detecting parts

        [FormerlySerializedAs("LimitFixToTags")]
        [Tooltip("Only fix objects with these tags (leave empty to fix all)")]
        public List<string> LimitToTags; //!< Limits all Fixing functions to objects with defined Tags

        [Tooltip("Raycast direction")] [ShowIf("UseRayCast")]
        public Vector3 RaycastDirection = new Vector3(1, 0, 0); //!< Raycast direction

        [Tooltip("Length of Raycast in mm, Scale is considered")] [ShowIf("UseRayCast")]
        public float RayCastLength = 100; //!<  Length of Raycast in mm

        [Tooltip("Raycast Layers")] [ShowIf("UseRayCast")]
        public List<string>
            RayCastLayers = new List<string>(new string[] { "rvMU", "rvMUSensor", }); //!< Raycast Layers

        [Tooltip("Automatically fix MUs on contact (disable for signal-controlled fixing)")]
        public bool FixMU = true;

        [Tooltip("Deactivate physics after unfixing the MU")]
        public bool DeactivatePhysicsWhenPlacing = false;

        [Tooltip("Fix MU when it reaches minimum distance (as it starts moving away)")]
        public bool
            AlignAndFixOnMinDistance; //!< true if MU should be fixed or aligned when Distance between MU and Fixer is minimum (distance is increasing again)

        [Tooltip("Align pivot point of MU to Fixer pivot point")]
        public bool AlignMU; //!< true if pivot Points of MU and Fixer should be aligned

        [Tooltip("Position offset in local coordinates for MU alignment")] [ShowIf("AlignMU")]
        public Vector3 DeltaAlign;

        [Tooltip("Tag to apply to MU after fixing")]
        public string SetTagAfterFix;

        [Tooltip("Rotation offset in degrees for MU alignment")] [ShowIf("AlignMU")]
        public Vector3 DeltaRot;

        [Tooltip("Display status of Raycast or BoxCollider")]
        public bool ShowStatus = true; //! true if Status of Collider or Raycast should be displayed

        [Tooltip("Opacity of Mesh in case of status display")] [ShowIf("ShowStatus")] [HideIf("UseRayCast")]
        public float StatusOpacity = 0.2f; //! Opacity of Mesh in case of status display

        [Tooltip("Disable handing over to onother fixer")]
        public bool
            BlockHandingOver; //! if true the fixer will not be able to hand over the MU to another fixer which is colliding with the MU

        [Tooltip("Only controlled by Signal FixerFix - with one bit")]
        public bool OneBitFix; //! Only controlled by Signal FixerFix - with one bit

        [Tooltip("PLCSignal for fixing current MUs and turning Fixer off")]
        public PLCOutputBool FixerFix;

        [Tooltip("PLCSignal for releasing current MUs and turning Fixer off")] [HideIf("OneBitFix")]
        public PLCOutputBool FixerRelease; //! PLCSignal for releasing current MUs and turning Fixer off

        [Tooltip("PLCSignal for blocking handing over to another fixer")]
        public PLCOutputBool SignalBlockHandingOver; // PLCOutpout for BlockHandingOver

        public bool DebugMode = false;
        private bool nextmunotnull;
        public List<MU> MUSEntered;
        public List<MU> MUSFixed;

        private MeshRenderer meshrenderer;
        private int layermask;
        private RaycastHit[] hits;

        private bool lastfix;
        private bool lastfixmu;
        private bool lastrelease;
        private bool signalfixerreleasenotnull;
        private bool signalfixerfixnotnull;
        private bool signalbockhandingovernotnull;
        private bool meshrenderernotnull;
        private bool Deactivated = false;
        private bool publiccaledpicking = false;
        private bool publiccaledplacing = false;

        //! public for Fix all currently colliding MUs
        public void Pick()
        {
            publiccaledpicking = true;
            Fix();
        }

        //! public for Fix all currently colliding MUs
        public void Place()
        {
            publiccaledpicking = false;
            publiccaledplacing = true;
            CheckRelease();
            publiccaledplacing = false;
        }

        // Trigger Enter and Exit from Sensor
        public void OnTriggerEnter(Collider other)
        {
            var mu = other.gameObject.GetComponentInParent<MU>();

            if (LimitToTags.Count > 0)
                if (!LimitToTags.Contains(mu.tag))
                {
                    if (DebugMode)
                        Debug.Log("DebugMode Fixer - MU not in LimitToTags " + mu.name);
                    MUSEntered.Remove(mu);
                    return;
                }

            if (mu != null)
            {
                if (!MUSFixed.Contains(mu) && !MUSEntered.Contains(mu))
                {
                    if (DebugMode)
                        Debug.Log("DebugMode Fixer - MU entered " + mu.name);
                    MUSEntered.Add(mu);
                    mu.FixerLastDistance = -1;
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            var mu = other.gameObject.GetComponentInParent<MU>();
            if (DebugMode && mu != null)
                Debug.Log("DebugMode Fixer - MU OnTriggerExit " + mu.name);
            if (MUSEntered.Contains(mu))
            {
                if (DebugMode)
                    Debug.Log("DebugMode Fixer - MUs in entered is leaving" + mu.name);
                MUSEntered.Remove(mu);
            }
        }

        void Reset()
        {
            if (GetComponent<BoxCollider>())
                UseRayCast = false;
            else
                UseRayCast = true;
        }

        public void CheckRelease()
        {
            var fix = signalfixerfixnotnull && FixerFix.Value && !OneBitFix;
            var release = signalfixerreleasenotnull && FixerRelease.Value;

            if (!FixMU && !fix || release || publiccaledplacing)
            {
                foreach (var mu in MUSFixed.ToArray())
                {
                    Unfix(mu);
                }
            }
        }

        public void Unfix(MU mu)
        {
            if (Deactivated)
                return;

            if (!MUSFixed.Contains(mu))
                return;
            if (DebugMode)
                Debug.Log("DebugMode Fixer - Unfix MU " + mu.name);
            mu.Unfix();
            if (DeactivatePhysicsWhenPlacing)
                mu.PhysicsOff();
            else
                mu.PhysicsOn();
            MUSFixed.Remove(mu);
        }


        private void Fix()
        {
            var mus = MUSEntered.ToArray();
            if (mus.Length == 0)
                return;

            for (int i = 0; i < mus.Length; i++)
            {
                Fix(mus[i]);
            }
        }


        public void DeActivate(bool activate)
        {
            Deactivated = activate;
        }

        public void Fix(MU mu)
        {
            // Check if the operation is valid based on certain conditions
            if (Deactivated || mu.FixedBy != null) return;
            
            // check if this fixer is in the children of the MU
            if(mu.GetComponentsInChildren<Fixer>().Any(child => child.gameObject == gameObject)) 
                return;
            
            // If the AfterFixTag is set, update the tag of the MU
            if (SetTagAfterFix != "")
                mu.tag = SetTagAfterFix;

            // Get the fix value from signal information
            bool fix = signalfixerfixnotnull && FixerFix.Value;

             // If FixMU is not set, and fixing is not needed based on the signal, then return.
            if (!FixMU && !fix && !publiccaledpicking) return;

            Fixer fixer = null;

            // If the MU is already fixed by a fixer, get the fix^r
            if (mu.FixedBy != null)
                fixer = mu.FixedBy.GetComponent<Fixer>();

            // If the fixer is blocking handing over, abort the fix
            if (fixer != null && fixer.BlockHandingOver)
                return;
            
            MUSEntered.Remove(mu);
            
            if (!MUSFixed.Contains(mu))
                MUSFixed.Add(mu);

                // If the alignment option is on, align the MU to the fixer
            if (AlignMU)
            {
                mu.transform.position = transform.position + transform.TransformDirection(DeltaAlign);
                mu.transform.rotation = transform.rotation;
                mu.transform.localRotation = mu.transform.localRotation * Quaternion.Euler(DeltaRot);
            }
            
            if (DebugMode)
                Debug.Log($"DebugMode Fixer - Fix MU {mu.name}");

            mu.Fix(gameObject);
        }


        private void AtPosition(MU mu)
        {
            if (Deactivated)
                return;
            if (mu.LastFixedBy == this.gameObject)
            {
                return;
            }

            var release = false;
            if (signalfixerreleasenotnull)
                release = FixerRelease.Value;

            if (release)
                return;

            /// Only fix if another fixer element has fixed it - don't fix it if it is fixed by a gripper
            var fixedby = mu.FixedBy;
            Fixer fixedbyfixer = null;
            if (fixedby != null)
                fixedbyfixer = mu.FixedBy.GetComponent<Fixer>();

            if (mu.FixedBy == null || (fixedbyfixer != null && fixedbyfixer != this))
            {
                Fix(mu);
            }
        }

        private new void Awake()
        {
            if (!UseRayCast)
            {
                {
                    var collider = GetComponent<BoxCollider>();
                    if (collider == null)
                        Warning("Fixer neeeds a Box Collider attached to if no Raycast is used!",this);
                    else
                    {
                        collider.isTrigger = true;
                    }
                }
            }
            
            // warning if FixMU is on and Signal also connected
            if (FixMU && FixerFix != null)
                Warning("FixMU is on and Signal FixerFix is connected - you should turm FixMU off",this);

            base.Awake();
            layermask = LayerMask.GetMask(RayCastLayers.ToArray());
        }

        private void Start()
        {
            meshrenderer = GetComponent<MeshRenderer>();

            if (!Application.isPlaying)
                return;
            signalfixerreleasenotnull = FixerRelease != null;
            signalfixerfixnotnull = FixerFix != null;
            signalbockhandingovernotnull = SignalBlockHandingOver != null;
            meshrenderernotnull = meshrenderer != null;
            var mus = GetComponentsInChildren<MU>();
            foreach (MU mu in mus)
            {
                if (mu.gameObject != this.gameObject)
                    if (!MUSFixed.Contains(mu))
                        Fix(mu);
            }
        }

        private void Raycast()
        {
            float scale = 1000;
            if (!Application.isPlaying)
            {
                if (Global.realvirtualcontroller != null) scale = Global.realvirtualcontroller.Scale;
            }
            else
            {
                scale = realvirtualController.Scale;
            }

            var globaldir = transform.TransformDirection(RaycastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;
            hits = Physics.RaycastAll(transform.position, globaldir, RayCastLength / scale, layermask,
                QueryTriggerInteraction.UseGlobal);
            if (hits.Length > 0)
            {
                if (ShowStatus) Debug.DrawRay(transform.position, display, Color.red, 0, true);
            }
            else
            {
                if (ShowStatus) Debug.DrawRay(transform.position, display, Color.yellow, 0, true);
            }
        }

        private float GetDistance(MU mu)
        {
            return Vector3.Distance(mu.gameObject.transform.position, this.transform.position);
        }


        void CheckEntered()
        {
            var entered = MUSEntered.ToArray();
            for (int i = 0; i < entered.Length; i++)
            {
                AtPosition(entered[i]);
            }
        }

        void Update()
        {
            if (Deactivated)
                return;
            if (!Application.isPlaying && ShowStatus && UseRayCast)
            {
                Raycast();
            }
        }

        void FixedUpdate()
        {
            if (Deactivated)
                return;

            if (signalbockhandingovernotnull)
                BlockHandingOver = SignalBlockHandingOver.Value;

            var checkrelease = false;
            if (signalfixerreleasenotnull)
                checkrelease = FixerRelease.Value;

            var checkfix = false;
            if (signalfixerfixnotnull)
                checkfix = FixerFix.Value;

            if (UseRayCast)
            {
                Raycast();
                if (hits.Length > 0)
                {
                    MUSEntered.Clear();
                    foreach (var hit in hits)
                    {
                        var mu = hit.collider.GetComponentInParent<MU>();
                        if (mu != null)
                        {
                            if (!MUSFixed.Contains(mu))
                            {
                                bool fix = true;
                                if (LimitToTags.Count > 0)
                                    fix = LimitToTags.Contains(mu.tag);
                                if (fix)
                                    MUSEntered.Add(mu);
                            }
                        }
                    }
                }
                else
                {
                    MUSEntered.Clear();
                }
            }

            if (FixMU && !checkrelease)
                Fix();
            if (!FixMU && !checkfix && MUSFixed.Count > 0 && !signalfixerfixnotnull && !publiccaledpicking)
                CheckRelease();


            if (OneBitFix)
            {
                // One Bit fixer - Fix = true fixes and false = releases - only on signal change
                if (signalfixerfixnotnull)
                {
                    if (FixerFix.Value && !lastfix && !checkrelease)
                    {
                        Fix();
                    }

                    if (!FixerFix.Value && lastfix)
                        CheckRelease();
                    lastfix = FixerFix.Value;
                }
            }
            else
            {
                // Two Bit fixer
                if (signalfixerreleasenotnull)
                {
                    if (FixerRelease.Value && lastrelease == false)
                        CheckRelease();
                    lastrelease = FixerRelease.Value;
                }

                if (signalfixerfixnotnull)
                {
                    if (FixerFix.Value && lastfix == false && !checkrelease)
                        Fix();
                    lastfix = FixerFix.Value;
                }
            }

            if (AlignAndFixOnMinDistance)
            {
                foreach (var mu in MUSEntered.ToArray())
                {
                    var distance = GetDistance(mu);
                    if (distance > mu.FixerLastDistance && mu.FixerLastDistance != -1)
                    {
                        if (DebugMode)
                            Debug.Log("DebugMode Fixer - AlignAndFixOnMinDistance - Mindistance reached " + mu.name);
                        AtPosition(mu);
                    }

                    mu.FixerLastDistance = distance;
                }
            }
            else
            {
                CheckEntered();
            }

            if (meshrenderernotnull)
            {
                if (ShowStatus && !UseRayCast && MUSFixed.Count == 0)
                    meshrenderer.material.color = new Color(1, 0, 0, StatusOpacity);
                else
                    meshrenderer.material.color = new Color(0, 1, 0, StatusOpacity);
            }
        }
    }
}