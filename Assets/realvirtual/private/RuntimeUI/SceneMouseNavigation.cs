// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    


using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN ) && !UNITY_WEBGL
using SpaceNavigatorDriver;
#endif
#if CINEMACHINE
using Cinemachine;
#endif


namespace realvirtual
{
    //! Controls the Mouse and Touch navigation in Game mode
    public class SceneMouseNavigation : realvirtualBehavior
    {
        //! delegate for starting and stopping camera interpolation
        public delegate void StartStopCameraInterpolation(bool start);

        //! delegate for starting and stopping panning
        public delegate void StartStopPanning(bool start);

        /// Delegates for Events
        //! delegate for starting and stopping rotation
        public delegate void StartStopRotation(bool start);

        [Tooltip("Toggle the orbit camera mode")]
        public bool UseOrbitCameraMode; //!< Toggle the orbit camera mode 

        public bool FirstPersonControllerActive = true; //!< Toggle the first person controller 

        [Tooltip("Rotate the camera with the left mouse button")]
        public bool RotateWithLeftMouseButton; //!< Rotate the camera with the left mouse button 

        [Tooltip("Rotates the camera to focused objects instead of panning the camera ")]
        public bool RotateToFocusObject; //!< Rotates the camera to focused objects instead of panning the camera 

        [Tooltip("Block rotation on selected objects")] [HideInInspector]
        public bool BlockRotationOnSelected;

        [Tooltip("Reference to the first person controller script")]
        public FirstPersonController FirstPersonController; //!< Reference to the first person controller script 

        [Tooltip("The last camera position before switching modes")] 
        public CameraPos LastCameraPosition; //!< The last camera position before switching modes

        [Tooltip("Set the camera position on start play")]
        public bool SetCameraPosOnStartPlay = true; //!< Set the camera position on start play

        [Tooltip("Save the camera position on quitting the application")]
        public bool SaveCameraPosOnQuit = true; //!< Save the camera position on quitting the application 

        [Tooltip("Set the editor camera position")]
        public bool SetEditorCameraPos = true; //!< Set the editor camera position

        [Tooltip("Interpolate to new saved Camerapositions")]
        public bool InterpolateToNewCamerapoitions = true; //!< Interpolate to new saved Camerapositions

        [Tooltip("Interpolate to new saved Camerapositions with this speed")]
        public float CameraInterpolationSpeed = 0.1f; //!< Interpolation speed to new saved Camerapositions

        [HideInInspector] [Tooltip("The target of the camera")]
        public Transform target; //!< The target of the camera

        [Tooltip("Offset of the camera's target")] [HideInInspector]
        public Vector3 targetOffset; //!< Offset of the camera's target 

        [Tooltip("The distance of the camera from its target")] [HideInInspector]
        public float distance = 5.0f; //!< The distance of the camera from its target

        [Tooltip(
            "The DPI scale of the screen, is automatically calculated and is used to scale all screen pixel related distances measurements")]
        [ReadOnly]
        public float
            DPIScale = 1; //!< The DPI scale of the screen, is automatically calculated and is used to scale all screen pixel related distances measurements

        [Tooltip("The minimum distance of the camera from its target")] [HideInInspector]
        public float minDistance = 1f; //!< The minimum distance of the camera from its target

        [Tooltip("The speed of mouse rotation, 1 is standard value")]
        public float MouseRotationSpeed = 1f; //!< The speed of rotation around the y-axis

        [Header("Master Controls")]
        [Tooltip("Master sensitivity multiplier for all mouse/touch navigation (0.1 to 3.0)")]
        [Range(0.1f, 3f)]
        [RuntimePersistenceRange(0.1f, 3.0f)]
        [RuntimePersistenceLabel("Mouse Sensitivity")]
        [RuntimePersistenceHint("(0.1-3.0)")]
        [RuntimePersistenceFormat("F2")]
        public float MasterSensitivity = 1f; //!< Master sensitivity multiplier affecting all navigation speeds

        [Tooltip("The minimum angle limit for the camera rotation around the horizontal axis ")]
        public float MinHorizontalRotation; //!< The minimum angle limit for the camera rotation around the x-axis 

        [Tooltip("The maximum angle limit for the camera rotation around the horizontal axis")]
        public float MaxHorizontalRotation = 100; //!< The maximum angle limit for the camera rotation around the x-axis

        [Tooltip("The speed of zooming in and out, 1 is standard")]
        public float ZoomSpeed = 1; //!< The speed of zooming in and out, 1 is standard 

        [Tooltip("The speed at which the zooming slows down, 1 is standard")]
        public float RotDamping = 1f; //!< The speed at which the zooming slows down, 1 is standard

        [Tooltip("The speed at which the panning slows down, 1 is standard")]
        public float PanDamping = 1; //!< The speed at which the panning slows down, 1 is standard
        
        [Tooltip("The speed at which movement with the cursor should be done, 1 is standard")]
        public float CursorSpeed = 1; //!< The speed at which the panning slows down, 1 is standard

        [Tooltip("The speed at which the zooming slows down, 1 is standard")]
        public float ZoomDamping = 1f; //!< The speed at which the zooming slows down, 1 is standard

        [Tooltip("The speed of panning the camera in orthographic mode")]
        public float orthoPanSpeed = 1; //!< The speed of panning the camera in orthographic mode

        [Tooltip("The time to wait before starting the demo due to inactivity")]
        public float StartDemoOnInactivity = 5.0f; //!< The time to wait before starting the demo due to inactivity 

