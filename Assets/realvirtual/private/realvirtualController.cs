// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#pragma warning disable 0168
#pragma warning disable 0649

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NaughtyAttributes;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Linq;
using IngameDebugConsole;
using RuntimeInspectorNamespace;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine.Serialization;

#if CINEMACHINE
using Cinemachine;
#endif
#if (UNITY_POST_PROCESSING_STACK_V2)
using UnityEngine.Rendering.PostProcessing;
#endif


namespace realvirtual
{
    [ExecuteAlways]
    //! This object needs to be in every realvirtual scene. It controls main central data (like scale...) and manages main realvirtual settings for the scene.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/game4automation")]
    public class realvirtualController : realvirtualBehavior, ISceneLoaded, IBeforeAwake
    {
        #region PublicVariables

        [Header("General options")]
        [Tooltip("Indicates connection status to interfaces for toggling realvirtual components based on active status")]
        public bool Connected = true; //!< Indicates connection status to interfaces for toggling realvirtual components based on active status
        
        [Tooltip("Performs model check before starting simulation")]
        public bool ModelCheckerEnabled = true; //!< Performs model check before starting simulation
        
        [Tooltip("Enables validation when components are added to GameObjects")]
        public bool ValidationOnComponentsAdded = true; //!< Enables validation when components are added to GameObjects
        
        [Tooltip("Enables validation before entering play mode")]
        public bool ValidateBeforeStart = true; //!< Enables validation before entering play mode
        
        [Tooltip("Stops physics when scene is paused")]
        public bool StopPhysicsWhenPaused = false; //!< Stops physics when scene is paused
        
        [Tooltip("Global scale factor in millimeters for size adjustment")]
        public float Scale = 1000; //!< Global scale factor in millimeters for size adjustment
        
        public bool DebugMode = false; //!< Enables debug mode for development

#if CINEMACHINE
        [FormerlySerializedAs("StartWithCamera"), Header("Camera")]
        [Tooltip("The Cinemachine Virtual Camera that the scene starts with")]
        public CinemachineVirtualCamera StartWithCinemachineCamera; //!< The Cinemachine Virtual Camera that the scene starts with
#endif

        [Range(0, 10)]
        [Tooltip("Overrides default speed settings with a factor for custom speed adjustment")]
        public float SpeedOverride = 1; //!< Speed override factor for custom speed adjustment

        [OnValueChanged("ChangedTimeScale"), Range(0, 20)]
        [Tooltip("Adjusts the scale of time, affecting animations and physics")]
        public float TimeScale = 1; //!< Time scale factor affecting animations and physics

        [ReorderableList]
        [Tooltip("A list of groups to be hidden from the user interface")]
        public List<string> HideGroups; //!< List of groups to hide from UI

        [Tooltip("Enables automatic scene restart")]
        public bool Restart = false; //!< Enables automatic scene restart

        [ShowIf("Restart")]
        [Tooltip("The delay in seconds after which the scene will automatically restart")]
        public float RestartSceneAfterSeconds = 60; //!< Restart delay in seconds

        [Header("Additive Scene Loading")]
        [OnValueChanged("LoadAdditiveScenes")]
        [Tooltip("Enables loading of additive scenes in the editor")]
        public bool AdditiveLoadScenes = true; //!< Enables additive scene loading in editor

        [ReorderableList]
        [Tooltip("A list of scenes to be loaded additively")]
        public List<string> AdditiveScenes; //!< Scenes to load additively

        private GameObject _debugconsole;

        [HideInInspector]
        [Tooltip("Enables position debugging for development purposes")]
        public bool EnablePositionDebug = true; //!< Enables position debugging

        [HideInInspector]
        [Tooltip("Specifies the layer on which debug elements are rendered")]
        public int DebugLayer = 13; //!< Debug rendering layer

        [Header("UI options Editor")]
        [Tooltip("Defines how frequently the hierarchy should be updated in the editor")]
        public float HierarchyUpdateCycle = 0.2f; //!< Hierarchy update frequency in seconds

        [BoxGroup("Hierarchy Icons")]
        [Tooltip("Enables the display of icons in the hierarchy view")]
        public bool ShowHierarchyIcons = true; //!< Shows icons in hierarchy view

        [BoxGroup("Hierarchy Icons"), ShowIf("ShowHierarchyIcons")]
        [Tooltip("Defines the width of the group names displayed next to hierarchy icons")]
        public float WidthGroupName = 10; //!< Width of group names in hierarchy

        [BoxGroup("Hierarchy Icons"), ShowIf("ShowHierarchyIcons")]
        [Tooltip("Toggle to display components of objects in the hierarchy icons")]
        public bool ShowComponents = true; //!< Shows component icons in hierarchy

        [Range(0f, 2f)]
        [Tooltip("Adjusts the scale of handles in the editor for easier manipulation")]
        public float ScaleHandles = 1; //!< Editor handle scale factor

        [Tooltip("The standard source object to be used by the hotkey")]
        public GameObject StandardSource; //!< Standard source object for hotkey operations

        [Tooltip("Connect a Setting for the Gizmos shown during Editor Mode")]
        public EditorGizmoOptions EditorGizmoSettings; //!< Editor gizmo display settings

