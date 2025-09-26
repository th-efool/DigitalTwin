// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

#if REALVIRTUAL_INTERACT
using XdeEngine.Core;
using XdeEngine.Core.Monitoring;
#endif

#if REALVIRTUAL_AGX
using AGXUnity;
#endif

namespace realvirtual
{
#pragma warning disable 0219
    [System.Serializable]
    public class DriveEvent : UnityEvent<Drive>
    {
    }
    
    
    [SelectionBase]
    [ExecuteInEditMode]
    #region doc
    //! Controls linear and rotational motion of GameObjects with precise position, speed, and acceleration control for industrial automation.

    //! The Drive is one of the core components in realvirtual for simulating motion in automation systems. It moves components including all sub-components
    //! along the local axis of the GameObject. Both rotational and linear movements are possible. A drive can be enhanced by DriveBehaviours which add
    //! special behaviors as well as Input and Output signals for PLC integration.
    //! 
    //! Key Features:
    //! - Precise position and speed control with configurable acceleration and deceleration
    //! - Support for linear movements (X, Y, Z axes) and rotational movements (around X, Y, Z axes)
    //! - Position limits with automatic limit switches and optional cyclic movement
    //! - PLC signal integration for industrial control system connectivity
    //! - Smooth acceleration profiles including S-curve motion for jerk-limited movement
    //! - Speed override capabilities for runtime speed adjustments
    //! - Visual feedback in Unity Editor with gizmos showing movement direction and limits
    //! - Support for both transform-based and physics-based (Rigidbody) movement
    //! 
    //! Common Applications:
    //! - Conveyor belts and transport systems
    //! - Linear actuators and pneumatic cylinders
    //! - Servo motors and rotational axes
    //! - Robot joints and kinematic chains
    //! - Lifting platforms and elevators
    //! - Sliding doors and gates
    //! 
    //! The Drive component can be controlled through:
    //! - Direct properties (JogForward, JogBackward, TargetPosition)
    //! - PLC signals via DriveBehavior components
    //! - Unity Events for position reached notifications
    //! - Custom scripts accessing the public API
    //! 
    //! For detailed documentation see: https://doc.realvirtual.io/components-and-scripts/motion/drive
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/drive")]
    public class Drive : BaseDrive, IXRPlaceable, IEditModeFinished
    {
        #region PublicVariables

        [Header("Settings")] [OnValueChanged("CalculateVectors")]
        [Tooltip("Direction of movement in local coordinate system (X, Y, Z for linear, RotationX/Y/Z for rotational)")]
        public DIRECTION
            Direction; //!< The direction in local coordinate system of the GameObject where the drive is attached to.

        [OnValueChanged("CalculateVectors")]
        [Tooltip("Reverses the movement direction")]
        public bool ReverseDirection; //!< Set to *true* if Direction needs to be inverted.

        [Tooltip("Offeset for defining another Drive 0 position. Drive will start at simulation start at Offset.")]
        public float Offset; //!< Start offset of the drive from zero position in millimeters or degrees.

        [Tooltip("Initial position when simulation starts (mm or degrees)")]
        public float StartPosition; //!< Start Position off the Drive 
        [Tooltip("Speed multiplier for this drive (1 = normal speed, 0.5 = half speed, 2 = double speed)")]
        public float SpeedOverride = 1;  //!< Factor for locally overriding the speed and acceleration of this drive 
        [Tooltip("Speed scaling factor for attached transport surfaces on radial drives")]
        public float
            SpeedScaleTransportSurface =
                1; //!< Scale of the Speed for radial transportsurfaces to feed in mm/s on radius

        [Tooltip(
            "Should be normally turned off. If set to true the RigidBodies are moved. Use it if moving part has attached colliders. If false the transforms are moved")]
        public bool
            MoveThisRigidBody =
                false; //!< If set to true the RigidBodies are moved (use it if moving) part has attached colliders, if false the transforms are moved

        [HideInInspector] public bool EditorMoveMode = false;
        
        [BoxGroup("Limits")]
        [Tooltip("Enable position limits for this drive")]
        public bool UseLimits;

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        [Tooltip("Minimum position limit (mm or degrees)")]
        public float LowerLimit = 0; //!< Lower Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        [Tooltip("Maximum position limit (mm or degrees)")]
        public float UpperLimit = 1000; //!< Upper Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        [Tooltip("Automatically jump to lower limit when upper limit is reached (creates cyclic movement)")]
        public bool JumpToLowerLimitOnUpperLimit = false;

        [ShowIf("UseLimits")]
        [BoxGroup("Limits")]
        [Tooltip("If assigned the Raycast measurment is the basis for the drive Limits")]
        public Sensor LimitRayCast;

        [Space(10)] [BoxGroup("Acceleration")]
        public bool UseAcceleration = false; //!< If set to true the drive uses the acceleration

        
        [BoxGroup("Acceleration")] [ShowIf("UseAcceleration")]
        public bool
            SmoothAcceleration = false; //!< if set to true the drive uses smooth acceleration with a sinoide function

        [ShowIf("UseAcceleration")] [BoxGroup("Acceleration")]
      
        public float Acceleration = 100; //!< The acceleration in millimeter per second^2

