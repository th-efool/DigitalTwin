// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
#if REALVIRTUAL_AGX
using AGXUnity;
#endif
using Mesh = UnityEngine.Mesh;
using Random = System.Random;

namespace realvirtual
{
    //! Unity event triggered when an MU is created, passing the created MU as parameter
    [System.Serializable]
    public class realvirtualEventMUCreated: UnityEvent<MU>
    {
    }
    
    [SelectionBase]
    #region doc
    //! Source component that generates MUs (Movable Units) during simulation runtime, simulating production systems and material supply points.
    
    //! The Source is a fundamental component in realvirtual for creating MUs (products, parts, materials) that flow through the automation system.
    //! It acts as the starting point of material flow, simulating everything from raw material feeders to production output points.
    //! Sources can generate MUs based on time intervals, distances, PLC signals, or manual triggers.
    //! 
    //! Key Features:
    //! - Template-based MU generation using any GameObject as a prototype
    //! - Multiple generation modes: interval-based, distance-based, signal-based, or manual
    //! - Automatic generation when previous MU reaches a specified distance
    //! - PLC signal integration for controlled generation (PLCGenerate signal)
    //! - Configurable physics properties including mass and center of mass
    //! - Support for random variations in generation timing and positioning
    //! - Batch generation capabilities for creating multiple MUs simultaneously
    //! - Layer management for proper collision detection setup
    //! - Component cleanup to remove unnecessary scripts from generated MUs
    //! 
    //! Generation Modes:
    //! - Interval Generation: Creates MUs at fixed time intervals
    //! - Distance-Based: Generates new MU when last one reaches specified distance
    //! - PLC Controlled: Generation triggered by PLC signals
    //! - Manual: Direct creation through Generate() method calls
    //! - Batch Mode: Creates multiple MUs at once with configurable spacing
    //! 
    //! Common Applications:
    //! - Raw material feeders in production lines
    //! - Product generation at machine outputs
    //! - Box/container suppliers for packaging systems
    //! - Pallet dispensers in warehouse automation
    //! - Part feeders for assembly stations
    //! - Test object generation for simulation scenarios
    //! - Order-based production simulation
    //! - Buffer replenishment systems
    //! 
    //! Advanced Features:
    //! - Random distance variations for realistic material flow
    //! - Maximum MU limits to prevent overflow
    //! - Automatic ID assignment for tracking and traceability
    //! - Support for multiple visual appearances (with PartChanger)
    //! - Configurable spawn positions and destinations
    //! - Automatic hiding of template object during simulation
    //! - Integration with AGX physics engine (Professional version)
    //! 
    //! PLC Integration:
    //! - PLCGenerate: Input signal to trigger MU generation
    //! - PLCIsGenerated: Output signal confirming MU creation
    //! - PLCNumGenerated: Integer output with total count of generated MUs
    //! - Signal-based control for Industry 4.0 scenarios
    //! 
    //! Performance Considerations:
    //! - Efficient GameObject pooling for high-frequency generation
    //! - Automatic component cleanup reduces memory overhead
    //! - Layer-based collision optimization
    //! - Configurable physics settings for performance tuning
    //! 
    //! Events:
    //! - EventMUCreated: Triggered when MU is successfully created
    //! - Provides created MU reference for custom logic integration
    //! 
    //! For detailed documentation see: https://doc.realvirtual.io/components-and-scripts/source
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/source")]
    public class Source : BaseSource,ISignalInterface,IXRPlaceable
    {
        // Public / UI Variablies
        #if REALVIRTUAL_AGX
        public bool UseAGXPhysics;
        #else
        [HideInInspector] public bool UseAGXPhysics=false;
        #endif
        [Header("General Settings")]
        [Tooltip("GameObject to use as template for creating MUs (leave empty to use this GameObject)")]
        public GameObject ThisObjectAsMU; //!< The referenced GameObject which should be used as a prototype for the MU. If it is null it will be this GameObject.
        [Tooltip("Optional destination where MUs will be created (leave empty to create at source position)")]
        public GameObject Destination; //!< The destination GameObject where the generated MU should be placed
        [Tooltip("Enable or disable MU generation")]
        public bool Enabled = true; //!< If set to true the Source is enabled
        [Tooltip("Freezes the source template position during simulation")]
        public bool FreezeSourcePosition = true; //!< If set to true the Source itself (the MU template) is fixed to its position
        [Tooltip("Hide the source template during simulation")]
        public bool DontVisualize = true; //!< True if the Source should not be visible during Simulation time
        [Tooltip("Hide source when simulation is stopped or paused")]
        public bool HideOnStop = true; //!< Hide this source when simulation is stopped / paused;
        [Tooltip("Mass of generated MUs in kilograms")]
        public float Mass = 1; //!< Mass of the generated MU in kilograms.
        [Tooltip("Manually set the center of mass for generated MUs")]
        public bool SetCenterOfMass = false;
        [Tooltip("Center of mass position in local coordinates")]
        public Vector3 CenterOfMass = new Vector3(0,0,0); //!< Center of mass position for the generated MU in local coordinates.
        [Tooltip("Layer name for generated MUs (leave empty to keep default layer)")]
        public string GenerateOnLayer =""; //!< Layer where the MUs should be generated to - if kept empty no layers are changed
        [HideInInspector] public bool ChangeDefaultLayer = true;  //!< If set to true Layers are automatically changed if default Layer is detected
        [ReorderableList]
        [Tooltip("Component names to remove from generated MUs (e.g., Source scripts)")]
        public List<string> OnCreateDestroyComponents = new List<string>(); //!< Destroy these components on MU when MU is created as a copy of the source - is used to delete additional source scripts
        [Header("Create in Intverval (0 if not)")]
        [Tooltip("Delay in seconds before starting interval generation (0 = no delay)")]
        public float StartInterval = 0; //!< Start MU creation with the given seconds after simulation start
        [Tooltip("Time interval in seconds between MU generation (0 = disabled)")]
        public float Interval = 0; //!< Interval in seconds between the generation of MUs. Needs to be set to 0 if no interval generation is wished.