        [BoxGroup("Hotkeys")]
        [Tooltip("Enables the use of hotkeys for quick actions within the editor")]
        public bool EnableHotkeys = true; //!< Enables editor hotkeys

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for quick edit overlay")]
        public KeyCode HotkeyQuickEdit; //!< Quick edit overlay hotkey

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey to insert the standard source object")]
        public KeyCode HotkeySource; //!< Insert standard source hotkey

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for deleting all MU objects")]
        public KeyCode HotkeyDelete; //!< Delete all MUs hotkey

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey to create objects on the source")]
        public KeyCode HotkeyCreateOnSource; //!< Create on source hotkey

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for focusing object - if nothing is selected it centers all")]
        public KeyCode HotKeyFocus; //!< Focus selected object or center all

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey resetting the view - centering all")]
        public KeyCode HotKeyResetView; //!< Reset view to center all

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for deselecting objects")]
        public KeyCode HotKeyDeselect; //!< Deselect all objects

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for top view")]
        public KeyCode HotKeyTopView; //!< Switch to top view

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for front view")]
        public KeyCode HotKeyFrontView; //!< Switch to front view

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for back view")]
        public KeyCode HotKeyBackView; //!< Switch to back view

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for left view")]
        public KeyCode HotKeyLeftView; //!< Switch to left view

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for right view")]
        public KeyCode HotKeyRightView; //!< Switch to right view

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for orthogonal views")]
        public KeyCode HotKeyOrthoViews; //!< Toggle orthogonal views

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey to increase orthogonal view size")]
        public KeyCode HotKeyOrhtoBigger; //!< Increase orthogonal view size

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey to decrease orthogonal view size")]
        public KeyCode HotKeyOrhtoSmaller; //!< Decrease orthogonal view size

        [ShowIf("EnableHotkeys"), BoxGroup("Hotkeys")]
        [Tooltip("Hotkey for changing orthogonal view direction")]
        public KeyCode HoteKeyOrthoDirection; //!< Change orthogonal view direction

        [Header("UI options Runtime")]
        [Tooltip("Enables the UI at the start of the runtime")]
        public bool UIEnabledOnStart = true; //!< Enables UI at runtime start

        [ShowIf("UIEnabledOnStart")]
        [Tooltip("Enables the runtime inspector when the game starts")]
        public bool RuntimeInspectorEnabled = true; //!< Enables runtime inspector

        [Tooltip("Enables the ability to select objects during runtime")]
        public bool ObjectSelectionEnabled = true; //!< Enables runtime object selection

        [ShowIf("UIEnabledOnStart")]
        [Tooltip("Hides the information box at runtime if enabled")]
        public bool HideInfoBox = false; //!< Hides runtime info box

        [Tooltip("Reference to the runtime application UI")]
        public GameObject RuntimeApplicationUI; //!< Runtime application UI reference

        [Tooltip("Reference to the runtime automation UI")]
        public GameObject RuntimeAutomationUI; //!< Runtime automation UI reference


        [HideInInspector] public List<GameObject> LockedObjects = new List<GameObject>();
        [HideInInspector] public List<string> HiddenGroups;
        [HideInInspector] public List<GameObject> ConnectionsActive = new List<GameObject>();

#if CINEMACHINE
        [HideInInspector] public CinemachineVirtualCamera CurrentCamera;
#endif

        #endregion

        #region Private Variables
        private int scenesloaded = 0;
        private int totalscenestoload = 0;
        private int _lastmuid = 0;
        private bool _stepwise = false;
        private float[] scalevalues = new float[] { 1, 10, 100, 1000 };
        private float[] speedvalues = new float[] { 0.1f, 0.5f, 1, 1.5f, 2, 5, 10, 20 };
        private float[] timescalevalues = new float[] { 0.1f, 0.5f, 1, 1.5f, 2, 5, 10, 20 };

        private DropdownList<int> qualityvalues = new DropdownList<int>()
        {
            { "Very Low", 0 },
            { "Low", 1 },
            { "Medium", 2 },
            { "Hight", 3 },
            { "VeryHigh", 4 },
            { "Ultra", 5 }
        };

        [HideInInspector] public InspectorController InspectorController;

        private Camera _maincamera;
        private GameObject _uimessages;
        private DateTime _lastupdatesingals;
        private SelectionRaycast currentSelectionRaycast;


        private static int undoIndex;
        private rvUIToolbarButton _buttonconnection;

        #endregion
        
        #region DebugCommands
        [ConsoleMethod("HideInfo", "Hides for future starts the Info Box")]
        //! Hides the info box permanently by saving preference
        public static void HideInfo()
        {
            var controllerInstance = FindFirstObjectByType<realvirtualController>();
            controllerInstance.HideInfoBox = true;
            Persistence.Save(controllerInstance.HideInfoBox, controllerInstance.name, "HideInfoBox");
            Logger.Message("Infobox hidden for future starts", controllerInstance);
        }
        
        [ConsoleMethod("DeletePrefs", "Deletes all Player Prefs")]
        //! Deletes all Unity PlayerPrefs stored data
        public static void DeleteControllerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Logger.Message("All PlayerPrefs deleted");
        }