        [ShowIf("SmoothAcceleration")] [BoxGroup("Acceleration")]
#if !REALVIRTUAL_PROFESSIONAL
        [InfoBox("Smooth Acceleration is only available in the Professional Version. Please check https://realvirtual.io/ for more information",EInfoBoxType.Error)]
#endif
        public float Jerk = 1000;
        
#if REALVIRTUAL_PROFESSIONAL
        [ShowIf("SmoothAcceleration")] [BoxGroup("Acceleration")]
        public SmoothMotion smoothMotion = new SmoothMotion();
#endif
        
        private GameObject scaleRoot;
        private float initialScale = 1;


        [Header("Drive IO's")] public bool JogForward = false; //!< A jog bit for jogging forward with the Target Speed
        public bool JogBackward = false; //!< A jog bit for jogging backwards with the Target Speed
        public float TargetPosition; //!< The target position of the Drive
        public float TargetSpeed = 100; //!< The target speed in millimeter / second of the Drive

        public bool
            TargetStartMove = false; //!< When changing to true the Drive is starting to move to the TargetPosition

        [HideInInspector]
        public bool
            BlockDestination =
                true; //!< If Block Drive is true it will not drive to its Target Positon, Jogging is possible

        public bool ResetDrive = false; //!< Resets the Drive to the zero position and stops all movements
        public bool _StopDrive = false; //!< Stops the Drive at the current position
        [Foldout("Drive Status")][ReadOnly] public float CurrentSpeed; //!< The current speed of the drive
        [Foldout("Drive Status")][ReadOnly] public float CurrentPosition;
        [Foldout("Drive Status")][ReadOnly] public float PositionOverwriteValue = 0; 
        [Foldout("Drive Status")][ReadOnly] public float IsPosition = 0;//!< current position, can be overwritten by DriveOverwrite or current position
        [Foldout("Drive Status")][ReadOnly] public float IsSpeed = 0;//!< current speed, can be overwritten by SpeedOverwrite or current position
        //!< The current position of the drive
        [Foldout("Drive Status")][ReadOnly] public bool IsStopped = false; //!< Is true if Drive is stopped
        [Foldout("Drive Status")][ReadOnly] public bool IsRunning = false; //!< Is true if Drive is running
        [Foldout("Drive Status")][ReadOnly] public bool IsAtTargetSpeed = false; //!< Is true if Drive is running
        [Foldout("Drive Status")][ReadOnly] public bool IsAtTarget = false; //!< Is true if Drive is at target position
        [Foldout("Drive Status")][ReadOnly] public bool IsAtLowerLimit = false; //!< Is true if Drive is jogging and reaching lower Limit
        [Foldout("Drive Status")][ReadOnly] public bool IsSubDrive;
        
       
        // Add a unity event - before calculating drive
        [Foldout("Drive Events")]public DriveEvent OnBeforeDriveCalculation; //!< Unity Event before calculating drive
        // Add a unity event - after calculating drive
        [Foldout("Drive Events")]public DriveEvent OnAfterDriveCalculation; //!< Unity Event after calculating drive
        [ReadOnly] public bool IsAtUpperLimit = false; //!< Is true if Drive is jogging and reaching upper Limit 
        [HideInInspector] public bool HideGizmos = false;
        [HideInInspector] public float StandardSpeed = 0;
        [HideInInspector] public float StandardAcceleration = 0;
        // XDE Integration
#if REALVIRTUAL_INTERACT
        [HideInInspector]
        public XdeUnitJointMonitor jointmonitor;
        [HideInInspector]
        public XdeUnitJointPDController jointcontroller;
#endif
#if !REALVIRTUAL_INTERACT
        [HideInInspector]
#endif
        public bool UseInteract = false;

        [HideInInspector] public bool IsRotation = false;
        [HideInInspector] public bool PositionOverwrite = false;   //!< true for overwriting position for replaying recordings or multiplayer
        [HideInInspector] public bool SpeedOverwrite = false; //!< true for overwriting speed (transportsurfaces) for replaying recordings or multiplayer
        [HideInInspector] public float SpeedOverwriteValue = 0; //! value for overwriting speed (transportsurfaces) for replaying recordings or multiplayer
        
        // c# delegate for events after drive start init
        [HideInInspector] 
        public delegate void DelegateDriveStartInit(Drive drive);
        [HideInInspector] public DelegateDriveStartInit AfterDriveStartInit;
  
            
        
        #endregion

        #region Private Variables

        private bool _jogactive;
        private float _lastspeed;
        private float _currentdestination;
        private float _timestartacceleration;
        private double _currentacceleration;
        private bool _laststartdrivetotarget;
        private bool _isdrivingtotarget = false;
        private bool _drivetostarted = false;
        private float _lastcurrentposition;
        private bool _istransportsurface = false;
        private bool _lastisattarget = false;
        private float _currentstoppos;
        private bool _stopjogging = false;

        private Vector3 _localdirection;
        private Vector3 _positiondirection;
        private Vector3 _globaldirection;
        private Vector3 _localdirectionscale;
        private Vector3 _localstartpos;
        private Vector3 _localstartrot;
        
        // Smoothing for PositionOverwrite speed calculation
        private float _smoothedSpeed = 0f;
        private const float SPEED_SMOOTHING_FACTOR = 0.15f; // Lower = more smoothing (0.1-0.3 range)
        private const float SPEED_DEADZONE = 0.5f; // Ignore speed changes smaller than this (mm/s or °/s)
        private Quaternion _localstartquat;
        private float _localscale;
        private Rigidbody _rigidbody;
        private Vector3 _rotationpoint;
        private TransportSurface[] _transportsurfaces;
        private Vector3 _globalpos;
        private Quaternion _globalrot;
        private float _controllerscale;
        private bool _lastjog;
        private bool _limitraycastnotnull;
        private IDriveBehavior[] _drivebehaviours;
        [HideInInspector]public Drive[] _subdrives;
       
