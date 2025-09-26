// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace realvirtual
{
    //! MoveMU (Move Unit) controls physics-based movement of objects with direction and velocity management.
    //! This component enables objects to move in specified directions with automatic alignment and physics material switching
    //! for smooth movement and stopping behavior. It integrates with Unity's physics system for realistic motion.
    public class MoveMU : MonoBehaviour
    {
        #region Public Attributes

        [Tooltip("Physics material applied when the object stops to prevent sliding")]
        public PhysicsMaterial MaterialStop; //!< Physics material applied when the object stops to prevent sliding
        [ReadOnly] public Vector3 Direction; //!< Current movement direction vector (normalized)
        [Tooltip("Aligns the object's rotation to match the movement direction")]
        public bool Align; //!< Aligns the object's rotation to match the movement direction
        [ReadOnly] public float Velocity; //!< Current movement velocity in units per second
        [ReadOnly] public BoxCollider BoxCollider; //!< Reference to the object's box collider component
        [ReadOnly] public Rigidbody Rigidbody; //!< Reference to the object's rigidbody component
        #endregion
        
        private Rigidbody _rigidbody;
        private Vector3 lastDirection;
        private PhysicsMaterial physicMat_move, physicMat_stop;
        private float angle;
        private Vector3 rot;
        private float time;
        private Vector3 curVelocity;

        void Start()
        {
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            BoxCollider = gameObject.GetComponentInChildren<BoxCollider>();
            physicMat_move = BoxCollider.material;
            physicMat_stop = MaterialStop;
        }
        
        //! Executes the movement logic based on Direction and Velocity settings.
        public void Move()
        {
            if(lastDirection == Vector3.zero)
            {
                lastDirection = Direction;
            }
		
            if (Direction != Vector3.zero)
            {
                if (Velocity != 0)
                {
                    if (Rigidbody.IsSleeping())
                    {
                        Rigidbody.WakeUp();
                    }
                    BoxCollider.material = physicMat_move;
                    Direction = Direction.normalized;

                    if(Align)
                    {
                        lastDirection.y = Direction.y;
                        angle = Vector3.Angle(Direction,lastDirection);
                        angle = -1* angle * Mathf.Sign(Vector3.Cross(Direction, lastDirection).y);
					
                        rot = Quaternion.Euler(0, angle, 0) * Rigidbody.transform.right;
                        float step = 2.0f * Time.deltaTime;
                        Rigidbody.transform.right = Vector3.MoveTowards(Rigidbody.transform.right, rot, step);
                        lastDirection = Direction;
                    }

                    curVelocity = Rigidbody.linearVelocity;
                    curVelocity.x = Velocity * Direction.x;
                    curVelocity.z = Velocity * Direction.z;
                    Rigidbody.linearVelocity = curVelocity;
                    Rigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    BoxCollider.material = physicMat_stop;
                    Rigidbody.linearVelocity = Vector3.zero;
                    Rigidbody.angularVelocity = Vector3.zero;
                }
            }
            Velocity = 0;
            Direction = Vector3.zero;
            Align = false;
        }
        
        public virtual void Update ()
        {
            if (!Rigidbody.isKinematic) {
                Move ();
            }
            else
            {
                Velocity = 0;
                Direction = Vector3.zero;
                lastDirection = Vector3.zero;
            }
        }

    }
}

#endif // UNITY_6000_0_OR_NEWER