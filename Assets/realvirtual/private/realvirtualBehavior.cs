// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace realvirtual
{
    public enum DIRECTION
    {
        LinearX,
        LinearY,
        LinearZ,
        RotationX,
        RotationY,
        RotationZ,
        Virtual
    }

    //! This is the base class for all realvirtual objects. This base clase is providing some additional scripts and properties for all components.
    public class realvirtualBehavior : MonoBehaviour
    {
        [HideIf("hidename")]
        public string Name; //!< The name of the component if it should be different from the GameObject name

        public enum ActiveOnly
        {
            Always,
            Connected,
            Disconnected,
            Never,
            DontChange
        }

        [HideIf("hideactiveonly")] public ActiveOnly Active;
        [HideInInspector] public GameObject FromTemplate;
        [HideInInspector] public realvirtualController realvirtualController;
        [HideInInspector] [SerializeField] public bool HideNonG44Components;
        [HideInInspector] public bool SceneIsAdditive;
        [HideInInspector] public bool ForceStop = false; 
        private static ILogger _logger = Debug.unityLogger;

        protected bool hidename()
        {
            return true;
        }

        protected bool hideactiveonly()
        {
            return false;
        }

        //! Is called by the realvirtualController before the component is started
        public void PreStartSim()
        {
            OnPreStartSim();
        }
        
        
        //! Is called by the realvirtualController to start or stop the component
        public void StartSim()
        {
            ForceStop = false;
            OnStartSim();
        }
        
        public void StopSim()
        {
            ForceStop = true;
            OnStopSim();
        }

        protected virtual void OnStopSim()
        {
            // Method might be overwritten if special forcestop action is needed
        }
        
        protected virtual void OnPreStartSim()
        {
            // Method might be overwritten if special Method before OnStart is needed
        }
        
        protected virtual void OnStartSim()
        {
            // Method might be overwritten if special OnStart is needed
        }
        
        //! Transfers the direction enumeration to a vector
        public Vector3 DirectionToVector(DIRECTION direction)
        {
            Vector3 result = Vector3.up;
            switch (direction)
            {
                case DIRECTION.LinearX:
                    result = Vector3.right;
                    break;
                case DIRECTION.LinearY:
                    result = Vector3.up;
                    break;
                case DIRECTION.LinearZ:
                    result = Vector3.forward;
                    break;
                case DIRECTION.RotationX:
                    result = Vector3.right;
                    break;
                case DIRECTION.RotationY:
                    result = Vector3.up;
                    break;
                case DIRECTION.RotationZ:
                    result = Vector3.forward;
                    break;
                case DIRECTION.Virtual:
                    result = Vector3.zero;
                    break;
            }

            return result;
        }

        //! Transfers a vector to the direction enumeration
        public DIRECTION VectorToDirection(bool torotatoin, Vector3 vector)
        {
            if (!torotatoin)
            {
                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.LinearX)) == 1)
                {
                    return DIRECTION.LinearX;
                }

                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.LinearY)) == 1)
                {
                    return DIRECTION.LinearY;
                }

                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.LinearZ)) == 1)
                {
                    return DIRECTION.LinearZ;
                }
            }
            else
            {
                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.RotationX)) == 1)
                {
                    return DIRECTION.RotationX;
                }

                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.RotationY)) == 1)
                {
                    return DIRECTION.RotationY;
                }

                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.RotationZ)) == 1)
                {
                    return DIRECTION.RotationZ;
                }
            }

            // if nothing return virtual
            return DIRECTION.Virtual;
        }

        public float GetLocalScale(Transform thetransform, DIRECTION direction)
        {
            float result = 1;
            switch (direction)
            {
                case DIRECTION.LinearX:
                    result = thetransform.lossyScale.x;
                    break;
                case DIRECTION.LinearY:
                    result = thetransform.lossyScale.y;
                    break;
                case DIRECTION.LinearZ:
                    result = thetransform.lossyScale.z;
                    break;
            }

            return result;
        }

        //! Gets back if the direction is linear or a rotation
        public static bool DirectionIsLinear(DIRECTION direction)
        {
            bool result = false;
            switch (direction)
            {
                case DIRECTION.LinearX:
                    result = true;
                    break;
                case DIRECTION.LinearY:
                    result = true;
                    break;
                case DIRECTION.LinearZ:
                    result = true;
                    break;
                case DIRECTION.RotationX:
                    result = false;
                    break;
                case DIRECTION.RotationY:
                    result = false;
                    break;
                case DIRECTION.RotationZ:
                    result = false;
                    break;
                case DIRECTION.Virtual:
                    result = true;
                    break;
            }

            return result;
        }

        public List<BehaviorInterfaceConnection> UpdateConnectionInfo()
        {
            var ConnectionInfo = new List<BehaviorInterfaceConnection>();
            Type mytype = this.GetType();
            FieldInfo[] fields = mytype.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field != null)
                {
                    var type = field.FieldType;
                    if (type.IsSubclassOf(typeof(Signal)))
                    {
                        var info = new BehaviorInterfaceConnection();
                        info.Name = field.Name;
                        info.Signal = (Signal) field.GetValue(this);
                        ConnectionInfo.Add(info);
                    }
                }
            }

            return ConnectionInfo;
        }


        public List<Signal> GetConnectedSignals()
        {
            var signals = new List<Signal>();
            Type mytype = this.GetType();
            FieldInfo[] fields = mytype.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field != null)
                {
                    var type = field.FieldType;
                    if (type.IsSubclassOf(typeof(Signal)))
                    {
                        var sig = (Signal) field.GetValue(this);
                        if (!ReferenceEquals(sig, null))
                            signals.Add((Signal) field.GetValue(this));
                    }
                }
            }

            return signals;
        }

        //! Sets the visibility of this object including all subobjects 
        public void SetVisibility(bool visibility)
        {
            Renderer[] components = gameObject.gameObject.GetComponentsInChildren<Renderer>();
            if (components != null)
            {
                foreach (Renderer component in components)
                    component.enabled = visibility;
            }

            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null)
            {
                foreach (MeshRenderer meshrenderer in meshRenderers)
                    meshrenderer.enabled = visibility;
            }
        }

        public List<BehaviorInterfaceConnection> GetConnections()
        {
            return UpdateConnectionInfo();
            ;
        }

        public List<Signal> GetSignals()
        {
            return GetConnectedSignals();
        }

        //! Gets a child by name 
        public GameObject GetChildByName(string name)
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }


        //! Gets all child by name 
        public List<GameObject> GetChildsByName(string name)
        {
            List<GameObject> childs = new List<GameObject>();
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    childs.Add(child.gameObject);
                }
            }

            return childs;
        }


        public GameObject GetChildByNameAlsoHidden(string name)
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        public List<GameObject> GetAllMeshesWithGroup(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = Object.FindObjectsByType<Group>(FindObjectsSortMode.None);
            foreach (var groupcomp in groupcomps)
            {
                if (groupcomp.GetGroupName() == group)
                {
                    // Check if one parent has the same group
                    var mesh = groupcomp.gameObject.GetComponent<MeshFilter>();

                    if (!ReferenceEquals(mesh, null))
                    {
                        list.Add(groupcomp.gameObject);
                    }
                }
            }

            return list;
        }

        public List<GameObject> GetAllWithGroup(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = Object.FindObjectsByType<Group>(FindObjectsSortMode.None);
            foreach (var groupcomp in groupcomps)
            {
                if (groupcomp.GetGroupName() == group && groupcomp.Active!= ActiveOnly.Never)
                {
                    // Check if one parent has the same group
                    var parent = groupcomp.transform.parent;
                    bool add = true;
                    if (!ReferenceEquals(parent, null))
                    {
                        // search upwards
                        var uppergroups = parent.gameObject.GetComponentsInParent<Group>();
                        // is the group in one of the upper parents?
                        foreach (var uppergroup in uppergroups)
                        {
                            if (uppergroup.GetGroupName() == group)
                            {
                                add = false;
                            }
                        }
                    }

                    if (add)
                        list.Add(groupcomp.gameObject);
                }
            }

            return list;
        }

        public List<GameObject> GetAllWithGroups(List<string> groups)
        {
            List<GameObject> first;
            first = GetAllWithGroup(groups[0]);

            for (int i = 1; i < groups.Count; i++)
            {
                var newobjs = GetAllWithGroup(groups[i]);
                IEnumerable<GameObject> res = first.AsQueryable().Intersect(newobjs);
                first = res.ToList();
            }

            return first;
        }

        public List<GameObject> GetAllMeshesWithGroups(List<string> groups)
        {
            List<GameObject> first;
            first = GetAllMeshesWithGroup(groups[0]);

            for (int i = 1; i < groups.Count; i++)
            {
                var newobjs = GetAllWithGroup(groups[i]);
                IEnumerable<GameObject> res = first.AsQueryable().Intersect(newobjs);
                first = res.ToList();
            }

            return first;
        }

        public List<string> GetMyGroups()
        {
            List<string> list = new List<string>();
            var groups = GetComponents<Group>();
            foreach (var group in groups)
            {
                list.Add(group.GroupName);
            }

            return list;
        }

        public List<GameObject> GetMeshesWithSameGroups()
        {
            var list = GetMyGroups();
            var list2 = GetAllMeshesWithGroups(list);
            list2.Remove(this.gameObject);
            return list2;
        }


        public List<GameObject> GetAllWithSameGroups()
        {
            var list = GetMyGroups();
            var list2 = GetAllWithGroups(list);
            list2.Remove(this.gameObject);
            return list2;
        }

        //! Gets the top of an MU component (the first MU script going up in the hierarchy)
        protected MU GetTopOfMu(GameObject obj)
        {
            if (obj != null)
            {
                var res = obj.transform.GetComponentsInParent<MU>(true);
                if (res.Length > 0)
                    return res[0];
                else
                    return null;
            }

            return null;
        }

        //!     Gets the mesh renderers in the childrens
        public MeshRenderer GetMeshRenderer()
        {
            MeshRenderer renderers = gameObject.GetComponentInChildren<MeshRenderer>();
            return renderers;
        }

        //! sets the collider in all child objects
        public void SetCollider(bool enabled)
        {
            Collider[] components = gameObject.GetComponentsInChildren<Collider>();
            if (components != null)
            {
                foreach (Collider component in components)
                    component.enabled = enabled;
            }
        }

        //! Displays an error message
        public void ErrorMessage(string message)
        {
#if (UNITY_EDITOR)
            EditorUtility.DisplayDialog("Game4Automation Error for [" + this.gameObject.name + "]", message, "OK", "");
#endif
            Error(message, this);
        }

        public void ChangeConnectionMode(bool isconnected)
        {
            if (Active == ActiveOnly.DontChange)
                return;

            if (Active == ActiveOnly.Always)
            {
                if (this.enabled == false)
                    this.enabled = true;
            }

            if (Active == ActiveOnly.Connected)
            {
                if (isconnected)
                    this.enabled = true;
                else
                    this.enabled = false;
            }

            if (Active == ActiveOnly.Disconnected)
            {
                if (!isconnected)
                    this.enabled = true;
                else
                    this.enabled = false;
            }

            if (Active == ActiveOnly.Never)
            {
                this.enabled = false;
            }
        }

        //! Logs a message
        public void Log(string message)
        {
            _logger.Log("game4automation: " + message);
        }

        //! Logs a message with a relation to an object
        public void Log(string message, object obj)
        {
            _logger.Log("game4automation: Object [" + this.gameObject.name + "] " + message, obj);
        }

        //! Logs a warinng with a relation to an object
        public void Warning(string message, object obj)
        {
            _logger.LogWarning("game4automation: Object [" + this.gameObject.name + "] " + message, obj);
        }

        //! Logs an error with a relation to an object
        public void Error(string message, object obj)
        {
            _logger.LogError("realvirtual: Object [" + this.gameObject.name + "] " + message, obj);
        }

        //! Logs an error
        public void Error(string message)
        {
            _logger.LogError("realvirtual: " + message, this);
        }

        //! Displays a gizmo for debugging positions
        public GameObject DebugPosition(string debugname, Vector3 position, Quaternion quaternation, float scale)
        {
            GameObject debuggizmo = null;

            if (realvirtualController.EnablePositionDebug)
            {
                debuggizmo = GameObject.Find("debugname");
                if (debuggizmo == null)
                {
                    var gizmo = UnityEngine.Resources.Load("Gizmo/Gizmo", typeof(GameObject));
                    debuggizmo = Instantiate((GameObject) gizmo, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                    debuggizmo.layer = realvirtualController.DebugLayer;
                }

                debuggizmo.transform.position = position;
                debuggizmo.transform.rotation = quaternation;
                debuggizmo.transform.localScale = Vector3.one * scale;
                debuggizmo.name = debugname;
            }

            return debuggizmo;
        }

        //! Freezes all child components to the current poosition
        public void SetFreezePosition(bool enabled)
        {
            Rigidbody[] components = gameObject.GetComponentsInChildren<Rigidbody>();
            if (components != null)
            {
                foreach (Rigidbody rigid in components)
                    if (enabled)
                    {
                        rigid.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        rigid.constraints = RigidbodyConstraints.None;
                    }
            }
        }
        
        public void SetRbConstraints(RigidbodyConstraints constraints)
        {
            Rigidbody[] components = gameObject.GetComponentsInChildren<Rigidbody>(true);
            if (components != null)
            {
                foreach (Rigidbody rigid in components)
                        rigid.constraints = constraints;
            }
        }

        //! Initialiates the components and gets the reference to the realvirtualController in the scene
        protected void InitGame4Automation()
        {
         
            if (this.gameObject.scene != SceneManager.GetActiveScene()) 
                SceneIsAdditive = true;
            else
                SceneIsAdditive = false;


            var controllers = FindObjectsByType<realvirtualController>(FindObjectsSortMode.None);
            foreach (var controller in controllers)
            {
                if (SceneIsAdditive)
                {
                    if (controller.gameObject.scene != this.gameObject.scene)
                    {
                        realvirtualController = controller;
                    }
                }
                else
                {
                    if (controller.gameObject.scene == this.gameObject.scene)
                    {
                        realvirtualController = controller;
                    }
                }
            }

            if (realvirtualController == null)
            {
                Error(
                    "No realvirtualController found - realvirtualController Script needs to be once inside every Game4Automation Scene");
                Debug.Break();
                return;
            }

            if (Name == "")
            {
                Name = gameObject.name;
            }

            ChangeConnectionMode(realvirtualController.Connected);
        }

        protected virtual void AfterAwake()
        {
        }


        public virtual void AwakeAlsoDeactivated()
        {
        }


        protected void Awake()
        {
            if (Application.isPlaying)
                InitGame4Automation();
            AfterAwake();
        }
    }
}