        [Header("Automatic Generation on Distance")]
        [Tooltip("Automatically generate new MU when last MU reaches specified distance")]
        public bool AutomaticGeneration = true; //!< Automatic generation of MUs if last MU is above the GenerateIfDistance distance from MU
        [ShowIf("AutomaticGeneration")]
        [Tooltip("Distance in mm from source to trigger new MU generation")]
        public float GenerateIfDistance = 300; //!< Distance in millimeters from Source when new MUs should be generated.
        [ShowIf("AutomaticGeneration")]
        [Tooltip("Add random variation to generation distance")]
        public bool RandomDistance = false; //!< If turned on Distance is Random Number with plus / minus Range Distance
        [ShowIf("RandomDistance")]
        [Tooltip("Random distance variation range in mm (+/- from base distance)")]
        public float RangeDistance = 300;  //!< Range of the distance in millimeters (plus / minus) if RandomDistance is turned on
        [Header("Number of MUs")]
        [Tooltip("Limit the maximum number of MUs that can be generated")]
        public bool LimitNumber = false;
        [ShowIf("LimitNumber")]
        [Tooltip("Maximum number of MUs to generate")]
        public int MaxNumberMUs = 1;
        [ShowIf("AutomaticGeneration")][ReadOnly]public int Created = 0;
        
        [Header("Source IO's")]
        [Tooltip("Toggle to generate a new MU (set to true to generate)")]
        public bool GenerateMU=true; //!< When changing from false to true a new MU is generated.
        [Tooltip("Toggle to delete all MUs generated by this source (set to true to delete)")]
        public bool DeleteAllMU; //!< When changing from false to true all MUs generated by this Source are deleted.

