// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace realvirtual
{
    //! Unity event for MU state changes, passing the MU and state (true/false)
    [System.Serializable]
    public class Game4AutomationEventMU : UnityEvent<MU, bool>
    {
    }

    //! Unity event triggered when an MU is deleted, passing the deleted MU
    [System.Serializable]
    public class Game4AutomationEventMUDelete : UnityEvent<MU>
    {
    }

    //! Unity event for MU fix/unfix operations, passing the MU and fixed state (true=fixed, false=unfixed)
    [System.Serializable]
    public class Game4AutomationEventMUFix : UnityEvent<MU, bool>
    {
    }


    [SelectionBase]
    #region doc
    //! MU (Movable Unit) represents physical objects that move through automation systems as products, parts, or assemblies.
    
    //! The MU is the central component in realvirtual for representing any movable object in the automation simulation.
    //! It can be anything from raw materials, work-in-progress items, finished products, containers, pallets, or any other
    //! object that needs to be transported, processed, or handled within the automation system.
    //! 
    //! Key Features:
    //! - Physics-based movement with automatic Rigidbody configuration
    //! - Seamless transport on conveyor belts and other transport surfaces
    //! - Can be gripped, fixed, and released by robots and handling devices
    //! - Hierarchical loading system (MUs can be loaded onto other MUs, like parts on pallets)
    //! - Unique identification with both local and global IDs for tracking
    //! - Collision detection and sensor interaction
    //! - Multiple visual appearances support for product variants
    //! - Automatic alignment to transport surfaces with configurable smoothing
    //! - Speed interpolation for smooth transitions when released from grippers
    //! - Full lifecycle management from creation at sources to destruction at sinks
    //! 
    //! Transport and Handling:
    //! - Automatically moves on TransportSurface components based on surface speed and direction
    //! - Can be fixed to grippers and other handling devices while maintaining parent-child relationships
    //! - Supports complex material flow with multiple transport surfaces simultaneously
    //! - Maintains velocity information for smooth transitions between transport elements
    //! 
    //! Common Applications:
    //! - Products on assembly lines
    //! - Boxes and packages in logistics systems
    //! - Pallets in warehouse automation
    //! - Parts in manufacturing processes
    //! - Containers in filling and packaging lines
    //! - Workpiece carriers in production systems
    //! - AGV-transported goods
    //! 
    //! Integration Points:
    //! - Sources: Created by Source components with configurable intervals
    //! - Sinks: Destroyed by Sink components at process endpoints
    //! - Sensors: Detected by Sensor components for position tracking
    //! - Grippers: Can be picked and placed by Grip and Fixer components
    //! - Transport: Moved by TransportSurface and TransportGuided components
    //! - PLCs: Tracked and controlled through various signal interfaces
    //! 
    //! Performance Optimization:
    //! - Rigidbody sleep mode for stationary MUs
    //! - Efficient collision layer management
    //! - Optimized parent-child relationship handling
    //! - Smart surface alignment algorithms
    //! 
    //! Events and Tracking:
    //! - EventMUSensor: Triggered when entering/exiting sensor areas
    //! - EventMUDelete: Triggered when MU is destroyed
    //! - EventMUFix: Triggered when MU is fixed or unfixed
    //! - Full tracking of creation source, current fixtures, and transport surfaces
    //! 
    //! For detailed documentation see: https://doc.realvirtual.io/components-and-scripts/mu-movable-unit
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/mu-movable-unit")]
    public class MU : realvirtualBehavior
    {
        #region Public Attributes

        [ReadOnly] public int ID; //!<  ID of this MU (increases on each creation on the MU source
        [ReadOnly] public int GlobalID; //!< Global ID, increases for each MU independent on the source
        [ReorderableList]
        [Tooltip("Different visual appearances for this MU (used with PartChanger)")]
        public List<GameObject> MUAppearences; //!< List of MU appearances for PartChanger
        [ReadOnly] public GameObject FixedBy; //!< Current Gripper which is picking the part
        [ReadOnly] public GameObject LastFixedBy; //!< Last Gripper which has been picking the part
        [ReadOnly] public GameObject LoadedOn; //!< Current Part the MU is loaded on
        [ReadOnly] public GameObject StandardParent; //!< The standard parent Gameobject of the MU
        [ReadOnly] public GameObject ParentBeforeFix; //!< The parent of the MU before the last Grip
        [ReadOnly] public List<Sensor> CollidedWithSensors; //!< The current Sensors the Part is colliding with
        [ReadOnly] public List<MU> LoadedMus; //!< List of MUs which are loaded on this MU
        [ReadOnly] public Source CreatedBy; //!< Source which created this MU
        [Tooltip("Smoothing factor for aligning MU to transport surfaces (higher = smoother)")]
        public float SurfaceAlignSmoothment = 50f; //!< Smoothing factor for surface alignment

        [Tooltip("Transfer interpolated speed to MU when unfixing from gripper")]
        public bool
            UnfixSpeedInterpolate =
                false; //!< When unfixing an MU the interpolated speed of the kinematic Rigidbody will be transferred to the MU

        [ShowIf("UnfixSpeedInterpolate")]
        [Tooltip("Number of speed interpolation samples (more = smoother speed transfer)")]
        public int
            NumInterpolations =
                10; //!< Number of interpolations - specially needed when speed is not constant like when connected to external robot controllers

        [HideInInspector] public float FixerLastDistance; //!< Last distance to fixer in millimeters
        [HideInInspector] public Rigidbody Rigidbody;
        [HideInInspector] public List<TransportSurface> AlignWithSurface = new List<TransportSurface>();
        [ReadOnly] public List<TransportSurface> TransportSurfaces = new List<TransportSurface>();
        [HideInInspector] public float DissolveDuration = 0.5f; //!< Duration in seconds for dissolve effect
        [HideInInspector] public float MaxDissolveValue = 0.2f; //!< Maximum dissolve shader value

        [ReadOnly] public float Velocity; //!< Current velocity in millimeters per second

        #endregion

        private Vector3 lastDirection;
        private List<Material> materials = new List<Material>();
        private Vector3 lastPosition;
        private Vector3 speedvector;
        private bool rigidbodynotnull;
        private float lasttime;
        private int interpolatenum = 0;

        // Deletes all MUs which are loaded on MU as Subcomponent 
        // (but not RigidBodies which are standing on this MU)

        #region Events

        [Foldout("Events")]
        public Game4AutomationEventMUDelete EventMUDeleted; //!< Event is called when MU is Deleted / Destroyed

        [Foldout("Events")]
        public Game4AutomationEventMU EventMUIsLoaded; //!< Event is called when MU is loaded onto another

        [Foldout("Events")]
        public Game4AutomationEventMU EventMUGetsLoad; //!< Event is called when MU gets a MU loaded onto itself

        [Foldout("Events")]
        public Game4AutomationEventMUSensor EventMUSensor; //!< Event is called when MU collides with a Sensor

        [Foldout("Events")]
        public Game4AutomationEventMUFix EventMUFix; //!< Event is called when MU is fixed or unfixed

        #endregion


        #region Public Methods

        //! Places the MU with its bottom on top of the defined position
        public void PlaceMUOnTopOfPosition(Vector3 position)
        {
            Bounds bounds = new Bounds(transform.position, new Vector3(0, 0, 0));

            // Calculate Bounds

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // get bottom center
            var center = new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y, bounds.min.z + bounds.extents.z);

            // get distance from center to bounds
            var distance = transform.position - center;

            transform.position = position + distance;
        }

        //! Load the named MU on this mu
        public void LoadMu(MU mu)
        {
            mu.transform.SetParent(this.transform);
            mu.EventMuLoad();
            LoadedMus.Add(mu);
            EventMUGetsLoad.Invoke(this, true);
        }

        //! Event that this called when MU enters sensor
        public void EventMUEnterSensor(Sensor sensor)
        {
            CollidedWithSensors.Add(sensor);
            EventMUSensor.Invoke(this, true);
        }

        //! Event that this called when MU enters sensor
        public void EventMUExitSensor(Sensor sensor)
        {
            CollidedWithSensors.Remove(sensor);
            EventMUSensor.Invoke(this, false);
        }


        //! Event that this MU is loaded onto another
        public void EventMuLoad()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
            LoadedOn = transform.parent.gameObject;
            EventMUIsLoaded.Invoke(this, true);
        }

        //! Event that this MU is unloaded from another
        public void EventMUUnLoad()
        {
            EventMUIsLoaded.Invoke(this, false);
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
            transform.parent = StandardParent.transform;
            LoadedOn = null;
            Rigidbody.WakeUp();
        }

        //  Init the MU wi MUName and IDs
        public void InitMu(string muname, int localid, int globalid)
        {
            ID = localid;
            GlobalID = globalid;
            name = muname + "-" + ID.ToString();
            ;
            if (transform.parent != null)
            {
                StandardParent = transform.parent.gameObject;
                // Check if MU is created underneath Fixer, if yes directly fix it
                var fixer = transform.parent.gameObject.GetComponent<IFix>();
                if (fixer != null)
                    fixer.Fix(this);
            }
            else
            {
                StandardParent = transform.root.gameObject;
            }

            Rigidbody = GetComponentInChildren<Rigidbody>();
        }

        //! Event that this MU is on Path
        public void EventMuEnterPathSimulation()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
        }

        //! Event that this MU is unloaded from Path
        public void EventMUExitPathSimulation()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
            Rigidbody.WakeUp();
        }

        // Public method for fixing MU to a gameobject
        public void Fix(GameObject fixto)
        {
            if (FixedBy != null)
            {
                var fix = FixedBy.GetComponent<IFix>();
                fix.Unfix(this);
            }
            else
            {
                if (this.transform.parent != null)
                    ParentBeforeFix = this.transform.parent.gameObject;
            }

            transform.SetParent(fixto.transform);
            if (Rigidbody == null)
                Rigidbody = GetComponentInChildren<Rigidbody>();
            Rigidbody.isKinematic = true;
            FixedBy = fixto;
            if (EventMUFix != null)
                EventMUFix.Invoke(this, true);
        }

        //! Public method for unfixing MU to a gameobject, parent changes are done based on parent before fix
        public void Unfix()
        {
            if (EventMUFix != null)
                EventMUFix.Invoke(this, false);

            if (ParentBeforeFix != null)
                transform.SetParent(ParentBeforeFix.transform);
            else
                transform.SetParent(null);
            ParentBeforeFix = null;
            Rigidbody.isKinematic = false;
            Rigidbody.WakeUp();
            if (UnfixSpeedInterpolate)
            {
#if UNITY_6000_0_OR_NEWER
                Rigidbody.linearVelocity = speedvector;
#else
                Rigidbody.velocity = speedvector;
#endif
            }

            FixedBy = null;
        }

        //! Public method for turning Physics off
        public void PhysicsOff()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
        }
        
        //! Public method for turning Physics on
        public void PhysicsOn()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
        }
        
        //! Unloads one of the MUs which are loaded on this MU
        public void UnloadOneMu(MU mu)
        {
            EventMUGetsLoad.Invoke(this, false);
            mu.EventMUUnLoad();
            LoadedMus.Remove(mu);
        }

        //! Unloads all  of the MUs which are loaded on this MU
        public void UnloadAllMUs()
        {
            var tmploaded = LoadedMus.ToArray();
            foreach (var mu in tmploaded)
            {
                UnloadOneMu(mu);
            }
        }

        //! Slowly dissolves MU and destroys it
        public void Dissolve(float duration)
        {
            DissolveDuration = duration;
            if (DissolveDuration > 0)
                StartCoroutine(DissolveCoroutine());
            Invoke("Destroy", DissolveDuration);
        }

        public void Appear(float duration)
        {
            DissolveDuration = duration;
            if (duration > 0)
                StartCoroutine(AppearCoroutine());
        }

        #endregion

        IEnumerator AppearCoroutine()
        {
            float dissolveValue = MaxDissolveValue;
            var duration = DissolveDuration / Time.timeScale;
            while (dissolveValue > 0)
            {
                dissolveValue -= 0.01f;
                foreach (Material mat in materials)
                {
                    mat.SetFloat("_DissolveAmount", dissolveValue);
                    yield return null;
                }

                yield return new WaitForSeconds(duration / 100f);
            }
        }

        IEnumerator DissolveCoroutine()
        {
            float dissolveValue = 0f;
            var duration = DissolveDuration / Time.timeScale;
            while (dissolveValue < MaxDissolveValue)
            {
                dissolveValue += 0.01f;
                foreach (Material mat in materials)
                {
                    mat.SetFloat("_DissolveAmount", dissolveValue);
                    yield return null;
                }

                yield return new WaitForSeconds(DissolveDuration / 100f);
            }
        }


        private void Destroy()
        {
            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            if (CollidedWithSensors != null)
                foreach (var sensor in CollidedWithSensors.ToArray())
                {
                    sensor.OnMUDelete(this);
                }

            if (EventMUDeleted != null)
                EventMUDeleted.Invoke(this);
            if (CreatedBy != null)
                CreatedBy.OnMUDelete(this);
        }

        private void Start()
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in rends)
            {
                materials.Add(rend.material);
            }

            Rigidbody = GetComponentInChildren<Rigidbody>();
            if (Rigidbody == null)
            {
                Warning("No Rigidbody attached to MU - Rigidbody will be automatically created", this);
                Rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            MaxDissolveValue = 0.3f;

            // Check if this MU is loaded on another MU - if yes deactivate Physics on loaded MU
            var loadedon = GetComponentsInParent<MU>();
            // loop through all parents and check if there is another parent than this MU itself
            var hasparent = false;
            foreach (var loaded in loadedon)
            {
                if (loaded != this)
                    hasparent = true;
            }

            if (hasparent)
            {
                if (this.Rigidbody.isKinematic != true)
                {
                    Warning("This MU is loaded on another MU, deactivating Physics for this (loaded) MU ", this);
                    Rigidbody.isKinematic = true;
                }
            }
        }

        public void FixedUpdate()
        {
            if (UnfixSpeedInterpolate && rigidbodynotnull && interpolatenum == NumInterpolations)
            {
                var deltatime = Time.time - lasttime;
                speedvector = (this.transform.position - lastPosition) * 1 / deltatime;
                lastPosition = this.transform.position;
                lasttime = Time.fixedTime;
                interpolatenum = 0;
            }
            else
            {
                interpolatenum++;
            }

            if (AlignWithSurface.Count > 0) // Only align if fully on one surface
            {
                var surface = AlignWithSurface[0];

                var destrot = Quaternion.FromToRotation(transform.up, surface.transform.up) * Rigidbody.rotation;
                Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, destrot,
                    SurfaceAlignSmoothment * Time.fixedDeltaTime);
            }
        }
    }
}