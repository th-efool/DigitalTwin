// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz


using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;
#if REALVIRTUAL_AGX
using AGXUnity;
#endif

namespace realvirtual
{
    #region doc
    //! TransportSurface simulates conveyor belts and transport systems for moving objects through industrial automation processes.
    
    //! The TransportSurface component is the core element for creating material flow simulation in realvirtual. It provides
    //! physics-based movement of objects along defined transport paths, supporting both linear conveyors and rotational
    //! systems like turntables. The surface works by applying forces or velocity changes to objects that come into
    //! contact with it, simulating real conveyor belt behavior.
    //!
    //! Key features:
    //! - Physics-based transport using Unity's physics engine or optional AGX physics
    //! - Support for both linear (straight conveyors) and radial (turntables, rotary tables) movement
    //! - Texture animation synchronized with transport speed for visual feedback
    //! - Automatic speed and direction inheritance from connected Drive components
    //! - Constraint management for stabilizing objects during transport
    //! - Guide path support for complex curved conveyor systems
    //! - Parent drive support for nested transport systems (e.g., conveyors on moving platforms)
    //! - Advanced surface visualization with optional detailed belt geometry
    //! - XR/AR placement support for interactive factory layout planning
    //!
    //! Common applications in industrial automation:
    //! - Assembly line conveyors for product transport
    //! - Sorting and distribution systems with multiple branches
    //! - Accumulation conveyors with zone control
    //! - Transfer stations between different transport levels
    //! - Turntables and rotary indexing tables for orientation changes
    //! - Pallet transport systems in warehouses and production
    //! - Material handling in packaging and palletizing systems
    //!
    //! Integration with other components:
    //! - Connects to Drive components for speed and direction control
    //! - Works with Sensor components for object detection
    //! - Integrates with TransportGuides for curved path following
    //! - Compatible with MU (Material Unit) components for tracked objects
    //! - Supports Kinematic groups for complex geometry handling
    //! - Can be controlled via PLC signals through Drive behaviors
    //!
    //! Performance considerations:
    //! - Use box colliders instead of mesh colliders for better performance
    //! - Enable texture animation only when visual feedback is needed
    //! - Consider using simplified collision meshes for complex conveyor geometry
    //! - Group multiple conveyor sections under single TransportSurface when possible
    //! - Use the rvTransport layer for optimized physics calculations
    //! - For high-speed conveyors, ensure adequate fixed timestep settings
    //!
    //! The TransportSurface automatically handles coordination with the Drive component's movement,
    //! ensuring synchronized transport speed and direction. It supports both editor-time preview
    //! and runtime simulation, making it ideal for virtual commissioning scenarios.
    //!
    //! For detailed documentation and examples, see:
    //! https://doc.realvirtual.io/components-and-scripts/motion/transportsurface
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/transportsurface")]
    public class TransportSurface : BaseTransportSurface, IGuidedSurface, IXRPlaceable
    {
        #region Public Variables

        [HideInInspector] private Drive _drive; //!< The associated drive component controlling this transport surface
        
        //! Gets or sets the Drive component controlling this transport surface
        public Drive Drive 
        { 
            get => _drive;
            set => SetDrive(value);
        }
#if REALVIRTUAL_AGX
        [Tooltip("Uses AGX physics engine for transport simulation instead of Unity physics")]
        public bool UseAGXPhysics; //!< Uses AGX physics engine for transport simulation instead of Unity physics
#else
        [HideInInspector] public bool UseAGXPhysics = false;
#endif


        [ReadOnly]
        public Vector3
            TransportDirection; //!< Transport direction in local coordinates, automatically set by the Drive component

        [Tooltip("Enables texture animation to visualize surface movement")]
        public bool AnimateSurface = true; //!< Enables texture animation to visualize surface movement

        [Tooltip("Enables advanced conveyor belt visualization with detailed geometry")]
        [OnValueChanged("ToggleAdvancedSurface")]
        public bool AdvancedSurface = false; //!< Enables advanced conveyor belt visualization with detailed geometry

        [Tooltip("Texture animation speed multiplier in texture units per meter")] [ShowIf("AnimateSurface")]
        public float TextureScale = 1; //!< Texture animation speed multiplier in texture units per meter 

        [Tooltip("Modifies rigidbody constraints when objects enter the surface")]
        public bool ChangeConstraintsOnEnter = false; //!< Modifies rigidbody 2constraints when objects enter the surface

        [Tooltip("Rigidbody constraints applied when objects enter")] [ShowIf("ChangeConstraintsOnEnter")]
        public RigidbodyConstraints ConstraintsEnter; //!< Rigidbody constraints applied when objects enter

        [Tooltip("Modifies rigidbody constraints when objects leave the surface")]
        public bool ChangeConstraintsOnExit = false; //!< Modifies rigidbody constraints when objects leave the surface

