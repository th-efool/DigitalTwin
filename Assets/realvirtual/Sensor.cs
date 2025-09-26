// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace realvirtual
{
    //! Unity event for MU sensor interactions, passing the MU and occupied state (true=enter, false=exit)
    [System.Serializable]
    public class Game4AutomationEventMUSensor : UnityEvent<MU, bool>
    {
    }

    //! Unity event for GameObject sensor interactions, passing the GameObject and occupied state (true=enter, false=exit)
    [System.Serializable]
    public class Game4AutomationEventGameobjectSensor : UnityEvent<GameObject, bool>
    {
    }
    
    [SelectionBase]
    #region doc
    //! Detects MUs and GameObjects using collider or raycast methods with PLC signal integration for industrial automation.

    //! The Sensor is a fundamental component in realvirtual for implementing detection and presence sensing in automation systems.
    //! It simulates various types of industrial sensors including proximity sensors, photoelectric sensors, and vision sensors.
    //! The sensor can detect MUs entering or leaving its detection area and communicate this information to control systems.
    //! 
    //! Key Features:
    //! - Two detection modes: Collider-based (volume detection) or Raycast-based (beam detection)
    //! - Configurable detection filtering by MU tags or names for selective sensing
    //! - Visual feedback through material changes showing occupied/not occupied states
    //! - PLC signal integration with both normally open (SensorOccupied) and normally closed (SensorNotOccupied) outputs
    //! - Unity Events for custom scripting integration (EventEnter and EventExit)
    //! - Support for detecting both MUs and standard GameObjects
    //! - Real-time visualization of sensor state in Unity Editor and runtime
    //! - Debugging features including pause-on-detection and status display
    //! 
    //! Detection Modes:
    //! - Collider Mode: Uses Unity trigger colliders for volumetric detection areas
    //! - Raycast Mode: Simulates beam-type sensors with configurable direction and length
    //! 
    //! Common Applications:
    //! - Conveyor belt position detection
    //! - Part presence verification at workstations
    //! - Safety light curtains and barriers
    //! - End position detection for drives and cylinders
    //! - Counting and tracking of products
    //! - Collision avoidance for AGVs and robots
    //! - Quality control and inspection stations
    //! 
    //! The Sensor component integrates with:
    //! - PLC systems through PLCInputBool signals
    //! - Drive components for limit switch functionality
    //! - Transport surfaces for product tracking
    //! - Grip and Fixer components for part detection
    //! - Custom control logic through Unity Events
    //! 
    //! Performance Considerations:
    //! - Use layer filtering to optimize raycast performance
    //! - Consider using tag filtering to reduce unnecessary detections
    //! - Disable visual feedback (DisplayStatus) in production builds for better performance
    //! 
    //! For detailed documentation see: https://doc.realvirtual.io/components-and-scripts/sensor
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/sensor")]
    [ExecuteInEditMode]
    public class 
        Sensor : BaseSensor, ISignalInterface
    {
        // Public - UI Variables 
        [BoxGroup("Settings")]
        [Tooltip("Show sensor status by changing material color")]
        public bool DisplayStatus = true; 
        [BoxGroup("Settings")]
        [Tooltip("Only detect MUs with this tag or name (leave empty to detect all)")]
        public string
            LimitSensorToTag; //!< Limits the function of the sensor to a certain MU tag - also MU names are working
        [BoxGroup("Settings")]
        [Tooltip("Use raycast detection instead of collider")]
        public bool UseRaycast = false;

        [BoxGroup("Settings")] [ShowIf("UseRaycast")]
        [Tooltip("Direction of the raycast in local coordinates")]
        public Vector3 RayCastDirection = new Vector3(1,0,0);
        [BoxGroup("Settings")] [ShowIf("UseRaycast")]
        [Tooltip("Maximum raycast detection distance in mm")]
        public float RayCastLength=1000f; //!< Raycast length in millimeters
        [BoxGroup("Settings")] [ShowIf("UseRaycast")]
        [Tooltip("Width of the raycast visualization line")]
        public float RayCastDisplayWidth = 0.01f; //!< Raycast display line width in Unity units
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] [ReorderableList]
        [Tooltip("Additional layers to include in raycast detection")]
        public List<string> AdditionalRayCastLayers = new List<string>();
        [BoxGroup("Settings")] [ShowIf("UseRaycast")]
        [Tooltip("Display the raycast line in the scene")]
        public bool ShowSensorLinerenderer = true;
      

        [BoxGroup("Settings")]
        [Tooltip("Material to display when sensor is occupied")]
        public Material MaterialOccupied; //!<  Material for displaying the occupied status.
        [BoxGroup("Settings")]
        [Tooltip("Material to display when sensor is not occupied")]
        public Material MaterialNotOccupied; //!<  Material for displaying the not occupied status.
        [BoxGroup("Settings")]
        [Tooltip("Pause simulation when sensor becomes occupied (for debugging)")]
        public bool PauseOnSensor = false; //!<  Pause simulation if sensor is getting high - can be used for debuging
        [BoxGroup("Interface Connection")]
        [Tooltip("PLC input signal - true when sensor is occupied")]
        public PLCInputBool SensorOccupied; //!< Boolean PLC input for the Sensor signal - is true if the Sensor is  occuied..
        [BoxGroup("Interface Connection")]
        [Tooltip("PLC input signal - true when sensor is NOT occupied")]
        public PLCInputBool SensorNotOccupied; //!< Boolen PLC input for the Sensor - is true if the Sensor is NOT occuied. 
      
   
        
        [Foldout("Events")] public Game4AutomationEventMUSensor
            EventMUSensor; //!<  Unity event which is called for MU enter and exit. On enter it passes MU and true. On exit it passes MU and false.
        [Foldout("Events")]  public Game4AutomationEventGameobjectSensor    
        EventNonMUGameObjectSensor; //!<  Unity event which is called for non MU objects enter and exit. On enter it passes gameobject (on which the collider was detected) and true. On exit it passes gameobject and false.

        [Foldout("Status")] public bool Occupied = false; //!<  True if sensor is occupied.
        [Foldout("Status")] public GameObject LastTriggeredBy; //!< Last MU which has triggered the sensor.
        [Foldout("Status")] [ShowIf("UseRaycast")] public float RayCastDistance; //!< Last RayCast Distance in millimeters if Raycast is used
        [Foldout("Status")] public int LastTriggeredID; //!< Last MUID which has triggered the sensor.
        [Foldout("Status")] public int LastTriggeredGlobalID; //!<  Last GloabalID which has triggerd the sensor.
        [Foldout("Status")] public int Counter;
        [Foldout("Status")] public int ColliderCounter;
        [Foldout("Status")] public List<MU> CollidingMus = new List<MU>(); // Currently colliding MUs with the sensor.
        [Foldout("Status")] public List<GameObject>
            CollidingObjects = new List<GameObject>(); // Currently colliding GameObjects with the sensor (which can be more than MU because a MU can contain several GameObjects.


        public delegate void
            OnEnterDelegate(GameObject obj); //!< Delegate function for GameObjects entering the Sensor

        public event OnEnterDelegate EventEnter; //!< Event triggered when a GameObject enters the sensor

        public delegate void OnExitDelegate(GameObject obj); //!< Delegate function for GameObjects leaving the Sensor

        public event OnExitDelegate EventExit; //!< Event triggered when a GameObject exits the sensor


        // Private Variables
        private bool _occupied = false;
        private MeshRenderer _meshrenderer;
        private BoxCollider _boxcollider;
        private int layermask;
        [SerializeField] [HideInInspector] private float scale = 1000;
        private RaycastHit hit;
        private RaycastHit lasthit;
        private bool raycasthasthit;
        private bool lastraycasthasthit;
        private bool raycasthitchanged;
        private Vector3 startposraycast;
        private Vector3 endposraycast;
        
        private LineRenderer linerenderer;
        private bool _isNotOccupiedNotNull;
        private bool _isOccupiedNotNull;
        private bool _isNetworkControlled = false;
        
        //! Sets the sensor to be controlled by network synchronization
        public void SetNetworkControlled()
        {
            _isNetworkControlled = true;
        }

        //! Manually sets the occupied state of the sensor
        public void SetOccupied(bool value)
        {
            Occupied = value;

            if (_meshrenderer != null)
            {

                if (Occupied)
                {
                    _occupied = true;
                    if (DisplayStatus && _meshrenderer != null)
                    {
                        _meshrenderer.material = MaterialOccupied;
                    }
                }
                else
                {
                    _occupied = false;
                    if (DisplayStatus && _meshrenderer != null)
                    {
                        _meshrenderer.material = MaterialNotOccupied;
                    }
                }

            }

            if (UseRaycast)
            {
                raycasthasthit = value;
            }
        }
        
        //! Delete all MUs in Sensor Area.
        public void DeleteMUs()
        {
            var tmpcolliding = CollidingObjects;
            foreach (var obj in tmpcolliding.ToArray())
            {
                var mu = GetTopOfMu(obj);
                if (mu != null)
                {
                    Destroy(mu.gameObject);
                }

                CollidingObjects.Remove(obj);
            }
        }


        // Use this when Script is inserted or Reset is pressed
        private void Reset()
        {
            AdditionalRayCastLayers = new List<string>();
            AdditionalRayCastLayers.Add("rvMU");
            AdditionalRayCastLayers.Add("rvMUSensor");
            if (MaterialOccupied == null)
            {
                MaterialOccupied = UnityEngine.Resources.Load("Materials/SensorOccupiedRed", typeof(Material)) as Material;
            }

            if (MaterialNotOccupied == null)
            {
                MaterialNotOccupied = UnityEngine.Resources.Load("Materials/SensorNotOccupied", typeof(Material)) as Material;
            }
    
            _boxcollider = GetComponent<BoxCollider>();
            if (_boxcollider != null)
                _boxcollider.isTrigger = true;
            else
                UseRaycast = true;
        }

        // Use this for initialization
        private void Start()
        {
            _isOccupiedNotNull = SensorOccupied != null;
            _isNotOccupiedNotNull = SensorNotOccupied != null;
            CollidingObjects = new List<GameObject>();
            CollidingMus = new List<MU>();
            if (LimitSensorToTag == null)
                LimitSensorToTag = "";
            _boxcollider = GetComponent<BoxCollider>();
            if (_boxcollider != null )
            {
                _meshrenderer = _boxcollider.gameObject.GetComponent<MeshRenderer>();
            }

            if (_boxcollider == null && !UseRaycast && Application.isPlaying)
            {
                ErrorMessage("Sensors which are not using a Raycast need to have a BoxCollider on the same Gameobject as this Sensor script is attached to");
            }
     
            if (Application.isPlaying)
            { 
                scale = realvirtualController.Scale;
                AdditionalRayCastLayers.Add(LayerMask.LayerToName(gameObject.layer));
                // create line renderer for raycast if not existing

                if (UseRaycast && ShowSensorLinerenderer)
                {
                    linerenderer = GetComponent<LineRenderer>();
                    if (linerenderer == null)
                        linerenderer = gameObject.AddComponent<LineRenderer>();
                }
                
            }

            if (AdditionalRayCastLayers == null)
                AdditionalRayCastLayers = new List<string>();
            layermask = LayerMask.GetMask(AdditionalRayCastLayers.ToArray());
            ShowStatus();
        }

        private void DrawLine()
        {
            if (ShowSensorLinerenderer)
            {
                linerenderer.enabled = true;
                List<Vector3> pos = new List<Vector3>();
                pos.Add(startposraycast);
                pos.Add(endposraycast);
                linerenderer.startWidth = RayCastDisplayWidth;
                linerenderer.endWidth = RayCastDisplayWidth;
                linerenderer.SetPositions(pos.ToArray());
                linerenderer.useWorldSpace = true;
                if (raycasthasthit)
                {
                    linerenderer.material = MaterialOccupied;
                }
                else
                {
                    linerenderer.material = MaterialNotOccupied;
                }
            }
        }
        
        private void Raycast()
        {
            if (!Application.isPlaying)
            {
                var list = new List<string>(AdditionalRayCastLayers);
                list.Add(LayerMask.LayerToName(gameObject.layer));
                layermask = LayerMask.GetMask(list.ToArray());
            }

            float scale = 1000;
            raycasthitchanged = false;
            var globaldir = transform.TransformDirection(RayCastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;
            startposraycast = transform.position;
            if (Physics.Raycast(transform.position, globaldir, out hit, RayCastLength/scale, layermask))
            {
                var dir = Vector3.Normalize(globaldir) * hit.distance;
                if (Application.isPlaying)
                    scale = realvirtualController.Scale;

                RayCastDistance = hit.distance * scale;
                if (DisplayStatus) Debug.DrawRay(transform.position, dir, Color.red,0,true);
                raycasthasthit = true;
                if (hit.collider != lasthit.collider)
                    raycasthitchanged = true;
                endposraycast = startposraycast + dir;
            }
            else
            {
                if (DisplayStatus) Debug.DrawRay(transform.position, display, Color.yellow,0,true);
                raycasthasthit = false;
                endposraycast = startposraycast + display;
                RayCastDistance = 0;
            }

        }

        // Shows Status of Sensor
        private void ShowStatus()
        {
          
            if (CollidingObjects.Count == 0)
            {
                LastTriggeredBy = null;
                LastTriggeredID = 0;
                LastTriggeredGlobalID = 0;
            }
            else
            {
                GameObject obj = CollidingObjects[CollidingObjects.Count - 1];
                if (!ReferenceEquals(obj, null))
                {
                    var LastTriggeredByMU = GetTopOfMu(obj);
                    if (!ReferenceEquals(LastTriggeredByMU, null))
                        LastTriggeredBy = LastTriggeredByMU.gameObject;
                    else
                        LastTriggeredBy = obj;

                    if (LastTriggeredByMU != null)
                    {
                        LastTriggeredID = LastTriggeredByMU.ID;
                        LastTriggeredGlobalID = LastTriggeredByMU.GlobalID;
                    }
                }
            }

            if (CollidingObjects.Count > 0)
            {
                _occupied = true;
                if (DisplayStatus && _meshrenderer != null)
                {
                    _meshrenderer.material = MaterialOccupied;
                }
            }
            else
            {
                _occupied = false;
                if (DisplayStatus && _meshrenderer != null)
                {
                    _meshrenderer.material = MaterialNotOccupied;
                }
            }

            Occupied = _occupied;
        }

        // ON Collission Enter
        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            var tmpcolliding = CollidingObjects;
            var muobj = GetTopOfMu(obj);

            if ((LimitSensorToTag == "" || ((muobj.tag == LimitSensorToTag) || muobj.Name == LimitSensorToTag)))
            {
                if (PauseOnSensor)
                    Debug.Break();
                if (!CollidingObjects.Contains(obj))
                    CollidingObjects.Add(obj);
            
            
                ShowStatus();
                ColliderCounter++;
                if (muobj != null)
                {
                    if (!CollidingMus.Contains(muobj))
                    {
                        if (EventEnter != null)
                            EventEnter(muobj.gameObject);
                   
                        muobj.EventMUEnterSensor(this);
                        CollidingMus.Add(muobj);
                        Counter++;
                        if (EventMUSensor!=null)
                            EventMUSensor.Invoke(muobj, true);
                    }
                }
                else
                {
                    if (EventEnter != null)
                        EventEnter(obj);
                    if (EventNonMUGameObjectSensor!=null)
                      EventNonMUGameObjectSensor.Invoke(obj,true);
                }
            }
        }
        
        public void OnMUPartsDestroyed(GameObject obj)
        {
            CollidingObjects.Remove(obj);
        }

        public void OnMUDelete(MU muobj)
        {
            
            CollidingObjects.Remove(muobj.gameObject);

            // Check if remaining colliding objects belong to same mu
            var coolliding = CollidingObjects.ToArray();
            var i = 0;
            do
            {
                if (i < coolliding.Length)
                {
                    var thismuobj = GetTopOfMu(coolliding[i]);
                    if (thismuobj == muobj)
                    {
                        CollidingObjects.Remove(coolliding[i]);
                    }
                }

                i++;
            } while (i < coolliding.Length);
            CollidingMus.Remove(muobj);
            if (EventExit != null)
                EventExit(muobj.gameObject);
            if (EventMUSensor!= null)
                  EventMUSensor.Invoke(muobj, false);
            muobj.EventMUExitSensor(this);
            LastTriggeredBy = null;
            LastTriggeredID = 0;
            LastTriggeredGlobalID = 0;
            ShowStatus();
        }
        


        // ON Collission Exit
        private void OnTriggerExit(Collider other)
        {
            if (other == null)
            {
                CollidingObjects.RemoveAll(item => item == null);
                ShowStatus();
                return;
            }
            
            GameObject obj = other.gameObject;
            if (!ReferenceEquals(obj, null))
            {
                
                var muobj = GetTopOfMu(obj);
                var tmpcolliding = CollidingObjects;
                var dontdelete = false;
                CollidingObjects.Remove(obj);

                // Check if remaining colliding objects belong to same mu
                foreach (var thisobj in CollidingObjects)
                {
                    var thismuobj = GetTopOfMu(thisobj);
                    if (thismuobj == muobj)
                    {
                        dontdelete = true;
                    }
                }

                if (!dontdelete)
                {
               
                    if (muobj != null && CollidingMus.Contains(muobj))
                    {
                        CollidingMus.Remove(muobj);
                        if (EventExit != null)
                            EventExit(muobj.gameObject);
                        if (EventMUSensor!=null)
                             EventMUSensor.Invoke(muobj, false);
                        muobj.EventMUExitSensor(this);
                    }
                    else
                    {
                        if (EventNonMUGameObjectSensor!=null)
                            EventNonMUGameObjectSensor.Invoke(obj,false);
                        if (EventExit != null)
                            EventExit(obj);
                    }
                }
                ShowStatus();
            }
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying && UseRaycast && !_isNetworkControlled)
            {
                Raycast();
                
                // last raycast has left
                if ((lastraycasthasthit && !raycasthasthit)|| raycasthitchanged) 
                {
                    if (lasthit.collider)
                           OnTriggerExit(lasthit.collider);
                    else
                    {
                        OnTriggerExit(null);
                    }
                }
                
                if ((raycasthasthit && !lastraycasthasthit) || raycasthitchanged)
                {
                    // new raycast hit
                    OnTriggerEnter(hit.collider);
                }

                lastraycasthasthit = raycasthasthit;
                lasthit = hit;

            }
            if (Application.isPlaying)
            // Set external PLC Outputs
               if (_isOccupiedNotNull)
                        SensorOccupied.Value = Occupied;
                 if (_isNotOccupiedNotNull)
                        SensorNotOccupied.Value = !Occupied;
        }

        private void Update()
        {
            if (!Application.isPlaying && UseRaycast && !_isNetworkControlled)
            {
                layermask = LayerMask.GetMask(AdditionalRayCastLayers.ToArray());
                Raycast();
            }

            if (Application.isPlaying && UseRaycast && DisplayStatus)
            {
                DrawLine();
            }
            
            if (Application.isPlaying && UseRaycast && !DisplayStatus)
            {
                if (linerenderer != null)
                     linerenderer.enabled = false;
            }

        }
    }
}