        [Header("Source Signals")]
        [Tooltip("PLC signal to trigger MU generation")]
        public PLCOutputBool SourceGenerate; //!< When changing from false to true a new MU is generated.
        [Tooltip("PLC signal to enable distance-based MU generation")]
        public PLCOutputBool SourceGenerateOnDistance; //!< When true MUs are generated on Distance
        [Header("Events")] public realvirtualEventMUCreated
            EventMUCreated; //!< Event triggered when a new MU is created

        [HideInInspector] public bool PositionOverwrite = false;
        // Private Variablies
        private bool _generatebefore = false;
        private bool _deleteallmusbefore = false;
        private bool _tmpoccupied;
        private GameObject _lastgenerated;
        private int ID = 0;
        private bool _generatenotnull = false;
        private List<GameObject> _generated = new List<GameObject>();
        private float nextdistance;
        private bool lastgenerateddeleted = false;
        private float xrscale=1;
        private RigidbodyConstraints rbconstraints;
        private bool initautomaticgeneration;
        private bool signaldistancenotnull;
        private bool signalautomaticgenarationnotnull;
        
        //! Deletes all MU generated by this Source
        public void DeleteAll()
        {
            foreach (GameObject obj in _generated)
            {
                Destroy(obj);
            }

            _generated.Clear();
        }
        
        //! Is Celled when MU is deleted which has been created by this source
        public void OnMUDelete(MU mu)
        {
            if (mu.gameObject == _lastgenerated)
            {
                lastgenerateddeleted = true;
            }
        }
        
        //! Deletes all MU generated by this Source
        public void DeleteAllImmediate()
        {
            foreach (GameObject obj in _generated)
            {
                DestroyImmediate(obj);
            }

            _generated.Clear();
        }
        
        //! Event called on Init in XR Space.
        //! IMPLEMENTS IXRPlaceable::OnXRInit
        public void OnXRInit(GameObject placedobj)
        {
            xrscale = placedobj.transform.localScale.x;
        }

        
        //! Event when XR is Starting placing.
        //! IMPLEMENTS IXRPlaceable::OnXRStartPlace
        public void OnXRStartPlace(GameObject placedobj)
        {
            PositionOverwrite = true;
          
        }

        //! Event when XR is Ending placing.
        //! IMPLEMENTS IXRPlaceable::OnXREndPlace
        public void OnXREndPlace(GameObject placedobj)
        {
            PositionOverwrite = false;
            xrscale = placedobj.transform.localScale.x;
        }
        
        
        protected void Reset()
        {
            if (ThisObjectAsMU == null)
            {
                ThisObjectAsMU = gameObject;
            }
        }

        void GenerateInterval()
        {
            if (!PositionOverwrite)
                Generate();
        }

        new void Awake()
        {
            base.Awake();
            var rb = GetComponentInChildren<Rigidbody>();
            if (rb != null && rb.constraints != RigidbodyConstraints.FreezeAll)
            {
                rbconstraints = rb.constraints;
            }
            else
            {
                rbconstraints = RigidbodyConstraints.None;
            }
        }
        protected void Start()
        {

            if (SourceGenerate != null)
            {
                _generatenotnull = true;
                AutomaticGeneration = false;
            }
                
            
            if (SourceGenerateOnDistance != null)
                signaldistancenotnull = true;
            
            if (SourceGenerateOnDistance != null)
                signalautomaticgenarationnotnull = true;
            
            if (ThisObjectAsMU == null)
            {
                ErrorMessage("Object to be created needs to be defined in [This Object As MU]");
            }

            if (ThisObjectAsMU != null)
            {
                if (ThisObjectAsMU.GetComponent<MU>() == null)
                {
                    ThisObjectAsMU.AddComponent<MU>();
                }
            }

            if (Interval > 0)
            {
                InvokeRepeating("GenerateInterval", StartInterval, Interval);
            }

            // Don't show source and don't collide - source is just a blueprint for generating the MUs
            SetVisibility(!DontVisualize);
            SetCollider(false);
            SetFreezePosition(FreezeSourcePosition);
#if REALVIRTUAL_AGX
            if (UseAGXPhysics)
            {
                var rbodies = GetComponentsInChildren<RigidBody>();
                foreach (var rbody in rbodies)
                {
                    rbody.enabled = false;
                }
            }
#endif

            if (GetComponent<Collider>() != null)
            {
                GetComponent<Collider>().enabled = false;
            }
            
            // Deactivate all fixers if included in Source
            var fixers = GetComponentsInChildren<IFix>();
            foreach (var fix in fixers)
            {
                fix.DeActivate(true);
            }

            nextdistance = GenerateIfDistance;
        }

        
        //! For Layout Editor mode Start  is called when the simulation is started
        protected override void OnStartSim()
        {
            if (HideOnStop)
                SetVisibility(true);
            if (DontVisualize)
                 SetVisibility(false);
          
          
        }
        
