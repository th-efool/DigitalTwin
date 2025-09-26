using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    //! MeasureForce measures forces and torques acting on Unity joints.
    //! This component reads force and torque values from a joint component and outputs them through PLC signals.
    //! Useful for monitoring forces in mechanical simulations and detecting load conditions.
    public class MeasureForce : BehaviorInterface
    {
        [Tooltip("Current force vector acting on the joint in Newtons")]
        public Vector3 Force; //!< Current force vector in Newtons acting on the joint
        
        [Tooltip("Current torque vector acting on the joint in Newton-meters")]
        public Vector3 Torque; //!< Current torque vector in Newton-meters acting on the joint

        [Tooltip("Absolute magnitude of the force in Newtons")]
        public float AbsForce; //!< Absolute magnitude of the force vector in Newtons
        
        [Tooltip("Absolute magnitude of the torque in Newton-meters")]
        public float AbsTorque; //!< Absolute magnitude of the torque vector in Newton-meters
        
        // Start is called before the first frame update
        private UnityEngine.Joint rb;

        [Tooltip("PLC input signal for force X component in Newtons")]
        public PLCInputFloat ForceX; //!< PLC signal output for force X component in Newtons
        
        [Tooltip("PLC input signal for force Y component in Newtons")]
        public PLCInputFloat ForceY; //!< PLC signal output for force Y component in Newtons
        
        [Tooltip("PLC input signal for force Z component in Newtons")]
        public PLCInputFloat ForceZ; //!< PLC signal output for force Z component in Newtons
        
        [Tooltip("PLC input signal for absolute force magnitude in Newtons")]
        public PLCInputFloat ForceAbs; //!< PLC signal output for absolute force magnitude in Newtons
        
        [Tooltip("PLC input signal for torque X component in Newton-meters")]
        public PLCInputFloat TorqueX; //!< PLC signal output for torque X component in Newton-meters
        
        [Tooltip("PLC input signal for torque Y component in Newton-meters")]
        public PLCInputFloat TorqueY; //!< PLC signal output for torque Y component in Newton-meters
        
        [Tooltip("PLC input signal for torque Z component in Newton-meters")]
        public PLCInputFloat TorqueZ; //!< PLC signal output for torque Z component in Newton-meters
        
        [Tooltip("PLC input signal for absolute torque magnitude in Newton-meters")]
        public PLCInputFloat TorqueAbs; //!< PLC signal output for absolute torque magnitude in Newton-meters

        private bool fx, fy, fz,fa, tx, ty, tz,ta;
        
        void Start()
        {
            rb = GetComponent<UnityEngine.Joint>();

            fx = ForceX != null;
            fy = ForceY != null;
            fz = ForceZ != null;
            fa = ForceAbs!= null;
            
            tx = TorqueX != null;
            ty = TorqueY != null;
            tz = TorqueZ != null;
            ta = TorqueAbs != null;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Force = rb.currentForce;
            Torque = rb.currentTorque;
            AbsForce = Force.magnitude;
            AbsTorque = Torque.magnitude;
            
            if (fx)
                ForceX.Value = Force.x;
            if (fy)
                ForceY.Value = Force.y;
            if (fz)
                ForceZ.Value = Force.z;
            if (fa)
                ForceAbs.Value = AbsForce;

            
            if (tx)
                TorqueX.Value = Torque.x;
            if (ty)
                TorqueY.Value = Torque.y;
            if (tz)
                TorqueZ.Value = Torque.z;
            if (ta)
                TorqueAbs.Value = AbsTorque;
        }
    }

}