        [Tooltip("The time without any mouse activity before considering the camera inactive")]
        public float
            DurationNoMouseActivity; //!< The time without any mouse activity before considering the camera inactive

        [Tooltip("A game object used for debugging purposes")]
        public GameObject DebugObj;

        [Header("Touch Controls")] [Tooltip("The touch interaction script")]
        public TouchInteraction Touch; //!< The touch interaction script 

        [Tooltip("The speed of pan speed with touch")]
        public float TouchPanSpeed = 1f; //!< The speed of rotating with touch

        [Tooltip("The speed of rotating with touch")]
        public float TouchRotationSpeed = 1f; //!< The speed of rotating with touch

        [Tooltip("The speed of zooming with touch")]
        public float TouchZoomSpeed = 1f; //!< The speed of zooming with touch

        [Tooltip("Invert vertical touch axis")]
        public bool TouchInvertVertical; //! Touch invert vertical

        [Tooltip("Invert horizohntal touch axis")]
        public bool TouchInvertHorizontal; //! Touch invert horizontal

        [Header("SpaceNavigator")] public bool EnableSpaceNavigator = true; //! Enable space navigator
        public float SpaceNavTransSpeed = 1; //! Space navigator translation speed


        [Header("Status")] [NaughtyAttributes.ReadOnly]
        public float currentDistance; //! Current distance

        [ReadOnly] public float desiredDistance; //! Desired distance
        [ReadOnly] public Quaternion currentRotation; //! Current rotation
        [ReadOnly] public Quaternion desiredRotation; //! Desired rotation
        [ReadOnly] public bool isRotating;
        [ReadOnly] public bool isPanning;
        [ReadOnly] public bool interpolatingToNewCameraPos;
        [ReadOnly] public bool CinemachineIsActive;
        [ReadOnly] public bool blockrotation;
        [ReadOnly] public bool blockleftmouserotation;
        [HideInInspector] public bool orthograhicview;
        [HideInInspector] public OrthoViewController orthoviewcontroller;
        private bool _demostarted;
        private float _lastmovement;
        private Vector3 _pos;

        private bool blockrotationbefore;
        private Vector3 calculatedboundscenter;
        public StartStopCameraInterpolation EventStartStopCameraInterpolation;
        public StartStopPanning EventStartStopPanning;
        public StartStopRotation EventStartStopRotation;
#if REALVIRTUAL_PROFESSIONAL
        private HMI_Controller hmiController;
#endif
        private Camera incamera;
        private float interpolatedistance;
        private Vector3 interpolatenewcampos;
        private Quaternion interpolatenewcamrot;
        private Vector3 interpolaterotation;

        private Vector3 interpolatetargetpos;
        private float lastperspectivedistance;
        
        private float maxdistance;
        private Camera mycamera;
        private Vector3 position;
        private Vector3 raycastto0plane;
        private Vector3 raycastto0planebefore;
        
        private Quaternion rotation;
        private GameObject selectedbefore;
        private SelectionRaycast selectionmanager;
        private bool selectionmanagernotnull;
        private bool startcameraposset;
        private float startpanhitdistance;
        private Vector3 targetposition;
        private bool touch;

        private bool touchnotnull;

       
        private bool verticalplanedetection;
        private float xDeg;
        private float yDeg;
        private float zoomhitdistance;
        private Vector3 zoomposviewport = Vector3.zero;
        private Vector3 zoomposworld = Vector3.zero;

        //checking focus on UI input field
        public TMP_InputField[] tmpInputFields;
        private VisualElement root;
        private FocusController focusController;

        // IsRotating Getter Setter
        private bool IsRotating
        {
            get => isRotating;
            set
            {
                if (isRotating != value)
                {
                    var oldValue = isRotating;
                    isRotating = value;
                    // Step 4: Call the delegate when the value changes.
                    EventStartStopRotation?.Invoke(isRotating);
                }
            }
        }

        // IsPanning Getter Setter
        private bool IsPanning
        {
            get => isPanning;
            set
            {
                if (isPanning != value)
                {
                    var oldValue = isPanning;
                    isPanning = value;
                    // Step 4: Call the delegate when the value changes.
                    EventStartStopPanning?.Invoke(isPanning);
                }
            }
        }

        private void Start()
        {
            selectionmanagernotnull = GetComponent<SelectionRaycast>() != null;
            if (selectionmanagernotnull)
            {
                selectionmanager = GetComponent<SelectionRaycast>();
                selectionmanager.EventSelected.AddListener(OnSelected);
                selectionmanager.EventBlockRotation.AddListener(BlockRotation);
                selectionmanager.EventMultiSelect.AddListener(OnMultiSelect);
            }

            if (LastCameraPosition != null)
                if (SetCameraPosOnStartPlay)
                    LastCameraPosition.SetCameraPositionPlaymode(this);

            // force to rotate with left mouse button if in webgl
#if UNITY_WEBGL
            RotateWithLeftMouseButton = true;
#endif
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument)
            {
                root = uiDocument.rootVisualElement;
                focusController = root.focusController;
            }
        }
        // check focused fields
        private bool CheckForFocusedFields()
        {
            bool focusActive = false;
            tmpInputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
            foreach (var inputField in tmpInputFields)
            {
                if (inputField.isFocused)
                {
                    focusActive = true;
                }
            }
            if (focusController != null && focusController.focusedElement is TextField textField)
            {
                focusActive = true;
            }
            return focusActive;
        }