        //! For Layout Editor mode Stop  is called when the simulation is stopped
        protected override void OnStopSim()
        {
            if (!HideOnStop) Invoke("DelayOnStop",0.1f);
            
        }

        private void DelayOnStop()
        {
            SetVisibility(true);
        }

        
        private void FixedUpdate()
        {
            if (signaldistancenotnull)
                if (AutomaticGeneration == false && SourceGenerateOnDistance.Value == true)
                    initautomaticgeneration = true;
            if (signalautomaticgenarationnotnull)
                AutomaticGeneration = SourceGenerateOnDistance.Value;
            
            // Delete  on Keypressed
            if (Input.GetKeyDown(realvirtualController.HotkeyDelete))
            {
                if (realvirtualController.EnableHotkeys)
                    DeleteAll();
            }
            if (PositionOverwrite)
                return;
            
            if (_generatenotnull)
                GenerateMU = SourceGenerate.Value;

            // Generate on Signal Genarate MU
            if (_generatebefore != GenerateMU)
            {
                if (GenerateMU)
                {
                    _generatebefore = GenerateMU;
                    Generate();
                }
            }

           
            // Handle automatic generation based on distance
            if (AutomaticGeneration)
            {
                if (_lastgenerated != null || lastgenerateddeleted || initautomaticgeneration)
                {
                    float distance = _lastgenerated != null 
                        ? Vector3.Distance(_lastgenerated.transform.position, transform.position) * realvirtualController.Scale / xrscale 
                        : 0;

                    bool create = distance > nextdistance || lastgenerateddeleted || (initautomaticgeneration && _lastgenerated == null);

                    if (create)
                    {
                        lastgenerateddeleted = false;
                        Generate();
                        nextdistance = RandomDistance ? GenerateIfDistance + UnityEngine.Random.Range(-RangeDistance, RangeDistance) : GenerateIfDistance;
                        initautomaticgeneration = false;
                    }
                }
            }
            
            // Generate on Keypressed
            if (Input.GetKeyDown(realvirtualController.HotkeyCreateOnSource))
                Generate();

            if (GenerateMU == false)
                _generatebefore = false;

            if (DeleteAllMU != _deleteallmusbefore && DeleteAllMU == true)
                DeleteAll();
            
            _deleteallmusbefore = DeleteAllMU;
        }

