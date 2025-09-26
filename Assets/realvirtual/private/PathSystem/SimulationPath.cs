// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#pragma warning disable 0618
namespace realvirtual
{
    //! SimulationPath provides intelligent path-based transport routing for material handling units in automation systems.
    //! This abstract class implements sophisticated path networks where MUs (Material Units) follow predefined routes through
    //! production facilities, warehouses, and assembly lines. Features automatic path snapping for network building,
    //! customizable path strategies for decision making at junctions, and event-driven control for station integration.
    //! Supports both linear and curved path segments with seamless transitions, enabling complex intralogistics scenarios
    //! like AGV routes, overhead conveyor systems, and flexible manufacturing system layouts.
    public abstract class SimulationPath : BasePath, ISnapable
    {
        #region PublicVariables

        public Drive Drive;//!< drive used on the simulation path
        [HideInInspector] public bool DriveNotNull = false;
        public PathStrategy PathStrategy;//!< Path strategy which defines the behavior at certain path events
        [Foldout("Connections")] [ReorderableList]
        public List<SimulationPath> Predecessors  = new List<SimulationPath>();//!< List of predecessors, is filled automatically
        [Foldout("Connections")] [ReorderableList]
        public List<SimulationPath> Successors  = new List<SimulationPath>(); //!< List of successors, is filled automatically
        [Foldout("Path Events")] public SimulationPathEvent OnPathEntered;//!< List with methods called when a path mover enter the path
        [Foldout("Path Events")] public SimulationPathEvent OnPathEnd;//!< List with methods called when a path mover reach the end of a path
        [Foldout("Path Events")] public SimulationPathEvent OnPathExit;//!< List with methods called when a path mover exit the path
        [OnValueChanged("LengthChanged")] public float Length = 1f;//!< Length of the path
        public bool ShowPathOnSimulation = false;//!< Boolean specifying whether the path is visible during runtime or not
        [OnValueChanged("DrawPath")] public float Thickness = 0.025f;//!< Thickness of the simulation path
        [OnValueChanged("DrawPath")] public Material MaterialPath;//!< Material of the simulation path
        [OnValueChanged("DrawPath")] public float SizeDirectionArrow = 0.08f;//!< Size of the direction arrow in the center of the path
        [HideInInspector] [SerializeField] public GameObject StartPoint;
        [HideInInspector] [SerializeField] public GameObject EndPoint;

        #endregion

        #region PrivateVariables
        
        protected LineRenderer Linerenderer;

        protected bool blockdraw;
        [HideInInspector] public bool EnableSnap = true;
 
        private Quaternion _oldstartrot;
        private Quaternion _oldendrot;
        protected Arrow _startarrow;
        protected Arrow _endarrow;

        private Vector3 _oldlocalstartpos;
        private Vector3 _oldlocalendpos;
        [HideInInspector] public bool movedlocally;

        private float _oldthickness;
        private Material _oldmaterial;
    
        private Vector3 _oldglobalposition;
        private bool selected;
        private bool oldselected;
        [HideInInspector] public bool transformposchanged;
        private SimulationPath path;


        #endregion

        #region PublicOverrideMethods

        // Virtual Methods to override in Childrens
        public virtual void DrawPath()
        {
        }

        public virtual void AttachTo(SimulationPath path)
        {
            Debug.LogError("Not Implemented");
        }

        protected virtual void LengthChanged()
        {
            Debug.LogError("Not Implemented");
        }
        
        // plus from IPath
        // public abstract  float GetLength();
        // public abstract Vector3 GetPosition(float normalizedposition, ref BasePath currentpath);
        // public abstract Vector3 GetClosestDirection(float normalizedposition);

        #endregion

        #region PublicMethods
        
        // local start position of the simulation path
        public Vector3 LocalStartPos
        {
            get
            {
                if (StartPoint != null)
                    return StartPoint.transform.localPosition;
                else
                    return Vector3.zero;
            }
            set
            {
                if (StartPoint.transform.localPosition != value)
                {
                    StartPoint.transform.localPosition = value;
                    DrawPath();
                }
            }
        }
        // local end position of the simulation path
        public Vector3 LocalEndPos
        {
            get
            {
                if (EndPoint != null)
                    return EndPoint.transform.localPosition;
                else
                    return Vector3.zero;
            }
            set
            {
                if (EndPoint.transform.localPosition != value)
                {
                    EndPoint.transform.localPosition = value;
                    DrawPath();
                }
            }
        }
        // start position of the simulation path
        public Vector3 StartPos
        {
            get
            {
                if (StartPoint != null)
                    return StartPoint.transform.position;
                else
                    return Vector3.zero;
            }
            set
            {
                if (transformposchanged)
                {
                    var deltapos = value - StartPoint.transform.position;
                    transform.position = transform.position + deltapos;
                }

                StartPoint.transform.position = value;
                DrawPath();
            }
        }
        // end position of the simulation path
        public Vector3 EndPos
        {
            get
            {
                if (EndPoint != null)
                    return EndPoint.transform.position;
                else
                    return Vector3.zero;
            }
            set
            {
                if (transformposchanged)
                {
                    var deltapos = value - EndPoint.transform.position;
                    transform.position = transform.position + deltapos;
                }

                EndPoint.transform.position = value;
                DrawPath();
            }
        }