        [Tooltip("Rigidbody constraints applied when objects exit")] [ShowIf("ChangeConstraintsOnExit")]
        public RigidbodyConstraints ConstraintsExit; //!< Rigidbody constraints applied when objects exit
        
        [Tooltip("Automatically set based on Drive direction (RotationX/Y/Z = true, Linear = false)")]
        [ReadOnly] public bool Radial = false; //!< Enables radial/rotational transport mode for turntables and curved conveyors

        
        [Tooltip("Optional drive reference for special cases - normally leave null to use auto-detected parent drive")]
        public Drive DriveReference; //!< Optional drive reference for special cases - normally leave null to use auto-detected parent drive
        
        [ReadOnly] public float speed = 0; //!< Current transport speed in millimeters per second

        [ReadOnly]
        public float SpeedScaleTransportSurface = 1; //!< Speed scaling factor applied to the transport surface

        [ReadOnly] public bool IsGuided = false; //!< Indicates if surface follows a guide path system

        [Tooltip("Physics layer for transport surface collision detection")]
        [InfoBox("Standard Setting for layer is rvTransport")]
        [OnValueChanged("RefreshReferences")]
        public string Layer = "rvTransport"; //!< Physics layer for transport surface collision detection

        [Tooltip("Uses mesh collider for precise collision, box collider for better performance")]
        [InfoBox("For Best performance unselect UseMeshCollider, for good transfer between conveyors select this")]
        [OnValueChanged("RefreshReferences")]
        public bool
            UseMeshCollider = false; //!< Uses mesh collider for precise collision, box collider for better performance

        [Tooltip("Enables visual debugging information during runtime")]
        public bool DebugMode = false; //!< Enables visual debugging information during runtime

        [Tooltip("Parent drive for hierarchical movement systems, transport surface moves relative to parent")]
        public Drive
            ParentDrive; //!< Parent drive for hierarchical movement systems, transport surface moves relative to parent

        public delegate void
            OnEnterExitDelegate(Collision collission,
                TransportSurface surface); //!< Delegate for collision enter/exit events

        public event OnEnterExitDelegate OnEnter; //!< Event triggered when objects enter the transport surface
        public event OnEnterExitDelegate OnExit; //!< Event triggered when objects leave the transport surface

        #endregion

        #region Private Variables

        private MeshRenderer _meshrenderer;
        private Collider _collider;
        private Rigidbody _rigidbody;
        private bool _rigidbodyNotNull;
        private bool _isMeshrendererNotNull;
        private bool parentDriveNotNull;
        private Transform _parent;
        [HideInInspector] public Vector3 parentposbefore;
        private Quaternion parentrotbefore;
        private Quaternion parentstartrot;
        private Quaternion startrot;
        private Quaternion startglobalrot;
        private IGuide guide;
        #pragma warning disable 0414 // Suppress "field assigned but never used" warning
        private bool isxrplacing = false;
        #pragma warning restore 0414
        
        private bool driveNotNull = false;

        private GameObject _xrscaleRoot;
        private float _xrscaleFactor = 1;
        
        // Texture animation state
        private float lastmovX = 0;
        private float lastmovY = 0;
        
        #pragma warning disable 0414 // Suppress "field assigned but never used" warning
        private float _textureRotationAngle = 0f;
        #pragma warning restore 0414
        private Material _materialInstance;
        private static readonly string[] TexturePropertyNames = { "_MainTex", "_BaseMap", "_BaseColorMap", "_AlbedoMap" };


        [ReadOnly]
        public List<Rigidbody>
            LoadedPart = new List<Rigidbody>(); //!< List of rigidbodies currently on the transport surface

        [HideInInspector]
        public Vector3 StartGlobalTransportDirection; //!< Initial global transport direction stored at start

        #endregion

        #region Public Methods

        //! Checks if the transport surface is following a guide path.
        public bool IsSurfaceGuided()
        {
            return guide?.IsActive() ?? false;
        }

        //! Gets the drive component controlling this transport surface.
        public Drive GetDrive()
        {
            return _drive;
        }

        //! Initializes the transport surface for XR/AR placement.
        public void OnXRInit(GameObject placedobj)
        {
            _xrscaleRoot = placedobj;
            _xrscaleFactor = ComputeScaleFactor();
        }

        //! Handles the start of XR/AR placement mode.
        public void OnXRStartPlace(GameObject placedobj)
        {
            _xrscaleRoot = placedobj;
            isxrplacing = true;
            ForceStop = true;
        }

        //! Handles the end of XR/AR placement mode.
        public void OnXREndPlace(GameObject placedobj)
        {
            isxrplacing = false;
            ForceStop = false;
            _xrscaleFactor = ComputeScaleFactor();
        }

        //! Gets the transport direction at the closest point to the given position in global coordinates.
        public Vector3 GetClosestDirection(Vector3 position)
        {
            if (IsGuided && guide != null)
                return guide.GetClosestDirection(position);
            return transform.TransformDirection(TransportDirection);
        }