        [ConsoleMethod("ConnectOn", "Turns on Connect Mode - is also saved for next start")]
        //! Enables connection mode and saves the preference
        public static void TurnOnConnectMode()
        {
            var controllerInstance = FindFirstObjectByType<realvirtualController>();
            controllerInstance.Connected = true;
            controllerInstance.ConnectionButtonToggleOn();
            Persistence.Save(controllerInstance.Connected, "realvirtualController", "Connected");
            Logger.Message("Connected Mode turned on - saved to Player Prefs", controllerInstance);
            controllerInstance.UpdateConnectionButton();
        }

        [ConsoleMethod("ConnectOff", "Turns off Connect Mode - is saved for next start")]
        //! Disables connection mode and saves the preference
        public static void TurnOffConnectMode()
        {
            var controllerInstance =  FindFirstObjectByType<realvirtualController>();
            controllerInstance.Connected = false;
            controllerInstance.ConnectionButtonToggleOff();
            Persistence.Save(controllerInstance.Connected, "realvirtualController", "Connected");
            Logger.Message("Connected Mode turned off - saved to Player Prefs", controllerInstance);
            controllerInstance.UpdateConnectionButton();
        }


        [ConsoleMethod("DebugOn", "Turns on Debug Mode - Player needs to be restarted to debug from Start on")]
        //! Enables debug mode and saves the preference (requires restart to debug from start)
        public static void TurnOnDebugMode()
        {
            var controllerInstance =  FindFirstObjectByType<realvirtualController>();
            controllerInstance.DebugMode = true;
            Persistence.Save(controllerInstance.DebugMode, "realvirtualController", "DebugMode");
            Logger.Message("Debug Mode turned on - Restart Player to debug from Start on", controllerInstance);
        }

        [ConsoleMethod("DebugOff", "Turns on Debug Mode - Player needs to be restarted to debug from Start on")]
        //! Disables debug mode and saves the preference
        public static void TurnOffDebugMode()
        {
            var controllerInstance =  FindFirstObjectByType<realvirtualController>();
            controllerInstance.DebugMode = false;
            Persistence.Save(controllerInstance.DebugMode, "realvirtualController", "DebugMode");
            Logger.Message("Debug Mode turned off - saved to Player Prefs", controllerInstance);
        }
        #endregion
        