        // To this pathes start another path is snapped
        public void OnMyStartSnapped(SimulationPath path)
        {
            if (!Predecessors.Contains(path))
                Predecessors.Add(path);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        // To this pathes start another path is unsnapped
       public void OnMyStartUnsnapped(SimulationPath path)
        {
            Predecessors.Remove(path);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
           
        }
        // To this pathes end  another path is snapped
        public void OnMyEndSnapped(SimulationPath path)
        {
            if (!Successors.Contains(path))
                Successors.Add(path);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        // To this pathes end  another path is unsnapped
        public void OnMyEndUnsnapped(SimulationPath path)
        {
            Successors.Remove(path);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }


        protected void SetStartTo0()
        {
            var _oldstart = StartPos;
            var _oldend = EndPos;
            transform.position = StartPos;
            StartPos = _oldstart;
            EndPos = _oldend;
        }
        

        protected void DrawArrows()
        {
        }
        
        #endregion

        private void SaveValues()
        {
            _oldthickness = Thickness;
            _oldmaterial = MaterialPath;
            _oldlocalstartpos = LocalStartPos;
            _oldlocalendpos = LocalEndPos;
            _oldglobalposition = transform.position;
        }
        // Is called when a path mover enter the path
        public void TransportableEntered(PathMover trans)
        {
            OnPathEntered.Invoke(this, trans);
        }
        // Is called when a path mover reach the end of the path
        public void TransportableOnEnd(PathMover trans)
        {
            OnPathEnd.Invoke(this, trans);
        }
        // Is called when a path mover has left the path
        public void TransportableExit(PathMover trans)
        {
            OnPathExit.Invoke(this, trans);
        }

        protected void BaseReset()
        {
            Debug.Log("BaseReset");

            DriveNotNull = Drive != null;
            transform.rotation = Quaternion.Euler(90, 90, 0);
            
            Linerenderer = GetComponent<LineRenderer>();

            if (MaterialPath == null)
            {
                MaterialPath = UnityEngine.Resources.Load("LineYellow", typeof(Material)) as Material;
            }
            

            CheckSnapping();
            SaveValues();
        }
        // checks possible snapping at the path ends
        public void CheckSnapping()
        {
            #if UNITY_EDITOR
            ClearConnections();      
           
            foreach (Transform trans in Selection.transforms)
            {
                foreach (SnapPoint snap in trans.GetComponentsInChildren<SnapPoint>())
                {
                    //if (!snap.snapped)
                    //{
                    snap.CheckSnap();
                    // }
                }
            }
            #endif
        }

        private void OnDestroy()
        {
            if ((Application.isPlaying == false) && (Application.isEditor == true) &&
                (Application.isLoadingLevel == false))
            {
                if (gameObject.scene.isLoaded)
                {
                    foreach (var succ in Successors)
                    {
                        succ.Predecessors.Remove(this);
                        succ.ClearConnections();
                    }

                    foreach (var pred in Predecessors)
                    {
                        pred.Successors.Remove(this);
                        pred.ClearConnections();
                    }
                }
            }
        }

        private new void Awake()
        {
            var drive = GetComponent<Drive>();
            if (drive != null && Drive == null)
                Drive = drive;
           
            DriveNotNull = Drive != null;
            Linerenderer = GetComponent<LineRenderer>();

            if (Linerenderer != null)
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                
                    Linerenderer.enabled = ShowPathOnSimulation;
                    var arrow = this.GetComponentInChildren<MidArrow>();
                    if (arrow != null)
                    {
                        arrow.enabled = ShowPathOnSimulation;
                    }
#else           
                Linerenderer.enabled = ShowPathOnSimulation;

                
#endif
            }
            base.Awake();
        }

        private bool V3Equal(Vector3 a, Vector3 b)
        {
            return Vector3.SqrMagnitude(a - b) < 0.0001;
        }
        // clean up the predecessor and successor list of the path. Remove double entries
        public void ClearConnections()
        {
            int i = 0;
            while (i < Predecessors.Count)
            {
                if (Predecessors[i] == null)
                {
                    Predecessors.Remove(Predecessors[i]);
                }
                else
                {
                    i++;
                }
            }

            i = 0;
            while (i < Successors.Count)
            {
                if (Successors[i] == null)
                {
                    Successors.Remove(Successors[i]);
                }
                else
                {
                    i++;
                }
            }
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR

            if (Array.IndexOf(UnityEditor.Selection.gameObjects, gameObject) >= 0)
            {
                return;
            }
#endif
        }


        private void Update()
        {
            
            if (!Application.isPlaying)
            {
                
                if (_oldthickness != Thickness || _oldmaterial != MaterialPath)
                {
                    DrawPath();
                }

                if (_oldlocalstartpos != LocalStartPos)
                {
                    DrawPath();
                }

                if (_oldlocalendpos != LocalEndPos)
                {
                    DrawPath();
                }
                CheckSnapping();
                SaveValues();
            }
        }

        public void AttachPathLine(string path, GameObject attachto)
        {
#if UNITY_EDITOR
            var original = (GameObject) AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            var newobj = PrefabUtility.InstantiatePrefab(original) as GameObject;
            Undo.RegisterCreatedObjectUndo (newobj, "Attach Line");
            var simpathobj = newobj.GetComponent<Line>();
            var parentPath = this.GetComponent<SimulationPath>();
            if (attachto != null)
            {
                simpathobj.gameObject.name = "Line";
                simpathobj.AttachTo(parentPath);
                simpathobj.OnMyStartSnapped(parentPath);
                parentPath.OnMyEndSnapped(simpathobj);
            }
            Selection.activeGameObject = simpathobj.gameObject;
#endif
        }
        // Attach a simulation path curve at the end of the current simulation path
        public void AttachPathCurve(string path, GameObject attachto)
        {
#if UNITY_EDITOR
            // Create new Gameobject
            var original = (GameObject) AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            var newobj = PrefabUtility.InstantiatePrefab(original) as GameObject;
            Undo.RegisterCreatedObjectUndo (newobj, "Attach Curcve");
            var simpathobj = newobj.GetComponent<Curve>();
            var parentPath = this.GetComponent<SimulationPath>();

            simpathobj.gameObject.name = "Curve";
            simpathobj.AttachTo(parentPath);
            
            simpathobj.OnMyStartSnapped(parentPath);
            parentPath.OnMyEndSnapped(simpathobj);
            Selection.activeGameObject = simpathobj.gameObject;
#endif
        }
        
        // is called when another simulation path is connected by the automatic snapping
        public OnSnappedEvent OnSnapped { get; set; }

        public void CheckSnap()
        {
            CheckSnapping();
        }


        // is called when another simulation path is connected by the automatic snapping
        public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject, bool ismoved)
        {
            var Mateline = snapPointMate.GetComponentInParent<SimulationPath>();
            var ownline = ownSnapPoint.GetComponentInParent<SimulationPath>();
            if (ismoved)
            {
               
                //  ownSnapPoint.transform.position = snapPointMate.transform.position;
                if (ownSnapPoint.name == "Start")
                {
                    ownline.transformposchanged = true;
                    if (snapPointMate.name == "Start")
                    {
                        ownline.StartPos = Mateline.StartPos;
                    }
                    else
                    {
                        ownline.StartPos = Mateline.EndPos;
                    }
                    ownline.OnMyStartSnapped(Mateline);
                    Mateline.OnMyEndSnapped(ownline);
                }
                else
                {
                    ownline.transformposchanged = true;
                    if (snapPointMate.name == "End")
                    {
                        ownline.EndPos = Mateline.EndPos;
                    }
                    else
                    {
                        ownline.EndPos = Mateline.StartPos;
                    }
                    ownline.OnMyEndSnapped(Mateline);
                    Mateline.OnMyStartSnapped(ownline);
                }
                ownSnapPoint.Hide(true);
                snapPointMate.Hide(true);
                
            }
        }

        // is called when another simulation path is disconnected by the automatic snapping
        public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable Mateobj, bool ismoved)
        {
            var Mateline = snapPointMate.GetComponentInParent<SimulationPath>();
            var ownline = snapPoint.GetComponentInParent<SimulationPath>();
            if (snapPoint.name == "Start")
            {
                ownline.OnMyStartUnsnapped(Mateline);
                Mateline.OnMyEndUnsnapped(ownline);
            }
            else
            {
                ownline.OnMyEndUnsnapped(Mateline);
                Mateline.OnMyStartUnsnapped(ownline);
            }
            snapPoint.Hide(false);
        }
        
        public void Modify()
        {
            Update();
        }

        public void AttachTo(SnapPoint attachto)
        {
            
        }
    }
}