        private bool articulatedbodynotnull;
        private ArticulationBody articulatedbody;

        private bool useagx;

        private float _targettime;
        
#if REALVIRTUAL_AGX
        private LockController agxlockcontroller;
#endif

        private bool _accelerationstarted = false;
        private bool _decelerationstarted = false;
        
        private bool eventbeforedrivecalculation = false;
        private bool eventafterdrivecalculation = false;

        private bool init = false;
        #endregion

        #region Public Events

        public delegate void
            OnAtPositionEvent(Drive drive); //!< Delegate function for the Drive reaching the destination position

        public event OnAtPositionEvent OnAtPosition; //!< Event triggered when drive reaches the destination position

        public delegate void OnJumpToLowerLimitEvent(Drive drive); //!< Delegate function for when drive jumps to lower limit

        public event OnJumpToLowerLimitEvent OnJumpToLowerLimit; //!< Event triggered when drive jumps to lower limit

        #endregion

        #region Public Methods

#if REALVIRTUAL_INTERACT
        [Button("Kinematize (Interact)")]
        public void Kinematize()
        {
            CalculateVectors();
            realvirtualPhysics.Kinematize(gameObject);
        }

        [Button("Unkinematize (Interact)")]
        public void Uninematize()
        {
            realvirtualPhysics.UnKinematize(gameObject);
        }
#endif
        public List<TransportSurface> GetTransportSurfaces()
        {
            List<TransportSurface> transportsurfaces = new List<TransportSurface>();

            // get all children transportsurfaces
            var surfaces = this.GetComponentsInChildren<TransportSurface>();
            
            // now check if surface is using this Drive
            foreach (var surface in surfaces)
            {
                if (surface.Drive == this)
                    transportsurfaces.Add(surface);
            }

            return transportsurfaces;
        }
        
    

        public void AddSubDrive(Drive drive)
        {
            if (_subdrives == null)
                _subdrives = new Drive[0];
            Array.Resize(ref _subdrives, _subdrives.Length + 1);
            _subdrives[_subdrives.Length - 1] = drive;
            drive.IsSubDrive = true;
        }
      
        //! Initializes the drive for XR/AR placement operations.
        //! IMPLEMENTS IXRPlaceable::OnXRInit
        public void OnXRInit(GameObject placedobj)
        {
          scaleRoot = placedobj;
          initialScale = placedobj.transform.localScale.x;
        }
        
        
        //! Stops the drive when XR/AR placement begins to prevent movement during positioning.
        //! IMPLEMENTS IXRPlaceable::OnXRStartPlace
        public void OnXRStartPlace(GameObject placedobj)
        {
            ForceStop = true;
        }

        //! Resumes drive operation after XR/AR placement is completed.
        //! IMPLEMENTS IXRPlaceable::OnXREndPlace
        public void OnXREndPlace(GameObject placedobj)
        {
            ForceStop = false;
            //currentScale = scaleRoot.transform.localScale.x;
        }

        //! Starts the drive to move forward with the target speed.
        public void Forward()
        {
            JogForward = true;
            JogBackward = false;
        }

        //! Starts the drive to move forward with the target speed.
        public void Backward()
        {
            JogForward = false;
            JogBackward = true;
        }

        //! Starts the drive to drive to the given Target with the target speed.
        public void DriveTo(float Target)
        {
            StandardSpeed = TargetSpeed;
            BlockDestination = false;
            TargetPosition = Target;
            _currentdestination = TargetPosition;
            TargetStartMove = true;
            IsAtTarget = false;
            _drivetostarted = true;
            _lastisattarget = false;
            _targettime = 0;
        }
        
        private void InitSmoothDriveTo(float Target)
        {
#if REALVIRTUAL_PROFESSIONAL
            smoothMotion.SetInitialPosition(CurrentPosition);
            smoothMotion.SetInitialVelocity(CurrentSpeed);
            smoothMotion.SetInitialAcceleration(0);
            smoothMotion.SetTarget(Target, 0);
#endif
        }
        
        private void InitSmoothDriveTo(float Target, float time)
        {
#if REALVIRTUAL_PROFESSIONAL
            smoothMotion.SetInitialPosition(CurrentPosition);
            smoothMotion.SetInitialVelocity(CurrentSpeed);
            smoothMotion.SetInitialAcceleration(0);
            smoothMotion.SetTarget(Target, 0);
            smoothMotion.AdjustDuration(time);
#endif
        }

        private float SmoothPositionUpdate()
        {
#if REALVIRTUAL_PROFESSIONAL
            smoothMotion.speedOverride = SpeedOverride;
            smoothMotion.Integrate(Time.fixedDeltaTime);
            float position = smoothMotion.GetPosition();
            CurrentSpeed = smoothMotion.GetVelocity();
            return position;
#else
            return 0;
#endif
        }
        