        //! Gets the closest point on the transport surface to the given position.
        public Vector3 GetClosestPoint(Vector3 position)
        {
            if (IsGuided)
                return guide.GetClosestPoint(position);

            return position;
        }

        //! Gets the bounds of the transport surface including kinematic groups.
        public Bounds GetTransportSurfaceBounds()
        {
            // Try to get bounds from collider first (runtime)
            var collider = gameObject.GetComponent<Collider>();
            if (collider != null && collider.bounds.size != Vector3.zero)
                return collider.bounds;

            // Check if we have a Kinematic component with group integration
            var kinematic = GetComponent<Kinematic>();
            if (kinematic != null && kinematic.IntegrateGroupEnable && !string.IsNullOrEmpty(kinematic.GroupName))
            {
                // Use Groups utility to calculate bounds from kinematic group
                var bounds = Groups.GetGroupBounds(kinematic.GroupName);
                if (bounds.size != Vector3.zero)
                    return bounds;
            }

            // If no kinematic group or no bounds found, use local renderers
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = new Bounds(transform.position, Vector3.zero);
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                        bounds.Encapsulate(renderer.bounds);
                }
                
                if (bounds.size != Vector3.zero)
                    return bounds;
            }

            // Return small default bounds at transform position
            Logger.Warning("Unable to calculate proper TransportSurface bounds. Using small default bounds.", this, false);
            return new Bounds(transform.position, Vector3.one * 0.1f);
        }

        //! Gets the center point on top of the transport surface for object placement.
        public Vector3 GetMiddleTopPoint()
        {
            if (gameObject == null)
                return Vector3.zero;

            var bounds = GetTransportSurfaceBounds();
            return new Vector3(bounds.center.x, bounds.center.y + bounds.extents.y, bounds.center.z);
        }

        //! Triggers the OnEnter event when an object collides with the transport surface.
        public void OnEnterSurface(Collision other)
        {
            OnEnter?.Invoke(other, this);
        }

        //! Triggers the OnExit event when an object leaves the transport surface.
        public void OnExitSurface(Collision other)
        {
            OnExit?.Invoke(other, this);
        }

        #endregion

        #region Private Methods

        private Vector3 ComputeAdvancedMeshBounds(MeshFilter mf, Transform targetTransform)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            Vector3[] vertices = mf.sharedMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = mf.transform.TransformPoint(vertices[i]);
                v = targetTransform.InverseTransformPoint(v);
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }

            return max - min;
        }
        
        private Vector3 ComputeAdvancedBoundsFromTransportSurface(Transform targetTransform)
        {
            Bounds bounds = GetTransportSurfaceBounds();
            
            // Convert global bounds to local dimensions for target transform
            Vector3 size = bounds.size;
            Vector3 localSize = targetTransform.InverseTransformVector(size);
            
            // Return absolute values to ensure positive dimensions
            return new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        }

        private void ToggleAdvancedSurface()
        {
            if (AdvancedSurface)
            {
                Logger.Message("Advanced Surface enabled", this);

                GameObject prefab = UnityEngine.Resources.Load<GameObject>("ConveyorBelt");
                if (prefab == null)
                {
                    Logger.Error("ConveyorBelt prefab not found in Resources. Advanced Surface requires this prefab.", this);
                    AdvancedSurface = false;
                    return;
                }

                GameObject instance = Instantiate(prefab, transform);
                instance.name = "Belt";

                // Use the robust bounds calculation that supports kinematic groups
                Bounds bounds = GetTransportSurfaceBounds();
                Vector3 center = bounds.center;
                
                instance.transform.position = center;

                Vector3 globalForward = transform.TransformDirection(TransportDirection);
                Vector3 globalUp = transform.TransformDirection(Vector3.up);
                instance.transform.rotation = Quaternion.LookRotation(globalForward, globalUp);

                // Calculate dimensions using the new method that supports kinematic groups
                Vector3 dimensions = ComputeAdvancedBoundsFromTransportSurface(instance.transform);
                
                ConveyorBelt belt = instance.GetComponent<ConveyorBelt>();
                if (belt == null)
                {
                    Logger.Error("ConveyorBelt component not found on prefab. Advanced Surface requires this component.", this);
                    if (Application.isPlaying)
                        Destroy(instance);
                    else
                        DestroyImmediate(instance);
                    AdvancedSurface = false;
                    return;
                }

                belt.SetDimensions(dimensions.z, dimensions.x, dimensions.y);
                instance.transform.parent = transform;

                // Hide original renderers - handle both direct and kinematic group scenarios
                HideOriginalRenderers();
            }
            else
            {
                Logger.Message("Advanced Surface disabled", this);
                
                // Find and destroy the belt
                Transform beltTransform = transform.Find("Belt");
                if (beltTransform != null)
                {
                    GameObject conveyorBelt = beltTransform.gameObject;
                    if (Application.isPlaying)
                        Destroy(conveyorBelt);
                    else
                        DestroyImmediate(conveyorBelt);
                }

                // Show original renderers
                ShowOriginalRenderers();
            }
        }
        
        private void HideOriginalRenderers()
        {
            // Check if we have a direct MeshRenderer
            MeshRenderer directRenderer = GetComponent<MeshRenderer>();
            if (directRenderer != null)
            {
                directRenderer.enabled = false;
                return;
            }
            
            // Check if we're using kinematic groups
            var kinematic = GetComponent<Kinematic>();
            if (kinematic != null && kinematic.IntegrateGroupEnable && !string.IsNullOrEmpty(kinematic.GroupName))
            {
                // Hide all renderers in the kinematic group
                var renderers = Groups.GetRenderersFromGroup(kinematic.GroupName);
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                        renderer.enabled = false;
                }
            }
            else
            {
                // Fallback: Hide all child renderers
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                        renderer.enabled = false;
                }
            }
        }
        
        private void ShowOriginalRenderers()
        {
            // Check if we have a direct MeshRenderer
            MeshRenderer directRenderer = GetComponent<MeshRenderer>();
            if (directRenderer != null)
            {
                directRenderer.enabled = true;
                return;
            }
            
            // Check if we're using kinematic groups
            var kinematic = GetComponent<Kinematic>();
            if (kinematic != null && kinematic.IntegrateGroupEnable && !string.IsNullOrEmpty(kinematic.GroupName))
            {
                // Show all renderers in the kinematic group
                var renderers = Groups.GetRenderersFromGroup(kinematic.GroupName);
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                        renderer.enabled = true;
                }
            }
            else
            {
                // Fallback: Show all child renderers
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                        renderer.enabled = true;
                }
            }
        }

        private void CombineMesh()
        {
            var _mesh = GetComponent<MeshCollider>();
            // save this mesh position and rotation
            var pos = this.transform.position;
            var rot = this.transform.rotation;
            this.transform.rotation = Quaternion.identity;
            this.transform.position = Vector3.zero;
            if (_mesh.sharedMesh == null)
            {
                MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[meshFilters.Length];
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                }

                Mesh newMesh = new Mesh();
                newMesh.CombineMeshes(combine);
                _mesh.sharedMesh = newMesh;
            }

            this.transform.position = pos;
            this.transform.rotation = rot;
        }

        private void CreateMeshCollider()
        {
            // Try kinematic collider first for MeshCollider requests
            if (TryUseKinematicCollider())
                return;

            var mesh = GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
            
            // Combine submeshes if no mesh filter on this object
            if (GetComponent<MeshFilter>() == null)
            {
                CombineMesh();
            }
        }

        private void CreateBoxCollider()
        {
            if (TryUseKinematicCollider())
                return;

            var box = GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
            
            // Setup box bounds if no mesh filter on this object
            if (GetComponent<MeshFilter>() == null)
            {
                SetupBoxColliderBounds(box);
            }
        }
        
        private bool TryUseKinematicCollider()
        {
            var kinematic = GetComponent<Kinematic>();
            return kinematic != null && kinematic.IntegrateGroupEnable && TryUseKinematicGroupCollider(kinematic);
        }
        
        private void SetupBoxColliderBounds(BoxCollider box)
        {
            if (box == null)
            {
                Logger.Warning("BoxCollider is null in SetupBoxColliderBounds. Setting standard collider but this will break simulation physics!", this, false);
                return;
            }
            
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                // No renderers found, use small default bounds
                box.center = Vector3.zero;
                box.size = Vector3.one * 0.1f;
                Logger.Warning("No renderers found for TransportSurface bounds calculation. Using small default bounds (0.1m) - this will affect transport physics accuracy!", this, false);
                return;
            }
            
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    bounds.Encapsulate(renderer.bounds);
            }

            // Check if bounds are valid
            if (bounds.size == Vector3.zero)
            {
                box.center = Vector3.zero;
                box.size = Vector3.one * 0.1f;
                Logger.Warning("Calculated bounds have zero size. Using small default bounds.", this, false);
                return;
            }

            // Transform bounds to local space
            var localCenter = transform.InverseTransformPoint(bounds.center);
            var localSize = transform.InverseTransformVector(bounds.size);

            box.center = localCenter;
            box.size = localSize;
        }

        private void RefreshReferences()
        {
            _meshrenderer = gameObject.GetComponent<MeshRenderer>();
            
            // Handle editor-time collider switching for same GameObject only
            if (!Application.isPlaying)
            {
                HandleEditorColliderSwitching();
                return;
            }

            if (UseAGXPhysics)
            {
                RemoveUnityPhysics();
                return;
            }

            SetupUnityPhysics();
        }
        
        private void HandleEditorColliderSwitching()
        {
            // Only switch colliders on the same GameObject, don't interfere with kinematic groups
            // This provides immediate visual feedback when toggling UseMeshCollider in editor
            
            if (UseMeshCollider)
            {
                // User wants MeshCollider - remove BoxCollider if it exists on this GameObject
                var box = GetComponent<BoxCollider>();
                if (box != null)
                {
                    Logger.Message($"Switching from BoxCollider to MeshCollider on {gameObject.name}", this);
                    DestroyImmediate(box);
                    
                    // Add MeshCollider if no collider exists now
                    if (GetComponent<Collider>() == null)
                    {
                        var meshCollider = gameObject.AddComponent<MeshCollider>();
                        Logger.Message($"Added MeshCollider to {gameObject.name}", this);
                    }
                }
            }
            else
            {
                // User wants BoxCollider - remove MeshCollider if it exists on this GameObject
                var mesh = GetComponent<MeshCollider>();
                if (mesh != null)
                {
                    Logger.Message($"Switching from MeshCollider to BoxCollider on {gameObject.name}", this);
                    DestroyImmediate(mesh);
                    
                    // Add BoxCollider if no collider exists now
                    if (GetComponent<Collider>() == null)
                    {
                        var boxCollider = gameObject.AddComponent<BoxCollider>();
                        Logger.Message($"Added BoxCollider to {gameObject.name}", this);
                    }
                }
            }
            
            // Update layer if collider was added
            gameObject.layer = LayerMask.NameToLayer(Layer);
        }
        
        private void RemoveUnityPhysics()
        {
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (Application.isPlaying)
                    Destroy(rb);
                else
                    DestroyImmediate(rb);
            }
        }
        
        private void SetupUnityPhysics()
        {
            var existingMesh = GetComponent<MeshCollider>();
            var existingBox = GetComponent<BoxCollider>();
            
            // Remove wrong collider type and track what was removed
            bool removedMesh = false;
            bool removedBox = false;
            
            if (UseMeshCollider)
            {
                if (existingBox != null)
                {
                    if (Application.isPlaying)
                        Destroy(existingBox);
                    else
                        DestroyImmediate(existingBox);
                    removedBox = true;
                }
            }
            else
            {
                if (existingMesh != null)
                {
                    if (Application.isPlaying)
                        Destroy(existingMesh);
                    else
                        DestroyImmediate(existingMesh);
                    removedMesh = true;
                }
            }

            // Create correct collider type - account for removed colliders
            if (UseMeshCollider && (existingMesh == null || removedMesh))
                CreateMeshCollider();
            else if (!UseMeshCollider && (existingBox == null || removedBox))
                CreateBoxCollider();

            // Setup rigidbody
            SetupRigidbody();
            
            // Get collider reference and check layer
            // In play mode, we need to get the correct collider type since Destroy() doesn't happen immediately
            if (Application.isPlaying)
            {
                if (UseMeshCollider)
                    _collider = gameObject.GetComponent<MeshCollider>();
                else
                    _collider = gameObject.GetComponent<BoxCollider>();
            }
            else
            {
                _collider = gameObject.GetComponent<Collider>();
            }
            CheckColliderLayer();
        }
        
        private void SetupRigidbody()
        {
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
                if (_rigidbody == null)
                {
                    Logger.Warning("Unable to add Rigidbody component to TransportSurface. Transport physics will not work correctly!", this, false);
                    _rigidbodyNotNull = false;
                    return;
                }
            }
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbodyNotNull = true;
        }


        private void Reset()
        {
            gameObject.layer = LayerMask.NameToLayer(Layer);
            // Only set layer in editor mode, don't create colliders
            if (!Application.isPlaying)
            {
                _meshrenderer = gameObject.GetComponent<MeshRenderer>();
                // Set initial Radial value based on Drive
                UpdateRadialFromDrive();
            }
            else
            {
                RefreshReferences();
            }
        }
        
        private void OnValidate()
        {
            // Keep Radial synchronized with Drive direction in editor
            if (!Application.isPlaying)
            {
                UpdateRadialFromDrive();
            }
        }
        
        // Sets the Drive and updates related properties
        public void SetDrive(Drive newDrive)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                NotifyDriveChange(_drive, newDrive);
                _drive = newDrive;
                
                if (_drive != null)
                {
                    _drive.OnTransportSurfaceAdded();
                    UpdateRadialFromDrive();
                }
            }
            else
            #endif
            {
                _drive = newDrive;
            }
        }
        
        #if UNITY_EDITOR
        private void NotifyDriveChange(Drive oldDrive, Drive newDrive)
        {
            if (oldDrive != null && oldDrive != newDrive)
            {
                oldDrive.OnTransportSurfaceRemoved();
            }
        }
        #endif
        
        private void UpdateRadialFromDrive()
        {
            // Use DriveReference if set, otherwise find Drive component if not already assigned
            if (_drive == null)
            {
                if (DriveReference != null)
                    SetDrive(DriveReference);
                else
                    SetDrive(GetComponentInParent<Drive>());
            }
                
            if (_drive != null)
            {
                // Check if Drive direction is rotational
                bool shouldBeRadial = (_drive.Direction == DIRECTION.RotationX || 
                                     _drive.Direction == DIRECTION.RotationY || 
                                     _drive.Direction == DIRECTION.RotationZ);
                
                // Update Radial if it doesn't match
                if (Radial != shouldBeRadial)
                {
                    Radial = shouldBeRadial;
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
                    #endif
                }
            }
        }

        [Button("Destroy Transport Surface")]
        private void DestroyTransportSurface()
        {
            Object.DestroyImmediate(this);
        }

        protected new void Awake()
        {
            base.Awake();
            // Use DriveReference if set, otherwise search in parent hierarchy
            if (DriveReference != null)
                SetDrive(DriveReference);
            else
                SetDrive(GetComponentInParent<Drive>());

            // Check if Drive was found and log warning if missing
            driveNotNull = _drive != null;

            if (driveNotNull)
            {
                _drive.AfterDriveStartInit += AfterDriveStartInit;
            }
            else
            {
                Logger.Warning("TransportSurface could not find a Drive component. Please add a Drive component to this GameObject or its parent hierarchy, or assign DriveReference.", this);
            }

            guide = GetComponentInChildren<IGuide>();
            IsGuided = guide != null;
            if (ParentDrive != null || IsGuided || OnEnter != null || OnExit != null ||
                ChangeConstraintsOnEnter || ChangeConstraintsOnExit)
            {
                Global.AddComponentIfNotExisting<TransportsurfaceCollider>(this.gameObject);
                LoadedPart.Clear();
            }

            base.Awake();
        }

        private void AfterDriveStartInit(Drive drive)
        {
            TransportDirection = transform.InverseTransformDirection(drive.GetGlobalDirection());
            StartGlobalTransportDirection = drive.GetGlobalDirection();
            Radial = drive.IsRotation;
            SpeedScaleTransportSurface = drive.SpeedScaleTransportSurface;
            
            // Create material instance for radial surfaces if needed
            if (Radial && _isMeshrendererNotNull && AnimateSurface && _materialInstance == null)
            {
                _materialInstance = new Material(_meshrenderer.sharedMaterial);
                _meshrenderer.material = _materialInstance;
                
                if (DebugMode)
                    Logger.Message("Created material instance in AfterDriveStartInit for radial animation", this);
            }
        }


        void InitDirections()
        {
            parentstartrot = ParentDrive != null ? ParentDrive.transform.localRotation : Quaternion.identity;
            startrot = transform.localRotation;
            startglobalrot = transform.rotation;
        }

        void Start()
        {
            // Apply layer and create colliders when starting play mode
            gameObject.layer = LayerMask.NameToLayer(Layer);
            RefreshReferences();

            parentposbefore = Vector3.zero;
            parentrotbefore = Quaternion.identity;
            parentDriveNotNull = ParentDrive != null;
            _isMeshrendererNotNull = _meshrenderer != null;
            InitDirections();
            
            // Material instance creation moved to AfterDriveStartInit for radial surfaces
            // This ensures Radial property is set before creating the instance

#if REALVIRTUAL_AGX
            if (UseAGXPhysics)
            {
                var rb = GetComponent<RigidBody>();
                if (rb == null)
                {
                    Logger.Warning("Transportsurface using AGX: Expecting an AGX RigidBody component.", this);
                    return;
                }

                if (GetComponent<AGXUnity.Collide.Box>() == null && GetComponent<AGXUnity.Collide.Mesh>() == null &&
                    GetComponent<AGXUnity.Collide.Sphere>() == null &&
                    GetComponent<AGXUnity.Collide.Cylinder>() == null && GetComponent<AGXUnity.Collide.Plane>() == null)
                {
                    Logger.Warning("Transportsurface using AGX: Expecting an AGX Shape Collider component.", this);
                    return;
                }

                Simulation.Instance.ContactCallbacks.OnContact(OnContact, rb);
            }
#else
            UseAGXPhysics = false;
#endif
        }

