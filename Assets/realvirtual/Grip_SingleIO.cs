
using UnityEngine;

namespace realvirtual
{
    [RequireComponent(typeof(Grip))]
    public class Grip_SingleIO : BehaviorInterface
    {
        [Tooltip("Integer PLC signal to control grip (>0 picks, 0 places)")]
        public PLCOutputInt GripInteger;
        [Tooltip("Boolean PLC signal to control grip (true picks, false places)")]
        public PLCOutputBool GripBoolean;
        [Tooltip("Optional cylinder drive to control along with grip operation")]
        public Drive_Cylinder ConnectedCylinder;
        [Tooltip("Extend cylinder when picking (true) or retract when picking (false)")]
        public bool OnPickCylinderOut = true;
        private bool _isoutint;
        private bool _isoutbool;

        private Grip grip;

        private bool _cylinder;

        // Start is called before the first frame update
        void Start()
        {
            _isoutint = GripInteger != null;
            _isoutbool = GripBoolean != null;
            grip = GetComponent<Grip>();
            _cylinder = ConnectedCylinder != null;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_isoutint)
            {
                if (GripInteger.Value > 0)
                {
                    grip.PickObjects = true;
                    grip.PlaceObjects = false;
                    if (_cylinder)
                    {
                        ConnectedCylinder._in = !OnPickCylinderOut;
                        ConnectedCylinder._out = OnPickCylinderOut;
                    }
                }
                else
                {
                    grip.PickObjects = false;
                    grip.PlaceObjects = true;
                    if (_cylinder)
                    {
                        ConnectedCylinder._in = OnPickCylinderOut;
                        ConnectedCylinder._out = !OnPickCylinderOut;
                    }
                }
            }

            if (_isoutbool)
            {
                if (GripBoolean.Value)
                {
                    grip.PickObjects = true;
                    grip.PlaceObjects = false;
                    if (_cylinder)
                    {
                        ConnectedCylinder._in = !OnPickCylinderOut;
                        ConnectedCylinder._out = OnPickCylinderOut;
                    }
                }
                else
                {
                    grip.PickObjects = false;
                    grip.PlaceObjects = true;
                    if (_cylinder)
                    {
                        ConnectedCylinder._in = OnPickCylinderOut;
                        ConnectedCylinder._out = !OnPickCylinderOut;
                    }
                }
            }
        }
    }
}