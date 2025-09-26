// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;
using UnityEngine.Serialization;
using NaughtyAttributes;

namespace realvirtual
{
    //! SimpleJoint streamlines Unity physics joint configuration for industrial automation simulations.
    //! Automatically creates and configures Unity Configurable or Hinge joints with proper axis alignment and limits.
    //! Essential for physics-based mechanisms like spring-loaded parts, dampers, or gravity-affected movements.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/joint")]
    public class SimpleJoint : realvirtualBehavior
    {
      
        [Tooltip("Joint axis direction in local coordinate system (Linear or Rotational)")]
        public DIRECTION
            Axis; //!< The direction in local coordinate system of the GameObject where the drive is attached to.
        [Tooltip("Rigidbody to connect this joint to (leave empty for world-fixed joint)")]
        [InfoBox("Keep Connected Body empty if joint position is not changing")]
        public Rigidbody ConnectedTo;

        [Tooltip("Enables custom pivot point position for the joint")]public bool ChangeCenter;
        [Tooltip("Offset from current position to the joint pivot point in mm")]
        [ShowIf("ChangeCenter")]public Vector3 DeltaCenter;
        [Tooltip("First object to define joint center position")]
        [ShowIf("ChangeCenter")]public GameObject CenterOnObject1;
        [Tooltip("Second object - joint center will be midpoint between Object1 and Object2")]
        [ShowIf("ChangeCenter")]public GameObject CenterOnObject2;
            
       [Tooltip("Enables movement limits for the joint")]
       public bool UseLimits;
        [Tooltip("Upper Limit in mm")][ShowIf("UseLimits")]public float UpperLimit;
        [Tooltip("Upper Limit in mm")][ShowIf("UseLimits")]public float LowerLimit;
        [Tooltip("Detection window in mm of upper and lower limit")][ShowIf("UseLimits")] public float LimitEndPosWindow=1;
        
        [Tooltip("Shows joint axis and connected body visualization in Scene view")]
        public bool ShowGizmo = true;
        
        [ReadOnly] public UnityEngine.Joint UnityJoint;
        [ReadOnly] public float Position;
        [ReadOnly] [ShowIf("UseLimits")]public bool OnLowerLimit;
        [ReadOnly] [ShowIf("UseLimits")]public bool OnUppperLimit;
        
        private Vector3 startpos,localdirection,rotationdir;
        private ConfigurableJoint linearjoint;
        private HingeJoint rotationjoint;
        
         Vector3 CalculateCenter()
        {
            var center = transform.position;
            if (ChangeCenter)
            {
                if (CenterOnObject1 != null)
                    center = CenterOnObject1.transform.position;
                if (CenterOnObject1 != null && CenterOnObject2 != null)
                    center = (CenterOnObject1.transform.position + CenterOnObject2.transform.position) / 2;
                center = center + transform.TransformDirection(DeltaCenter);
            }

            return center;
        }
         
        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR

            var center = CalculateCenter();
            var localdirection = DirectionToVector(Axis);
            var  globaldirection = transform.TransformDirection(localdirection);
            var size = 0.2f;
            Gizmos.color = Color.red;
            if (ShowGizmo)
            {
            
              
                    Bounds bounds = Global.GetTotalBounds(this.gameObject);
                    size = Vector3.Magnitude(bounds.size);
            

                Gizmos.DrawLine(center - globaldirection * size, center + globaldirection * size);
                Gizmos.color = Color.yellow;
                if (ConnectedTo != null)
                {
                    var meshes = ConnectedTo.gameObject.GetComponentsInChildren<MeshFilter>();
                    foreach (var mesh in meshes)
                    {
                        Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.gameObject.transform.position,
                            mesh.gameObject.transform.rotation,
                            mesh.gameObject.transform.localScale);
                    }

                    var kin = ConnectedTo.GetComponent<Kinematic>();
                    if (kin != null)
                    {
                        kin.DisplayGroupMeshes(Color.yellow);
                    }
                }

             
                
            }

#endif
        }


        public void Break(bool breaking)
        {
            var rb = GetComponent<Rigidbody>();
            if (breaking)
            {
                if (Axis == DIRECTION.RotationX || Axis == DIRECTION.RotationY || Axis == DIRECTION.RotationZ)
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
                if (Axis == DIRECTION.RotationX || Axis == DIRECTION.RotationY || Axis == DIRECTION.RotationZ)
                {
                    rb.freezeRotation = false;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
                
          
            }
         
        }
        
        public void InitJoint()
        {
            
            if (Axis == DIRECTION.RotationX || Axis == DIRECTION.RotationY || Axis == DIRECTION.RotationZ)
            { 
                rotationjoint = gameObject.AddComponent<HingeJoint>();
                var center = CalculateCenter();
                rotationjoint.anchor = transform.InverseTransformPoint(center);
                rotationjoint.axis = DirectionToVector(Axis);
                rotationjoint.connectedBody = ConnectedTo;
            }
            
            if (Axis == DIRECTION.LinearX || Axis == DIRECTION.LinearY || Axis == DIRECTION.LinearZ)
            {
                linearjoint = gameObject.AddComponent<ConfigurableJoint>();
                var center = CalculateCenter();
                linearjoint.anchor = transform.InverseTransformPoint(center);
                linearjoint.axis = DirectionToVector(Axis);
                linearjoint.connectedBody = ConnectedTo;
                
                linearjoint.angularXMotion = ConfigurableJointMotion.Locked;
                linearjoint.angularYMotion = ConfigurableJointMotion.Locked;
                linearjoint.angularZMotion = ConfigurableJointMotion.Locked;

                if (Axis == DIRECTION.LinearX)
                {
                    linearjoint.xMotion = ConfigurableJointMotion.Free;
                    linearjoint.yMotion = ConfigurableJointMotion.Locked;
                    linearjoint.zMotion = ConfigurableJointMotion.Locked;
                }
                
                if (Axis == DIRECTION.LinearY)
                {
                    linearjoint.xMotion = ConfigurableJointMotion.Locked;
                    linearjoint.yMotion = ConfigurableJointMotion.Free;
                    linearjoint.zMotion = ConfigurableJointMotion.Locked;
                }
                
                if (Axis == DIRECTION.LinearZ)
                {
                    linearjoint.xMotion = ConfigurableJointMotion.Locked;
                    linearjoint.yMotion = ConfigurableJointMotion.Locked;
                    linearjoint.zMotion = ConfigurableJointMotion.Free;
                }
                
            }


            if (UseLimits)
            {
                var linearjoint = gameObject.GetComponent<ConfigurableJoint>();
                if (Axis == DIRECTION.LinearX || Axis == DIRECTION.LinearY || Axis == DIRECTION.LinearZ)
                {
                    if (Axis == DIRECTION.LinearX)
                    {
                        linearjoint.xMotion = ConfigurableJointMotion.Limited;
                    }

                    if (Axis == DIRECTION.LinearY)
                    {
                        linearjoint.yMotion = ConfigurableJointMotion.Limited;
                    }

                    if (Axis == DIRECTION.LinearZ)
                    {
                        linearjoint.zMotion = ConfigurableJointMotion.Limited;
                    }


                    localdirection = DirectionToVector(Axis);
                    var ul = UpperLimit / realvirtualController.Scale;
                    var ll = LowerLimit / realvirtualController.Scale;
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
        
        // Start is called before the first frame update
        new void Awake()
        {
            base.Awake();
            if (this.enabled)
                  InitJoint();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!DirectionIsLinear(Axis))
            {
                Position = rotationjoint.angle;
            }
            else
            {
                float diff = Vector3.Dot(localdirection, transform.localPosition - startpos);
                Position = diff * realvirtualController.Scale;
            }

            if (UseLimits)
            {
                OnUppperLimit = (Position > UpperLimit - LimitEndPosWindow);
                OnLowerLimit = (Position <  LowerLimit + LimitEndPosWindow);
                
            }
        }
    }

}