        private void LateUpdate()
        {
#if CINEMACHINE && REALVIRTUAL_PROFESSIONAL
            if (hmiController != null && hmiController.BlockUserMouseNavigation)
                return;
#endif


            // if currently interpolating to new camera position, do not allow any other camera movement
            if (interpolatingToNewCameraPos)
            {
                InterpolateToNewCameraPos();
                return;
            }

            // if it is over a UI object nothing else exit
            if (EventSystem.current.IsPointerOverGameObject() || CheckForFocusedFields())
                return;
            
          
            // Check Touch Status
            var istouching = false;
            var touchpanning = false;
            var starttouch = false;
            var touchrotate = false;
            var endtouch = false;
            var iszooming = false;
            var istwofinger = false;

            if (touchnotnull)
            {
                istouching = Touch.IsTouching;
                touchpanning = Touch.IsTwoFinger;
                starttouch = Touch.IsBeginPhase;
                istwofinger = Touch.IsTwoFinger;
                endtouch = Touch.IsEndPhase;
                iszooming = Touch.TwoFingerDeltaDistance != 0;
                // if touch is over UI do nothing
                if (Touch.IsOverUI)
                    return;

                if (istouching && !touchpanning)
                    touchrotate = true;
            }

            // check for shift and set status for later use
            var shift = false;
            float shiftfactor = 1;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                shift = true;
                shiftfactor = 0.5f;
            }


            // sets the button for rotation (in web it must be left mouse button)
            var buttonrotate = 1;
            if (RotateWithLeftMouseButton)
                buttonrotate = 0;

            // check if camera is in overlay orthograhic view - for navigation in view and not in main camera
            var MouseInOrthoCamera = CheckMouseOverOrthoView();

            // leaeve if first person controller is active
            if (FirstPersonControllerActive)
                return;


            if (UseOrbitCameraMode)
                return;

