// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using NaughtyAttributes;
using UnityEngine;
using Pixelplacement;
using Object = UnityEngine.Object;
#if REALVIRTUAL_SPLINES
using UnityEngine.Splines;
#endif

using Spline = Pixelplacement.Spline;

namespace realvirtual
{
    public enum ChainOrientation
    {
          Horizontal,
            Vertical
    }    
    #region doc
    //! Chain component creates continuous loop transport systems with elements following spline-defined paths in industrial automation.
    
    //! The Chain component is designed for simulating chain-driven transport systems where individual elements
    //! (links, buckets, carriers, or fixtures) move along a continuous path. It automatically generates and
    //! positions chain elements along spline curves, providing realistic visualization and physics simulation
    //! of chain conveyors, bucket elevators, overhead conveyors, and similar transport mechanisms.
    //!
    //! Key features:
    //! - Automatic generation of chain elements along spline paths
    //! - Support for both Pixelplacement Splines and Unity Splines (Unity 2022.1+)
    //! - Configurable element spacing with automatic or manual distribution
    //! - Horizontal and vertical chain orientations for different applications
    //! - Edit-mode preview for design-time visualization
    //! - Integration with Drive components for speed control
    //! - Scalable chain length with automatic element adjustment
    //! - Support for complex 3D paths including curves and elevation changes
    //!
    //! Common applications in industrial automation:
    //! - Chain conveyors for heavy-duty material transport
    //! - Bucket elevators for vertical material handling
    //! - Overhead power and free conveyors in assembly lines
    //! - Carousel systems for buffering and accumulation
    //! - Pallet transport systems with carriers
    //! - Drag chain conveyors for bulk material
    //! - Accumulating chain conveyors with individual carriers
    //! - Festoon systems for cable management
    //!
    //! The Chain component works by:
    //! 1. Analyzing the spline path to calculate total length
    //! 2. Generating specified number of elements at calculated intervals
    //! 3. Positioning each element along the spline based on normalized position
    //! 4. Updating element positions based on drive speed during simulation
    //! 5. Maintaining proper orientation along the path tangent
    //!
    //! Chain element types and configurations:
    //! - Simple chain links for basic visualization
    //! - Buckets for bucket elevator simulation
    //! - Carriers with fixtures for workpiece transport
    //! - Pallets for automated storage systems
    //! - Custom prefabs for specialized applications
    //!
    //! Integration with other components:
    //! - Requires IChain interface implementation (ChainBelt or ChainPath)
    //! - Connects to Drive component for movement control
    //! - Chain elements implement IChainElement for position updates
    //! - Works with sensors for element detection
    //! - Compatible with MU components for load handling
    //! - Can trigger events based on element positions
    //!
    //! Path definition options:
    //! - Pixelplacement Spline for intuitive path editing
    //! - Unity Splines for advanced curve control
    //! - Support for closed loops and open-ended paths
    //! - Multiple spline segments for complex routing
    //! - Tangent and normal control for element orientation
    //!
    //! Performance considerations:
    //! - Element count affects performance - optimize for visible detail
    //! - Use simplified meshes for chain elements when possible
    //! - Consider LOD systems for large chain installations
    //! - Batch element updates for better performance
    //! - Use appropriate update rates based on chain speed
    //!
    //! The Chain component provides essential functionality for industries requiring
    //! continuous material flow along defined paths, offering both visual accuracy
    //! and functional simulation capabilities.
    //!
    //! For detailed documentation and examples, see:
    //! https://doc.realvirtual.io/components-and-scripts/motion/chain
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/chain")]
    [RequireComponent(typeof(IChain))]
    public class Chain : realvirtualBehavior
    {
        [Tooltip("Orientation of the chain elements along the spline")]
        public ChainOrientation chainOrientation = ChainOrientation.Horizontal; //!< Orientation of the chain (Horizontal or Vertical)
        [Tooltip("Prefab to use as chain element (e.g., chain link, bucket, carrier)")]
        public GameObject ChainElement; //!< Chainelements which needs to be created along the chain
        [Tooltip("Base name for generated chain elements")]
        public string NameChainElement; //!< Name for the chain elements
        [Tooltip("Drive component that controls the chain movement")]
        public Drive ConnectedDrive; //!< The drive which is moving this chain
        [OnValueChanged("Modify")]
        [Tooltip("Number of chain elements to create along the spline")]
        public int NumberOfElements; //!< The number of elements which needs to be created along the chain
        [Tooltip("Starting position offset in mm for the first chain element")]
        public float StartPosition; //!< The start position in millimeters on the chain (offset) for the first element
        [OnValueChanged("Modify")]
        [Tooltip("Create and position chain elements in edit mode (preview)")]
        public bool CreateElementeInEditMode = false; //!< Create chain elements in edit mode
        [Tooltip("Automatically calculate spacing based on chain length and element count")]
        public bool CalculatedDeltaPosition = true; //!< True if the distance (DeltaPosition) between the chain elements should be calculated based on number and chain length