        //! Starts the drive to drive to the given Target with a custom time - acceleration (or Speed on no Acceleration) will be calculated to reach time
        public void DriveTo(float Target, float time)
        {
      
            BlockDestination = false;
            StandardSpeed = TargetSpeed;
            StandardAcceleration = Acceleration;
            var deltapos = Math.Abs(CurrentPosition - Target);
            if (deltapos < 0.001f)
            {
                Acceleration = StandardAcceleration;
                TargetSpeed = StandardSpeed;
            }
            else if(!SmoothAcceleration)
            {
                float newacc = 0;
                float newspeed = 0;

                if (time == 0)
                {
                    Acceleration = StandardAcceleration;
                    TargetSpeed = StandardSpeed;
                }
                else
                {
                    // Calc Acceleration
                    if (UseAcceleration)
                    {
                        Acceleration = deltapos / Mathf.Pow(time*0.5f,2);
                    }
                    else
                    {
                        newspeed = deltapos / time;
                        TargetSpeed = newspeed;
                    }
                }
            }
            
            TargetPosition = Target;
            _currentdestination = TargetPosition;
            TargetStartMove = true;
            IsAtTarget = false;
            _drivetostarted = true;
            _lastisattarget = false;
            _targettime = time;
        }
        
        
        //! Calculates the time to the target
        public float GetTimeTo(float Target)
        {
#if REALVIRTUAL_PROFESSIONAL
            if (SmoothAcceleration)
            {
                InitSmoothDriveTo(Target);
                return smoothMotion.GetDuration();
            }
#else
            if (SmoothAcceleration)
            {
                Debug.LogError("Smooth Acceleration is only available in the Professional Version. Please check https://realvirtual.io/ for more information");
            }
#endif            
            
            double totaltime = 0.0f;
            var currpos = CurrentPosition;
            var tarpos = Target;
            var deltapos = Math.Abs(tarpos - currpos);
            var currspeed = CurrentSpeed;
            var tarspeed = TargetSpeed;
            // time for acceleration to target speed
            double acctime = 0;
            if (UseAcceleration && Acceleration != 0)
                acctime = (TargetSpeed *SpeedOverride / Acceleration);  
            
            // distance during acceleration
            double distacc = 0.5 * (double)Acceleration * Math.Pow(acctime,2);
            
            // time for deceleration to 0
            double dectime = 0;
            if (UseAcceleration && Acceleration != 0)
                dectime = (TargetSpeed*SpeedOverride) / (Acceleration);
            
            // distance during decceleration
            double distdecc = 0.5 * Acceleration * Math.Pow(dectime,2);

            var fullacceleration = true;
            // Is full acceleration needed
            if (distacc + distacc > deltapos)
            {
                fullacceleration = false;
            }

            if (fullacceleration)
            { 
                totaltime = acctime + dectime;
                var distconst = deltapos - (distacc + distdecc);
                totaltime += distconst / (tarspeed*SpeedOverride);
            }
            else
            {
                // No full acceleration so how much can we accelerate and decellerate
                totaltime = 2 * Math.Sqrt((2 * deltapos / 2) / (Acceleration));
            }
            return (float)totaltime;
        }
        

        //! Starts the drive - it will speed up with sinoide if turned on

        public void Accelerate()
        {
            _accelerationstarted = true;
            IsAtTarget = false;
            _decelerationstarted = false;
            _timestartacceleration = Time.time;
            _StopDrive = false;
            IsStopped = false;
        }


        public void Decelerate()
        {
            _decelerationstarted = true;
            _accelerationstarted = false;
            _timestartacceleration = Time.time;
            IsAtTarget = false;
            _StopDrive = false;
            IsStopped = false;
        }

        //! Stops the drive at the current position
        public void Stop()
        {
            TargetStartMove = false;
            _decelerationstarted = false;
            _accelerationstarted = false;
            _currentacceleration = 0;
            IsRunning = false;
            JogForward = false;
            JogBackward = false;
            CurrentSpeed = 0;
            _StopDrive = false;
            IsStopped = true;
        }

        //! Gets the axis vector of the drive
        public Vector3 GetLocalDirection()
        {
            return _localdirection;
        }

        public void OnDestroy()
        {
            if (EditorMoveMode && !Application.isPlaying)
                EndEditorMoveMode();
        }

        public void StartEditorMoveMode()
        {
            if (EditorMoveMode)
                return;
            CalculateVectors();
            EditorMoveMode = true;
#if UNITY_EDITOR
            Global.SetLockObject(this.gameObject, true);
#endif
        }

        public void SetPositionEditorMoveMode(float editorposition)
        {
            if (EditorMoveMode)
            {
                if (realvirtualController == null)
                    realvirtualController = UnityEngine.Object.FindAnyObjectByType<realvirtualController>();
                CurrentPosition = editorposition;
                SetPosition();
            }
        }
        public void EndEditorMoveMode()
        {
#if UNITY_EDITOR
            Global.SetLockObject(this.gameObject, false);
#endif
            if (realvirtualController == null)
                realvirtualController = UnityEngine.Object.FindAnyObjectByType<realvirtualController>();
            CurrentPosition = 0;
            SetPosition();
            EditorMoveMode = false;
        }


        //! Gets the start position of the drive in local scale
        public Vector3 GetStartPos()
        {
            return _localstartpos;
        }

        //! Gets the start position of the drive in local scale
        public Vector3 GetStartRot()
        {
            return _localstartrot;
        }

      
        public void SetPositionAndSpeed(float Position, float Speed)
        {
            if (SpeedOverwrite)
            {
                SpeedOverwriteValue = Speed;
            }
            else
            {
                CurrentSpeed = Speed;
            }
            IsSpeed = Speed;
            SetPosition();
        }
        