        #region GlobalInitEvents
        //! Called before Awake to initialize global settings and load preferences.
        //! IMPLEMENTS IBeforeAwake::OnBeforeAwake
        public void OnBeforeAwake()
        {
            if (DebugMode) Logger.Message("<color=green>---------------- STARTING PLAYMODE DEBUG</color>");
            Global.SetG4AController(this);
           
            if(!Application.isPlaying)
                return;
            
            var loaded = Persistence.Load<bool>(ref DebugMode, "realvirtualController", "DebugMode");
            if (DebugMode)
            {
                if (loaded)
                    Logger.Message("Global Debug Mode loaded from Player Prefs", this);
                Logger.Message("Global Debug Mode (RealvirtualController) turned on", this);
            }

            loaded = Persistence.Load<bool>(ref Connected, "realvirtualController", "Connected");
            {
                if (loaded)
                    Logger.Message("Global Connected Mode loaded from Player Prefs", this);
                UpdateConnectionButton();
            }
            
            // call all IInitStart Interfaces
            if (DebugMode) Logger.Message("<color=green>---------------- INIT realvirtualController - Call IInitStart</color>");
            var initStarts = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).OfType<IInitStart>();
            foreach (var initstart in initStarts)
            {
                initstart.InitStart();
            }
            
//#if !UNITY_EDITOR
                LoadAdditiveScenes();
//#endif
            
       
            
        }

        private void AllScenesAreLoaded()
        {
            // call all IInitStart Interfaces
            if (DebugMode) Logger.Message("<color=green>---------------- INIT realvirtualController - Call IAllScenesLoaded</color>");
            var iallscenesloaded = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).OfType<IAllScenesLoaded>();
            foreach (var allscenesloaded in iallscenesloaded)
            {
                allscenesloaded.AllScenesLoaded();
            }
            if (DebugMode) Logger.Message("<color=green>---------------- INIT realvirtualController - Call IPostAllScenesLoaded</color>");
            var ipostallscenesloaded = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).OfType<IPostAllScenesLoaded>();
            foreach (var ipostallscenes in ipostallscenesloaded)
            {
                ipostallscenes.PostAllScenesLoaded();
            }
            
        }

        //! Called when the component is enabled, handles initialization and scene setup
        public void OnEnable()
        {
#if UNITY_EDITOR
            Global.SetG4AController(this);

            if (LockedObjects == null)
                LockedObjects = new List<GameObject>();

            if (Application.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                // After End of Play
                QuickToggle.SetGame4Automation(this);
                UpdateAllLockedAndHidden();
            }

            if (Application.isPlaying == true && EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                // When Play Started
                QuickToggle.SetGame4Automation(this);
                UpdateAllLockedAndHidden();
            }
   
#endif
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (Application.isPlaying)
            {
                // call all IInitEnable
                if (DebugMode) Logger.Message("<color=green>---------------- INIT realvirtualController - OnEnable - Call IInitEnable</color>");
                var initEnables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).OfType<IInitEnable>();  
                foreach (var initenable in initEnables)
                {
                    initenable.InitEnable();
                }
            }
         
        }
        
        new void Awake()
        {
            /// Set all UI Elements to standards
            var settingscontroller = Global.GetComponentAlsoInactive<SettingsController>(this.gameObject);
            if (settingscontroller != null)
            {
                settingscontroller.gameObject.SetActive(true);
                var window = GetChildByNameAlsoHidden("Window");
                window.SetActive(false);
                Global.SetActiveSubObjects(window, true);
            }
            
            Persistence.Load(ref HideInfoBox, this.name, "HideInfoBox");

            var info = GetChildByNameAlsoHidden("Info");
            if (info != null)
                Global.SetActiveIncludingSubObjects(info, !HideInfoBox);

            _maincamera = GetComponentInChildren<Camera>();
            _uimessages = GetChildByName("MessageBoxes");

            UpdateConnectionButton();

            Global.realvirtualcontroller = this;
            Invoke("ChangeUIEnable", 0.01f);

            var inspector = GetChildByNameAlsoHidden("Inspector");
            if (InspectorController != null)
            {
                Global.SetActiveIncludingSubObjects(inspector, RuntimeInspectorEnabled);
            }

            HideGroupsOnStart();

            // Call all G4AAwake
            var behaviors = Object.FindObjectsByType<realvirtualBehavior>(FindObjectsSortMode.None);
            foreach (var behavior in behaviors)
            {
                behavior.AwakeAlsoDeactivated();
            }
#if UNITY_EDITOR
            EditorApplication.update += CeckSignalUpdate;
#endif

            var objectbutton = Global.GetComponentByName<Component>(gameObject, "ObjectSelection");
            var spaceobjectbutton = Global.GetComponentByName<Component>(gameObject, "SpaceObjectSelection");
            currentSelectionRaycast = GetComponentInChildren<SelectionRaycast>();

            if (ObjectSelectionEnabled)
            {
                if (objectbutton != null)
                    objectbutton.gameObject.SetActive(true);
                if (spaceobjectbutton != null)
                    spaceobjectbutton.gameObject.SetActive(true);
                if (currentSelectionRaycast != null)
                    currentSelectionRaycast.IsActive = false;
            }
            else
            {
                if (objectbutton != null)
                    objectbutton.gameObject.SetActive(false);
                if (spaceobjectbutton != null)
                    spaceobjectbutton.gameObject.SetActive(false);
                if (currentSelectionRaycast != null)
                    currentSelectionRaycast.IsActive = false;
            }
            // detect current scene name
            
            
           if (Application.isPlaying)
               Logger.Message("Starting scene " +SceneManager.GetActiveScene().name + " realvirtual Version " + Global.Version);

            if (EditorGizmoSettings != null)
                EditorGizmoSettings.SelectedMeshes.Clear();

            if (Application.isPlaying)
            {
                // call all IInitAwake
                if (DebugMode) Logger.Message("<color=green>---------------- INIT realvirtualController - Awake - Call IInitAwake</color>");
                var initAwakes = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).OfType<IInitAwake>();
                foreach (var initawake in initAwakes)
                {
                    initawake.InitAwake();
                }
            }
          
        }
        //! Stops the simulation for all realvirtual behaviors
        public new void StopSim()
        {
            if (DebugMode) Logger.Message("<color=yellow>---------------- STOP SIMULATION</color>");
            var realvirtualBehaviors =
                UnityEngine.Object.FindObjectsByType<realvirtualBehavior>(FindObjectsSortMode.None);
            // get all Game4automationBehaviors
            foreach (var behavior in realvirtualBehaviors)
            {
                behavior.StopSim();
            }
        }

        //! Starts the simulation for all realvirtual behaviors
        public new void StartSim()
        {
            if (DebugMode) Logger.Message("<color=yellow>---------------- PRE START SIMULATION</color>");
            var realvirtualBehaviors =
                UnityEngine.Object.FindObjectsByType<realvirtualBehavior>(FindObjectsSortMode.None);
            foreach (var behavior in realvirtualBehaviors)
            {
                behavior.PreStartSim();
            }
            
            if (DebugMode) Logger.Message("<color=yellow>---------------- START SIMULATION</color>");

            // get all Game4automationBehaviors
            foreach (var behavior in realvirtualBehaviors)
            {
                behavior.StartSim();
            }
        }
        
        //! Quits the application
        public void Quit()
        {
            Application.Quit();
        }
        
        //! Starts the simulation and sets the time scale
        public void Play()
        {
            Time.timeScale = TimeScale;
            StartSim();
        }

        //! Pauses the simulation and optionally stops physics
        public void Pause()
        {
            if (StopPhysicsWhenPaused)
                Time.timeScale = 0;
            StopSim();
            if (_stepwise)
            {
                BroadcastMessage("SetToggleOn", "Pause", SendMessageOptions.DontRequireReceiver);
            }
        }
        
       
        //! Enables connection mode and updates all realvirtual behaviors
        public void ConnectionButtonToggleOn()
        {
            Connected = true;
            var objs = UnityEngine.Resources.FindObjectsOfTypeAll<realvirtualBehavior>();
            foreach (var obj in objs)
            {
                obj.ChangeConnectionMode(Connected);
            }
        }

        //! Disables connection mode and updates all realvirtual behaviors
        public void ConnectionButtonToggleOff()
        {
            Connected = false;
            var objs = UnityEngine.Resources.FindObjectsOfTypeAll<realvirtualBehavior>();
            foreach (var obj in objs)
            {
                obj.ChangeConnectionMode(Connected);
            }
        }

        //! Called when an interface connection is opened
        public void OnConnectionOpened(GameObject Interface)
        {
            if (!ConnectionsActive.Contains(Interface))
                ConnectionsActive.Add(Interface);
            UpdateInterfaceButtonStatus();
        }

        //! Called when an interface connection is closed
        public void OnConnectionClosed(GameObject Interface)
        {
            if (ConnectionsActive.Contains(Interface))
                ConnectionsActive.Remove(Interface);
            UpdateInterfaceButtonStatus();
        }

        

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scenesloaded++;
            if (scenesloaded >= totalscenestoload)
            {
                AllScenesAreLoaded();
            }
                
        }
        