#if REALVIRTUAL_AGX
        private bool OnContact(ref ContactData data)
        {
            if (Radial)
            {
                Logger.Error("Radial AGX Transport Surfaces are not yet supported", this, false);
            }
            else
            {  
                var global = transform.TransformDirection(TransportDirection);
                foreach (ref var point in data.Points)
                {
                    
                    point.SurfaceVelocity =
 -global * speed * realvirtualController.SpeedOverride / realvirtualController.Scale;
                }
            }

            return true;
        }
#endif

        float ComputeScaleFactor() => _xrscaleRoot?.transform.localScale.x ?? 1f;

        void Update()
        {
            UpdateTextureAnimation();
        }
        
        private void UpdateTextureAnimation()
        {
            if (!ShouldUpdateTexture())
                return;

            if (Radial)
                UpdateRadialTextureAnimation();
            else
                UpdateLinearTextureAnimation();
        }
        
        private bool ShouldUpdateTexture()
        {
            return !ForceStop && speed != 0 && _isMeshrendererNotNull && AnimateSurface;
        }
        
        private void UpdateLinearTextureAnimation()
        {
            float speedFactor = TextureScale * Time.deltaTime * speed * _xrscaleFactor *
                realvirtualController.SpeedOverride / realvirtualController.Scale;

            Vector3 localTransportDir = TransportDirection.normalized;
            Vector2 uvOffset = new Vector2(localTransportDir.x, localTransportDir.z) * speedFactor;
            _meshrenderer.material.mainTextureOffset += uvOffset;
        }
        
        private void UpdateRadialTextureAnimation()
        {
            // For rotational drives, speed is already in degrees/second
            float angularSpeedDegrees = speed * realvirtualController.SpeedOverride;
            Vector3 localDir = TransportDirection.normalized;
            float direction = Mathf.Sign(localDir.y); // For Y-axis rotation
            
            // Convert to texture movement
            float rotationSpeed = angularSpeedDegrees / 360.0f; // Convert to revolutions per second
            float movement = rotationSpeed * Time.deltaTime * _xrscaleFactor;
            
            // Update texture offset
            lastmovX += movement * direction;
            lastmovX = Mathf.Repeat(lastmovX, 1.0f); // Simpler wrapping
            
            LogRadialDebugInfo(angularSpeedDegrees, movement, direction);
            
            // Apply texture offset
            if (_materialInstance != null)
            {
                ApplyTextureOffset(new Vector2(lastmovX, lastmovY));
            }
            else if (DebugMode)
            {
                Logger.Warning("No material instance created! Check if Radial mode was set before Start()", this);
            }
        }
        
        private void LogRadialDebugInfo(float speed, float movement, float direction)
        {
            if (!DebugMode) return;
            
            Logger.Message($"Radial texture rotation: speed={speed:F2}°/s, movement={movement:F6}, offset=({lastmovX:F4},{lastmovY:F4})", this);
            Logger.Message($"Material: {_materialInstance?.name ?? "null"}, Direction={direction}", this);
        }
        
        private void ApplyTextureOffset(Vector2 offset)
        {
            foreach (string propName in TexturePropertyNames)
            {
                if (_materialInstance.HasProperty(propName))
                {
                    _materialInstance.SetTextureOffset(propName, offset);
                    
                    if (DebugMode)
                    {
                        Vector2 currentOffset = _materialInstance.GetTextureOffset(propName);
                        Vector2 currentScale = _materialInstance.GetTextureScale(propName);
                        Logger.Message($"Set offset on property '{propName}' to ({offset.x:F4},{offset.y:F4}), verified: ({currentOffset.x:F4},{currentOffset.y:F4})", this);
                        Logger.Message($"Texture scale: ({currentScale.x:F2},{currentScale.y:F2}), Shader: {_materialInstance.shader.name}", this);
                    }
                    return;
                }
            }
            
            if (DebugMode)
            {
                Logger.Warning($"Material doesn't have any known texture properties! Shader: {_materialInstance.shader?.name ?? "null"}", this);
            }
        }


        void FixedUpdate()
        {
            if (!ShouldUpdatePhysics())
                return;

            if (Radial)
                UpdateRadialPhysics();
            else
                UpdateLinearPhysics();
        }
        
        private bool ShouldUpdatePhysics()
        {
            if (!driveNotNull) return false;
            
            speed = _drive.CurrentSpeed;
            
            return !((speed == 0 && !parentDriveNotNull) || ForceStop || UseAGXPhysics || IsGuided);
        }
        
        private void UpdateLinearPhysics()
        {
            if (parentDriveNotNull)
                UpdateLinearWithParentDrive();
            else if (speed != 0)
                UpdateSimpleLinearPhysics();
        }
        
        private void UpdateSimpleLinearPhysics()
        {
            if (!_rigidbodyNotNull) return;
            
            var dirglobal = transform.TransformDirection(TransportDirection);
            var movement = CalculateLinearMovement(dirglobal);
            
            _rigidbody.position = _rigidbody.position - movement;
            Physics.SyncTransforms();
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }
        
        private Vector3 CalculateLinearMovement(Vector3 direction)
        {
            return direction * Time.fixedDeltaTime * speed * _xrscaleFactor *
                   realvirtualController.SpeedOverride / realvirtualController.Scale;
        }
        
        private void UpdateLinearWithParentDrive()
        {
            InitializeParentTracking();
            
            if (!HasParentMoved() && speed == 0)
                return;

            var dir = StartGlobalTransportDirection;
            var deltarot = parentrotbefore * Quaternion.Inverse(parentstartrot);
            var movement = deltarot * CalculateLinearMovement(dir);
            
            var parentDelta = CalculateParentDelta();
            ApplyLinearMovementWithParent(movement, parentDelta, deltarot);
            
            UpdateParentTracking();
        }
        
        private void InitializeParentTracking()
        {
            if (parentposbefore == Vector3.zero)
                parentposbefore = ParentDrive.transform.position;
            if (parentrotbefore == Quaternion.identity)
                parentrotbefore = ParentDrive.transform.localRotation;
        }
        
        private bool HasParentMoved()
        {
            return parentposbefore != ParentDrive.transform.position ||
                   parentrotbefore != ParentDrive.transform.localRotation;
        }
        
        private (Vector3 deltaUp, Vector3 deltaArea) CalculateParentDelta()
        {
            var parentpos = ParentDrive.transform.position;
            var deltaparent = parentpos - parentposbefore;
            var deltaUp = GetVertikalMov(deltaparent);
            var deltaArea = deltaparent - deltaUp;
            return (deltaUp, deltaArea);
        }
        
        private void ApplyLinearMovementWithParent(Vector3 movement, (Vector3 deltaUp, Vector3 deltaArea) parentDelta, Quaternion deltarot)
        {
            if (!_rigidbodyNotNull) return;
            
            var dirtotal = movement + parentDelta.deltaArea;
            var dirback = -movement;
            
            if (DebugMode)
            {
                var debugPos = transform.position + new Vector3(0, 0.5f, 0);
                Global.DebugDrawArrow(debugPos, parentDelta.deltaUp * 1000, Color.cyan);
                Global.DebugDrawArrow(debugPos, parentDelta.deltaArea * 1000, Color.green);
                Global.DebugDrawArrow(debugPos, dirtotal * 1000, Color.red);
            }
            
            _rigidbody.position = _rigidbody.position + dirback;
            Physics.SyncTransforms();
            _rigidbody.MovePosition(_rigidbody.position + dirtotal + parentDelta.deltaUp);
            _rigidbody.MoveRotation(startglobalrot * deltarot.normalized);
        }
        
        private void UpdateParentTracking()
        {
            parentposbefore = ParentDrive.transform.position;
            parentrotbefore = ParentDrive.transform.localRotation;
        }
        
        private void UpdateRadialPhysics()
        {
            if (ParentDrive != null)
            {
                Logger.Error("Radial conveyor with parent drive not implemented!", this, false);
                return;
            }
            
            if (speed == 0 || !_rigidbodyNotNull) return;
            
            var dirglobal = transform.TransformDirection(TransportDirection);
            var localAxis = transform.InverseTransformVector(dirglobal);
            var angle = speed * Time.fixedDeltaTime * realvirtualController.SpeedOverride * SpeedScaleTransportSurface;

            _rigidbody.rotation = _rigidbody.rotation * Quaternion.AngleAxis(-angle, localAxis);
            _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.AngleAxis(angle, localAxis));
        }

        private Vector3 GetVertikalMov(Vector3 deltacomplete)
        {
            // Isolate movement along primary axes to prevent rounding errors
            if (Vector3.Angle(deltacomplete, Vector3.up) == 0)
                return new Vector3(0, deltacomplete.y, 0);
                
            if (Vector3.Angle(deltacomplete, Vector3.right) == 0)
                return new Vector3(deltacomplete.x, 0, 0);
                
            return new Vector3(0, 0, deltacomplete.z);
        }

        private bool TryUseKinematicGroupCollider(Kinematic kinematic)
        {
            // Try to find the first collider in the group using Groups utility
            var collider = Groups.GetFirstColliderInGroup(kinematic.GroupName);
            if (collider != null)
            {
                _collider = collider;

                // Ensure the TransportSurface GameObject has a rigidbody for physics simulation
                SetupRigidbody();
                
                // Check if the kinematic group collider is on the correct layer
                CheckColliderLayer();
                
                return true;
            }

            // No collider found in group, create a mesh collider surrounding the full group
            Logger.Message($"No collider found in Kinematic group '{kinematic.GroupName}', creating MeshCollider for the group", this);

            var meshCollider = Groups.CreateMeshColliderForGroup(gameObject, kinematic.GroupName);
            if (meshCollider != null)
            {
                _collider = meshCollider;
                // Ensure the TransportSurface GameObject has a rigidbody for physics simulation
                SetupRigidbody();
                
                // Check if the created mesh collider is on the correct layer
                CheckColliderLayer();
                
                return true;
            }

            return false;
        }

        
        private void CheckColliderLayer()
        {
            if (_collider != null)
            {
                int expectedLayer = LayerMask.NameToLayer(Layer);
                if (_collider.gameObject.layer != expectedLayer)
                {
                    Logger.Warning($"Collider on '{_collider.gameObject.name}' is on layer '{LayerMask.LayerToName(_collider.gameObject.layer)}' " +
                                   $"instead of the expected '{Layer}' layer. This may cause performance issues or objects falling through the transport surface. " +
                                   $"Please ensure the collider GameObject is on the '{Layer}' layer for optimal performance.", this);
                }
            }
        }
        
        private void OnDestroy()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                _drive?.OnTransportSurfaceRemoved();
            #endif
            
            // Clean up material instance to prevent memory leaks
            if (_materialInstance != null)
                DestroyImmediate(_materialInstance);
        }

        #endregion
    }
}