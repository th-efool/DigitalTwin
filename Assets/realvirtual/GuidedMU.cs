// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace realvirtual
{

    //! GuidedMU enables MU (material unit) objects to follow guided transport surfaces using physics constraints.
    //! It automatically detects transport surfaces below the MU and creates physical joints to guide movement along conveyors,
    //! paths, and other transport systems while maintaining proper orientation and speed synchronization.
    [RequireComponent(typeof(MU))]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class GuidedMU : realvirtualBehavior, ISourceCreated
    {
        
        [Header("Settings")] 
        [Tooltip("Maximum distance in meters to detect transport surfaces below the MU")]
        public float RaycastLength = 0.3f; //!< Maximum distance in meters to detect transport surfaces below the MU
        [Tooltip("Layer mask defining which layers contain transport surfaces")]
        [SerializeField] public LayerMask RaycastLayer; //!< Layer mask defining which layers contain transport surfaces
        [Tooltip("Enables visual debugging gizmos showing raycast and connection points")]
        [SerializeField]
        public bool DebugMode; //!< Enables visual debugging gizmos showing raycast and connection points
        [ReadOnly] public bool IsFixed; //!< Indicates if the MU is currently fixed by a Fixer component
        [Header("State")]
        private  IGuidedSurface transportSurface;
        private MU mu;
        private Transform _transform;
        private IGuidedSurface lastTransport;
        private Rigidbody _rigidbody;
        private ConfigurableJoint _joint;
        private float _angleOffset;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[2];
        private bool issource = false;
        private GameObject lasthitgo;
        private GameObject currenthitgo;
        

        private void OnEnable()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponentInChildren<Rigidbody>();
            mu = GetComponent<MU>();
            issource = GetComponent<Source>() != null;
            mu.EventMUFix.AddListener(OnMUFixed);
        }

        private void OnMUFixed(MU mu, bool isfixed)
        {
            IsFixed = isfixed;
            if (isfixed)
            {
                if (_joint != null) DestroyImmediate(_joint);
                lasthitgo = null;
            } 
       
        }

        private void FixedUpdate()
        {
            if (issource) return;
            Raycast();
            Move();
        }

        private void Reset()
        {
            RaycastLayer = LayerMask.GetMask("rvTransport", "rvSimStatic");
        }

        private void Raycast()
        {
            if (_rigidbody.isKinematic) return;
            if (IsFixed) return;
            var raycastPosition = transform.position + Vector3.up * 0.05f;;
            var hits = Physics.RaycastNonAlloc(raycastPosition, Vector3.down,
                _raycastHits, RaycastLength, RaycastLayer);

            if (hits == 0)
            {
                if (_joint != null) DestroyImmediate(_joint);
                transportSurface = null;
                return;
            }

            var hitIndex = 0;
            if (hits > 1)
            {
                hitIndex = GetClosestHitIndex(_raycastHits);
            }
            
            currenthitgo = _raycastHits[hitIndex].transform.gameObject;
            if (currenthitgo != lasthitgo)
            {
                transportSurface = currenthitgo.GetComponentInChildren<IGuidedSurface>();
                if (transportSurface == null)
                {
                    if (_joint != null) Destroy(_joint);
                    currenthitgo = null;
                }
                else
                {
                    if (transportSurface.IsSurfaceGuided())
                    {
                        _angleOffset = GetOffsetAngle(transportSurface);
                        CreateJoint();
                    }
                    else
                    {
                        if (_joint != null) DestroyImmediate(_joint);
                        currenthitgo = null;
                    }
                }
            }

            lastTransport = transportSurface;
            lasthitgo = currenthitgo;
        }

        private int GetClosestHitIndex(IReadOnlyList<RaycastHit> hits)
        {
            var distance = Mathf.Infinity;
            var result = 0; 
            for (var i = 0; i < hits.Count; i++)
            {
                if (distance < hits[i].distance) continue;
                distance = hits[i].distance;
                result = i;
            }

            return result;
        }

        private void CreateJoint()
        {
            _joint = TryGetComponent(out ConfigurableJoint joint) ? joint : gameObject.AddComponent<ConfigurableJoint>();
            _joint.anchor = new Vector3(0, 0, 0);
            _joint.secondaryAxis = new Vector3(0, 1, 0);
            _joint.autoConfigureConnectedAnchor = false;
            _joint.xMotion = ConfigurableJointMotion.Free;
            _joint.yMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;
            _joint.angularXMotion = ConfigurableJointMotion.Locked;
            _joint.angularYMotion = ConfigurableJointMotion.Free;
            _joint.angularZMotion = ConfigurableJointMotion.Locked;
        }


        protected override void OnStopSim()
        {
            if (_joint != null) 
                    mu.PhysicsOff();
        }

        protected override void OnStartSim()
        {
            if (_joint != null)
                  mu.PhysicsOn();
        }

        private void Move()
        {
           
            if (_joint == null) return;
            if (transportSurface == null) return;
        
            _joint.connectedAnchor =  transportSurface.GetClosestPoint(_transform.position);
            _joint.axis = Quaternion.AngleAxis(_angleOffset, Vector3.up) * Vector3.forward;
            
            var normal = transportSurface.GetClosestDirection(_transform.position);
            var newrot = Quaternion.LookRotation(normal, Vector3.up) * Quaternion.AngleAxis(_angleOffset, Vector3.up);
            _rigidbody.transform.rotation = newrot;
            _rigidbody.angularVelocity = Vector3.zero;

            var drive = transportSurface.GetDrive();
            if (drive == null)
            {
                Logger.Warning($"Transport surface '{transportSurface}' has no Drive component. GuidedMU cannot move.", this);
#if UNITY_6000_0_OR_NEWER
                _rigidbody.linearVelocity = Vector3.zero;
#else
                _rigidbody.velocity = Vector3.zero;
#endif
                return;
            }
            
            var speed = drive.IsSpeed;
            if (speed == 0) 
#if UNITY_6000_0_OR_NEWER
                _rigidbody.linearVelocity = Vector3.zero;
#else
                _rigidbody.velocity = Vector3.zero;
#endif
            else
#if UNITY_6000_0_OR_NEWER
                _rigidbody.linearVelocity = normal * (speed/realvirtualController.Scale);
#else
                _rigidbody.velocity = normal * (speed/realvirtualController.Scale);
#endif
        }

    
        private float GetOffsetAngle(IGuidedSurface transport)
        {
            var normal = transport.GetClosestDirection(_transform.position);
            var angle  = Vector3.SignedAngle(normal, transform.forward, Vector3.up);
            return Mathf.Round(angle / 90f) * 90f;
        }

        private void OnDrawGizmos()
        {
            if (!DebugMode) return;
            if (transportSurface == null) return;

            var point = transportSurface.GetClosestPoint(_transform.position);
            var normal = transportSurface.GetClosestDirection(_transform.position);
            var forward =  _rigidbody.transform.right;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.02f);
            Gizmos.DrawLine(transform.position, transform.position + normal*0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * RaycastLength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + forward * 0.2f);

        }

        //! Called when this MU is no longer a source object and can start being guided by transport surfaces.
        public void OnSourceCreated()
        {
            issource = false;
        }
    }
}