        //! Generates an MU.
        public MU Generate()
        {

#if !REALVIRTUAL_AGX
          UseAGXPhysics = false;
#endif
            if (LimitNumber && (Created >= MaxNumberMUs))
                return null;
            
            if (Enabled)
            {
                GameObject newmu = GameObject.Instantiate(ThisObjectAsMU, transform.position, transform.rotation);
                if (GenerateOnLayer != "")
                    if (LayerMask.NameToLayer(GenerateOnLayer) != -1)
                        newmu.layer = LayerMask.NameToLayer(GenerateOnLayer);

                if (ChangeDefaultLayer)
                {
                    /// Check if still default layer -- if yes then set box collider to g4a MU
                    var box = newmu.GetComponentInChildren<BoxCollider>();
                    if (box != null)
                    {
                        if (box.gameObject.layer == LayerMask.NameToLayer("Default"))
                            box.gameObject.layer = LayerMask.NameToLayer("rvMU");
                    }

                    var mesh = newmu.GetComponentInChildren<MeshCollider>();
                    if (mesh != null)
                    {
                        if (mesh.gameObject.layer == LayerMask.NameToLayer("Default"))
                            mesh.gameObject.layer = LayerMask.NameToLayer("rvMUTransport");
                    }
                }

                Source source = newmu.GetComponent<Source>();

                Created++;
                if (!UseAGXPhysics)
                {
                    Rigidbody newrigid = newmu.GetComponentInChildren<Rigidbody>();
                    if (newrigid == null)
                        newrigid = newmu.AddComponent<Rigidbody>();
                
                    newrigid.mass = Mass;
                    
                    Collider collider = newmu.GetComponentInChildren<Collider>();
                    BoxCollider newboxcollider;
                    if (collider == null)
                    {
                        newboxcollider = newmu.AddComponent<BoxCollider>();
                        MeshFilter mumsmeshfilter = newmu.GetComponentInChildren<MeshFilter>();
                        Mesh mumesh = mumsmeshfilter.mesh;
                        GameObject obj = mumsmeshfilter.gameObject;
                        if (mumesh != null)
                        {
                            Vector3 globalcenter = obj.transform.TransformPoint(mumesh.bounds.center);
                            Vector3 globalsize = obj.transform.TransformVector(mumesh.bounds.size);
                            newboxcollider.center = newmu.transform.InverseTransformPoint(globalcenter);
                            Vector3 size = newmu.transform.InverseTransformVector(globalsize);
                            if (size.x < 0)
                                size.x = -size.x;

                            if (size.y < 0)
                                size.y = -size.y;

                            if (size.z < 0)
                                size.z = -size.z;

                            newboxcollider.size = size;
                        }
                    }
                    else
                    {
                      //  newboxcollider.enabled = true;
                    }
                    newrigid.mass = Mass;
                    if (SetCenterOfMass)
                        newrigid.centerOfMass = CenterOfMass;
                }
                else
                {
#if REALVIRTUAL_AGX
                    // Enable AGX Rigidbodies when newmu is created
                    var rbodies = newmu.GetComponentsInChildren<RigidBody>();
                        foreach (var rbody in rbodies)
                        {
                            rbody.enabled = true;
                        }
#endif
                }

                if (source != null)
                {
                    source.SetVisibility(true);
                    source.SetCollider(true);
                    if (rbconstraints == RigidbodyConstraints.FreezeAll)
                    {
                        source.SetFreezePosition(false);
                    }
                    else
                    {
                        source.SetRbConstraints(rbconstraints);
                    }
                
                    source.Enabled = false;
                    source.enabled = false;
                }

                ID++;
                MU mu = newmu.GetComponent<MU>();
                if (Destination != null)
                    newmu.transform.parent = Destination.transform;
                
                newmu.transform.localScale = this.transform.localScale;
            
                if (mu == null)
                {
                    ErrorMessage("Object generated by source need to have MU script attached!");
                }
                else
                {
                    mu.InitMu(name,ID,realvirtualController.GetMUID(newmu));
                }
                
                mu.CreatedBy = this;

                DestroyImmediate(source);
                // Destroy Additional Components
                foreach (var componentname in OnCreateDestroyComponents)
                {
                    
                    Component[] components = newmu.GetComponents(typeof(Component));
                    foreach(Component component in components)
                    {
                        var ty = component.GetType();
                        if (ty.ToString()==componentname)    
                            Destroy(component);
                    }
                }
                
                // Activate all Fixers if included
                var fixers = mu.GetComponentsInChildren<IFix>();
                foreach (var fix in fixers)
                {
                    fix.DeActivate(false);
                }
            
                _lastgenerated = newmu;
                _generated.Add(newmu);
                EventMUCreated.Invoke(mu);
                var isources = newmu.GetComponents<ISourceCreated>();
                foreach (var isource in isources)
                {
                    isource.OnSourceCreated();
                }
                return mu;
            }

            return null;
        }

    }
}