        //! Gets the current speed of the drive and updates the visual position
        public void SetPosition(float Position)
        {
            if (PositionOverwrite)
            {
                PositionOverwriteValue = Position;
            }
            else
            {
                CurrentPosition = Position;
            }
            SetPosition();
        }

        public Vector3 GetGlobalDirection()
        {
            return _globaldirection;
        }


        #endregion

        #region PrivateMethods

        public void CalculateVectors()
        {
            if (useagx)
            {
                return;
            }
            
            _localdirection = DirectionToVector(Direction);
            _globaldirection = transform.TransformDirection(_localdirection);
            if (!ReferenceEquals(transform.parent, null))
            {
                _positiondirection = transform.parent.transform.InverseTransformDirection(_globaldirection);
            }
            else
            {
                _positiondirection = _globaldirection;
            }

            if (transform.parent != null)
                _localscale = GetLocalScale(transform.parent.transform, Direction);
            else
                _localscale = 1;

            _localstartpos = transform.localPosition;
            _localstartrot = transform.localEulerAngles;
            _localstartquat = transform.localRotation;
            if (ReverseDirection)
            {
                _globaldirection = -_globaldirection;
                _localdirection = -_localdirection;
                _positiondirection = -_positiondirection;
            }

            IsRotation = false;
            if (Direction == DIRECTION.RotationX || Direction ==
                DIRECTION.RotationY || Direction == DIRECTION.RotationZ)
            {
                IsRotation = true;
            }
            if (realvirtualController == null)
                realvirtualController = UnityEngine.Object.FindAnyObjectByType<realvirtualController>();
            _controllerscale = realvirtualController.Scale;
       

#if REALVIRTUAL_INTERACT
            if (UseInteract && !Application.isPlaying)
                realvirtualPhysics.Kinematize(gameObject);
#endif
        }

        
      
        private void SetPosition()
        {
            float nextPosition;
            if (PositionOverwrite)
            {
                nextPosition= PositionOverwriteValue;
            }
            else
            {
                nextPosition =CurrentPosition;
            }
            IsPosition = nextPosition;
            if (Direction == DIRECTION.Virtual)
            {
 #if REALVIRTUAL_AGX
                if (useagx)
                {
                    var dir = 1;
                    if (ReverseDirection)
                        dir = -1;
                    if (IsRotation)
                        agxlockcontroller.Position = dir*Mathf.Deg2Rad * (nextPosition+Offset);
                    else
                        agxlockcontroller.Position = dir*nextPosition+Offset;

                    return;
                }
#endif

                float scale = 1;
                if (articulatedbodynotnull)
                {
                    if (articulatedbody.jointType != ArticulationJointType.RevoluteJoint)
                        scale = 1/realvirtualController.Scale;
                    ArticulationDrive currentDrive = articulatedbody.xDrive;
                    if (nextPosition > currentDrive.upperLimit)
                    {
                        currentDrive.target = currentDrive.upperLimit;
                    }
                    else if (nextPosition < currentDrive.lowerLimit)
                    {
                        currentDrive.target = currentDrive.lowerLimit;
                    }
                    else
                    {
                        currentDrive.target = nextPosition*scale;
                    }
                    articulatedbody.xDrive = currentDrive;
                }
                return;
            }
            
            if (!UseInteract)
            {
                if (!_istransportsurface)
                {
                    if (!IsRotation)
                    {
                        
                        Vector3 localpos = _localstartpos +
                                           _positiondirection *
                                           ((nextPosition + Offset) / _controllerscale)  / 
                                           _localscale * initialScale;

                        if (MoveThisRigidBody)
                        {
                            if (!ReferenceEquals(transform.parent, null))
                                _globalpos = transform.parent.TransformPoint(localpos);
                            else
                                _globalpos = localpos;
                            _rigidbody.MovePosition(_globalpos);
                        }
                        else
                        {
                            transform.localPosition = localpos;
                        }
                    }
                    else
                    {
                        Quaternion localrot =
                            _localstartquat * Quaternion.Euler(_localdirection * (nextPosition + Offset));
                        if (MoveThisRigidBody)
                        {
                            if (!ReferenceEquals(transform.parent, null))
                            {
                                _globalrot = transform.parent.rotation * localrot;
                                _globalpos = transform.parent.TransformPoint(_localstartpos);
                                _rigidbody.MovePosition(_globalpos);
                            }
                            else
                            {
                                _globalrot = localrot;
                            }

                            _rigidbody.MoveRotation(_globalrot);
                        }
                        else
                            transform.localRotation = localrot;
                    }
                }
            }
            else
            {
#if REALVIRTUAL_INTERACT
                   realvirtualPhysics.SetPosition(this,nextPosition);
#endif
            }
        }


#if REALVIRTUAL_INTERACT

        public override void AwakeAlsoDeactivated()
        {
            realvirtualPhysics.EnableDrive(this,this.enabled);
        }
        
#endif
        
        private new void Awake()
        {
            IsAtTarget = true;
            BlockDestination = true;
            _limitraycastnotnull = LimitRayCast != null;
            eventbeforedrivecalculation = OnBeforeDriveCalculation != null;
            eventafterdrivecalculation = OnAfterDriveCalculation != null;
            base.Awake();
        }