            // if cinemachiune is active and there is any mouse activity, deactivate cinemachine
            if (CinemachineIsActive)
            {
                var scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Input.GetMouseButton(2) || Input.GetMouseButton(3) || Input.GetMouseButton(1)
                    || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl) ||
                    Input.GetKey(KeyCode.RightControl)
                    || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) ||
                    Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) || Math.Abs(scroll) > 0.001f ||
                    Input.GetKey(KeyCode.Escape))
                    ActivateCinemachine(false);
            }

            // is panning starting? Set init position when mouse bottom is going down
            if (Input.GetMouseButtonDown(2) || starttouch)
            {
                startpanhitdistance = GetClosestHitDistance();
                if (startpanhitdistance == 0)
                    startpanhitdistance = 3.0f;
                raycastto0planebefore = RaycastToPanPlane(istouching, startpanhitdistance);
                targetposition = target.position;
            }

            // is rotation starting?
            if (Input.GetMouseButtonDown(buttonrotate) || starttouch)
                if (!blockrotation)
                {
                    desiredRotation = transform.rotation;
                    xDeg = transform.rotation.eulerAngles.y;
                    yDeg = transform.rotation.eulerAngles.x;
                    IsRotating = true;
                }

            //  ending of rotation on buttonrotate up
            if (Input.GetMouseButtonUp(buttonrotate) || endtouch || istwofinger) IsRotating = false;

            // If pannig is active raycasst to the pan plane
            if (Input.GetMouseButton(2) || touchpanning)
            {
                raycastto0plane = RaycastToPanPlane(istouching, startpanhitdistance);
                IsPanning = true;
            }

            if (!Input.GetMouseButton(2) && !touchpanning) IsPanning = false;

            // If Control and Middle button? ZOOM!
            if (Input.GetMouseButton(2) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * ZoomSpeed * MasterSensitivity * shiftfactor * 0.7f *
                                   DPIScale * 6 * Mathf.Abs(currentDistance);
            }

            // If right mouse (or left if rotation is with left) is selected ORBIT
            else if (IsRotating && !blockrotation && !(blockleftmouserotation && RotateWithLeftMouseButton))
            {
                _lastmovement = Time.realtimeSinceStartup;
                if (!touchrotate)
                {
                    var scale = 0.05f * DPIScale * shiftfactor * MouseRotationSpeed * MasterSensitivity * 100;
                    xDeg += Input.GetAxis("Mouse X") * scale;
                    yDeg -= Input.GetAxis("Mouse Y") * scale;
                }
                else
                {
                    xDeg += Touch.TouchDeltaPos.x * DPIScale * TouchRotationSpeed * MasterSensitivity * 400 * 0.001f;
                    yDeg -= Touch.TouchDeltaPos.y * DPIScale * TouchRotationSpeed * MasterSensitivity * 400 * 0.001f;
                }

                //Clamp the vertical axis for the orbit
                yDeg = ClampAngle(yDeg, MinHorizontalRotation, MaxHorizontalRotation);
                // set camera rotation 
                desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                currentRotation = transform.rotation;
                rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * RotDamping * 3f);
                transform.rotation = rotation;
            }
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
            else if (Input.GetMouseButton(2) || touchpanning)
            {
                if (!MouseInOrthoCamera)
                {
                    var delta = raycastto0planebefore - raycastto0plane;
                    _lastmovement = Time.realtimeSinceStartup;
                    var vec = delta;
                    var norm = mycamera.transform.forward;
                    vec -= norm * Vector3.Dot(vec, norm); //the result is parallel to the plane defined by norm
                    targetposition = targetposition + vec;
                }
                else
                {
                    if (orthoviewcontroller != null)
                    {
                        if (incamera.name == "Side")
                        {
                            orthoviewcontroller.transform.Translate(Vector3.right * -Input.GetAxis("Mouse X") *
                                orthoPanSpeed * MasterSensitivity * shiftfactor * orthoviewcontroller.Distance / 10);
                            orthoviewcontroller.transform.Translate(Vector3.up * -Input.GetAxis("Mouse Y") *
                                orthoPanSpeed * MasterSensitivity * shiftfactor * orthoviewcontroller.Distance / 10);
                        }

                        if (incamera.name == "Top")
                        {
                            orthoviewcontroller.transform.Translate(new Vector3(0, 0, -1) * -Input.GetAxis("Mouse X") *
                                orthoPanSpeed * MasterSensitivity * shiftfactor * orthoviewcontroller.Distance / 10);
                            orthoviewcontroller.transform.Translate(new Vector3(1, 0, 0) * -Input.GetAxis("Mouse Y") *
                                orthoPanSpeed * MasterSensitivity * shiftfactor * orthoviewcontroller.Distance / 10);
                        }

                        if (incamera.name == "Front")
                        {
                            orthoviewcontroller.transform.Translate(new Vector3(0, 0, -1) * -Input.GetAxis("Mouse X") *
                                orthoPanSpeed * MasterSensitivity * orthoviewcontroller.Distance / 10);
                            orthoviewcontroller.transform.Translate(new Vector3(0, 1, 0) * -Input.GetAxis("Mouse Y") *
                                orthoPanSpeed * MasterSensitivity * orthoviewcontroller.Distance / 10);
                        }
                    }
                }
            }

            /// Zoom in and out
            // affect the desired Zoom distance if we roll the scrollwheel
            var mousescroll = Input.GetAxis("Mouse ScrollWheel");
            if (mousescroll != 0 || istwofinger)
            {
                var distancefactor = .25f;
                zoomhitdistance = GetClosestHitDistance();
                if (zoomhitdistance > 5)
                    distancefactor = 1f;
                _lastmovement = Time.realtimeSinceStartup;
                if (!MouseInOrthoCamera)
                {
                    if (!iszooming)
                    {
                        desiredDistance -= mousescroll * distancefactor * 0.05f * shiftfactor * ZoomSpeed * MasterSensitivity * 0.7f * 65 *
                                           Mathf.Abs(currentDistance);
                    }
                    else
                    {
#if UNITY_WEBGL && !UNITY_EDITOR
                        desiredDistance -=
 Touch.TwoFingerDeltaDistance * shiftfactor * TouchZoomSpeed * MasterSensitivity * 0.0042f * DPIScale*
                                           Mathf.Abs(currentDistance);
#else
                        desiredDistance -= Touch.TwoFingerDeltaDistance * shiftfactor * TouchZoomSpeed * MasterSensitivity * 0.0042f *
                                           DPIScale *
                                           Mathf.Abs(currentDistance);
#endif
                    }
                }
                else
                {
                    if (orthoviewcontroller != null)
                    {
                        orthoviewcontroller.Distance += mousescroll * orthoviewcontroller.Distance;
                        orthoviewcontroller.UpdateViews();
                    }
                }

                if (desiredDistance > maxdistance)
                    desiredDistance = maxdistance;
                if (desiredDistance <= minDistance)
                {
                    var delta = minDistance - desiredDistance;
                    desiredDistance = minDistance;
                    // move targetposition in delta in camera view direction
                    targetposition = targetposition + mycamera.transform.forward * delta;
                }
            }

            var focuspressed = false;
            if (Input.GetKey(realvirtualController.HotKeyFocus)) focuspressed = true;

            // if hotkey focus is pressed and selectionamanger has a selected object, focus it
            if (selectionmanagernotnull)
            {
                if ((focuspressed || (selectionmanager.DoubleSelect &&
                                      (selectionmanager.FocusDoubleClickedObject ||
                                       selectionmanager.ZoomDoubleClickedObject))) &&
                    selectionmanager.SelectedObject != null)
                {
                    focuspressed = false;
                    _lastmovement = Time.realtimeSinceStartup;
                    var pos = selectionmanager.GetHitpoint();
                    selectionmanager.ShowCenterIcon(true);

                    // get bounding box of all children of selected object
                    distance = CalculateFocusViewDistance(selectionmanager.SelectedObject
                        .GetComponentsInChildren<Renderer>());

                    if (!RotateToFocusObject)
                    {
                        // If not rotate to target object on focus then just move the targetposition (=panning the camera)
                        targetposition = pos;
                    }
                    else
                    {
                        targetposition = pos;
                        var tonewtarget = pos - position;
                        // if rotation is wished - calculate desired rotation and new distance (camera should not move)
                        desiredDistance = Vector3.Magnitude(tonewtarget);
                        // desiredRotation = Quaternion.LookRotation(targetposition-this.position,this.transform.up);
                        desiredRotation = Quaternion.LookRotation(tonewtarget, transform.up);
                        var euler = desiredRotation.eulerAngles;
                        desiredRotation = Quaternion.Euler(euler.x, euler.y, 0);
                    }

                    if (selectionmanager.ZoomDoubleClickedObject || Input.GetKey(KeyCode.F))
                        desiredDistance = distance;
                }

                if (selectionmanager.SelectedObject != null &&
                    (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ||
                     selectionmanager.AutoCenterSelectedObject))
                {
                    var pos = selectionmanager.GetHitpoint();
                    selectionmanager.ShowCenterIcon(true);
                    if (!RotateToFocusObject)
                    {
                        // If not rotate to target object on focus then just move the targetposition (=panning the camera)
                        targetposition = pos;
                    }
                    else
                    {
                        // if rotation is wished - calculate desired rotation and new distance (camera should not move)
                        targetposition = pos;
                        var tonewtarget = pos - position;
                        // if rotation is wished - calculate desired rotation and new distance (camera should not move)
                        desiredDistance = Vector3.Magnitude(tonewtarget);
                        //desiredRotation = Quaternion.LookRotation(targetposition-this.position,this.transform.up);
                        desiredRotation = Quaternion.LookRotation(tonewtarget, transform.up);
                        var euler = desiredRotation.eulerAngles;
                        desiredRotation = Quaternion.Euler(euler.x, euler.y, 0);
                    }
                }
            }

            // if F is pressed without anything selected or relection manager focus the whole scene
            if (focuspressed || Input.GetKey(realvirtualController.HotKeyResetView))
            {
                var dist = CalculateFocusViewDistance(FindObjectsByType<Renderer>(FindObjectsSortMode.None));
                targetposition = calculatedboundscenter;
                desiredDistance = distance;
            }


            if (!MouseInOrthoCamera)
            {
                // Key Navigation
                var control = false;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    control = true;
                // Key 3D Navigation
                if (Input.GetKey(KeyCode.UpArrow) && shift && !control)
                    CameraTransform(mycamera.transform.forward);

                if (Input.GetKey(KeyCode.DownArrow) && shift && !control)
                    CameraTransform(-mycamera.transform.forward);

                if (Input.GetKey(KeyCode.UpArrow) && !shift && !control)
                    CameraTransform(mycamera.transform.up);

                if (Input.GetKey(KeyCode.DownArrow) && !shift && !control)
                    CameraTransform(-mycamera.transform.up);

                if (Input.GetKey(KeyCode.RightArrow) && !control)
                    CameraTransform(mycamera.transform.right);

                if (Input.GetKey(KeyCode.LeftArrow) && !control)
                    CameraTransform(-mycamera.transform.right);

                if (realvirtualController.EnableHotkeys)
                {
                    if (Input.GetKey(realvirtualController.HotKeyTopView)) SetViewDirection(new Vector3(90, 90, 0));

                    if (Input.GetKey(realvirtualController.HotKeyFrontView))
                        if (selectionmanagernotnull && realvirtualController.HotKeyFrontView ==
                            realvirtualController.HotKeyFocus)
                            if (selectionmanager.SelectedObject == null)
                                SetViewDirection(new Vector3(0, 90, 0));
                            else
                                SetViewDirection(new Vector3(0, 90, 0));

                    if (Input.GetKey(realvirtualController.HotKeyBackView)) SetViewDirection(new Vector3(0, 180, 0));

                    if (Input.GetKey(realvirtualController.HotKeyLeftView)) SetViewDirection(new Vector3(0, 180, 0));

                    if (Input.GetKey(realvirtualController.HotKeyRightView)) SetViewDirection(new Vector3(0, 0, 0));
                }
            }
            else
            {
                if (realvirtualController.EnableHotkeys)
                {
                    if (Input.GetKeyDown(realvirtualController.HotKeyOrhtoBigger))
                        orthoviewcontroller.Size += 0.05f;
                    if (Input.GetKeyDown(realvirtualController.HotKeyOrhtoSmaller))
                        orthoviewcontroller.Size -= 0.05f;
                    if (orthoviewcontroller.Size > 0.45f)
                        orthoviewcontroller.Size = 0.45f;
                    if (orthoviewcontroller.Size < 0.1f)
                        orthoviewcontroller.Size = 0.1f;
                    if (Input.GetKeyDown(realvirtualController.HoteKeyOrthoDirection))
                        orthoviewcontroller.Angle += 90;
                    if (orthoviewcontroller.Angle >= 360)
                        orthoviewcontroller.Angle = 0;
                    orthoviewcontroller.UpdateViews();
                }
            }

            if (realvirtualController.EnableHotkeys)
                if (Input.GetKeyDown(realvirtualController.HotKeyOrthoViews))
                {
                    orthoviewcontroller.OrthoEnabled = !orthoviewcontroller.OrthoEnabled;
                    var button =
                        Global.GetComponentByName<GenericButton>(realvirtualController.gameObject, "OrthoViews");
                    if (button != null)
                        button.SetStatus(orthoviewcontroller.OrthoEnabled);
                    orthoviewcontroller.UpdateViews();
                }

            if (mycamera.orthographic)
            {
                mycamera.orthographicSize += mousescroll * mycamera.orthographicSize;
                desiredDistance = 0;
            }