#if UNITY_EDITOR
        //! Called when the component is disabled, cleans up references
        public void OnDisable()
        {
            QuickToggle.SetGame4Automation(null);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Global.SetG4AController(null);
        }
        
        //! Called when edit mode is finished in the Unity Editor
        public void OnEditModeFinished()
        {
            if (DebugMode) Logger.Message("On Editmode Finished", this);
        }

        //! Called when play mode is finished in the Unity Editor
        public void OnPlayModeFinished()
        {
#if UNITY_EDITOR
            if (DebugMode)
                Logger.Message("realvirtual Controller - Play Mode Finished");

            UpdateAllLockedAndHidden();
            if (Camera.main != null)
            {
                var nav = Camera.main.GetComponent<SceneMouseNavigation>();
                if (nav != null)
                    if (nav.SetEditorCameraPos)
                        if (nav.LastCameraPosition != null)
                            nav.LastCameraPosition.SetCameraPositionEditor(Camera.main);
            }
#endif
        }
#endif
        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.update -= CeckSignalUpdate;
#endif
        }

        #endregion
        
        #region PublicMethods
        public new void ChangeConnectionMode(bool isconnected)
        {
        }

        public void SetStartView()
        {
            var camposes = Global.GetComponentsByName<CameraPosition>(this.gameObject, "Main Camera");
            camposes.Select(x => x.ActivateOnStart = false);
            var campose = camposes[0];
            campose.ActivateOnStart = true;
            campose.GetCameraPosition();
        }

#if CINEMACHINE
        public void SetCinemachineCamera(CinemachineVirtualCamera camera)
        {
            var scenemousenavigation = UnityEngine.Object.FindAnyObjectByType<SceneMouseNavigation>();
            if (scenemousenavigation != null)
                scenemousenavigation.ActivateCinemachine(true);
            camera.Priority = 100;
            if (CurrentCamera != null && CurrentCamera != camera)
                CurrentCamera.Priority = 10;
            CurrentCamera = camera;
        }
#endif

        public void ChangeTimeScale(float scale)
        {
            TimeScale = scale;
            Time.timeScale = scale;
        }

        public void SetView(int view)
        {
            var camposes = Global.GetComponentsByName<CameraPosition>(this.gameObject, "Main Camera");
            var campose = camposes[view];
            campose.GetCameraPosition();
        }

        public void ActiveateView(int view)
        {
            var camposes = Global.GetComponentsByName<CameraPosition>(this.gameObject, "Main Camera");
            var campose = camposes[view];
            campose.SetCameraPosition();
        }

        public void AddHideGroup(string group)
        {
#if UNITY_EDITOR
            if (!HiddenGroups.Contains(group))
                HiddenGroups.Add(group);
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveHideGroup(string group)
        {
#if UNITY_EDITOR
            if (HiddenGroups.Contains(group))
                HiddenGroups.Remove(group);
            EditorUtility.SetDirty(this);
#endif
        }

        public bool GroupIsHidden(string group)
        {
            return HiddenGroups.Contains(group);
        }

        public void ChangeUIEnable()
        {
            var info = GetChildByNameAlsoHidden("Info");
            if (UIEnabledOnStart && Application.isPlaying)
            {
                if (RuntimeAutomationUI != null)
                    RuntimeAutomationUI.SetActive(true);
                if (RuntimeApplicationUI != null)
                {
                    var toolbar = Global.GetComponentByName<Transform>(RuntimeApplicationUI, "Toolbar");
                }

                if (HideInfoBox)
                    if (info != null)
                        info.SetActive(false);
            }
            else
            {
                if (RuntimeAutomationUI != null)
                    RuntimeAutomationUI.SetActive(false);
                if (Application.isPlaying)
                {
                    if (RuntimeApplicationUI != null)
                    {
                        var toolbar = Global.GetComponentByName<Transform>(RuntimeApplicationUI, "Toolbar");
                        Global.SetActiveIncludingSubObjects(toolbar.gameObject, false);
                    }
                }

                if (!Application.isPlaying)
                {
                    if (info != null)
                        info.SetActive(false);
                }
            }
        }

        public void MessageBox(string message, bool autoclose, float closeafterseconds)
        {
            if (_uimessages == null)
            {
                return;
            }

            var uimessage = (GameObject)Instantiate(UnityEngine.Resources.Load<GameObject>("UIMessageBox"));
            uimessage.name = "MessageBox";
            uimessage.transform.localScale = new Vector3(1, 1, 1);
            uimessage.transform.SetParent(_uimessages.transform);
            UIMessageBox messageobj = uimessage.GetComponent<UIMessageBox>();
            messageobj.DisplayMessage(message, autoclose, closeafterseconds);
        }

        public int GetMUID(GameObject caller)
        {
            _lastmuid++;
            return _lastmuid;
        }

#if UNITY_EDITOR


        public void SetVisible(GameObject target, bool isActive)
        {
            Global.SetVisible(target, isActive);
        }

        public void UpdateAllLockedAndHidden()
        {
            foreach (var obj in LockedObjects.ToArray())
            {
                SetLockObject(obj, true);
            }
        }

        public void SetLockObject(GameObject target, bool isLocked)
        {
            Global.SetLockObject(target, isLocked);
            Undo.IncrementCurrentGroup();

            if (Selection.objects.Length > 1)
                foreach (var obj in Selection.objects)
                {
                    if (obj.GetType() == typeof(GameObject))
                    {
                        if (obj != target)
                            SetLockObject((GameObject)obj, isLocked);
                    }
                }
        }


        public void ResetView()
        {
            var objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in objs)
            {
                HideSubObjects(obj, false);
                Global.SetExpandedRecursive(obj, false);
                Global.SetLockObject(obj, false);
            }

            LockedObjects = new List<GameObject>();
        }

        public void SetSimpleView(bool simple, bool expanded)
        {
            var objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            if (simple)
            {
                foreach (var obj in objs)
                {
                    HideSubObjects(obj, true);
                }
            }

            if (!expanded)
            {
                foreach (var obj in objs)
                {
                    if (obj != this)
                        Global.SetExpandedRecursive(obj, false);
                }
            }
        }

        public void HideSubObjects(GameObject target, bool hide)
        {
            Global.HideSubObjects(target, hide);
        }