        // When Script is added or reset ist pushed
        private void Reset()
        {
#if REALVIRTUAL_INTERACT
            realvirtualPhysics.InitDrive(this);
#endif
#if REALVIRTUAL_AGX
            var agxconstraint = GetComponent<Constraint>();
            useagx = agxconstraint != null;
            if (useagx)
                Direction = DIRECTION.Virtual;
#endif
            if (!UseInteract && !useagx)
            {
                /// Automatically create RigidBody if not there
                _rigidbody = gameObject.GetComponent<Rigidbody>();
                if (_rigidbody == null)
                {
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
            
        }
        // Is called when RuntimeEditor is starting the simulation - Start needs to be called to init again all variables because maybe direction of the drive has changed because object has been rotated 
        protected override void OnStartSim()
        {
        
        }
        
        private void OnValidate()
        {
            // Update IsRotation property based on Direction
            IsRotation = (Direction == DIRECTION.RotationX || 
                         Direction == DIRECTION.RotationY || 
                         Direction == DIRECTION.RotationZ);
            
            // Notify TransportSurfaces when direction changes
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Use existing method to get transport surfaces
                var surfaces = GetTransportSurfaces();
                foreach (var surface in surfaces)
                {
                    if (surface != null)
                    {
                        // Trigger radial update through property setter
                        surface.Drive = this;
                    }
                }
            }
            #endif
        }
        
        #if UNITY_EDITOR
        // Called when a TransportSurface is added
        public void OnTransportSurfaceAdded()
        {
            if (!Application.isPlaying)
            {
                // Update gizmos or any other visual elements
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        
        // Called when a TransportSurface is removed
        public void OnTransportSurfaceRemoved()
        {
            if (!Application.isPlaying)
            {
                // Update gizmos or any other visual elements
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        #endif
        
        // Simulation Scripts - Start, Update ....
        private void Start()
        {
            // Use Articulated bodies
            articulatedbody = GetComponent<ArticulationBody>();
            if (articulatedbody != null)
                articulatedbodynotnull = true;
            
            if (EditorMoveMode)
            {
                EndEditorMoveMode();
            }

            _rigidbody = gameObject.GetComponent<Rigidbody>();
            
            if (UseInteract)
            {
#if REALVIRTUAL_INTERACT
                jointmonitor = GetComponent<XdeUnitJointMonitor>();
                jointcontroller = GetComponent<XdeUnitJointPDController>();
#endif
#if !REALVIRTUAL_INTERACT
                Error("INTERACT is not installed or not enabled - please check Game4Automation main menu and enable INTERACT");
#endif
            }

#if REALVIRTUAL_AGX
            var agxconstraint = GetComponent<Constraint>();
            useagx = agxconstraint != null;
            if (useagx)
            {
                agxlockcontroller = agxconstraint.GetController<LockController>();
                Direction = DIRECTION.Virtual;
                IsRotation = agxconstraint.Type == ConstraintType.Hinge;
                articulatedbodynotnull = false;
            }
#endif
            
            CalculateVectors();
          

            CurrentPosition = StartPosition;
            
            // Init DriveBehaviours
            _drivebehaviours = GetComponents<IDriveBehavior>();
            if (_drivebehaviours == null)
               _drivebehaviours = Array.Empty<IDriveBehavior>();
            
            if (_subdrives == null)
                _subdrives = Array.Empty<Drive>();
            
            SetSmoothMotionParameters();
     
            if (GetTransportSurfaces().Count>0)
                _istransportsurface = true;
            AfterDriveStartInit?.Invoke(this);
            
        }

        private void SetSmoothMotionParameters()
        {
            #if REALVIRTUAL_PROFESSIONAL
            smoothMotion.SetInitialPosition(CurrentPosition);
            smoothMotion.maxVelocity = TargetSpeed;
            smoothMotion.maxAcceleration = Acceleration;
            smoothMotion.jerk = Jerk;
            #endif
        }
        
        public void DriveReset()
        {
            CurrentPosition = Offset;
            CurrentSpeed = 0;
            IsRunning = false;
            BlockDestination = true;
        }
        
        
        private void CalcFixedUpdate()
        {
            if (ForceStop)
                return;

            // now call the unity event
            if (eventbeforedrivecalculation)
                OnBeforeDriveCalculation.Invoke(this);

            
            // First calculate the drive behaviors and then this drive
            foreach (var drivebehaviour in _drivebehaviours)
            {
                if (drivebehaviour is MonoBehaviour mb && mb.isActiveAndEnabled)
                {
                    drivebehaviour.CalcFixedUpdate();
                }
            }
            
            var currentposition = CurrentPosition;
            
            if (ResetDrive)
                DriveReset();

            if (_StopDrive)
                Stop();

            // Jog stopped
            if (_lastjog && !JogBackward && !JogForward && !UseAcceleration)
            {
                Stop();
            }

            if (_lastjog && !JogBackward && !JogForward && UseAcceleration)
            {
                _stopjogging = true;
                if (CurrentSpeed > 0)
                    _currentacceleration = -Acceleration;
                else
                    _currentacceleration = Acceleration;
            }

            // Drive Decellerated totally - stop drive
            if (_decelerationstarted && CurrentSpeed < 0)
            {
                Stop();
            }


            var newtarget = false;

            // New Target Position
            if (_laststartdrivetotarget != TargetStartMove && TargetStartMove)
            {
                IsStopped = false;
                _stopjogging = false;
                BlockDestination = false;
                _currentdestination = TargetPosition;
                _currentacceleration = Acceleration;
                _isdrivingtotarget = true;
                _timestartacceleration = Time.time;
                IsAtTarget = false;
                _StopDrive = false;
                if (_drivetostarted)
                {
                    TargetStartMove = false;
                    _drivetostarted = false;
                }

                if (TargetPosition == currentposition) // Already at target position
                {
                    IsStopped = true;
                    BlockDestination = true;
                    currentposition = _currentdestination;
                    IsAtTarget = true;
                    _isdrivingtotarget = false;
                    _stopjogging = false;
                }
                else
                {
                    if (SmoothAcceleration)
                    {
                        if(_targettime == 0){
                            InitSmoothDriveTo(_currentdestination);
                        }
                        else
                        {
                            InitSmoothDriveTo(_currentdestination, _targettime);
                            _targettime = 0;
                        }
                    }
                }
                newtarget = true;
            }

            
        
            // Calculate Position if Speed > 0 
            if (!IsStopped && !IsSubDrive && !SmoothAcceleration)  // adding !issubdrive for avoiding calculation if it is a gear, cam or comparable slave drive
                if (!ResetDrive && (CurrentSpeed != 0) && !_StopDrive)
                {
                    currentposition = currentposition +
                                      CurrentSpeed * realvirtualController.SpeedOverride * Time.fixedDeltaTime;
                }
            
            // Need to slow down - negative acceleration
            if (_isdrivingtotarget && !_StopDrive && !ResetDrive && !JogBackward && !JogForward && !SmoothAcceleration)
            {
                if (UseAcceleration)
                {
                    if (CurrentSpeed > 0)
                    {
                        _currentstoppos = currentposition + ((CurrentSpeed * CurrentSpeed) / (2 * Acceleration));
                    }
                    else
                    {
                        _currentstoppos = currentposition - ((CurrentSpeed * CurrentSpeed) / (2 * Acceleration));
                    }
                }
                else
                {
                    _currentstoppos = currentposition;
                }
            }

            if (JogBackward || JogForward)
                IsStopped = false;

            // Calculate Acceleration
            if (!IsStopped)
                if ((_accelerationstarted || _decelerationstarted) ||
                    ((!IsAtTarget && _isdrivingtotarget) && !_StopDrive && !ResetDrive &&
                     UseAcceleration && !JogBackward && !JogForward &&
                     !_stopjogging))
                {
                    if (SmoothAcceleration == false)
                    {
                        if (!_accelerationstarted && !_decelerationstarted)
                        {
                            if (_currentdestination > _currentstoppos)
                            {
                                _currentacceleration = Acceleration;
                            }
                            else
                            {
                                _currentacceleration = -Acceleration;
                            }
                        }
                        else
                        {
                            if (_accelerationstarted)
                                _currentacceleration = Acceleration;
                            if (_decelerationstarted)
                                _currentacceleration = -Acceleration;
                        }
                    }
                    
                }
            
            

            // Calculate Acceleration if Jogging
            if (!IsStopped)
                if ((JogBackward || JogForward) && UseAcceleration)
                {
                    _stopjogging = false;
                    if (JogForward)
                    {
                        if (CurrentSpeed < TargetSpeed*SpeedOverride)
                            _currentacceleration = Acceleration;
                        if (CurrentSpeed > TargetSpeed*SpeedOverride)
                            _currentacceleration = -Acceleration;
                    }
                    else
                    {
                        if (CurrentSpeed < TargetSpeed*SpeedOverride)
                            _currentacceleration = -Acceleration;
                        if (CurrentSpeed > TargetSpeed*SpeedOverride)
                            _currentacceleration = Acceleration;
                    }
                }

            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && !newtarget)
                {
                    if ((_isdrivingtotarget && CurrentSpeed > 0 && currentposition >= _currentdestination &&
                         _lastcurrentposition < _currentdestination) ||
                        (_isdrivingtotarget && CurrentSpeed < 0 && currentposition <= _currentdestination &&
                         _lastcurrentposition > _currentdestination))
                    {
                        Stop();
                        BlockDestination = true;
                        currentposition = _currentdestination;
                        IsAtTarget = true;
                        _isdrivingtotarget = false;
                        _stopjogging = false;
                    }
                }
            
            
            
            // Calculate Speed
            if (!IsStopped)
                if (!ResetDrive && !_StopDrive && (!IsAtTarget || JogBackward || JogForward))
                {
                    if (!UseAcceleration)
                    {
                        if (!JogForward && !JogBackward && !BlockDestination)
                        {
                            if (currentposition < _currentdestination)
                                CurrentSpeed = TargetSpeed*SpeedOverride;
                            if (currentposition > _currentdestination)
                                CurrentSpeed = -TargetSpeed*SpeedOverride;
                        }
                        else
                        {
                            if (JogForward)
                                CurrentSpeed = TargetSpeed*SpeedOverride;
                            if (JogBackward)
                                CurrentSpeed = -TargetSpeed*SpeedOverride;
                        }
                    }
                    else if(!SmoothAcceleration)
                    {
                        CurrentSpeed = CurrentSpeed + (float) _currentacceleration * Time.fixedDeltaTime;
                        // Limit Speed to maximum
                        if (CurrentSpeed > 0 && CurrentSpeed > TargetSpeed*SpeedOverride && _currentacceleration > 0)
                        {
                            _accelerationstarted = false;
                            _currentacceleration = 0;
                            CurrentSpeed = TargetSpeed*SpeedOverride;
                        }

                        if (CurrentSpeed < 0 && CurrentSpeed < -TargetSpeed*SpeedOverride && _currentacceleration < 0)
                        {
                            _decelerationstarted = false;
                            _currentacceleration = 0;
                            CurrentSpeed = -TargetSpeed*SpeedOverride;
                        }
                    }
                }
            
            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && _stopjogging)
                {
                    if ((CurrentSpeed > 0 && _lastspeed < 0) || (CurrentSpeed < 0 && _lastspeed > 0))
                    {
                        Stop();
                        _currentacceleration = 0;
                        _stopjogging = false;
                        IsAtTarget = false;
                        _currentdestination = currentposition;
                    }
                }
            
            if (SmoothAcceleration && !JogForward && !JogBackward && !IsStopped)
            {
                currentposition = SmoothPositionUpdate();
            }

         
            if (UseLimits)
            {
                IsAtLowerLimit = false;
                IsAtUpperLimit = false;
                var currpos = currentposition;
                if (_limitraycastnotnull)
                    currpos = LimitRayCast.RayCastDistance;
                if (JogForward && currpos >= UpperLimit)
                {
                    if (!JumpToLowerLimitOnUpperLimit)
                    {
                        CurrentSpeed = 0;
                        currentposition = UpperLimit;
                        IsAtUpperLimit = true;
                    }
                    else
                    {
                        currentposition = currpos - UpperLimit;
                        if (OnJumpToLowerLimit != null)
                            OnJumpToLowerLimit.Invoke(this);
                    }
                }

                if (JogBackward && currpos <= LowerLimit)
                {
                    CurrentSpeed = 0;
                    currentposition = LowerLimit;
                    IsAtLowerLimit = true;
                }

                if (!JogForward && !JogBackward)
                {
                    if (!_limitraycastnotnull)
                    {
                        // Normal Limits
                        if (currpos > UpperLimit)
                            currentposition = UpperLimit;
                        if (currpos < LowerLimit)
                            currentposition = LowerLimit;
                    }
                    else
                    {
                        //  With Raycast
                        var diff = 0.0f;
                        if (currpos > UpperLimit)
                            diff = UpperLimit - currpos;
                        if (currpos < LowerLimit)
                            diff = LowerLimit - currpos;
                        currentposition = currentposition - diff;
                    }
                }
            }
            
           
            
            //  Current Values / Status
            if (CurrentSpeed == 0)
            {
                IsRunning = false;
            }
            else
            {
                IsRunning = true;
            }

            if (CurrentSpeed == TargetSpeed*SpeedOverride && TargetSpeed != 0)
                IsAtTargetSpeed = true;
            else
                IsAtTargetSpeed = false;


            bool isonposition = false;
            if (currentposition == _currentdestination)
            {
                IsAtTarget = true;
                if (_lastisattarget != IsAtTarget && OnAtPosition != null)
                {
                    if (StandardSpeed!=0)
                       TargetSpeed = StandardSpeed;
                    if (StandardAcceleration != 0)
                        Acceleration = StandardAcceleration;
                    isonposition = true;
                }
            }
            else
            {
                IsAtTarget = false;
            }
            
            
            _laststartdrivetotarget = TargetStartMove;
            _lastspeed = CurrentSpeed;
            _lastisattarget = IsAtTarget;
          
            _lastjog = JogBackward || JogForward;
            
            if (isonposition)
                OnAtPosition(this);
            
            // Set new Position
            if (PositionOverwrite)
                currentposition = PositionOverwriteValue;
            
            
            // Set IsSpeed - use CurrentSpeed as it's already properly calculated
            // Note: IsSpeed is used by other components (ConveyorBelt, GuidedMU) for physics calculations
            // so we keep it consistent with CurrentSpeed to avoid jitter
            if (SpeedOverwrite)
            {
                IsSpeed = SpeedOverwriteValue;
            }
            else if (PositionOverwrite && Time.fixedDeltaTime > 0 && init)
            {
                // When position is overridden, calculate speed from position delta with smoothing
                float deltaPosition = currentposition - _lastcurrentposition;
                float instantSpeed = deltaPosition / Time.fixedDeltaTime;
                
                // Apply exponential smoothing (low-pass filter) to reduce flickering
                _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, instantSpeed, SPEED_SMOOTHING_FACTOR);
                
                // Apply deadzone to ignore very small speed changes (noise)
                if (Mathf.Abs(_smoothedSpeed) < SPEED_DEADZONE)
                {
                    _smoothedSpeed = 0f;
                }
                
                IsSpeed = _smoothedSpeed;
                CurrentSpeed = IsSpeed; // Update CurrentSpeed to match smoothed movement
            }
            else
            {
                IsSpeed = CurrentSpeed;
            }

            
            
            if ((!float.IsNaN(currentposition) && _lastcurrentposition != currentposition ) || !init)
            { 
                SetPosition(currentposition); 
                init = true;
            }
            

            

            
            _lastcurrentposition = currentposition;
            
            // now call the Unity event after drive calc
            if (eventafterdrivecalculation)
              OnAfterDriveCalculation.Invoke(this);
            
            // now calculate the subdrives
            foreach (var subdrive in _subdrives)
            {
                subdrive.CalcFixedUpdate();
            }
            
            
            
        }

        //! Cleans up references when Unity Editor exits edit mode.
        //! IMPLEMENTS IEditModeFinished::OnEditModeFinished
        public void OnEditModeFinished()
        {
            realvirtualController = null;
        }

        private void FixedUpdate()
        {
            // if it is a subdrive it is calculated by the parent drives in the needed calculation order
            if (IsSubDrive)
                return;
            
            CalcFixedUpdate();
        }

        #endregion
    }
}