#if ((!UNITY_IOS && !UNITY_ANDROID && !UNITY_EDITOR_OSX && !UNITY_WEBGL) || (UNITY_EDITOR && !UNITY_WEBGL && !UNITY_EDITOR_OSX && !UNITY_EDTOR_LINUX))
            // Space Navigator
            if (EnableSpaceNavigator)
            {
                if (SpaceNavigator.Translation != Vector3.zero)
                {
                    target.rotation = transform.rotation;
                    var spacetrans = SpaceNavigator.Translation;
                    var newtrans = new Vector3(-spacetrans.x, spacetrans.y, -spacetrans.z) * SpaceNavTransSpeed;
                    target.Translate(newtrans, Space.Self);
                }

                if (SpaceNavigator.Rotation.eulerAngles != Vector3.zero)
                {
                    transform.Rotate(-SpaceNavigator.Rotation.eulerAngles);
                    rotation = transform.rotation;
                }
            }
#endif
            currentRotation = transform.rotation;
            if (desiredRotation != currentRotation)
            {
                rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * RotDamping * 6f);
                transform.rotation = rotation;
            }

            // Lerp the target movement
            target.position = Vector3.Lerp(target.position, targetposition, Time.deltaTime * PanDamping * 10);
            //target.position = targetposition;

            // For smoothing of the zoom, lerp distance
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * ZoomDamping * 10.0f);

            // calculate position based on the new currentDistance 
            CalculateCamPos();

            zoomposworld = Vector3.zero;
            DurationNoMouseActivity = Time.realtimeSinceStartup - _lastmovement;
            raycastto0planebefore = RaycastToPanPlane(istouching, startpanhitdistance);
            blockrotationbefore = blockrotation;

