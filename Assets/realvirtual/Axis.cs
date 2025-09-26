
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using realvirtual.Resources.Gizmo;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace realvirtual
{
#pragma warning disable CS3001, CS3002, CS3003, CS3009
    //! Axis defines the kinematic and physical movement axis for drives and kinematic systems.
    //! It specifies the movement direction, limits, and physical joint properties for both linear and rotational movements.
    //! Axes can be hierarchically connected to create complex kinematic chains for robotic systems.
    public class Axis : MonoBehaviour
    {
        [Tooltip("Reference GameObject that defines the axis position and orientation")]
        public GameObject AxisReferenceGameObject; //!< Reference game object that defines the axis position and orientation
        public enum AxisPositionTypes
        {
           None,
           Pivot,
           BoundingBoxCenter,
           RadiusCenter
        }
        [ReadOnly]public bool Balljoint; //!< Indicates if this is a ball joint axis allowing rotation in all directions
        [ReadOnly]public bool IsSecondaryAxis = false; //!< Indicates if this axis is a secondary axis in a kinematic chain
        [ReadOnly]public AxisPositionTypes PositionMode; //!< Defines how the axis position is calculated (Pivot, BoundingBoxCenter, or RadiusCenter) 
        [ReadOnly]public DIRECTION Direction; //!< The movement direction of this axis (LinearX/Y/Z or RotationX/Y/Z)
        [ReadOnly]public Vector3 RotationOffset; //!< Rotation offset applied to the axis in degrees
        [Tooltip("Show visual gizmos for this axis in the Scene view")]
        public bool DrawGizmos = true; //!< Enables or disables the visual gizmos for this axis in the Scene view
        [ReadOnly]public float GizmoDistance=0.5f; //!< Distance in meters for displaying the axis gizmo
        [ReadOnly] public Vector3 axisReferencePosition; //!< The reference position of the axis in world coordinates
        [ReadOnly]public bool UseLimits = false; //!< Enables position or rotation limits for this axis
        [ShowIf("UseLimits")]
        [Tooltip("Lower limit position in millimeters (linear) or degrees (rotational)")]
        public float LowerLimit; //!< Lower limit position in millimeters for linear axes or degrees for rotational axes
        [ShowIf("UseLimits")]
        [Tooltip("Upper limit position in millimeters (linear) or degrees (rotational)")]
        public float UpperLimit; //!< Upper limit position in millimeters for linear axes or degrees for rotational axes
        [ShowIf("UseLimits")]
        [Tooltip("Detection window in millimeters or degrees for limit switches")]
        public float LimitEndPosWindow=1; //!< Window in millimeters or degrees for detecting when the axis reaches its limits
        [ReadOnly] [ShowIf("UseLimits")]public bool OnLowerLimit; //!< Indicates if the axis is currently at its lower limit
        [ReadOnly] [ShowIf("UseLimits")]public bool OnUppperLimit; //!< Indicates if the axis is currently at its upper limit
        [ReadOnly]public Vector3 AnchorPos; //!< The anchor position for the physical joint in world coordinates
        [ReadOnly]public Vector3 LocalAnchorPos; //!< The anchor position for the physical joint in local coordinates
        [ReadOnly] public Vector3 ConnectedLocalAnchorPos; //!< The connected anchor position in local coordinates of the connected body
        [ReadOnly]public float AxisGizmoSize = 0.5f; //!< Size multiplier for the axis gizmo visualization
        [ReadOnly]public GameObject ConnectedAxis=null; //!< The connected axis GameObject in the kinematic chain
        [ReadOnly] public float Position; //!< Current position in millimeters for linear axes or degrees for rotational axes
        [HideInInspector] public string icon="linearaxis.png";
        [HideInInspector] public Color MeshColor;
        [HideInInspector] public Color AxisColor;
        [HideInInspector] public bool activeInKinTool = false;
        [ReadOnly]public List<GameObject> secondaryAxis = new List<GameObject>(); //!< List of secondary axes connected to this axis
        [HideInInspector] public Axis parentAxis;
        [ReadOnly] public List<Axis> SubDriveAxises = new List<Axis>(); //!< List of sub-drive axes controlled by this axis

        private Vector3 IconDist=Vector3.zero;
        private Vector3 LineLength=Vector3.zero;
        private bool physicalAxis = false;
        private Vector3 startpos,localdirection,rotationdir;
        private ConfigurableJoint Mainlinearjoint;
        private HingeJoint Mainrotationjoint;
        private ConfigurableJoint Seclinearjoint;
        private HingeJoint Secrotationjoint;
        private Kinematic currentKinematic;
        void OnDrawGizmos()
        {
#if UNITY_EDITOR 
            if ((Selection.activeGameObject!= gameObject ||Application.isPlaying) && !activeInKinTool)
            {
                    return;
            }
#endif
            if (DrawGizmos)
            {
                currentKinematic = GetComponent<Kinematic>();
                var rotation = transform.localRotation;
#if UNITY_EDITOR
                if (Global.g4acontrollernotnull)
                {
                    if (Selection.activeGameObject == gameObject)
                    {
                        AxisColor =Global.realvirtualcontroller.GetGizmoOptions().AxisColor;
                        MeshColor = Global.realvirtualcontroller.GetGizmoOptions().KT_SelectedMeshColor;
                    }
                    else
                    {
                        AxisColor =Global.realvirtualcontroller.GetGizmoOptions().AxisColor;
                        MeshColor = Global.realvirtualcontroller.GetGizmoOptions().AxisColorSecondaryAxis;
                    }
                }
                else
                {
                    if (Selection.activeGameObject == gameObject)
                    {
                        Gizmos.color = new Color(0.4f, 0.08f, 0.4f, 1f);
                        MeshColor= new Color(0.3f, 0.06f, 0.3f, 1f);
                    }
                    else
                    {
                        Gizmos.color = AxisColor;
                        MeshColor= new Color(0.3f, 0.06f, 0.3f, 1f);
                    }
                }
                DrawAxisGizmo(AxisColor,MeshColor);
                if(currentKinematic == null || !currentKinematic.ShowGroupGizmo || Selection.activeGameObject != gameObject)
                    DisplayGroupMeshes(MeshColor, true);
#else
                 Gizmos.color = AxisColor;
#endif
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (DrawGizmos)
            {
#if UNITY_EDITOR
                if (Selection.activeGameObject != gameObject && !Application.isPlaying)
                {
                    if (Global.g4acontrollernotnull)
                    {
                        AxisColor = Global.realvirtualcontroller.GetGizmoOptions().AxisColorSecondaryAxis;
                        MeshColor = Global.realvirtualcontroller.GetGizmoOptions().MeshColorConnectedAxis;
                    }
                    else
                    {
                        AxisColor = new Color(0.4f, 0.4f, 0, 1f); // darker yellow
                        MeshColor = new Color(0, 0.3f, 0, 1f); // darker green
                    }
                    DrawAxisGizmo(AxisColor,MeshColor);
                    DisplayGroupMeshes(MeshColor,true);
                }
#endif
            }
        }

        //! Signals this axis when a sub-drive axis is added or removed from the hierarchy.
        public void SignalSubDriveAxis(Axis subAxis, bool added)
        {
            if (!added)
            {
                if (SubDriveAxises.Contains(subAxis))
                    SubDriveAxises.Remove(subAxis);
            }
            else
            {
                if(!SubDriveAxises.Contains(subAxis))
                    SubDriveAxises.Add(subAxis);
            }
        }
        //! Removes a secondary axis from this axis's list of secondary axes.
        public void removeSecondaryAxis(GameObject obj)
        {
            secondaryAxis.Remove(obj);
        }
        //! Displays wireframe meshes for all objects in the same kinematic group with the specified color.
        public void DisplayGroupMeshes(Color color, bool forcecolor = false )
        {
#if UNITY_EDITOR
            if(currentKinematic==null)
                currentKinematic = GetComponent<Kinematic>();
            string groupname = "";
            if(currentKinematic!=null)
                groupname = currentKinematic.GetGroupName();
            else
                groupname=gameObject.name;
            var objs = GetAllMeshesWithGroup(groupname);
            
            // Make the gizmos lighter and more subtle by reducing color intensity
            Color lightColor = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, color.a);
            Gizmos.color = lightColor;
            
            foreach (var obj in objs)
            {
                var meshParent = obj.GetComponent<MeshFilter>();
                if (meshParent != null)
                {
                    if(!Global.realvirtualcontroller.CheckIfMeshIsHovered(obj))
                        Gizmos.DrawWireMesh(meshParent.sharedMesh, obj.transform.position, obj.transform.rotation,
                        obj.transform.lossyScale);
                }
                var meshes = obj.GetComponentsInChildren<MeshFilter>();
                foreach (var mesh in meshes)
                {
                    var group = mesh.GetComponent<Group>();
                    if(group==null || group.GetGroupName()!=groupname)
                        continue;
                    if (Global.g4acontrollernotnull)
                    {
                        if(!Global.realvirtualcontroller.CheckIfMeshIsHovered(mesh.gameObject))
                            Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.transform.position, mesh.transform.rotation,
                            obj.transform.lossyScale);
                    }
                    else
                    {
                        Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.transform.position, mesh.transform.rotation,
                            obj.transform.lossyScale);
                    }
                }
            }
#endif
        }

        //! Aligns this axis to the position and rotation of the AxisReferenceGameObject.
        public void AlignToReference()
        {
            transform.position = AxisReferenceGameObject.transform.position;
            axisReferencePosition = transform.position;
            transform.rotation=AxisReferenceGameObject.transform.rotation;
        }
        private void DrawAxisGizmo(Color AxisColor,Color MeshColor)
        {
#if UNITY_EDITOR
            // Make axis lines lighter and more subtle by reducing color intensity
            Color lightAxisColor = new Color(AxisColor.r * 0.4f, AxisColor.g * 0.4f, AxisColor.b * 0.4f, AxisColor.a);
            Gizmos.color = lightAxisColor;
            if (!Balljoint)
            {
                switch (Direction)
                {
                    case DIRECTION.LinearX:
                    case DIRECTION.RotationX:
                    {
                        Vector3 direct = new Vector3(1, 0, 0);
                        LineLength = transform.TransformDirection(direct.normalized) * GizmoDistance;
                        Gizmos.DrawLine(axisReferencePosition - LineLength * AxisGizmoSize,
                                axisReferencePosition + LineLength * AxisGizmoSize);

                        IconDist = axisReferencePosition + LineLength * AxisGizmoSize;
                        break;
                    }

                    case DIRECTION.LinearY:
                    case DIRECTION.RotationY:
                    {
                        Vector3 direct = new Vector3(0, 1, 0);
                        LineLength = transform.TransformDirection(direct.normalized) * GizmoDistance;
                        Gizmos.DrawLine(axisReferencePosition - LineLength * AxisGizmoSize,
                                axisReferencePosition + LineLength * AxisGizmoSize);
                        IconDist = axisReferencePosition + LineLength * AxisGizmoSize;
                        break;
                    }
                    case DIRECTION.LinearZ:
                    case DIRECTION.RotationZ:
                    {
                        Vector3 direct = new Vector3(0, 0, 1);
                        LineLength = transform.TransformDirection(direct.normalized) * GizmoDistance;
                        Gizmos.DrawLine(axisReferencePosition - LineLength * AxisGizmoSize,
                                axisReferencePosition + LineLength * AxisGizmoSize);
                        IconDist = axisReferencePosition + LineLength * AxisGizmoSize;
                        break;
                    }
                }

                if (icon != null && icon != "")
                    Gizmos.DrawIcon(IconDist, icon, true);
            }
            else //Balljoint Axis
            {
                Vector3 direct = new Vector3(0, 1, 0);
                LineLength = direct.normalized * GizmoDistance;
                Gizmos.DrawLine(axisReferencePosition - LineLength * AxisGizmoSize,
                    axisReferencePosition + LineLength * AxisGizmoSize);
                IconDist = axisReferencePosition + LineLength * AxisGizmoSize;
                direct = new Vector3(1, 0, 0);
                LineLength = direct.normalized * GizmoDistance;
                Gizmos.DrawLine(axisReferencePosition - LineLength * AxisGizmoSize,
                    axisReferencePosition + LineLength * AxisGizmoSize);
                direct = new Vector3(0, 0, 1);
                LineLength = direct.normalized * GizmoDistance;
                Gizmos.DrawLine(axisReferencePosition - LineLength * AxisGizmoSize,
                    axisReferencePosition + LineLength * AxisGizmoSize);
               
                if (icon != null && icon != "")
                    Gizmos.DrawIcon(IconDist, icon, true);
            }
            if (ConnectedAxis != null)
            {
                if(Global.g4acontrollernotnull)
                {
                    if (Selection.activeGameObject == gameObject)
                    {
                        if (gameObject.GetComponent<Drive>())
                            MeshColor = Global.realvirtualcontroller.GetGizmoOptions().MeshColorUpperAxis;
                        else
                        {
                            MeshColor = Global.realvirtualcontroller.GetGizmoOptions().MeshColorConnectedAxis;
                        }
                    }
                }
                else
                {
                    MeshColor=Color.green;
                }
                var ConAxis = ConnectedAxis.GetComponent<Axis>();
                if (ConAxis != null)
                {
                 //   ConAxis.DisplayGroupMeshes(MeshColor,true);
                }
            }

            if (SubDriveAxises.Count > 0)
            {
                if (Global.g4acontrollernotnull)
                {
                    MeshColor = Global.realvirtualcontroller.GetGizmoOptions().MeshColorConnectedAxis;
                }
                else
                {
                    MeshColor=Color.green;
                }

                foreach (var axis in SubDriveAxises)
                {
                  // axis.DisplayGroupMeshes(MeshColor,true); 
                }
            }
#endif
        }
        private void CheckforSecondaryAxis()
        {
            var second = gameObject.GetComponentsInChildren<Axis>();
            if (second.Length > 0)
            {
                foreach (var secAxis in second)
                {
                    if (!secondaryAxis.Contains(secAxis.gameObject) && secAxis.gameObject!=gameObject)
                    {
                        secondaryAxis.Add(secAxis.gameObject);
                    }
                }
            }
        }
        private void Awake()
        {
            CheckforSecondaryAxis();
            if (gameObject.GetComponent<Drive>() == null)
                physicalAxis = true;
            
            if (ConnectedAxis != null && physicalAxis==false)
            {
                transform.parent = ConnectedAxis.transform;
            }
            // physic axis
            if (physicalAxis)
            {
                if(ConnectedAxis!=null && !IsSecondaryAxis)
                {
                    if (!ConnectedAxis.GetComponent<Rigidbody>())
                        ConnectedAxis.AddComponent<Rigidbody>();
                    
                    CreateJoint(ref Mainrotationjoint, ref Mainlinearjoint,Balljoint,Direction,transform.position, ConnectedAxis.GetComponent<Rigidbody>(),UseLimits,UpperLimit,LowerLimit);
                }
                if (secondaryAxis.Count > 0)
                {
                    foreach (var secAxis in secondaryAxis)
                    {
                        var axisComp = secAxis.GetComponent<Axis>();
                        if (axisComp.ConnectedAxis == null)
                        {
                            //messagebox in play mode
                            string message = "No connected axis defined for secondary axis of " + gameObject.name + ".";
                            Debug.LogError(message);
#if UNITY_EDITOR
                            EditorUtility.DisplayDialog("Parameter Missing", message, "OK");
                            // To stop the play mode
                            EditorApplication.isPlaying = false;
#endif
                        }
                        else
                        {
                            if (!axisComp.ConnectedAxis.GetComponent<Rigidbody>())
                                axisComp.ConnectedAxis.AddComponent<Rigidbody>();

                            CreateJoint(ref axisComp.Secrotationjoint, ref axisComp.Seclinearjoint, axisComp.Balljoint,
                                axisComp.Direction, axisComp.axisReferencePosition,
                                axisComp.ConnectedAxis.GetComponent<Rigidbody>(),
                                axisComp.UseLimits, axisComp.UpperLimit, axisComp.LowerLimit);
                        }
                    }
                }
            }
            else
            {
                var drive = gameObject.GetComponent<Drive>();
                drive.UseLimits = UseLimits;
                if(UseLimits)
                {
                    drive.UpperLimit = UpperLimit;
                    drive.LowerLimit = LowerLimit;
                }
            }
        }

        //! Deletes this axis and all connected secondary axes from the hierarchy.
        public void AxisDelete()
        {
            if (parentAxis != null)
            {
                parentAxis.removeSecondaryAxis(gameObject);
            }

            if (secondaryAxis.Count > 0)
            {
                foreach (var secAxis in secondaryAxis)
                {
                    Axis subaxis = secAxis.GetComponent<Axis>();
                    if(subaxis!=null)
                        subaxis.AxisDelete();
                }
            }
        }

        private void ObjectDelete()
        {
            if (AxisReferenceGameObject != null)
            {
                if (AxisReferenceGameObject.GetComponent<Group>())
                {
                    DestroyImmediate(AxisReferenceGameObject.GetComponent<Group>());
                }
            }

            if (parentAxis != null)
            {
                parentAxis.removeSecondaryAxis(gameObject);
            }
        }
        public void OnDestroy()
        {
            var Kinematic = GetComponent<Kinematic>();
            if (Kinematic != null)
            {
                var comps=Kinematic.GetAllWithGroup(Kinematic.GetGroupName());
                foreach (var obj in comps)
                {
                    var group = obj.GetComponent<Group>();
                    if (group != null)
                    {
                        DestroyImmediate(group);
                    }
                }
            }
        }
         private void CreateJoint(ref HingeJoint rotationjoint,ref ConfigurableJoint ConfJoint  ,bool Balljoint,DIRECTION Direction,Vector3 center, Rigidbody ConnectedTo,bool UseLimits,float UpperLimit,float LowerLimit)
        {
            if (Balljoint)
            {
                ConfJoint = gameObject.AddComponent<ConfigurableJoint>();
                AnchorPos = center;
                ConfJoint.anchor = transform.InverseTransformPoint(center);
     
                ConfJoint.axis = DirectionToVector(Direction);
                ConfJoint.connectedBody = ConnectedTo;

                ConfJoint.angularXMotion = ConfigurableJointMotion.Free;
                ConfJoint.angularYMotion = ConfigurableJointMotion.Free;
                ConfJoint.angularZMotion = ConfigurableJointMotion.Free;
                
                ConfJoint.xMotion = ConfigurableJointMotion.Locked;
                ConfJoint.yMotion = ConfigurableJointMotion.Locked;
                ConfJoint.zMotion = ConfigurableJointMotion.Locked;
            }

            if (!Balljoint)
            {
                if (Direction == DIRECTION.RotationX || Direction == DIRECTION.RotationY ||
                    Direction == DIRECTION.RotationZ)
                {
                    rotationjoint = gameObject.AddComponent<HingeJoint>();
                    rotationjoint.anchor = transform.InverseTransformPoint(center);
                    rotationjoint.axis = DirectionToVector(Direction);
                    rotationjoint.connectedBody = ConnectedTo;
                }

                if (Direction == DIRECTION.LinearX || Direction == DIRECTION.LinearY || Direction == DIRECTION.LinearZ)
                {
                    ConfJoint = gameObject.AddComponent<ConfigurableJoint>();
                    ConfJoint.anchor = transform.InverseTransformPoint(center);
                    ConfJoint.axis = DirectionToVector(Direction);
                    ConfJoint.connectedBody = ConnectedTo;

                    ConfJoint.angularXMotion = ConfigurableJointMotion.Locked;
                    ConfJoint.angularYMotion = ConfigurableJointMotion.Locked;
                    ConfJoint.angularZMotion = ConfigurableJointMotion.Locked;

                    if (Direction == DIRECTION.LinearX)
                    {
                        ConfJoint.xMotion = ConfigurableJointMotion.Free;
                        ConfJoint.yMotion = ConfigurableJointMotion.Locked;
                        ConfJoint.zMotion = ConfigurableJointMotion.Locked;
                    }

                    if (Direction == DIRECTION.LinearY)
                    {
                        ConfJoint.xMotion = ConfigurableJointMotion.Locked;
                        ConfJoint.yMotion = ConfigurableJointMotion.Free;
                        ConfJoint.zMotion = ConfigurableJointMotion.Locked;
                    }

                    if (Direction == DIRECTION.LinearZ)
                    {
                        ConfJoint.xMotion = ConfigurableJointMotion.Locked;
                        ConfJoint.yMotion = ConfigurableJointMotion.Locked;
                        ConfJoint.zMotion = ConfigurableJointMotion.Free;
                    }

                }
            }


            if (UseLimits)
            {
                var linearjoint = gameObject.GetComponent<ConfigurableJoint>();
                if (Direction == DIRECTION.LinearX || Direction == DIRECTION.LinearY || Direction == DIRECTION.LinearZ)
                {
                    if (Direction == DIRECTION.LinearX)
                    {
                        linearjoint.xMotion = ConfigurableJointMotion.Limited;
                    }

                    if (Direction == DIRECTION.LinearY)
                    {
                        linearjoint.yMotion = ConfigurableJointMotion.Limited;
                    }

                    if (Direction == DIRECTION.LinearZ)
                    {
                        linearjoint.zMotion = ConfigurableJointMotion.Limited;
                    }


                    localdirection = DirectionToVector(Direction);
                    var ul = UpperLimit / Global.realvirtualcontroller.Scale;
                    var ll = LowerLimit / Global.realvirtualcontroller.Scale;
                    linearjoint.autoConfigureConnectedAnchor = false;
                    var deltapos = ((ul - ll) / 2) + ll;
                    var globalpos = transform.position + transform.TransformDirection(localdirection) * deltapos;
                    if (ConnectedTo == null)
                        linearjoint.connectedAnchor = globalpos;
                    else
                        linearjoint.connectedAnchor = ConnectedTo.transform.InverseTransformPoint(globalpos);
                    startpos = this.transform.localPosition;
                    linearjoint.linearLimit = new SoftJointLimit();
                    SoftJointLimit limit = linearjoint.linearLimit; //First we get a copy of the limit we want to change
                    limit.limit = ((ul - ll) / 2); //set the value that we want to change
                    linearjoint.linearLimit = limit; //set the joint's limit to our edited version.
                }
                else
                {
                    JointLimits limits = rotationjoint.limits;
                    limits.min = LowerLimit;
                    limits.max = UpperLimit;
                    rotationjoint.limits = limits;
                    rotationjoint.useLimits = true;
                }
            }
            else
            {
                startpos = this.transform.localPosition;
            }
            var rb = GetComponent<Rigidbody>();
            rb.WakeUp();
        }
         //! Enables or disables the breaking (locking) of this axis movement.
         public void Break(bool breaking)
         {
             var rb = GetComponent<Rigidbody>();
             if (breaking)
             {
                 if (Direction == DIRECTION.RotationX || Direction == DIRECTION.RotationY || Direction == DIRECTION.RotationZ)
                 {
                   
                     rb.freezeRotation = true;
                 }
                 else
                 {
                     rb.constraints = RigidbodyConstraints.FreezePosition;
                 }

             }
             else
             {
                 if (Direction == DIRECTION.RotationX || Direction == DIRECTION.RotationY || Direction == DIRECTION.RotationZ)
                 {
                     rb.freezeRotation = false;
                 }
                 else
                 {
                     rb.constraints = RigidbodyConstraints.None;
                 }
                
          
             }
         
         }
         //! Converts a DIRECTION enum value to its corresponding Vector3 direction.
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
         //! Determines if the given direction represents linear movement (returns true) or rotational movement (returns false).
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
        void FixedUpdate()
        {
            if(physicalAxis)
            {
               
                if (!DirectionIsLinear(Direction))
                {
                    if(!IsSecondaryAxis)
                    {
                        if (!Balljoint)
                            Position = Mainrotationjoint.angle;
                    }
                    else
                    {
                        if(!Balljoint)
                            Position = Secrotationjoint.angle;
                    }
                }
                else 
                { 
                    float diff = Vector3.Dot(localdirection, transform.localPosition - startpos);
                    Position = diff * Global.realvirtualcontroller.Scale;
                }
                

                if (UseLimits)
                {
                    OnUppperLimit = (Position > UpperLimit - LimitEndPosWindow);
                    OnLowerLimit = (Position < LowerLimit + LimitEndPosWindow);

                }
            }
        }
        private List<GameObject> GetAllMeshesWithGroup(string group)
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
    }
#pragma warning restore CS3001, CS3002, CS3003, CS3009
}