        [Tooltip("Distance in mm between chain elements (manual setting)")]
        public float DeltaPosition; //!< Distance in millimeters between chain elements
        [Tooltip("Scale chain elements to maintain a fixed total length")]
        public bool ScaledOnFixedLength = false; //!< Scale chain to fixed length
        [ShowIf("ScaledOnFixedLength")]
        [Tooltip("Target total length of the chain in mm")]
        public float FixedLength = 1500; //!< Fixed chain length in millimeters
        [ReadOnly] public float Length; //!< The calculated length of the spline in millimeters
        [HideInInspector]public Spline spline;
#if REALVIRTUAL_SPLINES
        [HideInInspector]public SplineContainer splineContainer;
#endif   
        [HideInInspector]public bool unitySplineActive = false;
        private GameObject newbeltelement;
        private IChain ichain;
        [HideInInspector]public bool usepath = false;
        

        private void Init()
        {
            ichain = gameObject.GetComponent<IChain>();
            if (ichain != null)
            {
                usepath = ichain.UseSimulationPath();
            }

            spline = GetComponent<Spline>();
            
#if REALVIRTUAL_SPLINES
            splineContainer = GetComponent<SplineContainer>();
            
            if(spline!=null && splineContainer==null)
            {
                if (CreateElementeInEditMode)
                {
                    var anchors = GetComponentsInChildren<SplineAnchor>();
                    foreach (var currAnchor in anchors)
                    {
                        currAnchor.Initialize();
                    }
                }
            }
            else if(splineContainer!=null && spline==null)
            {
                unitySplineActive = true;
            }
            else
            {
                if(!usepath)
                    Debug.LogError("No Spline or SplineContainer found in Chain");
            }
#else
             if(spline!=null)
            {
                if (CreateElementeInEditMode)
                {
                    var anchors = GetComponentsInChildren<SplineAnchor>();
                    foreach (var currAnchor in anchors)
                    {
                        currAnchor.Initialize();
                    }
                }
            }
            else
            {
                if(!usepath)
                    Debug.LogError("No Spline or SplineContainer found in Chain");
            }
#endif

            if (realvirtualController == null)
                realvirtualController = FindFirstObjectByType<realvirtualController>();
            
            if(ScaledOnFixedLength && FixedLength==0)
                Debug.LogError("FixedLength of "+this.gameObject.name+" is 0. Please set a value for FixedLength");
            
        }
        private void Reset()
        {
            Init();
        }
        
        private void CreateElements()
        {
            if (NameChainElement == "" && ChainElement != null)
                NameChainElement = ChainElement.name;
            var position = StartPosition;
            if (ichain != null)
            {
                Length = ichain.CalculateLength();
                if (!usepath)
                    Length = Length * realvirtualController.Scale;
            }

            if (CalculatedDeltaPosition)
                DeltaPosition = Length / NumberOfElements;
            for (int i = 0; i < NumberOfElements; i++)
            {
                var j = i + 1;
                GameObject newelement;
                if (CreateElementeInEditMode && GetChildByName(NameChainElement + "_" + j)!=null )
                {
                    newelement = GetChildByName(NameChainElement + "_" + j);
                }
                else
                {
                    newelement = Instantiate(ChainElement, ChainElement.transform.parent);
                }
                newelement.transform.parent = this.transform;
                newelement.name = NameChainElement + "_" + j;
                var chainelement = newelement.GetComponent<IChainElement>();
                chainelement.UsePath = usepath;
                chainelement.StartPosition = position;
                chainelement.Position = position;
                chainelement.ConnectedDrive = ConnectedDrive;
                chainelement.Chain = this;
                chainelement.UseUnitySpline = unitySplineActive;
                chainelement.InitPos(position);
                position = position + DeltaPosition;
            }
        }
        
        protected override void AfterAwake()
        {
            Init();
            CreateElements();
            
        }
       public void Modify()
        {
            if (!CreateElementeInEditMode)
            {
                var chainelements = GetComponentsInChildren<ChainElement>();
                foreach (var ele in chainelements)
                {
                    DestroyImmediate(ele.gameObject);
                }
                return;
            }
            Init();
            CreateElements();
        }
       public Vector3 GetPosition(float normalizedposition)
        {
            if (ichain != null)
                return ichain.GetPosition(normalizedposition,true);
            else
            {
                return Vector3.zero;
            }
        }
        public Vector3 GetTangent(float normalizedposition)
        {
            if (ichain != null)
                return ichain.GetDirection(normalizedposition,true);
            else
            {
                return Vector3.zero;
            }
            
        }
        public Vector3 GetUpDirection(float normalizedposition)
        {
            if (ichain != null)
                return ichain.GetUpDirection(normalizedposition,true);
            else
            {
                return Vector3.zero;
            }
        }
        
        
    }
}