#endif

 
        public void BreakTriggeredByStep(rvUIToolbarButton breakbutton)
        {
            breakbutton.SetStatus(true);
        }

       

        public void ChangedTimeScale()
        {
            Time.timeScale = TimeScale;
        }
        
        
        private void UpdateInterfaceButtonStatus()
        {
            var button = GetChildByNameAlsoHidden("Connected");

            if (ConnectionsActive.Count > 0)
            {
                if (button != null)
                {
                    var rvbutton = button.GetComponent<rvUIToolbarButton>();
                    rvbutton.SetColor(Color.green);
                }
            }
            else
            {
                if (button != null)
                {
                    var rvbutton = button.GetComponent<rvUIToolbarButton>();
                    rvbutton.SetColor(Color.white);
                }
            }
        }

        public void OnUIButtonPressed(GameObject Button)
        {
            var buttonname = Button.name;
            var buttonpressed = false;
            if (Button.GetComponent<rvUIToolbarButton>() != null)
            {
                buttonpressed = Button.GetComponent<rvUIToolbarButton>().IsOn;
            }

            var genericbutton = Button.GetComponent<rvUIToolbarButton>();
            var ison = false;
            if (genericbutton != null)
                ison = genericbutton.IsOn;

            switch (buttonname)
            {
                case "Play":
                {
                    var insp = GetComponentInChildren<RuntimeHierarchy>();
                    if (insp != null)
                    {
                        insp.SceneReloaded();
                    }

                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    Pause();
                    break;
                }
                case "Pause":
                    if (buttonpressed)
                    {
                        Pause();
                    }
                    else
                    {
                        if (_stepwise)
                        {
                            _stepwise = false;
                        }

                        Play();
                    }

                    break;
                case "Step":
                    if (_stepwise == true)
                    {
                        Play();
                    }

                    _stepwise = true;
                    Invoke("Pause", 0.1F);
                    break;
                case "ObjectSelection":
                {
                    if (currentSelectionRaycast == null)
                        return;
                    if (buttonpressed)
                    {
                        currentSelectionRaycast.IsActive = true;
                    }
                    else
                    {
                        currentSelectionRaycast.IsActive = false;
                    }

                    break;
                }
            }
        }

        public static void BroadcastAll(string fun)
        {
            GameObject[] gos = (GameObject[])GameObject.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None);
            foreach (GameObject go in gos)
            {
                if (go && go.transform.parent == null)
                {
                    try
                    {
                        go.gameObject.BroadcastMessage(fun, SendMessageOptions.DontRequireReceiver);
                    }
                    catch
                        (Exception e)
                    {
                    }
                }
            }
        }

        private bool IsSceneLoaded(string scenename)
        {
            var loadedscenes = SceneManager.sceneCount;
            for (int i = 0; i < loadedscenes; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == scenename)
                    return true;
            }

            return false;
        }
       

        private void LoadAdditiveScenes()
        {
            totalscenestoload = AdditiveScenes.Count+1;
            scenesloaded = 0;
            if (DebugMode) Logger.Log("Load Additive Scenes");
            
            // write out all loaded scenes
            var loadedscenes = SceneManager.sceneCount;
         
            if (AdditiveScenes != null)
            {
                foreach (var scene in AdditiveScenes)
                {
                    try
                    {
                        if (AdditiveLoadScenes)
                        {
                            var scenePath = scene; // Assuming `scene` is the path to the scene
                            var loadedScene = SceneManager.GetSceneByPath(scenePath);
                            var scenename = loadedScene.name;

                            if (!Application.isPlaying)
                            {
                                if (!loadedScene.isLoaded)
                                {
                                    #if UNITY_EDITOR
                                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                                    if (DebugMode) Logger.Message("Additive Scene " + scene + " loaded in Editor", this);
                                    #endif
                                }
                            }
                            else
                            {
                                // Fix: Check if the scene is valid before checking if it's loaded
                                if (!IsSceneLoaded(scenename))
                                {
                                    SceneManager.LoadScene(scenePath, LoadSceneMode.Additive);
                                    if (DebugMode) Logger.Message("Additive Scene " + scenePath + " loaded in Playmode", this);
                                }
                                else
                                {
                                    if (DebugMode) Logger.Message("Scene " + scenePath + " was already loaded in Editor", this);
                                }

                            }
                        }
                        else
                        {
#if UNITY_EDITOR
                            var scenePath = scene; // Assuming `scene` is the path to the scene
                            var loadedScene = SceneManager.GetSceneByPath(scenePath);

                            if (loadedScene.isLoaded)
                            {
                                EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetSceneByPath(scene));
                            }
#endif
                        }
                    }
                    catch
                    {
                    }

                    // get the realvirtualController in the loaded scene
                    var controllers =
                        UnityEngine.Object.FindObjectsByType<realvirtualController>(FindObjectsSortMode.None);
                    // disable the gameobject
                    foreach (var controller in controllers)
                    {
                        if (controller.gameObject.scene != this.gameObject.scene)
                        {
                            controller.gameObject.SetActive(false);
                        }
                    }
                    
                }
            }
        }



        public void RemoveMeshGizmo(meshGizmo lasthovered)
        {
            EditorGizmoSettings.SelectedMeshes.Remove(lasthovered);
        }

        public void ResetSelectedMeshes()
        {
            if (EditorGizmoSettings != null)
            {
                EditorGizmoSettings.SelectedMeshes.Clear();
#if UNITY_EDITOR
                EditorUtility.SetDirty(EditorGizmoSettings);
#endif
            }
        }

        public void SetMeshGizmoToSelectedMesh(meshGizmo clickedMesh, bool second)
        {
            if (!second)
                clickedMesh.MeshColor = EditorGizmoSettings.FirstSelectedMeshColor;
            else
            {
                clickedMesh.MeshColor = EditorGizmoSettings.SecondSelectedMeshColor;
            }
        }

        public meshGizmo signalGizmoMesh(GameObject obj, float pivotSize, Color meshColor, bool drawpivot,
            bool drawcenter)
        {
            var mesh = obj.GetComponent<MeshFilter>();
            var meshGizmo = new meshGizmo();
            if (mesh != null)
            {
                meshGizmo.meshFilterList.Add(mesh);
            }
            else
            {
                var meshes = obj.GetComponentsInChildren<MeshFilter>();
                foreach (var childMesh in meshes)
                {
                    meshGizmo.meshFilterList.Add(childMesh);
                }
            }

            meshGizmo.mainGO = obj;
            meshGizmo.pivotSize = pivotSize;
            meshGizmo.MeshColor = meshColor;
            meshGizmo.DrawMeshCenter = drawcenter;
            meshGizmo.DrawMeshPivot = drawpivot;
            EditorGizmoSettings.SelectedMeshes.Add(meshGizmo);
            return meshGizmo;
        }

        public bool CheckIfMeshIsHovered(GameObject obj)
        {
            var Result = false;
            var i = 0;
            while (!Result && i < EditorGizmoSettings.SelectedMeshes.Count)
            {
                if (EditorGizmoSettings.SelectedMeshes[i].mainGO == obj)
                {
                    Result = true;
                }

                i++;
            }

            return Result;
        }

        public EditorGizmoOptions GetGizmoOptions()
        {
#if UNITY_EDITOR
            if (EditorGizmoSettings == null)
            {
                //get a default setting in an another Folder
                EditorGizmoSettings = AssetDatabase.LoadAssetAtPath<EditorGizmoOptions>(
                    "Assets/realvirtual/Settings/EditorGizmoOptionsDefalut.asset");
            }

            return EditorGizmoSettings;
#endif
#if !UNITY_EDITOR
            return null;
#endif
        }

        #endregion

        #region PrivateMethods

        protected new bool hideactiveonly()
        {
            return true;
        }


        private void HideGroupsOnStart()
        {
            foreach (var group in HideGroups)
            {
                var elements = GetAllWithGroup(group);

                foreach (var element in elements)
                {
                    element.gameObject.SetActive(false);
                }
            }

            foreach (var group in HiddenGroups)
            {
                Logger.Message("Group hidden " + group, this);
            }
        }

        private void RestartScene()
        {
            if (DebugMode) Logger.Message("<color=red>---------------- INIT Restarting Scene</color>");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Pause();
        }

        void Start()
        {
            if (!Application.isPlaying)
                return;
            Time.timeScale = 1;
            InvokeRepeating("UpdateHierarchy", 1.0F, HierarchyUpdateCycle);
            var obj = GameObject.Find("__MonoContext__");

            Object.DestroyImmediate(obj);
#if CINEMACHINE
            if (StartWithCinemachineCamera != null)
                SetCinemachineCamera(StartWithCinemachineCamera);
#endif

            if (Restart)
                if (RestartSceneAfterSeconds > 0)
                    Invoke("RestartScene", RestartSceneAfterSeconds);

            StartSim();
#if UNITY_EDITOR
            if (ModelCheckerEnabled)
                ModelChecker.Init();
#endif
        }

        private void UpdateHierarchy()
        {
#if UNITY_EDITOR
            EditorApplication.RepaintHierarchyWindow();
#endif
        }

        public void UpdateSignals()
        {
            /// Clear Info on all Signals
            var signals = FindObjectsByType<Signal>(FindObjectsSortMode.None);
            foreach (var signal in signals)
            {
                signal.DeleteSignalConnectionInfos();
            }

            /// get all Behavior models
            var behaviors = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISignalInterface>();
            foreach (var behavior in behaviors)
            {
                // now get all signals in behaviors
                var connections = behavior.GetConnections();
                foreach (var info in connections)
                {
                    if (info.Signal != null)
                    {
                        info.Signal.AddSignalConnectionInfo(behavior.gameObject
                            , info.Name);
                    }
                }
            }
        }

        void Reset()
        {
            Logger.Message("Reset", this);
            LockedObjects = new List<GameObject>();
            HiddenGroups = new List<string>();
        }

        public void UpdateConnectionButton()
        {
            var button = GetChildByName("Connected");

            if (button != null)
            {
                _buttonconnection = button.GetComponent<rvUIToolbarButton>();
                if (_buttonconnection != null)
                    _buttonconnection.SetStatus(Connected);
            }
        }


        

        private void CeckSignalUpdate()
        {
            if (!Application.isPlaying)
            {
                if (DateTime.Now - _lastupdatesingals > TimeSpan.FromSeconds(3))
                {
                    UpdateSignals();
                    _lastupdatesingals = System.DateTime.Now;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            var center = new Vector3(0, 0, 0);
            var rotation = Quaternion.identity;
            if (EditorGizmoSettings != null)
            {
                if (EditorGizmoSettings.SelectedMeshes.Count == 0)
                    return;
                foreach (meshGizmo meshGizmo in EditorGizmoSettings.SelectedMeshes)
                {
                    var size = meshGizmo.pivotSize;
                    center = meshGizmo.mainGO.transform.position;

                    foreach (var mesh in meshGizmo.meshFilterList)
                    {
                        Gizmos.color = meshGizmo.MeshColor;
                        Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.gameObject.transform.position,
                            mesh.gameObject.transform.rotation,
                            mesh.gameObject.transform.lossyScale);
                        if (meshGizmo.DrawMeshCenter)
                        {
                            var boundscenterlocal = mesh.sharedMesh.bounds.center;
                            // transform to global position
                            var boundscenter = mesh.transform.TransformPoint(boundscenterlocal);

                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(boundscenter - mesh.transform.forward * size,
                                boundscenter + mesh.transform.forward * size);
                            Gizmos.DrawLine(boundscenter - mesh.transform.up * size,
                                boundscenter + mesh.transform.up * size);
                            Gizmos.DrawLine(boundscenter - mesh.transform.right * size,
                                boundscenter + mesh.transform.right * size);
                        }
                    }

                    if (meshGizmo.DrawMeshPivot)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(center - meshGizmo.mainGO.transform.forward * size,
                            center + meshGizmo.mainGO.transform.forward * size);
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(center - meshGizmo.mainGO.transform.up * size,
                            center + meshGizmo.mainGO.transform.up * size);
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(center - meshGizmo.mainGO.transform.right * size,
                            center + meshGizmo.mainGO.transform.right * size);
                    }
                }
            }
        }


        void Update()
        {
            //   QuickToggle.SetGame4Automation(this);
            if (Application.isPlaying)
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                   // Quit();
                }

                if (Input.GetKeyDown(KeyCode.F12))
                {
                    if (_debugconsole != null)
                    {
                        _debugconsole.SetActive(!_debugconsole.activeSelf);
                    }
                }
            }
        }

        static void AddComponent(string assetpath)
        {
#if UNITY_EDITOR
            GameObject component = Selection.activeGameObject;
            Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = new Vector3(0, 0, 0);
            if (component != null)
            {
                go.transform.parent = component.transform;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
#endif
        }

        #endregion

        //! Called when a scene is loaded to perform initialization tasks.
        //! IMPLEMENTS ISceneLoaded::OnSceneLoaded
        public void OnSceneLoaded()
        {
            if (DebugMode) Logger.Log("realvirtual Controller Scene loaded: " + SceneManager.GetActiveScene().name);
            Scene activeScene = SceneManager.GetActiveScene();

            // Check if this is the first loaded scene
            if (SceneManager.sceneCount > 0 && SceneManager.GetSceneAt(0) == activeScene)
            {
                LoadAdditiveScenes();
            }
        }
    }
}