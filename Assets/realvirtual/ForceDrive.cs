// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SimpleJoint))]
    //! Moves Rigidbodies based on joints and forces. Acts like a cylinder, forward and backward movement can controlled by PLC signals. In comparison to Drive_Cylinder this component is fully
    //! compatible with Unity physics but the positioning to an exact position is not possible due to pure physics based behaviour
#pragma warning disable 0108
    public class ForceDrive : BaseDrive, ISignalInterface
    {
        [Header("Settings")]
        [Tooltip("Force in Newton")]
        public float ForceNewton=100;  //!< Force in Newton
        [Tooltip("Displays Force Direction")]
        public bool DebugMode=false; 
        [ShowIf("DebugMode")]
        [Tooltip("Scale divider for Debug, if 1 then 1 Newton = 1 Unity Scale")]
        public float DebugScale=1; 
        [Header("Signals")]
        [Tooltip("Apply the force in forward direction based on attached Joint direction")]
        public bool Forward;  //!< Apply the force in forward direction based on attached Joint direction
        [Tooltip("Apply the force in backward direction based on attached Joint direction")]
        public bool Backward;  //!< Apply the force in backward direction based on attached Joint direction

        [Header("PLC IOs")] 
        [Tooltip("PLC output signal for force (connection is optional)")]
        public PLCOutputFloat SignalForce;  //!< PLC output signal for force (connection is optional)
        [Tooltip("PLC output signal for applying force in forward direction (connection is optional)")]
        public PLCOutputBool ForceForward;  //!< PLC output signal for applying force in forward direction (connection is optional)
        [Tooltip("PLC output signal for applying force in backward direction (connection is optional)")]
        public PLCOutputBool ForceBackward;  //!< PLC output signal for applying force in backward direction (connection is optional)
        [Tooltip("PLC output signal for breaking (blocking the Joint)")]
        public PLCOutputBool SignalBrake; //!< PLC Signal for breaking
        [Tooltip("PLC input signal, true if attached joint reaches upper limit")]
        public PLCInputBool OnUpperLimit;  //!< PLC input signal, true if attached joint reaches upper limit
        [Tooltip("PLC input signal, true if attached joint reaches lower limit")]
        public PLCInputBool OnLowerLimit;  //!< PLC input signal, true if attached joint reaches lower limit
        [Tooltip("PLC input signal for current joint position")]
        public PLCInputFloat CurrentPosition; //!< PLC Signal for the current joints position
        private Vector3 dir;
        private Vector2 localdir;
        private  Rigidbody rigidbody;
        private SimpleJoint _simmpleJoint;
        private bool signalforcenotnull,signalforwardnotnull,signalbackwardnotnull,onupperlimitnotnull,onlowerlimitnotnull,signalbrakenotnull,breakbefore,signalpositionnotnull;

        // Start is called before the first frame update
        void Start()
        {
          
            rigidbody = GetComponent<Rigidbody>();
            _simmpleJoint = GetComponent<SimpleJoint>();
            localdir = DirectionToVector(_simmpleJoint.Axis);
            signalforcenotnull = SignalForce != null;
            signalforwardnotnull = ForceForward != null;
            signalbackwardnotnull = ForceBackward != null;
            onupperlimitnotnull = OnLowerLimit != null;
            onlowerlimitnotnull = OnLowerLimit != null;
            signalbrakenotnull = SignalBrake != null;
            signalpositionnotnull = CurrentPosition != null;
            breakbefore = false;
        }

        void Update()
        {
            if (DebugMode)
            {
                Global.DebugDrawArrow(rigidbody.position,dir*ForceNewton/DebugScale,Color.green);
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            dir = transform.TransformDirection(localdir);
            if (signalforcenotnull)
                ForceNewton = SignalForce.Value;

            if (signalforwardnotnull)
                Forward = ForceForward.Value;

            if (signalbackwardnotnull)
                Backward = ForceBackward.Value;

            if (signalbrakenotnull)
            {
                if (SignalBrake.Value != breakbefore)
                {
                    _simmpleJoint.Break(SignalBrake.Value);
                }
                breakbefore = SignalBrake.Value;
            }

            if (DirectionIsLinear(_simmpleJoint.Axis))
            {
                if (Forward)
                    rigidbody.AddForce(dir * ForceNewton);
                if (Backward)
                    rigidbody.AddForce(-dir * ForceNewton);
            }
            else
            {
                if (Forward)
                    rigidbody.AddTorque(dir * ForceNewton);
                if (Backward)
                    rigidbody.AddTorque(-dir * ForceNewton);
            }

            if (onlowerlimitnotnull)
            {
                OnLowerLimit.Value = _simmpleJoint.OnLowerLimit;
            }

            if (onupperlimitnotnull)
            {
                OnUpperLimit.Value = _simmpleJoint.OnUppperLimit;
            }

            if (signalpositionnotnull)
            {
                CurrentPosition.Value = _simmpleJoint.Position;
            }
            
         
        }
    }
}