#if CINEMACHINE
            if (Time.realtimeSinceStartup - _lastmovement > StartDemoOnInactivity)
                if (!CinemachineIsActive)
                    ActivateCinemachine(true);
#endif
        }

        //! is called when the component is enabled
        private void OnEnable()
        {
            if (realvirtualController == null)
            {
                Awake();
            }
            
            
            if (Touch != null) touchnotnull = true;
            Init();
        }

        //! is called when the application is left - e.g. for saving last camera position
        private void OnApplicationQuit()
        {
            if (LastCameraPosition != null)
                if (SaveCameraPosOnQuit)
                    LastCameraPosition.SaveCameraPosition(this);
        }

        //! is called when orthographic overlay views button is pressed in the main menu bar
        public void OnButtonOrthoOverlay(GenericButton button)
        {
            orthoviewcontroller.OrthoEnabled = button.IsOn;
            orthoviewcontroller.UpdateViews();
        }

        public void OrthoOverlayToggleOn()
        {
            orthoviewcontroller.OrthoEnabled = true;
            orthoviewcontroller.UpdateViews();
        }

        public void OrthoOverlayToggleOff()
        {
            orthoviewcontroller.OrthoEnabled = false;
            orthoviewcontroller.UpdateViews();
        }

        //! is called when orthographic view is enabled or disabled
        public void OnButtonOrthographicView(GenericButton button)
        {
            SetOrthographicView(button.IsOn);
        }

        //! is called when orthographic view is enabled or disabled
        public void OrthographicViewToggleOn()
        {
            SetOrthographicView(true);
        }

        public void OrthographicViewToggleOff()
        {
            SetOrthographicView(false);
        }

        public void SetOrthographicView(bool active)
        {
            if (active == orthograhicview && Application.isPlaying)
                return; /// no changes
            orthograhicview = active;
            if (mycamera == null)
                mycamera = GetComponent<Camera>();
            mycamera.orthographic = active;
            if (!active)
            {
                desiredDistance = lastperspectivedistance;
                mycamera.farClipPlane = 5000f;
                mycamera.nearClipPlane = 0.1f;
            }
            else
            {
                lastperspectivedistance = desiredDistance;
                mycamera.farClipPlane = 5000f;
                mycamera.nearClipPlane = -5000f;
            }

            // change button in UI
            var button = Global.GetComponentByName<GenericButton>(Global.realvirtualcontroller.gameObject, "Perspective");
            if (button != null)
                if (button.IsOn != active)
                    button.SetStatus(active);
        }

        private void OnMultiSelect(bool multisel)
        {
        }

        //! Blocks the rotation of the camera e.g. when an element is selected with left mouse and left mouse rotation is on
        public void BlockRotation(bool block, bool onlyleftmouse)
        {
            if (block)
                IsRotating = false;


            blockrotation = block;

            if (onlyleftmouse)
            {
                blockrotation = false;
                blockleftmouserotation = block;
            }
        }

        //! is called by object selection manager when an object is selected
        private void OnSelected(GameObject go, bool selected, bool multiselect, bool changedSelection)
        {
            var touch = Input.touchCount > 0;
            if ((RotateWithLeftMouseButton || touch) && selected) BlockRotation(true, true);
            if ((RotateWithLeftMouseButton || touch) && !selected) BlockRotation(false, true);
        }

        //! called by main menu view button to switch between first person controller and normal mouse navigation
        public void OnViewButton(GenericButton button)
        {
            if (button.IsOn && FirstPersonController != null)
            {
                SetOrthographicView(false);
                if (CinemachineIsActive)
                    ActivateCinemachine(false);
                Global.SetActiveIncludingSubObjects(FirstPersonController.gameObject, true);
                FirstPersonControllerActive = true;
                FirstPersonController.SetActive(true);
            }
            else
            {
                FirstPersonControllerActive = false;
                Global.SetActiveIncludingSubObjects(FirstPersonController.gameObject, false);
            }
        }

        public void ViewButtonToggleOn()
        {
            if (FirstPersonController != null)
            {
                SetOrthographicView(false);
                if (CinemachineIsActive)
                    ActivateCinemachine(false);
                Global.SetActiveIncludingSubObjects(FirstPersonController.gameObject, true);
                FirstPersonControllerActive = true;
                FirstPersonController.SetActive(true);
            }
        }

        public void ViewButtonToggleOff()
        {
            if (FirstPersonController != null)
            {
                FirstPersonControllerActive = false;
                Global.SetActiveIncludingSubObjects(FirstPersonController.gameObject, false);
            }
        }

        private void CalculateCamPos()
        {
            // calculate position based on the new currentDistance 
            position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
            if (position != transform.position) transform.position = position;
        }

        //! sets a new camera position based ob targetos, distance to targetois and rotation
        public void SetNewCameraPosition(Vector3 targetpos, float camdistance, Vector3 camrotation,
            bool nointerpolate = false)
        {
            // End first person controller if it is on
            if (FirstPersonControllerActive)
            {
                FirstPersonController.SetActive(false);
                FirstPersonControllerActive = false;
            }

            if (target == null)
                return;
            if (InterpolateToNewCamerapoitions && !nointerpolate)
            {
                interpolatingToNewCameraPos = true;
                interpolaterotation = camrotation;
                interpolatedistance = camdistance;
                interpolatetargetpos = targetpos;
                interpolatenewcamrot = Quaternion.Euler(camrotation);
                interpolatenewcampos = interpolatetargetpos -
                                       (interpolatenewcamrot * Vector3.forward * interpolatedistance + targetOffset);
                EventStartStopCameraInterpolation?.Invoke(true);
                InterpolateToNewCameraPos();
                return;
            }

            desiredDistance = camdistance;
            currentDistance = camdistance;
            target.position = targetpos;
            targetposition = targetpos;
            desiredRotation = Quaternion.Euler(camrotation);
            currentRotation = Quaternion.Euler(camrotation);
            rotation = Quaternion.Euler(camrotation);
            transform.rotation = Quaternion.Euler(camrotation);

            CalculateCamPos();
        }

        private void InterpolateToNewCameraPos()
        {
            var atpos = false;
            var atrot = false;
            // lerping to new camera position
            if (Vector3.Distance(transform.position, interpolatenewcampos) > 0.01f)
                transform.position = Vector3.Lerp(transform.position, interpolatenewcampos,
                    Time.deltaTime * CameraInterpolationSpeed * 5);
            else
                atpos = true;

            // lerp to new rotation
            if (Quaternion.Angle(transform.rotation, interpolatenewcamrot) > 0.01f)
                transform.rotation = Quaternion.Lerp(transform.rotation, interpolatenewcamrot,
                    Time.deltaTime * CameraInterpolationSpeed * 5);
            else
                atrot = true;

            if (atpos && atrot)
            {
                interpolatingToNewCameraPos = false;
                SetNewCameraPosition(interpolatetargetpos, interpolatedistance, interpolaterotation, true);
                EventStartStopCameraInterpolation?.Invoke(false);
            }
        }

        //! sets the camera view direction based on a vector
        public void SetViewDirection(Vector3 camrotation)
        {
            desiredRotation = Quaternion.Euler(camrotation);
            currentRotation = Quaternion.Euler(camrotation);
            rotation = Quaternion.Euler(camrotation);
            transform.rotation = Quaternion.Euler(camrotation);
        }

        public void ActivateCinemachine(bool activate)
        {
#if CINEMACHINE
            CinemachineBrain brain;
            brain = GetComponent<CinemachineBrain>();
            if (brain == null)
                return;

            if (!activate)
                if (brain.ActiveVirtualCamera != null)
                {
                    var camrot = brain.ActiveVirtualCamera.VirtualCameraGameObject.transform.rotation;
                    var rot = camrot.eulerAngles;
                    distance = Vector3.Distance(transform.position, target.position);
                    var tarpos = brain.ActiveVirtualCamera.VirtualCameraGameObject.transform.position +
                                 (camrot * Vector3.forward * distance + targetOffset);
                    SetNewCameraPosition(tarpos, distance, rot);
                }

            if (brain != null)
            {
                if (activate)
                    brain.enabled = true;
                else
                    brain.enabled = false;
            }

            CinemachineIsActive = activate;
#endif
        }

#if CINEMACHINE
        //! Activates the cinemachine camera and sets it to the highest priority
        public void ActivateCinemachineCam(CinemachineVirtualCamera vcam)
        {
            vcam.enabled = true;
            vcam.Priority = 100;
            if (CinemachineIsActive == false)
                ActivateCinemachine(true);

            // Set low priority to all other vcams
            var vcams = FindObjectsByType(typeof(CinemachineVirtualCamera),FindObjectsSortMode.None);
            foreach (CinemachineVirtualCamera vc in vcams)
                if (vc != vcam)
                    vc.Priority = 10;
        }
#endif

        public void Init()
        {
#if CINEMACHINE
            ActivateCinemachine(false);
            
#endif
#if REALVIRTUAL_PROFESSIONAL
            hmiController = FindAnyObjectByType<HMI_Controller>();
#endif
            var rnds = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
            if (!target)
            {
                var go = new GameObject("Cam Target");
                go.transform.position = transform.position + transform.forward * distance;
                target = go.transform;
            }

            mycamera = GetComponent<Camera>();

            distance = Vector3.Distance(transform.position, target.position);
            currentDistance = distance;
            desiredDistance = distance;

            //be sure to grab the current rotations as starting points.
            position = transform.position;
            rotation = transform.rotation;
            currentRotation = transform.rotation;
            desiredRotation = transform.rotation;

            xDeg = Vector3.Angle(Vector3.right, transform.right);
            yDeg = Vector3.Angle(Vector3.up, transform.up);

            if (LastCameraPosition != null && !FirstPersonControllerActive && !startcameraposset)
            {
                if (SetCameraPosOnStartPlay)
                    SetNewCameraPosition(LastCameraPosition.TargetPos, LastCameraPosition.CameraDistance,
                        LastCameraPosition.CameraRot);
                startcameraposset = true;
            }

            if (FirstPersonController != null)
            {
                if (FirstPersonControllerActive)
                    FirstPersonController.SetActive(true);
                else
                    FirstPersonController.SetActive(false);
            }
#if (UNITY_EDTOR_LINUX || UNITY_STANDALONE_LINUX)
            		            DPIScale = 1;
#else
            DPIScale = 144 / Screen.dpi;
#endif
            orthoviewcontroller = transform.parent.GetComponentInChildren<OrthoViewController>();
            maxdistance = CalculateFocusViewDistance(rnds) * 2;
        }

        private void CameraTransform(Vector3 direction)
        {
            target.rotation = transform.rotation;
            targetposition = targetposition+direction * CursorSpeed * MasterSensitivity * 0.02f;
            
            _lastmovement = Time.realtimeSinceStartup;
        }

        private void CamereSetDirection(Vector3 direction)
        {
            desiredDistance = 10f;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        private bool MouseOverViewport(Camera main_cam, Camera local_cam)
        {
            if (!Input.mousePresent) return true; //always true if no mouse??

            var main_mou = main_cam.ScreenToViewportPoint(Input.mousePosition);
            return local_cam.rect.Contains(main_mou);
        }

        //! gets the closest distance of an object which is hit by a raycast at m ouse position
        private float GetClosestHitDistance()
        {
            RaycastHit[] hits;
            var hitdistance = 0f;
            var hitpoint = Vector3.zero;
            var pointerpos = Input.mousePosition;
            if (touch)
                pointerpos = Touch.TouchPos;
            var ray = mycamera.ScreenPointToRay(pointerpos);
            hits = Physics.RaycastAll(ray);
            var min = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.distance < min)
                {
                    min = hit.distance;
                    hitpoint = hit.point;
                }

                hitdistance = Vector3.Distance(hitpoint, transform.position);
            }

            return hitdistance;
        }

        //! raycasts to a plane for panning in parallel to the camera. distance of the plane is selected at the closest hitpoint (if there is a hit) or at 3m 
        private Vector3 RaycastToPanPlane(bool touch, float distance)
        {
            try
            {
                var pointerpos = Input.mousePosition;
                if (touch)
                    pointerpos = Touch.TouchPos;
                Ray ray;
                try
                {
                     ray = mycamera.ScreenPointToRay(pointerpos);
                } catch 
                {
                    return Vector3.zero;
                }
               

                // find distance to the bottom plane
                var bottom = new Plane(transform.forward,
                    transform.position + transform.forward * distance);
                if (bottom.Raycast(ray, out distance)) return ray.GetPoint(distance);

                return Vector3.zero;
            }
            catch (Exception e)
            {
                Debug.Log("Error in RaycastToPanPlane: " + e.Message);
                return Vector3.zero;
            }
        }


        private Vector3 RayCastToBottomZoom(bool touch = false)
        {
            Ray ray;
            if (!touch)
                ray = mycamera.ScreenPointToRay(Input.mousePosition);
            else
                ray = mycamera.ScreenPointToRay(Touch.TwoFingerMiddlePos);

            var plane = new Plane(Vector3.up, Vector3.zero);
            // raycast from mouseposition to this plane
            float distance;
            if (plane.Raycast(ray, out distance)) return ray.GetPoint(distance);

            return Vector3.zero;
        }

        private bool CheckMouseOverOrthoView()
        {
         
                incamera = mycamera;
                if (Camera.allCameras.Length > 1)
                    foreach (var cam in Camera.allCameras)
                    {
                        // get a parent of cam
                        var parent = cam.transform.parent;
                        if (parent != null)
                            if (parent.name == "OrthoViews")
                            {
                                if (cam != Camera.main)
                                    if (MouseOverViewport(mycamera, cam))
                                    {
                                        incamera = cam;
                                        return true;
                                    }
                            }
                    }
                return false;
        }

        private float CalculateFocusViewDistance(Renderer[] renderers)
        {
            var combinedBounds = new Bounds();
            
            
            // fist deleta everything which is under realvirtual from the bounds
            for (var i = 0; i < renderers.Length; i++)
                if (renderers[i].transform.IsChildOf(transform))
                    renderers[i] = null;
            
            // Get the renderer for each child object and combine their bounds
            foreach (var renderer in renderers)
                if (renderer != null)
                {
                    if (combinedBounds.size == Vector3.zero)
                        combinedBounds = renderer.bounds;
                    else
                        combinedBounds.Encapsulate(renderer.bounds);
                }

            var cameraDistance = 2.0f;
            var objectSizes = combinedBounds.max - combinedBounds.min;
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            var cameraView =
                2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad *
                                 mycamera.fieldOfView); // Visible height 1 meter in front
            var distance =
                cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
            distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
            calculatedboundscenter = combinedBounds.center;
            return distance;
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}