// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace realvirtual
{
    //! Complete QuickEdit overlay for all QuickEdit functionality in Scene view
    [Overlay(typeof(SceneView), "realvirtual Quick Edit", true)]
    [Icon("Icons/button-0local")]
    public partial class QuickEditOverlay : Overlay
    {
        private VisualElement root;
        internal VisualElement buttonContainer;
        
        // Static event for visibility changes
        #pragma warning disable CS0067 // The event is never used
        public static event System.Action<bool> OnVisibilityChangeRequested;
        #pragma warning restore CS0067
        
        // Static method to request visibility change from external code
        public static void RequestVisibilityChange(bool visible)
        {
            OnVisibilityChangeRequested?.Invoke(visible);
        }
        
        // Legacy compatibility - event for drawing additional UI
        public static event Action OnQuickEditDraw;
        
        // Legacy compatibility - properties for backward compatibility
        public static GameObject obj => Selection.activeGameObject;
        public static bool noselection => Selection.activeGameObject == null;
        
        // Legacy GUI styles for backward compatibility
        private static GUIStyle _w;
        private static GUIStyle _w2;
        
        public static GUIStyle w 
        { 
            get 
            {
                if (_w == null)
                {
                    _w = new GUIStyle(EditorStyles.miniButton);
                    _w.margin = new RectOffset(2, 2, 2, 2);
                }
                return _w;
            }
        }
        
        public static GUIStyle w2
        {
            get
            {
                if (_w2 == null)
                {
                    _w2 = new GUIStyle(EditorStyles.miniButton);
                    _w2.margin = new RectOffset(2, 2, 2, 2);
                    _w2.fixedWidth = 120;
                }
                return _w2;
            }
        }
        
        // Method to trigger legacy drawing
        public static void InvokeLegacyDraw()
        {
            OnQuickEditDraw?.Invoke();
        }
        
        // Icon references
        private static Texture icon0local, icon0global, iconpivot;
        private static Texture iconrotxplus, iconrotyplus, iconrotzplus;
        private static Texture iconrotxminus, iconrotyminus, iconrotzminus;
        private static Texture iconempty, icontoempty;
        private static Texture icondrive, iconkinematic, iconaxis;
        private static Texture iconinbool, iconinfloat, iconinint;
        private static Texture iconoutbool, iconoutfloat, iconoutint;
        
        // Play mode controls
        private Slider timeScaleSlider;
        private Label timeScaleLabel;
        private Label timeLabel;
        private Slider speedOverrideSlider;
        private Label speedOverrideLabel;
        private VisualElement drivesContainer;
        
        // Preset buttons
        private List<Button> timeScalePresetButtons = new List<Button>();
        private List<Button> speedPresetButtons = new List<Button>();
        
        // Field references for focus tracking
        private FloatField currentSpeedField;
        
        // State
        internal static List<Drive> drives;
        private static Drive joggingDrive;
        
        // Pivot reference and speed override (moved from old QuickEdit)
        private static GameObject pivotReference;
        public static GameObject PivotReference 
        { 
            get { return pivotReference; } 
            set { pivotReference = value; }
        }
        public static float speedoverride = 1;
        
        // Helper to check if running in window mode
        private static bool IsWindowMode()
        {
            // Check if any QuickEditWindow is open
            var windows = global::UnityEngine.Resources.FindObjectsOfTypeAll<QuickEditWindow>();
            return windows != null && windows.Length > 0;
        }
        
        // Add menu item to toggle between overlay and window
        // [MenuItem("Window/realvirtual/Toggle Quick Edit Mode")] // Moved to main realvirtual menu
        public static void ToggleQuickEditMode()
        {
            bool useOverlay = EditorPrefs.GetBool("realvirtual_UseQuickEditOverlay", true);
            
            if (useOverlay)
            {
                // Switch to window mode
                EditorPrefs.SetBool("realvirtual_UseQuickEditOverlay", false);
                EditorPrefs.SetBool("realvirtual_QuickEditVisible", false);
                
                // Synchronize with menu system and Global state
                Global.QuickEditDisplay = false;
                EditorPrefs.SetBool(QuickEditMenuItem.MenuName, false);
                UnityEditor.Menu.SetChecked(QuickEditMenuItem.MenuName, false);
                
                // Close the overlay
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    Overlay quickEditOverlay;
                    if (sceneView.TryGetOverlay("realvirtual Quick Edit", out quickEditOverlay))
                    {
                        quickEditOverlay.displayed = false;
                    }
                }
                
                // Show the window
                QuickEditWindow.ShowWindow();
                
                Logger.Message("Quick Edit switched to dockable window mode", null);
            }
            else
            {
                // Switch to overlay mode
                EditorPrefs.SetBool("realvirtual_UseQuickEditOverlay", true);
                EditorPrefs.SetBool("realvirtual_QuickEditVisible", true);
                
                // Synchronize with menu system and Global state
                Global.QuickEditDisplay = true;
                EditorPrefs.SetBool(QuickEditMenuItem.MenuName, true);
                UnityEditor.Menu.SetChecked(QuickEditMenuItem.MenuName, true);
                
                // Close any open windows
                var window = EditorWindow.GetWindow<QuickEditWindow>(false);
                if (window != null)
                    window.Close();
                
                // Request overlay visibility
                OnVisibilityChangeRequested?.Invoke(true);
                
                Logger.Message("Quick Edit switched to overlay mode", null);
            }
        }
        
        public override VisualElement CreatePanelContent()
        {
            root = new VisualElement();
            root.AddToClassList("quickedit-root");
            
            // Load the stylesheet
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/realvirtual/private/Editor/QuickEdit/QuickEdit.uss");
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);
            
            LoadIcons();
            
            buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("quickedit-button-container");
            root.Add(buttonContainer);
            
            UpdateContent();
            
            // Subscribe to events
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            
            // Always subscribe to update for warning display
            EditorApplication.update += OnEditorUpdate;
            
            // DISABLED: No longer forcing visibility - user can control via toolbar
            // EditorApplication.update += CheckVisibility;
            
            // Subscribe to F1 key detection
            SceneView.duringSceneGui += OnSceneGUI;
            
            // Subscribe to static visibility change event
            OnVisibilityChangeRequested += HandleVisibilityChangeRequest;
            
            return root;
        }
        
        private void CheckVisibility()
        {
            bool shouldBeVisible = EditorPrefs.GetBool("realvirtual_QuickEditVisible", true);
            if (displayed != shouldBeVisible)
            {
                displayed = shouldBeVisible;
                
                // If showing, also expand the overlay
                if (shouldBeVisible && collapsed)
                {
                    collapsed = false;
                }
                
                // Force the overlay canvas to refresh
                if (containerWindow != null && containerWindow is SceneView sceneView)
                {
                    sceneView.Repaint();
                    
                    // Also try to force the overlay to rebuild its content
                    EditorApplication.delayCall += () =>
                    {
                        if (root != null)
                        {
                            UpdateContent();
                        }
                    };
                }
            }
        }
        
        private void HandleVisibilityChangeRequest(bool visible)
        {
            // Immediate response to visibility change request
            displayed = visible;
            
            // If showing, also expand the overlay
            if (visible && collapsed)
            {
                collapsed = false;
            }
            
            // Force immediate update
            if (containerWindow != null && containerWindow is SceneView sceneView)
            {
                sceneView.Repaint();
            }
            
            // Update content if becoming visible
            if (visible && root != null)
            {
                UpdateContent();
            }
        }
        
        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
            // EditorApplication.update -= CheckVisibility; // DISABLED: No longer forcing visibility
            SceneView.duringSceneGui -= OnSceneGUI;
            OnVisibilityChangeRequested -= HandleVisibilityChangeRequest;
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.F1)
            {
                // Toggle QuickEdit overlay visibility with F1
                displayed = !displayed;
                if (displayed)
                {
                    collapsed = false;
                }
                e.Use(); // Consume the event
            }
        }
        
        internal void LoadIcons()
        {
            icon0local = UnityEngine.Resources.Load<Texture>("Icons/button-0local");
            icon0global = UnityEngine.Resources.Load<Texture>("Icons/button-0global");
            iconpivot = UnityEngine.Resources.Load<Texture>("Icons/button-pivot");
            iconrotxplus = UnityEngine.Resources.Load<Texture>("Icons/button-xplus");
            iconrotyplus = UnityEngine.Resources.Load<Texture>("Icons/button-yplus");
            iconrotzplus = UnityEngine.Resources.Load<Texture>("Icons/button-zplus");
            iconrotxminus = UnityEngine.Resources.Load<Texture>("Icons/button-xminus");
            iconrotyminus = UnityEngine.Resources.Load<Texture>("Icons/button-yminus");
            iconrotzminus = UnityEngine.Resources.Load<Texture>("Icons/button-zminus");
            iconempty = UnityEngine.Resources.Load<Texture>("Icons/button-empty");
            icontoempty = UnityEngine.Resources.Load<Texture>("Icons/button-toempty");
            icondrive = UnityEngine.Resources.Load<Texture>("Icons/button-drive");
            iconkinematic = UnityEngine.Resources.Load<Texture>("Icons/button-kinematic");
            iconaxis = UnityEngine.Resources.Load<Texture>("Icons/button-axis");
            iconinbool = UnityEngine.Resources.Load<Texture>("Icons/button-inputbool");
            iconinfloat = UnityEngine.Resources.Load<Texture>("Icons/button-inputfloat");
            iconinint = UnityEngine.Resources.Load<Texture>("Icons/button-inputint");
            iconoutbool = UnityEngine.Resources.Load<Texture>("Icons/button-outputbool");
            iconoutfloat = UnityEngine.Resources.Load<Texture>("Icons/button-outputfloat");
            iconoutint = UnityEngine.Resources.Load<Texture>("Icons/button-outputint");
        }
        
        private void OnSelectionChanged()
        {
            UpdateContent();
        }
        
        private void OnHierarchyChanged()
        {
            // Update content when hierarchy changes (e.g., adding/removing drives)
            UpdateContent();
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                drives = Global.GetAllSceneComponents<Drive>();
            }
            UpdateContent();
        }
        
        private void OnEditorUpdate()
        {
            if (Application.isPlaying)
            {
                // Update time display
                if (timeLabel != null)
                    timeLabel.text = Time.time.ToString("0.0");
                    
                // Update drives display
                UpdateDrivesDisplay();
            }
            else
            {
                // Update jog button states in edit mode
                UpdateJogButtonStates();
            }
        }
        
        internal void UpdateContent()
        {
            buttonContainer.Clear();
            currentSpeedField = null; // Clear field reference
            
            if (Application.isPlaying)
            {
                CreatePlayModeUI();
            }
            else
            {
                CreateEditModeUI();
            }
        }
        
        private void CreateEditModeUI()
        {
            var context = new QuickEditContext(Selection.activeGameObject);
            
            // Check if realvirtualController is selected
            if (context.Controller != null)
            {
                CreateControllerUI(context.Controller);
                return;
            }
            
            // Adjust context for signal selection
            if (context.HasSignal && context.SelectedObject?.transform.parent != null)
            {
                context = new QuickEditContext(context.SelectedObject.transform.parent.gameObject);
            }
            
            // Create UI sections based on context
            CreateTransformSection(context);
            CreateObjectCreationSection(context);
            CreateComponentSection(context);
            CreateSignalSection(context);
            CreateDriveControlsSection(context);
            
            // Add legacy IMGUI support
            AddLegacyIMGUISupport();
        }
        
        private void CreateTransformSection(QuickEditContext context)
        {
            if (!context.HasSelection)
            {
                // Show "No Selection" header with mode switch
                var headerRow = new VisualElement();
                headerRow.style.flexDirection = FlexDirection.Row;
                headerRow.style.justifyContent = Justify.SpaceBetween;
                headerRow.style.alignItems = Align.Center;
                headerRow.style.marginBottom = 4;
                headerRow.style.width = 238;
                
                var label = new Label("No Selection");
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                label.style.color = new Color(0.7f, 0.7f, 0.7f);
                label.style.marginLeft = 2;
                headerRow.Add(label);
                
                var switchButton = new Button(() => ToggleQuickEditMode());
                switchButton.text = IsWindowMode() ? "Overlay" : "Window";
                switchButton.tooltip = IsWindowMode() ? "Switch to overlay mode in Scene view" : "Switch to dockable window mode";
                switchButton.style.fontSize = 9;
                switchButton.style.height = 18;
                switchButton.style.paddingTop = 1;
                switchButton.style.paddingBottom = 1;
                switchButton.style.paddingLeft = 6;
                switchButton.style.paddingRight = 6;
                headerRow.Add(switchButton);
                
                buttonContainer.Add(headerRow);
                return;
            }
            
            if (QuickEditVisibility.TransformButtons.IsVisible(context))
            {
                    // Object name and mode switch on same line
                    var headerRow = new VisualElement();
                    headerRow.style.flexDirection = FlexDirection.Row;
                    headerRow.style.justifyContent = Justify.SpaceBetween;
                    headerRow.style.alignItems = Align.Center;
                    headerRow.style.marginBottom = 3;
                    headerRow.style.width = 243; // Match button row width
                    
                    // Truncate long names to ensure button stays visible
                    string displayName = context.SelectedObject.name;
                    const int maxNameLength = 20;
                    if (displayName.Length > maxNameLength)
                    {
                        displayName = displayName.Substring(0, maxNameLength - 3) + "...";
                    }
                    
                    var label = new Label(displayName);
                    label.tooltip = context.SelectedObject.name; // Show full name in tooltip
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    label.style.marginLeft = 0; // No extra margin needed
                    label.style.overflow = Overflow.Hidden;
                    label.style.textOverflow = TextOverflow.Ellipsis;
                    label.style.maxWidth = 160; // Ensure space for button
                    headerRow.Add(label);
                    
                    var switchButton = new Button(() => {
                        ToggleQuickEditMode();
                    });
                    switchButton.text = IsWindowMode() ? "Overlay" : "Window";
                    switchButton.tooltip = IsWindowMode() ? "Switch to overlay mode in Scene view" : "Switch to dockable window mode";
                    switchButton.style.fontSize = 9;
                    switchButton.style.height = 18;
                    switchButton.style.paddingTop = 1;
                    switchButton.style.paddingBottom = 1;
                    switchButton.style.paddingLeft = 6;
                    switchButton.style.paddingRight = 6;
                    headerRow.Add(switchButton);
                    
                    buttonContainer.Add(headerRow);
                    
                    // Transform position row
                    var transformRow = CreateButtonRow();
                    transformRow.Add(CreateIconButton(icon0local, "Zero Local Position", () => ZeroLocal(), "quickedit-button-transform"));
                    transformRow.Add(CreateIconButton(icon0global, "Zero Global Position", () => ZeroGlobal(), "quickedit-button-transform"));
                    
                    var pivotBtn = CreateIconButton(iconpivot, "Align Pivot (Pro)", () => AlignPivot(), "quickedit-button-transform");
                    if (PivotReference != null)
                        pivotBtn.AddToClassList("quickedit-button-active");
                    transformRow.Add(pivotBtn);
                    
                    buttonContainer.Add(transformRow);
                    
                    // Rotation buttons - check visibility rule
                    if (QuickEditVisibility.RotationButtons.IsVisible(context))
                    {
                        // Plus rotation row (with Shift for minus)
                        var rotPlusRow = CreateButtonRow();
                        rotPlusRow.Add(CreateIconButton(iconrotxplus, 
                            "Rotate +90° X\nShift+Click: Rotate -90° X", 
                            () => {
                                if (Event.current != null && Event.current.shift)
                                    Rotation(new Vector3(-90, 0, 0));
                                else
                                    Rotation(new Vector3(90, 0, 0));
                            }, 
                            "quickedit-button-rotation"));
                        rotPlusRow.Add(CreateIconButton(iconrotyplus, 
                            "Rotate +90° Y\nShift+Click: Rotate -90° Y", 
                            () => {
                                if (Event.current != null && Event.current.shift)
                                    Rotation(new Vector3(0, -90, 0));
                                else
                                    Rotation(new Vector3(0, 90, 0));
                            }, 
                            "quickedit-button-rotation"));
                        rotPlusRow.Add(CreateIconButton(iconrotzplus, 
                            "Rotate +90° Z\nShift+Click: Rotate -90° Z", 
                            () => {
                                if (Event.current != null && Event.current.shift)
                                    Rotation(new Vector3(0, 0, -90));
                                else
                                    Rotation(new Vector3(0, 0, 90));
                            }, 
                            "quickedit-button-rotation"));
                        buttonContainer.Add(rotPlusRow);
                    }
                    
                    // To Ground, Pivot to Bottom, and Align Y Up buttons - always visible for non-signal objects
                    var groundRow = CreateButtonRow();
                    
                    var groundBtn = CreateTextButton("To Ground", 
                        "Place object on Y=0 based on its bounding box bottom", 
                        () => PlaceOnGround(context.SelectedObject));
                    groundRow.Add(groundBtn);
                    
                    var pivotBottomBtn = CreateTextButton("Pivot to Bottom", 
                        "Move pivot to bottom center of bounding box without moving children", 
                        () => PivotToY0(context.SelectedObject));
                    groundRow.Add(pivotBottomBtn);
                    
                    var alignYBtn = CreateTextButton("Align Y Up", 
                        "Align object's Y axis to world up without moving children", 
                        () => AlignYUp(context.SelectedObject));
                    groundRow.Add(alignYBtn);
                    
                    buttonContainer.Add(groundRow);
                }
        }
        
        private void CreateObjectCreationSection(QuickEditContext context)
        {
            
            // Object creation row (always visible)
            AddSeparator();
            var creationRow = CreateButtonRow();
            creationRow.Add(CreateIconButton(iconempty, "Create Empty GameObject", () => NewEmpty(), "quickedit-button-create", true));
            
            var intoEmptyBtn = CreateIconButton(icontoempty, "Group Selection into Empty", () => IntoEmpty(), "quickedit-button-create", true);
            if (!context.HasSelection)
                intoEmptyBtn.SetEnabled(false);
            creationRow.Add(intoEmptyBtn);
            
            buttonContainer.Add(creationRow);
            
            // Empty at Root button - full width
            var emptyAtRootRow = CreateButtonRow();
            var emptyAtRootBtn = CreateTextButton("Empty at Root", 
                "Create an empty GameObject at the root level of the hierarchy", 
                () => NewEmptyAtRoot());
            emptyAtRootBtn.style.flexGrow = 1; // Make button fill the entire row
            emptyAtRootRow.Add(emptyAtRootBtn);
            buttonContainer.Add(emptyAtRootRow);
            
            // Check if selected object is the root of a prefab instance and add unpack button
            if (context.HasSelection && context.SelectedObject != null)
            {
                // Check if this is the root of a prefab instance
                var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(context.SelectedObject);
                var isRootOfPrefab = PrefabUtility.IsOutermostPrefabInstanceRoot(context.SelectedObject);
                
                if (isRootOfPrefab && prefabStatus == PrefabInstanceStatus.Connected)
                {
                    var prefabRow = CreateButtonRow();
                    var unpackBtn = CreateTextButton("Unpack Prefab", 
                        "Unpack this prefab instance completely, breaking all prefab connections", 
                        () => UnpackPrefabCompletely(context.SelectedObject));
                    unpackBtn.style.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);
                    prefabRow.Add(unpackBtn);
                    buttonContainer.Add(prefabRow);
                }
            }
        }
        
        private void CreateComponentSection(QuickEditContext context)
        {
            // Show kinematic tool when no selection
            if (!context.HasSelection)
            {
                var axisRow = CreateButtonRow();
                var axisBtn = CreateTextButton("Kinematic Tool", "Open Kinematic Tool (Pro)", () => OpenKinematicTool());
                axisRow.Add(axisBtn);
                buttonContainer.Add(axisRow);
                return;
            }
            
            // Check component creation visibility
            if (QuickEditVisibility.ComponentCreationButtons.IsVisible(context))
            {
                AddSeparator();
                
                // Component creation buttons
                var compRow = CreateButtonRow();
                if (QuickEditVisibility.TransportSurfaceButtons.IsVisible(context))
                    compRow.Add(CreateIconButton(iconaxis, "Kinematic Tool (Pro)", () => OpenKinematicTool(), "quickedit-button-component"));
                if (QuickEditVisibility.DriveButtons.IsVisible(context))
                    compRow.Add(CreateIconButton(icondrive, "Add Drive Component", () => AddComponent(typeof(Drive)), "quickedit-button-component"));
                if (QuickEditVisibility.KinematicButtons.IsVisible(context))
                    compRow.Add(CreateIconButton(iconkinematic, "Add Kinematic Component", () => AddComponent(typeof(Kinematic)), "quickedit-button-component"));
                if (compRow.childCount > 0)
                    buttonContainer.Add(compRow);
                
                // Transport and automation components
                var transportRow1 = CreateButtonRow();
                transportRow1.Add(CreateTextButton("Transport Surface", "Add Transport Surface", () => AddComponent(typeof(TransportSurface))));
                transportRow1.Add(CreateTextButton("Sensor", "Add Sensor", () => AddComponent(typeof(Sensor))));
                buttonContainer.Add(transportRow1);
                
                var transportRow2 = CreateButtonRow();
                if (QuickEditVisibility.DriveButtons.IsVisible(context))
                    transportRow2.Add(CreateTextButton("Transport Guided", "Add Transport Guided", () => AddTransportGuided()));
                if (QuickEditVisibility.TransportSurfaceButtons.IsVisible(context))
                    transportRow2.Add(CreateTextButton("Grip", "Add Grip", () => AddComponent(typeof(Grip))));
                if (transportRow2.childCount > 0)
                    buttonContainer.Add(transportRow2);
                
                if (context.HasTransportSurface)
                {
                    var guideRow = CreateButtonRow();
                    guideRow.Add(CreateTextButton("Guide Line", "Add Guide Line", () => AddComponent(typeof(GuideLine))));
                    guideRow.Add(CreateTextButton("Guide Circle", "Add Guide Circle", () => AddComponent(typeof(GuideCircle))));
                    buttonContainer.Add(guideRow);
                }
                
                var miscRow = CreateButtonRow();
                if (QuickEditVisibility.TransportSurfaceButtons.IsVisible(context))
                {
                    miscRow.Add(CreateTextButton("Fixer", "Add Fixer", () => AddComponent(typeof(Fixer))));
                    miscRow.Add(CreateTextButton("Joint", "Add Joint", () => AddComponent(typeof(SimpleJoint))));
                }
                if (miscRow.childCount > 0)
                    buttonContainer.Add(miscRow);
            }
            
            // Drive behaviors
            if (QuickEditVisibility.DriveBehaviors.IsVisible(context))
            {
                        AddSeparator();
                        
                        // Create container with subtle yellow background
                        var driveContainer = new VisualElement();
                        driveContainer.AddToClassList("quickedit-drive-container");
                        
                        var driveRow1 = CreateButtonRow();
                        var simpleDriveBtn = CreateTextButton("Simple Drive", "Add Simple Drive", () => AddComponent(typeof(Drive_Simple)));
                        simpleDriveBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow1.Add(simpleDriveBtn);
                        var cylinderBtn = CreateTextButton("Cylinder", "Add Cylinder Drive", () => AddComponent(typeof(Drive_Cylinder)));
                        cylinderBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow1.Add(cylinderBtn);
                        driveContainer.Add(driveRow1);
                        
                        var driveRow2 = CreateButtonRow();
                        var gearBtn = CreateTextButton("Gear", "Add Gear Drive", () => AddComponent(typeof(Drive_Gear)));
                        gearBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow2.Add(gearBtn);
                        var camBtn = CreateTextButton("CAM", "Add CAM Drive", () => AddComponent(typeof(CAM)));
                        camBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow2.Add(camBtn);
                        driveContainer.Add(driveRow2);
                        
                        var driveRow3 = CreateButtonRow();
                        var followBtn = CreateTextButton("Follow Position", "Add Follow Position", () => AddComponent(typeof(Drive_FollowPosition)));
                        followBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow3.Add(followBtn);
                        var destBtn = CreateTextButton("Destination Drive", "Add Destination Motor", () => AddComponent(typeof(Drive_DestinationMotor)));
                        destBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow3.Add(destBtn);
                        driveContainer.Add(driveRow3);
                        
                        var driveRow4 = CreateButtonRow();
                        var erraticBtn = CreateTextButton("Drive Erratic", "Add Erratic Position", () => AddComponent(typeof(Drive_ErraticPosition)));
                        erraticBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow4.Add(erraticBtn);
                        var speedBtn = CreateTextButton("Drive Speed", "Add Speed Drive", () => AddComponent(typeof(Drive_Speed)));
                        speedBtn.AddToClassList("quickedit-drive-behavior-button");
                        driveRow4.Add(speedBtn);
                        driveContainer.Add(driveRow4);
                        
                        buttonContainer.Add(driveContainer);
            }
            
#if REALVIRTUAL_PROFESSIONAL
            // Logic steps section - simplified version
            if (ShouldShowLogicSteps(context))
            {
                AddSeparator();
                
                // Create container with subtle blue/purple background
                var logicContainer = new VisualElement();
                logicContainer.AddToClassList("quickedit-logic-container");
                
                // Check if we're under a serial container
                var serialContainer = context.SelectedObject?.GetComponentInParent<LogicStep_SerialContainer>();
                
                if (serialContainer != null)
                {
                    // Container buttons first
                    var containerRow = CreateButtonRow();
                    containerRow.Add(CreateTextButton("Serial Container", "Add Serial Container", () => AddLogicStep(typeof(LogicStep_SerialContainer))));
                    containerRow.Add(CreateTextButton("Parallel Container", "Add Parallel Container", () => AddLogicStep(typeof(LogicStep_ParallelContainer))));
                    logicContainer.Add(containerRow);
                    
                    // Show all LogicStep buttons when under a serial container
                    var logicRow1 = CreateButtonRow();
                    logicRow1.Add(CreateTextButton("Drive to", "Add Drive To Step", () => AddLogicStep(typeof(LogicStep_DriveTo))));
                    logicRow1.Add(CreateTextButton("Start Drive", "Add Start Drive Step", () => AddLogicStep(typeof(LogicStep_StartDriveTo))));
                    logicContainer.Add(logicRow1);
                    
                    var logicRow2 = CreateButtonRow();
                    logicRow2.Add(CreateTextButton("Set Signal", "Add Set Signal Step", () => AddLogicStep(typeof(LogicStep_SetSignalBool))));
                    logicRow2.Add(CreateTextButton("Jump", "Add Jump Step", () => AddLogicStep(typeof(LogicStep_JumpOnSignal))));
                    logicContainer.Add(logicRow2);
                    
                    var logicRow3 = CreateButtonRow();
                    logicRow3.Add(CreateTextButton("Wait Sensor", "Add Wait Sensor Step", () => AddLogicStep(typeof(LogicStep_WaitForSensor))));
                    logicRow3.Add(CreateTextButton("Wait Signal", "Add Wait Signal Step", () => AddLogicStep(typeof(LogicStep_WaitForSignalBool))));
                    logicContainer.Add(logicRow3);
                    
                    var logicRow4 = CreateButtonRow();
                    logicRow4.Add(CreateTextButton("Wait Drives", "Add Wait Drives Step", () => AddLogicStep(typeof(LogicStep_WaitForDrivesAtTarget))));
                    logicRow4.Add(CreateTextButton("Delay", "Add Delay Step", () => AddLogicStep(typeof(LogicStep_Delay))));
                    logicContainer.Add(logicRow4);
                }
                else
                {
                    // Only show Serial Container button when there are no other realvirtual components on the GameObject
                    var hasOtherRealvirtualComponents = HasOtherRealvirtualComponents(context.SelectedObject);
                    
                    if (!hasOtherRealvirtualComponents)
                    {
                        var containerRow = CreateButtonRow();
                        containerRow.Add(CreateTextButton("LogicSteps", "Create LogicSteps Container", () => AddLogicStep(typeof(LogicStep_SerialContainer))));
                        logicContainer.Add(containerRow);
                    }
                }
                
                buttonContainer.Add(logicContainer);
            }
#endif
        }
        
        private void CreateSignalSection(QuickEditContext context)
        {
            if (!context.HasSelection) return;
            
            // Signals section
            if (QuickEditVisibility.SignalButtons.IsVisible(context))
            {
                    
                    // Output signals
                    var signalRow1 = CreateButtonRow();
                    signalRow1.Add(CreateIconButton(iconoutbool, "Create Output Bool Signal", () => CreateSignal(typeof(PLCOutputBool)), "quickedit-button-signal-output"));
                    signalRow1.Add(CreateIconButton(iconoutint, "Create Output Int Signal", () => CreateSignal(typeof(PLCOutputInt)), "quickedit-button-signal-output"));
                    signalRow1.Add(CreateIconButton(iconoutfloat, "Create Output Float Signal", () => CreateSignal(typeof(PLCOutputFloat)), "quickedit-button-signal-output"));
                    buttonContainer.Add(signalRow1);
                    
                    // Input signals
                    var signalRow2 = CreateButtonRow();
                    signalRow2.Add(CreateIconButton(iconinbool, "Create Input Bool Signal", () => CreateSignal(typeof(PLCInputBool)), "quickedit-button-signal-input"));
                    signalRow2.Add(CreateIconButton(iconinint, "Create Input Int Signal", () => CreateSignal(typeof(PLCInputInt)), "quickedit-button-signal-input"));
                    signalRow2.Add(CreateIconButton(iconinfloat, "Create Input Float Signal", () => CreateSignal(typeof(PLCInputFloat)), "quickedit-button-signal-input"));
                    buttonContainer.Add(signalRow2);
                    
                
                if (context.HasSignal)
                {
                    var changeRow = CreateButtonRow();
                    changeRow.Add(CreateTextButton("To Bool", "Change to Bool Signal", () => ChangeSignal("bool")));
                    changeRow.Add(CreateTextButton("To Int", "Change to Int Signal", () => ChangeSignal("int")));
                    changeRow.Add(CreateTextButton("To Float", "Change to Float Signal", () => ChangeSignal("float")));
                    buttonContainer.Add(changeRow);
                    
                    var directionRow = CreateButtonRow();
                    directionRow.Add(CreateTextButton("Change Signal Direction", "Toggle Input/Output", () => SignalHierarchyContextMenu.HierarchyChangeSignalDirection()));
                    buttonContainer.Add(directionRow);
                }
            }
        }
        
        private void CreateDriveControlsSection(QuickEditContext context)
        {
            // Drive controls at bottom - only show if exactly one drive is selected
            if (context.HasSelection && context.IsSingleSelection && context.HasDrive)
            {
                AddSeparator();
                CreateDriveJogControls(context.Drive);
            }
        }
        
        private void CreatePlayModeUI()
        {
            // Add playmode container class for compact styling
            buttonContainer.AddToClassList("quickedit-playmode-container");
            
            // Clear preset button lists
            timeScalePresetButtons.Clear();
            speedPresetButtons.Clear();
            
            // Time scale control
            var timeScaleContainer = new VisualElement();
            timeScaleContainer.style.flexDirection = FlexDirection.Column;
            timeScaleContainer.style.marginBottom = 2;
            buttonContainer.Add(timeScaleContainer);
            
            var timeScaleHeader = new VisualElement();
            timeScaleHeader.style.flexDirection = FlexDirection.Row;
            timeScaleHeader.style.justifyContent = Justify.SpaceBetween;
            timeScaleContainer.Add(timeScaleHeader);
            
            var timeScaleTitle = new Label("Timescale");
            timeScaleTitle.style.width = 80;
            timeScaleTitle.AddToClassList("quickedit-playmode-label");
            timeScaleHeader.Add(timeScaleTitle);
            
            timeLabel = new Label(Time.time.ToString("0.0"));
            timeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            timeLabel.style.width = 50;
            timeLabel.AddToClassList("quickedit-playmode-label");
            timeScaleHeader.Add(timeLabel);
            
            var timeScaleControl = new VisualElement();
            timeScaleControl.style.flexDirection = FlexDirection.Row;
            timeScaleControl.style.alignItems = Align.Center;
            timeScaleControl.style.marginBottom = 3; // Match button row spacing
            timeScaleControl.style.width = 243; // Match button row width
            timeScaleContainer.Add(timeScaleControl);
            
            timeScaleSlider = new Slider(0f, 100f);
            timeScaleSlider.value = Time.timeScale;
            timeScaleSlider.AddToClassList("quickedit-playmode-slider");
            timeScaleSlider.RegisterValueChangedCallback(evt => {
                Time.timeScale = evt.newValue;
                timeScaleLabel.text = evt.newValue.ToString("0.0");
                UpdateTimeScalePresetButtons();
            });
            timeScaleControl.Add(timeScaleSlider);
            
            timeScaleLabel = new Label(Time.timeScale.ToString("0.0"));
            timeScaleLabel.AddToClassList("quickedit-playmode-speed-display");
            timeScaleControl.Add(timeScaleLabel);
            
            // Time scale preset buttons - moved to separate row below slider
            var presetRow = CreateButtonRow();
            presetRow.AddToClassList("quickedit-playmode-row");
            var preset01 = CreateTextButton("0.1", "Set timescale to 0.1", () => { timeScaleSlider.value = 0.1f; });
            preset01.AddToClassList("quickedit-playmode-button");
            presetRow.Add(preset01);
            timeScalePresetButtons.Add(preset01);
            
            var preset1 = CreateTextButton("1", "Set timescale to 1", () => { timeScaleSlider.value = 1f; });
            preset1.AddToClassList("quickedit-playmode-button");
            presetRow.Add(preset1);
            timeScalePresetButtons.Add(preset1);
            
            var preset4 = CreateTextButton("4", "Set timescale to 4", () => { timeScaleSlider.value = 4f; });
            preset4.AddToClassList("quickedit-playmode-button");
            presetRow.Add(preset4);
            timeScalePresetButtons.Add(preset4);
            
            var presetMax = CreateTextButton("max", "Set timescale to 100", () => { timeScaleSlider.value = 100f; });
            presetMax.AddToClassList("quickedit-playmode-button");
            presetRow.Add(presetMax);
            timeScalePresetButtons.Add(presetMax);
            
            timeScaleContainer.Add(presetRow); // Add to container, not control
            
            // Update initial button states
            UpdateTimeScalePresetButtons();
            
            // Speed override control
            AddSeparator();
            var speedContainer = new VisualElement();
            speedContainer.style.flexDirection = FlexDirection.Column;
            speedContainer.style.marginBottom = 2;
            buttonContainer.Add(speedContainer);
            
            var speedLabel = new Label("Drive Speed Override");
            speedLabel.style.marginBottom = 2;
            speedLabel.AddToClassList("quickedit-playmode-label");
            speedContainer.Add(speedLabel);
            
            var speedControl = new VisualElement();
            speedControl.style.flexDirection = FlexDirection.Row;
            speedControl.style.alignItems = Align.Center;
            speedControl.style.marginBottom = 3; // Match button row spacing
            speedControl.style.width = 243; // Match button row width
            speedContainer.Add(speedControl);
            
            speedOverrideSlider = new Slider(0f, 10f);
            speedOverrideSlider.value = speedoverride;
            speedOverrideSlider.AddToClassList("quickedit-playmode-slider");
            speedOverrideSlider.RegisterValueChangedCallback(evt => {
                speedoverride = evt.newValue;
                speedOverrideLabel.text = evt.newValue.ToString("0.0");
                UpdateSpeedOverride(evt.newValue);
                UpdateSpeedPresetButtons();
            });
            speedControl.Add(speedOverrideSlider);
            
            speedOverrideLabel = new Label(speedoverride.ToString("0.0"));
            speedOverrideLabel.AddToClassList("quickedit-playmode-speed-display");
            speedControl.Add(speedOverrideLabel);
            
            // Speed preset buttons - moved to separate row below slider
            var speedPresetRow = CreateButtonRow();
            speedPresetRow.AddToClassList("quickedit-playmode-row");
            var speed0 = CreateTextButton("0", "Set speed to 0", () => { speedOverrideSlider.value = 0f; });
            speed0.AddToClassList("quickedit-playmode-button");
            speedPresetRow.Add(speed0);
            speedPresetButtons.Add(speed0);
            
            var speed01 = CreateTextButton("0.1", "Set speed to 0.1", () => { speedOverrideSlider.value = 0.1f; });
            speed01.AddToClassList("quickedit-playmode-button");
            speedPresetRow.Add(speed01);
            speedPresetButtons.Add(speed01);
            
            var speed1 = CreateTextButton("1", "Set speed to 1", () => { speedOverrideSlider.value = 1f; });
            speed1.AddToClassList("quickedit-playmode-button");
            speedPresetRow.Add(speed1);
            speedPresetButtons.Add(speed1);
            
            var speed4 = CreateTextButton("4", "Set speed to 4", () => { speedOverrideSlider.value = 4f; });
            speed4.AddToClassList("quickedit-playmode-button");
            speedPresetRow.Add(speed4);
            speedPresetButtons.Add(speed4);
            
            speedContainer.Add(speedPresetRow); // Add to container, not control
            
            // Update initial button states
            UpdateSpeedPresetButtons();
            
            // Check if a LogicStep is selected and show its visualization after speed controls
            CreatePlayModeLogicStepVisualization();
            
            // Selected drive section - only show if exactly one drive is selected
            var selectedObj = Selection.activeGameObject;
            if (selectedObj != null && Selection.objects.Length == 1)
            {
                var selectedDrive = selectedObj.GetComponent<Drive>();
                if (selectedDrive != null)
                {
                    // Check if drive has active behavior interface
                    var behaviors = selectedDrive.GetComponents<BehaviorInterface>();
                    bool hasActiveBehavior = false;
                    foreach (var behavior in behaviors)
                    {
                        if (behavior.isActiveAndEnabled)
                        {
                            hasActiveBehavior = true;
                            break;
                        }
                    }
                    
                    if (!hasActiveBehavior)
                    {
                        AddSeparator();
                        CreatePlayModeDriveControls(selectedDrive);
                    }
                }
            }
        }
        
        private void CreatePlayModeDriveControls(Drive drive)
        {
            // Drive name and position container
            var driveContainer = new VisualElement();
            driveContainer.style.flexDirection = FlexDirection.Column;
            buttonContainer.Add(driveContainer);
            
            // Drive name and position in one row
            var driveInfoRow = new VisualElement();
            driveInfoRow.style.flexDirection = FlexDirection.Row;
            driveInfoRow.style.justifyContent = Justify.SpaceBetween;
            driveInfoRow.style.alignItems = Align.Center;
            driveInfoRow.style.marginBottom = 6;
            driveInfoRow.style.width = 243; // Match button row width
            driveContainer.Add(driveInfoRow);
            
            // Truncate long drive names to ensure position display stays visible
            string driveName = drive.name;
            const int maxDriveNameLength = 20;
            if (driveName.Length > maxDriveNameLength)
            {
                driveName = driveName.Substring(0, maxDriveNameLength - 3) + "...";
            }
            
            var driveLabel = new Label(driveName);
            driveLabel.tooltip = drive.name; // Show full name in tooltip
            driveLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            driveLabel.AddToClassList("quickedit-playmode-label");
            driveLabel.style.overflow = Overflow.Hidden;
            driveLabel.style.textOverflow = TextOverflow.Ellipsis;
            driveLabel.style.maxWidth = 160; // Ensure space for position display
            driveInfoRow.Add(driveLabel);
            
            // Position display with proper units
            string unit = GetDrivePositionUnit(drive);
            var positionLabel = new Label($"{drive.IsPosition:F1} {unit}");
            positionLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            positionLabel.AddToClassList("quickedit-playmode-label");
            driveInfoRow.Add(positionLabel);
            
            // Update position label during play mode
            EditorApplication.CallbackFunction updatePosition = () => {
                if (drive != null && positionLabel != null)
                {
                    string u = GetDrivePositionUnit(drive);
                    positionLabel.text = $"{drive.IsPosition:F1} {u}";
                }
            };
            EditorApplication.update += updatePosition;
            
            // Jog controls row
            var jogRow = CreateButtonRow();
            jogRow.AddToClassList("quickedit-playmode-row");
            
            // Jog backward button
            var jogBackBtn = new Button();
            jogBackBtn.text = "◀ Back";
            jogBackBtn.AddToClassList("quickedit-jog-button");
            jogBackBtn.style.width = 75;
            jogBackBtn.style.height = 26;
            
            // Jog forward button
            var jogFwdBtn = new Button();
            jogFwdBtn.text = "Fwd ▶";
            jogFwdBtn.AddToClassList("quickedit-jog-button");
            jogFwdBtn.style.width = 75;
            jogFwdBtn.style.height = 26;
            
            // Update initial button states
            if (drive.JogBackward)
                jogBackBtn.AddToClassList("quickedit-drive-button-active");
            if (drive.JogForward)
                jogFwdBtn.AddToClassList("quickedit-drive-button-active");
            
            // Jog backward click callback
            jogBackBtn.clicked += () => {
                // Toggle state
                if (drive.JogBackward)
                {
                    drive.JogBackward = false;
                    jogBackBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
                else
                {
                    drive.JogForward = false; // Ensure forward is off
                    drive.JogBackward = true;
                    jogBackBtn.AddToClassList("quickedit-drive-button-active");
                    jogFwdBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
            };
            
            // Jog forward click callback
            jogFwdBtn.clicked += () => {
                // Toggle state
                if (drive.JogForward)
                {
                    drive.JogForward = false;
                    jogFwdBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
                else
                {
                    drive.JogBackward = false; // Ensure backward is off
                    drive.JogForward = true;
                    jogFwdBtn.AddToClassList("quickedit-drive-button-active");
                    jogBackBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
            };
            
            jogRow.Add(jogBackBtn);
            
            // Stop button
            var stopBtn = CreateTextButton("■ Stop", "Stop jogging", () => {
                drive.JogBackward = false;
                drive.JogForward = false;
                jogBackBtn.RemoveFromClassList("quickedit-drive-button-active");
                jogFwdBtn.RemoveFromClassList("quickedit-drive-button-active");
            });
            stopBtn.AddToClassList("quickedit-jog-button");
            stopBtn.style.width = 75;
            stopBtn.style.height = 26;
            jogRow.Add(stopBtn);
            
            jogRow.Add(jogFwdBtn);
            
            driveContainer.Add(jogRow);
            
            // Store reference to update button states
            drivesContainer = driveContainer;
        }
        
        private void UpdateDrivesDisplay()
        {
            // Update button states for selected drive in play mode
            if (!Application.isPlaying || drivesContainer == null) return;
            
            var selectedObj = Selection.activeGameObject;
            if (selectedObj != null && Selection.objects.Length == 1)
            {
                var drive = selectedObj.GetComponent<Drive>();
                if (drive != null)
                {
                    // Find and update jog button states
                    var buttons = drivesContainer.Query<Button>().ToList();
                    foreach (var button in buttons)
                    {
                        if (button.text == "◀ Back")
                        {
                            if (drive.JogBackward)
                                button.AddToClassList("quickedit-drive-button-active");
                            else
                                button.RemoveFromClassList("quickedit-drive-button-active");
                        }
                        else if (button.text == "Fwd ▶")
                        {
                            if (drive.JogForward)
                                button.AddToClassList("quickedit-drive-button-active");
                            else
                                button.RemoveFromClassList("quickedit-drive-button-active");
                        }
                    }
                }
            }
        }
        
        internal void UpdateJogButtonStates()
        {
            // Update button states for jog controls in edit mode
            if (buttonContainer == null) return;
            
            var selectedObj = Selection.activeGameObject;
            if (selectedObj != null && Selection.objects.Length == 1)
            {
                var drive = selectedObj.GetComponent<Drive>();
                if (drive != null)
                {
                    // Find and update jog button states
                    var buttons = buttonContainer.Query<Button>().ToList();
                    foreach (var button in buttons)
                    {
                        if (button.text == "◀ Back")
                        {
                            if (drive.JogBackward)
                                button.AddToClassList("quickedit-drive-button-active");
                            else
                                button.RemoveFromClassList("quickedit-drive-button-active");
                        }
                        else if (button.text == "Fwd ▶")
                        {
                            if (drive.JogForward)
                                button.AddToClassList("quickedit-drive-button-active");
                            else
                                button.RemoveFromClassList("quickedit-drive-button-active");
                        }
                    }
                    
                    // Update speed field value using stored reference
                    if (currentSpeedField != null)
                    {
                        // Only update if the field is not currently focused (user not typing)
                        bool isFocused = false;
                        
                        // Check if this specific field has focus
                        if (currentSpeedField.panel != null && 
                            currentSpeedField.panel.focusController != null)
                        {
                            var focusedElement = currentSpeedField.panel.focusController.focusedElement;
                            isFocused = focusedElement == currentSpeedField || 
                                      (focusedElement != null && focusedElement is VisualElement ve && currentSpeedField.Contains(ve));
                        }
                        
                        if (!isFocused && Math.Abs(currentSpeedField.value - drive.TargetSpeed) > 0.001f)
                        {
                            currentSpeedField.SetValueWithoutNotify(drive.TargetSpeed);
                        }
                    }
                }
            }
        }
        
        private void UpdateTimeScalePresetButtons()
        {
            float[] presetValues = { 0.1f, 1f, 4f, 100f };
            UpdatePresetButtonStates(timeScalePresetButtons, Time.timeScale, presetValues);
        }
        
        private void UpdateSpeedPresetButtons()
        {
            float[] presetValues = { 0f, 0.1f, 1f, 4f };
            UpdatePresetButtonStates(speedPresetButtons, speedoverride, presetValues);
        }
        
        private void UpdatePresetButtonStates(List<Button> buttons, float currentValue, float[] presetValues)
        {
            for (int i = 0; i < buttons.Count && i < presetValues.Length; i++)
            {
                bool isActive = Mathf.Approximately(currentValue, presetValues[i]);
                
                if (isActive)
                    buttons[i].AddToClassList("quickedit-preset-button-active");
                else
                    buttons[i].RemoveFromClassList("quickedit-preset-button-active");
            }
        }
        
        private void UpdateSpeedOverride(float speed)
        {
            var rootObjects = new List<GameObject>();
            var scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);
            foreach (var obj in rootObjects)
            {
                var controller = obj.GetComponent<realvirtualController>();
                if (controller != null)
                {
                    controller.SpeedOverride = speed;
                    break;
                }
            }
        }
        
        private static string GetDriveSpeedUnit(Drive drive)
        {
            bool isRotational = drive.Direction == DIRECTION.RotationX || 
                               drive.Direction == DIRECTION.RotationY || 
                               drive.Direction == DIRECTION.RotationZ;
            return isRotational ? "°/s" : "mm/s";
        }
        
        private static string GetDrivePositionUnit(Drive drive)
        {
            bool isRotational = drive.Direction == DIRECTION.RotationX || 
                               drive.Direction == DIRECTION.RotationY || 
                               drive.Direction == DIRECTION.RotationZ;
            return isRotational ? "°" : "mm";
        }
        
        private void CreatePlayModeLogicStepVisualization()
        {
            // Check if a LogicStep is selected
            var selectedObj = Selection.activeGameObject;
            if (selectedObj == null || Selection.objects.Length != 1)
                return;
            
            // Get the LogicStep component
            var logicStep = selectedObj.GetComponent<LogicStep>();
            if (logicStep == null)
                return;
            
            // Create container for LogicStep visualization
            var logicStepContainer = new VisualElement();
            logicStepContainer.style.flexDirection = FlexDirection.Column;
            logicStepContainer.style.backgroundColor = new Color(0.3f, 0.3f, 0.5f, 0.1f); // Subtle blue/purple background
            logicStepContainer.style.borderTopLeftRadius = 3;
            logicStepContainer.style.borderTopRightRadius = 3;
            logicStepContainer.style.borderBottomLeftRadius = 3;
            logicStepContainer.style.borderBottomRightRadius = 3;
            logicStepContainer.style.paddingTop = 4;
            logicStepContainer.style.paddingBottom = 4;
            logicStepContainer.style.paddingLeft = 4;
            logicStepContainer.style.paddingRight = 4;
            logicStepContainer.style.marginBottom = 6;
            buttonContainer.Add(logicStepContainer);
            
            // Header with LogicStep name and type
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 4;
            headerRow.style.width = 235; // Account for container padding
            logicStepContainer.Add(headerRow);
            
            // LogicStep name
            string stepName = logicStep.Name;
            if (string.IsNullOrEmpty(stepName))
                stepName = logicStep.GetType().Name.Replace("LogicStep_", "");
            
            // Truncate long names
            const int maxNameLength = 20;
            if (stepName.Length > maxNameLength)
            {
                stepName = stepName.Substring(0, maxNameLength - 3) + "...";
            }
            
            var nameLabel = new Label("Logic: " + stepName);
            nameLabel.tooltip = logicStep.Name ?? logicStep.GetType().Name;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new Color(0.9f, 0.9f, 1f); // Light blue/purple text
            nameLabel.AddToClassList("quickedit-playmode-label");
            headerRow.Add(nameLabel);
            
            // State indicator
            var stateLabel = new Label();
            stateLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            stateLabel.AddToClassList("quickedit-playmode-label");
            stateLabel.style.minWidth = 60;
            headerRow.Add(stateLabel);
            
            // Progress bar for State property
            var progressContainer = new VisualElement();
            progressContainer.style.height = 20;
            progressContainer.style.marginBottom = 4;
            progressContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            progressContainer.style.borderTopLeftRadius = 2;
            progressContainer.style.borderTopRightRadius = 2;
            progressContainer.style.borderBottomLeftRadius = 2;
            progressContainer.style.borderBottomRightRadius = 2;
            logicStepContainer.Add(progressContainer);
            
            var progressBar = new VisualElement();
            progressBar.style.height = Length.Percent(100);
            progressBar.style.position = Position.Absolute;
            progressBar.style.left = 0;
            progressBar.style.top = 0;
            progressBar.style.borderTopLeftRadius = 2;
            progressBar.style.borderBottomLeftRadius = 2;
            progressContainer.Add(progressBar);
            
            var progressLabel = new Label("0%");
            progressLabel.style.position = Position.Absolute;
            progressLabel.style.width = Length.Percent(100);
            progressLabel.style.height = Length.Percent(100);
            progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            progressLabel.style.fontSize = 10;
            progressLabel.style.color = Color.white;
            progressContainer.Add(progressLabel);
            
            // Parent container info if applicable
            var serialContainer = logicStep.GetComponentInParent<LogicStep_SerialContainer>();
            if (serialContainer != null && serialContainer.gameObject != selectedObj)
            {
                var containerInfo = new Label($"In: {serialContainer.name}");
                containerInfo.style.fontSize = 10;
                containerInfo.style.color = new Color(0.7f, 0.7f, 0.7f);
                containerInfo.style.marginTop = 2;
                logicStepContainer.Add(containerInfo);
            }
            
            // Update function for real-time state
            EditorApplication.CallbackFunction updateLogicStep = () => {
                if (logicStep != null && stateLabel != null)
                {
                    // Update state label
                    if (logicStep.StepActive)
                    {
                        stateLabel.text = "ACTIVE";
                        stateLabel.style.color = new Color(0.2f, 1f, 0.2f); // Green
                    }
                    else if (logicStep.IsWaiting)
                    {
                        stateLabel.text = "WAITING";
                        stateLabel.style.color = new Color(1f, 1f, 0.2f); // Yellow
                    }
                    else
                    {
                        stateLabel.text = "INACTIVE";
                        stateLabel.style.color = new Color(0.7f, 0.7f, 0.7f); // Gray
                    }
                    
                    // Update progress bar
                    float progress = logicStep.State;
                    progressBar.style.width = Length.Percent(progress);
                    progressLabel.text = $"{progress:F0}%";
                    
                    // Update progress bar color based on state
                    if (logicStep.StepActive)
                    {
                        progressBar.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Green
                    }
                    else if (logicStep.IsWaiting)
                    {
                        progressBar.style.backgroundColor = new Color(0.8f, 0.8f, 0.2f, 0.8f); // Yellow
                    }
                    else
                    {
                        progressBar.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gray
                    }
                    
                    // If it's a container, show active child info
                    if (logicStep.IsContainer && serialContainer != null && serialContainer.gameObject == selectedObj)
                    {
                        var activeStepIndex = serialContainer.ActiveLogicStep;
                        if (activeStepIndex > 0 && activeStepIndex <= serialContainer.NumberLogicSteps)
                        {
                            progressLabel.text = $"{progress:F0}% - Step {activeStepIndex} of {serialContainer.NumberLogicSteps}";
                        }
                    }
                }
            };
            EditorApplication.update += updateLogicStep;
            
            // Store reference to clean up later (would need to track this properly in production)
            // For now, it will be cleaned when UpdateContent is called again
            
            // Add cycle time statistics for serial containers
            if (serialContainer != null && serialContainer.gameObject == selectedObj)
            {
                AddSeparator();
                
                // Create cycle time statistics container
                var cycleTimeContainer = new VisualElement();
                cycleTimeContainer.style.flexDirection = FlexDirection.Column;
                cycleTimeContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.4f, 0.1f); // Slightly darker blue/purple
                cycleTimeContainer.style.borderTopLeftRadius = 3;
                cycleTimeContainer.style.borderTopRightRadius = 3;
                cycleTimeContainer.style.borderBottomLeftRadius = 3;
                cycleTimeContainer.style.borderBottomRightRadius = 3;
                cycleTimeContainer.style.paddingTop = 4;
                cycleTimeContainer.style.paddingBottom = 4;
                cycleTimeContainer.style.paddingLeft = 4;
                cycleTimeContainer.style.paddingRight = 4;
                cycleTimeContainer.style.marginBottom = 6;
                buttonContainer.Add(cycleTimeContainer);
                
                // Header
                var cycleTimeHeader = new Label("Cycle Time Statistics");
                cycleTimeHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                cycleTimeHeader.style.color = new Color(0.9f, 0.9f, 1f);
                cycleTimeHeader.style.fontSize = 11;
                cycleTimeHeader.style.marginBottom = 4;
                cycleTimeContainer.Add(cycleTimeHeader);
                
                // Statistics rows
                var statsContainer = new VisualElement();
                statsContainer.style.flexDirection = FlexDirection.Column;
                cycleTimeContainer.Add(statsContainer);
                
                // Min time row
                var minRow = new VisualElement();
                minRow.style.flexDirection = FlexDirection.Row;
                minRow.style.justifyContent = Justify.SpaceBetween;
                minRow.style.marginBottom = 2;
                statsContainer.Add(minRow);
                
                var minLabel = new Label("Min:");
                minLabel.style.fontSize = 10;
                minLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                minRow.Add(minLabel);
                
                var minValue = new Label("--");
                minValue.style.fontSize = 10;
                minValue.style.color = Color.white;
                minRow.Add(minValue);
                
                // Max time row
                var maxRow = new VisualElement();
                maxRow.style.flexDirection = FlexDirection.Row;
                maxRow.style.justifyContent = Justify.SpaceBetween;
                maxRow.style.marginBottom = 2;
                statsContainer.Add(maxRow);
                
                var maxLabel = new Label("Max:");
                maxLabel.style.fontSize = 10;
                maxLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                maxRow.Add(maxLabel);
                
                var maxValue = new Label("--");
                maxValue.style.fontSize = 10;
                maxValue.style.color = Color.white;
                maxRow.Add(maxValue);
                
                // Median time row
                var medianRow = new VisualElement();
                medianRow.style.flexDirection = FlexDirection.Row;
                medianRow.style.justifyContent = Justify.SpaceBetween;
                medianRow.style.marginBottom = 2;
                statsContainer.Add(medianRow);
                
                var medianLabel = new Label("Median:");
                medianLabel.style.fontSize = 10;
                medianLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                medianRow.Add(medianLabel);
                
                var medianValue = new Label("--");
                medianValue.style.fontSize = 10;
                medianValue.style.color = Color.white;
                medianRow.Add(medianValue);
                
                // Sequences completed row
                var sequencesRow = new VisualElement();
                sequencesRow.style.flexDirection = FlexDirection.Row;
                sequencesRow.style.justifyContent = Justify.SpaceBetween;
                statsContainer.Add(sequencesRow);
                
                var sequencesLabel = new Label("Sequences:");
                sequencesLabel.style.fontSize = 10;
                sequencesLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                sequencesRow.Add(sequencesLabel);
                
                var sequencesValue = new Label("0");
                sequencesValue.style.fontSize = 10;
                sequencesValue.style.color = Color.white;
                sequencesRow.Add(sequencesValue);
                
                // Add reset button
                var resetButton = new Button(() => {
                    if (serialContainer != null)
                    {
                        serialContainer.ResetCycleStatistics();
                        minValue.text = "--";
                        maxValue.text = "--";
                        medianValue.text = "--";
                        sequencesValue.text = "0";
                    }
                });
                resetButton.text = "Reset";
                resetButton.style.fontSize = 9;
                resetButton.style.marginTop = 4;
                resetButton.style.alignSelf = Align.Center;
                cycleTimeContainer.Add(resetButton);
                
                // Update function for cycle time statistics
                EditorApplication.CallbackFunction updateCycleStats = () => {
                    if (serialContainer != null && Application.isPlaying)
                    {
                        // Update min time
                        if (serialContainer.MinCycleTime > 0)
                            minValue.text = $"{serialContainer.MinCycleTime:F2}s";
                        
                        // Update max time
                        if (serialContainer.MaxCycleTime > 0)
                            maxValue.text = $"{serialContainer.MaxCycleTime:F2}s";
                        
                        // Update median time
                        if (serialContainer.MedianCycleTime > 0)
                            medianValue.text = $"{serialContainer.MedianCycleTime:F2}s";
                        
                        // Update sequences count
                        sequencesValue.text = serialContainer.CompletedCycles.ToString();
                    }
                };
                EditorApplication.update += updateCycleStats;
            }
            
            AddSeparator();
        }
        
        private static bool HasMeshComponents(GameObject obj)
        {
            return obj.GetComponent<MeshFilter>() != null || 
                   obj.GetComponent<MeshRenderer>() != null || 
                   obj.GetComponent<SkinnedMeshRenderer>() != null;
        }
        
        private void AddSeparator()
        {
            var separatorContainer = new VisualElement();
            separatorContainer.style.marginBottom = 3;
            
            var separator = new VisualElement();
            separator.AddToClassList("quickedit-separator");
            separatorContainer.Add(separator);
            
            buttonContainer.Add(separatorContainer);
        }
        
        private VisualElement CreateButtonRow()
        {
            var row = new VisualElement();
            row.AddToClassList("quickedit-button-row");
            return row;
        }
        
        private Button CreateIconButton(Texture icon, string tooltip, System.Action onClick, string buttonClass = null, bool isEmptyIcon = false)
        {
            var button = new Button(onClick);
            button.tooltip = tooltip;
            button.AddToClassList("quickedit-button");
            
            if (!string.IsNullOrEmpty(buttonClass))
                button.AddToClassList(buttonClass);
            
            if (icon != null)
            {
                var image = new Image();
                image.image = icon;
                image.AddToClassList(isEmptyIcon ? "quickedit-icon-empty" : "quickedit-icon");
                button.Add(image);
            }
            
            return button;
        }
        
        private Button CreateTextButton(string text, string tooltip, System.Action onClick, string buttonClass = null)
        {
            var button = new Button(onClick);
            button.text = text;
            button.tooltip = tooltip;
            button.AddToClassList("quickedit-text-button");
            
            if (!string.IsNullOrEmpty(buttonClass))
                button.AddToClassList(buttonClass);
            
            return button;
        }
        
        // Transform operations from QuickEdit
        private static void ZeroLocal()
        {
            var sel = Selection.activeGameObject;
            if (sel == null) return;
            Undo.RecordObject(sel.transform, "Transform to local zero");
            sel.transform.localPosition = Vector3.zero;
            
            // Ping effect
            EditorGUIUtility.PingObject(sel);
        }
        
        private static void ZeroGlobal()
        {
            var sel = Selection.activeGameObject;
            if (sel == null) return;
            Undo.RecordObject(sel.transform, "Transform to global zero");
            sel.transform.position = Vector3.zero;
            
            // Ping effect
            EditorGUIUtility.PingObject(sel);
        }
        
        private static void AlignPivot()
        {
#if REALVIRTUAL_PROFESSIONAL
            MovePivotTool.ShowWindow();
#else
            EditorUtility.DisplayDialog("Warning",
                "Move Pivot Tool is only included in realvirtual Professional", "OK");
#endif
        }
        
        private static void Rotation(Vector3 rotation)
        {
            var obj = Selection.activeGameObject;
            if (obj == null) return;
            Undo.RecordObject(obj.transform, "Rotation");
            obj.transform.rotation = obj.transform.rotation * Quaternion.Euler(rotation);
            
            // Ping effect
            EditorGUIUtility.PingObject(obj);
        }
        
        private static void NewEmpty()
        {
            var sel = Selection.activeGameObject;
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Created GameObject");
            if (sel != null)
                go.transform.parent = sel.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            Selection.activeGameObject = go;
            
            // Expand in hierarchy and focus
            EditorGUIUtility.PingObject(go);
            EditorApplication.DirtyHierarchyWindowSorting();
        }
        
        private static void NewEmptyAtRoot()
        {
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Created GameObject at Root");
            go.transform.parent = null; // Ensure it's at root level
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            Selection.activeGameObject = go;
            
            // Expand in hierarchy and focus
            EditorGUIUtility.PingObject(go);
            EditorApplication.DirtyHierarchyWindowSorting();
        }
        
        private static void IntoEmpty()
        {
            var sel = Selection.activeGameObject;
            if (sel == null) return;
            
            var go = new GameObject();
            go.transform.parent = sel.transform.parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            
            var sels = Selection.gameObjects;
            foreach (var obj in sels)
            {
                obj.transform.parent = go.transform;
            }
            
            Global.SetExpandedRecursive(go, true);
            Selection.activeGameObject = go;
            
            // Expand in hierarchy and focus
            EditorGUIUtility.PingObject(go);
            EditorApplication.DirtyHierarchyWindowSorting();
        }
        
        private static void OpenKinematicTool()
        {
#if REALVIRTUAL_PROFESSIONAL
            var tool = (KinematicTool)EditorWindow.GetWindow(typeof(KinematicTool));
            tool.Show();
#else
            EditorUtility.DisplayDialog("Warning",
                "Kinematic Tool is only included in realvirtual Professional", "OK");
#endif
        }
        
        private static bool ShouldShowLogicSteps(QuickEditContext context)
        {
            if (!context.HasSelection) return false;
            
            // Show if we're under a serial container OR if there's no LogicStep on current object
            var hasLogicStep = context.SelectedObject.GetComponent<LogicStep>() != null;
            var underSerialContainer = context.SelectedObject.GetComponentInParent<LogicStep_SerialContainer>() != null;
            
            return !hasLogicStep || underSerialContainer;
        }
        
        private static bool HasOtherRealvirtualComponents(GameObject obj)
        {
            if (obj == null) return false;
            
            // Get all components that inherit from realvirtualBehavior
            var realvirtualComponents = obj.GetComponents<realvirtualBehavior>();
            
            // Check if there are any realvirtual components
            if (realvirtualComponents.Length > 0)
            {
                return true;
            }
            
            // Also check for other common realvirtual components that might not inherit from realvirtualBehavior
            if (obj.GetComponent<Drive>() != null) return true;
            if (obj.GetComponent<Sensor>() != null) return true;
            if (obj.GetComponent<Source>() != null) return true;
            if (obj.GetComponent<Sink>() != null) return true;
            if (obj.GetComponent<Grip>() != null) return true;
            if (obj.GetComponent<TransportSurface>() != null) return true;
            if (obj.GetComponent<MU>() != null) return true;
            if (obj.GetComponent<CAM>() != null) return true;
            if (obj.GetComponent<PLCInputBool>() != null) return true;
            if (obj.GetComponent<PLCInputFloat>() != null) return true;
            if (obj.GetComponent<PLCInputInt>() != null) return true;
            if (obj.GetComponent<PLCOutputBool>() != null) return true;
            if (obj.GetComponent<PLCOutputFloat>() != null) return true;
            if (obj.GetComponent<PLCOutputInt>() != null) return true;
            
            return false;
        }
        
        private static void AddLogicStep(System.Type logicStepType)
        {
            var sel = Selection.gameObjects;
            foreach (var obj in sel)
            {
                var existingStep = obj.GetComponent<LogicStep>();
                if (existingStep != null)
                {
                    // If there's already a LogicStep on this GameObject, create a new child GameObject
                    var newStepObject = new GameObject(logicStepType.Name.Replace("LogicStep_", ""));
                    newStepObject.transform.SetParent(obj.transform.parent);
                    newStepObject.transform.SetSiblingIndex(obj.transform.GetSiblingIndex() + 1);
                    Undo.RegisterCreatedObjectUndo(newStepObject, $"Create {logicStepType.Name}");
                    Undo.AddComponent(newStepObject, logicStepType);
                    EditorGUIUtility.PingObject(newStepObject);
                    Selection.activeGameObject = newStepObject;
                }
                else
                {
                    // No LogicStep on this GameObject, add it directly
                    Undo.AddComponent(obj, logicStepType);
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeGameObject = obj;
                }
            }
        }
        
        private static void AddComponent(System.Type com)
        {
            var res = new List<Component>();
            var sel = Selection.gameObjects;
            foreach (var obj in sel)
            {
                // Check if we're trying to add a Drive when one already exists
                if (com == typeof(Drive) && obj.GetComponent<Drive>() != null)
                {
                    Logger.Warning($"GameObject '{obj.name}' already has a Drive component. Multiple drives are not allowed.", null);
                    continue;
                }
                
                // Check if we're adding a BehaviorInterface (base class for drive behaviors)
                if (typeof(BehaviorInterface).IsAssignableFrom(com))
                {
                    // Turn off jogging on any existing drive
                    var drive = obj.GetComponent<Drive>();
                    if (drive != null && (drive.JogForward || drive.JogBackward))
                    {
                        drive.JogForward = false;
                        drive.JogBackward = false;
                        EditorUtility.SetDirty(drive);
                    }
                }
                
                res.Add(Undo.AddComponent(obj, com));
                
                // Ping effect
                EditorGUIUtility.PingObject(obj);
            }
        }
        
        private static void AddTransportGuided()
        {
            var sel = Selection.gameObjects;
            foreach (var obj in sel)
            {
                // Check if Drive already exists
                var existingDrive = obj.GetComponent<Drive>();
                if (existingDrive != null)
                {
                    Logger.Warning($"GameObject '{obj.name}' already has a Drive component. TransportGuided requires its own Drive component.", null);
                    continue;
                }
                
                var drive = Undo.AddComponent<Drive>(obj);
                drive.Direction = DIRECTION.Virtual;
                var transportGuided = obj.AddComponent<TransportGuided>();
                transportGuided.Init();
                
                // Ping effect
                EditorGUIUtility.PingObject(obj);
            }
        }
        
        private static void CreateSignal(System.Type com)
        {
            var sel = Selection.activeGameObject;
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Create Signal");
            if (sel != null)
            {
                go.transform.parent = sel.transform;
                if (sel.GetComponent<Signal>() != null)
                    go.transform.parent = sel.transform.parent;
            }
            
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.name = com.Name;
            go.AddComponent(com);
            Selection.activeGameObject = go;
            
            // Ping effect
            EditorGUIUtility.PingObject(go);
            EditorApplication.DirtyHierarchyWindowSorting();
        }
        
        private static void ChangeSignal(string type)
        {
            foreach (var sel in Selection.gameObjects)
            {
                Undo.RegisterCreatedObjectUndo(sel, "Change Signal");
                var sig = sel.GetComponent<Signal>();
                if (sig.IsInput())
                {
                    switch (type)
                    {
                        case "int":
                            sel.AddComponent<PLCOutputInt>();
                            break;
                        case "float":
                            sel.AddComponent<PLCOutputFloat>();
                            break;
                        case "bool":
                            sel.AddComponent<PLCOutputBool>();
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case "int":
                            sel.AddComponent<PLCInputInt>();
                            break;
                        case "float":
                            sel.AddComponent<PLCInputFloat>();
                            break;
                        case "bool":
                            sel.AddComponent<PLCInputBool>();
                            break;
                    }
                }
                UnityEngine.Object.DestroyImmediate(sig);
                
                // Ping effect
                EditorGUIUtility.PingObject(sel);
            }
        }
        
        private void CreateDriveJogControls(Drive drive)
        {
            // Check if drive has active behavior interface
            var behaviors = drive.GetComponents<BehaviorInterface>();
            bool hasActiveBehavior = false;
            foreach (var behavior in behaviors)
            {
                if (behavior.isActiveAndEnabled)
                {
                    hasActiveBehavior = true;
                    break;
                }
            }
            
            // If has active behavior, show warning instead of jog controls
            if (hasActiveBehavior)
            {
                var warningLabel = new Label("Drive has active behavior - jogging disabled");
                warningLabel.style.color = new Color(1f, 0.7f, 0f);
                warningLabel.style.fontSize = 10;
                warningLabel.style.whiteSpace = WhiteSpace.Normal;
                warningLabel.style.marginTop = 4;
                buttonContainer.Add(warningLabel);
                return;
            }
            
            // Jog controls row
            var jogRow = CreateButtonRow();
            jogRow.style.justifyContent = Justify.Center;
            
            // Create all buttons first before setting up callbacks
            var jogBackBtn = new Button();
            jogBackBtn.text = "◀ Back";
            jogBackBtn.style.width = 75;
            jogBackBtn.style.height = 26;
            jogBackBtn.AddToClassList("quickedit-jog-button");
            
            var jogFwdBtn = new Button();
            jogFwdBtn.text = "Fwd ▶";
            jogFwdBtn.style.width = 75;
            jogFwdBtn.style.height = 26;
            jogFwdBtn.AddToClassList("quickedit-jog-button");
            
            var stopBtn = CreateTextButton("■ Stop", "Stop jogging", () => {
                Undo.RecordObject(drive, "Stop Jog");
                drive.JogBackward = false;
                drive.JogForward = false;
                EditorUtility.SetDirty(drive);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                jogBackBtn.RemoveFromClassList("quickedit-drive-button-active");
                jogFwdBtn.RemoveFromClassList("quickedit-drive-button-active");
            });
            stopBtn.style.width = 75;
            stopBtn.style.height = 26;
            stopBtn.AddToClassList("quickedit-jog-button");
            
            // Register click callback for backward button
            jogBackBtn.clicked += () => {
                Undo.RecordObject(drive, "Toggle Jog Backward");
                
                // Toggle state
                if (drive.JogBackward)
                {
                    drive.JogBackward = false;
                    jogBackBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
                else
                {
                    drive.JogForward = false; // Ensure forward is off
                    drive.JogBackward = true;
                    jogBackBtn.AddToClassList("quickedit-drive-button-active");
                    jogFwdBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
                
                EditorUtility.SetDirty(drive);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            };
            
            // Register click callback for forward button
            jogFwdBtn.clicked += () => {
                Undo.RecordObject(drive, "Toggle Jog Forward");
                
                // Toggle state
                if (drive.JogForward)
                {
                    drive.JogForward = false;
                    jogFwdBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
                else
                {
                    drive.JogBackward = false; // Ensure backward is off
                    drive.JogForward = true;
                    jogFwdBtn.AddToClassList("quickedit-drive-button-active");
                    jogBackBtn.RemoveFromClassList("quickedit-drive-button-active");
                }
                
                EditorUtility.SetDirty(drive);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            };
            
            // Add buttons to row
            jogRow.Add(jogBackBtn);
            jogRow.Add(stopBtn);
            jogRow.Add(jogFwdBtn);
            
            buttonContainer.Add(jogRow);
            
            // Set initial button states based on drive status
            if (drive.JogBackward)
                jogBackBtn.AddToClassList("quickedit-drive-button-active");
            if (drive.JogForward)
                jogFwdBtn.AddToClassList("quickedit-drive-button-active");
            
            // Target speed row
            var speedRow = CreateButtonRow();
            speedRow.style.justifyContent = Justify.SpaceBetween;
            speedRow.style.alignItems = Align.Center;
            
            var speedFieldLabel = new Label("Target Speed:");
            speedFieldLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            speedFieldLabel.style.fontSize = 11;
            speedRow.Add(speedFieldLabel);
            
            // Container for field and unit aligned to right
            var speedInputContainer = new VisualElement();
            speedInputContainer.style.flexDirection = FlexDirection.Row;
            speedInputContainer.style.alignItems = Align.Center;
            
            var speedField = new FloatField();
            speedField.value = drive.TargetSpeed;
            speedField.style.width = 60;
            
            // Store reference for focus tracking
            currentSpeedField = speedField;
            
            // Add focus callbacks to prevent updates while editing
            speedField.RegisterCallback<FocusInEvent>(evt => {
                // Field is being edited - don't update from Inspector
            });
            
            speedField.RegisterCallback<FocusOutEvent>(evt => {
                // Apply final value when focus is lost
                if (Math.Abs(speedField.value - drive.TargetSpeed) > 0.001f)
                {
                    Undo.RecordObject(drive, "Change Target Speed");
                    drive.TargetSpeed = speedField.value;
                    EditorUtility.SetDirty(drive);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            });
            
            speedField.RegisterValueChangedCallback(evt => {
                // Only update on explicit value changes (Enter key or focus loss)
                if (evt.newValue != evt.previousValue)
                {
                    Undo.RecordObject(drive, "Change Target Speed");
                    drive.TargetSpeed = evt.newValue;
                    EditorUtility.SetDirty(drive);
                }
            });
            
            speedInputContainer.Add(speedField);
            
            // Determine unit based on drive direction
            string unit = GetDriveSpeedUnit(drive);
            
            var speedUnit = new Label(unit);
            speedUnit.style.marginLeft = 4;
            speedUnit.style.unityTextAlign = TextAnchor.MiddleLeft;
            speedUnit.style.fontSize = 11;
            speedInputContainer.Add(speedUnit);
            
            speedRow.Add(speedInputContainer);
            
            buttonContainer.Add(speedRow);
        }
        
        private void CreateControllerUI(realvirtualController controller)
        {
            // Title
            var titleLabel = new Label("realvirtual Controller");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 8;
            titleLabel.style.fontSize = 14;
            buttonContainer.Add(titleLabel);
            
            // Connection status button
            var connectionRow = CreateButtonRow();
            string connectionText = controller.Connected ? "Change to Disconnected" : "Change to Connected";
            var connectionBtn = CreateTextButton(connectionText, 
                controller.Connected ? "Disconnect from PLC" : "Connect to PLC", 
                () => {
                    Undo.RecordObject(controller, "Toggle Connection");
                    controller.Connected = !controller.Connected;
                    EditorUtility.SetDirty(controller);
                    UpdateContent(); // Refresh the UI
                });
            connectionBtn.style.width = 200;
            connectionRow.Add(connectionBtn);
            buttonContainer.Add(connectionRow);
            
            // Info box toggle button
            if (controller.HideInfoBox)
            {
                var infoBoxRow = CreateButtonRow();
                var infoBoxBtn = CreateTextButton("Show Info Box", 
                    "Show the info box in scene view", 
                    () => {
                        Undo.RecordObject(controller, "Toggle Info Box");
                        controller.HideInfoBox = false;
                        EditorUtility.SetDirty(controller);
                        UpdateContent(); // Refresh the UI
                    });
                infoBoxBtn.style.width = 200;
                infoBoxRow.Add(infoBoxBtn);
                buttonContainer.Add(infoBoxRow);
            }
            
            // Add separator
            AddSeparator();
            
            // Help text
            var helpLabel = new Label("Select a scene object to use QuickEdit tools");
            helpLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            helpLabel.style.fontSize = 11;
            helpLabel.style.whiteSpace = WhiteSpace.Normal;
            helpLabel.style.marginTop = 4;
            buttonContainer.Add(helpLabel);
        }
        
        private static void UnpackPrefabCompletely(GameObject obj)
        {
            // Confirm with user
            if (EditorUtility.DisplayDialog("Unpack Prefab Completely", 
                $"Are you sure you want to completely unpack the prefab '{obj.name}'?\n\nThis will break all connections to the prefab asset and cannot be undone easily.", 
                "Unpack", "Cancel"))
            {
                try
                {
                    // Record for undo
                    Undo.RegisterFullObjectHierarchyUndo(obj, "Unpack Prefab Completely");
                    
                    // Unpack completely
                    PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                    
                    Logger.Message($"Prefab '{obj.name}' has been unpacked completely.", null);
                    
                    // Ping effect
                    EditorGUIUtility.PingObject(obj);
                }
                catch (System.Exception e)
                {
                    Logger.Error($"Failed to unpack prefab: {e.Message}", null);
                }
            }
        }
        
        private static void PlaceOnGround(GameObject obj)
        {
            if (obj == null) return;
            
            // Record for undo
            Undo.RecordObject(obj.transform, "Place on Ground");
            
            // Get all renderers in the object and its children
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
            {
                Logger.Warning($"GameObject '{obj.name}' has no renderers. Cannot calculate bounding box.", null);
                return;
            }
            
            // Calculate combined bounds
            Bounds combinedBounds = new Bounds();
            bool boundsInitialized = false;
            
            foreach (var renderer in renderers)
            {
                if (renderer.bounds.size != Vector3.zero) // Skip empty bounds
                {
                    if (!boundsInitialized)
                    {
                        combinedBounds = new Bounds(renderer.bounds.center, renderer.bounds.size);
                        boundsInitialized = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(renderer.bounds);
                    }
                }
            }
            
            if (!boundsInitialized)
            {
                Logger.Warning($"GameObject '{obj.name}' has no valid bounds.", null);
                return;
            }
            
            // Calculate the bottom of the bounding box in world space
            float bottomY = combinedBounds.min.y;
            
            // Calculate the offset needed to place the bottom at Y=0
            float offsetY = -bottomY;
            
            // Apply the offset to the object's position
            Vector3 newPosition = obj.transform.position;
            newPosition.y += offsetY;
            obj.transform.position = newPosition;
            
            // Ping effect
            EditorGUIUtility.PingObject(obj);
            
            Logger.Message($"Placed '{obj.name}' on ground. Bottom of bounding box is now at Y=0.", null);
        }
        
        private static void PivotToY0(GameObject obj)
        {
            if (obj == null) return;
            
            // Check if object has a mesh - warn and return if it does
            if (HasMeshComponents(obj))
            {
                EditorUtility.DisplayDialog("Warning", 
                    "Cannot adjust pivot on objects with mesh components.\n\n" +
                    "This function only works on parent objects without meshes.\n" +
                    "The pivot adjustment would distort the mesh.", 
                    "OK");
                return;
            }
            
            // Get all renderers in children (not including self)
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
            {
                Logger.Warning($"GameObject '{obj.name}' has no child renderers. Cannot calculate bounding box.", null);
                return;
            }
            
            // Calculate combined bounds of children
            Bounds combinedBounds = new Bounds();
            bool boundsInitialized = false;
            
            foreach (var renderer in renderers)
            {
                // Skip if renderer is on the object itself (should be caught above, but double-check)
                if (renderer.gameObject == obj)
                    continue;
                    
                if (renderer.bounds.size != Vector3.zero) // Skip empty bounds
                {
                    if (!boundsInitialized)
                    {
                        combinedBounds = new Bounds(renderer.bounds.center, renderer.bounds.size);
                        boundsInitialized = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(renderer.bounds);
                    }
                }
            }
            
            if (!boundsInitialized)
            {
                Logger.Warning($"GameObject '{obj.name}' has no valid child bounds.", null);
                return;
            }
            
            // Record for undo
            Undo.RecordObject(obj.transform, "Pivot to Y 0");
            
            // Store current world positions of all children
            List<Transform> children = new List<Transform>();
            List<Vector3> childWorldPositions = new List<Vector3>();
            List<Quaternion> childWorldRotations = new List<Quaternion>();
            
            foreach (Transform child in obj.transform)
            {
                children.Add(child);
                childWorldPositions.Add(child.position);
                childWorldRotations.Add(child.rotation);
            }
            
            // Calculate new pivot position (bottom center of bounding box)
            Vector3 newPivotWorld = new Vector3(
                combinedBounds.center.x,
                combinedBounds.min.y,
                combinedBounds.center.z
            );
            
            // Move the object to the new pivot position
            obj.transform.position = newPivotWorld;
            
            // Restore all children to their original world positions
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null) // Check if child still exists
                {
                    children[i].position = childWorldPositions[i];
                    children[i].rotation = childWorldRotations[i];
                }
            }
            
            // Ping effect
            EditorGUIUtility.PingObject(obj);
            
            Logger.Message($"Moved pivot of '{obj.name}' to bottom center of bounding box.", null);
        }
        
        private static void AlignYUp(GameObject obj)
        {
            if (obj == null) return;
            
            // Check if object has a mesh - warn and return if it does
            var meshFilter = obj.GetComponent<MeshFilter>();
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            var skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
            
            if (meshFilter != null || meshRenderer != null || skinnedMeshRenderer != null)
            {
                EditorUtility.DisplayDialog("Warning", 
                    "Cannot align Y up on objects with mesh components.\n\n" +
                    "This function only works on parent objects without meshes.\n" +
                    "Aligning would distort the mesh geometry.", 
                    "OK");
                return;
            }
            
            // Record for undo
            Undo.RecordObject(obj.transform, "Align Y Up");
            
            // Store current world positions and rotations of all children
            List<Transform> children = new List<Transform>();
            List<Vector3> childWorldPositions = new List<Vector3>();
            List<Quaternion> childWorldRotations = new List<Quaternion>();
            
            foreach (Transform child in obj.transform)
            {
                children.Add(child);
                childWorldPositions.Add(child.position);
                childWorldRotations.Add(child.rotation);
            }
            
            // Calculate the rotation needed to align Y axis with world up
            Vector3 currentUp = obj.transform.up;
            Vector3 worldUp = Vector3.up;
            
            // If already aligned, do nothing
            if (Vector3.Angle(currentUp, worldUp) < 0.01f)
            {
                Logger.Message($"GameObject '{obj.name}' is already aligned with Y up.", null);
                return;
            }
            
            // Calculate rotation axis and angle
            Vector3 rotationAxis = Vector3.Cross(currentUp, worldUp);
            float angle = Vector3.Angle(currentUp, worldUp);
            
            // Handle case where vectors are opposite (180 degrees)
            if (rotationAxis.magnitude < 0.001f)
            {
                // Choose an arbitrary perpendicular axis
                rotationAxis = Vector3.Cross(currentUp, Vector3.right);
                if (rotationAxis.magnitude < 0.001f)
                {
                    rotationAxis = Vector3.Cross(currentUp, Vector3.forward);
                }
            }
            
            rotationAxis.Normalize();
            
            // Apply the rotation
            Quaternion alignmentRotation = Quaternion.AngleAxis(angle, rotationAxis);
            obj.transform.rotation = alignmentRotation * obj.transform.rotation;
            
            // Restore all children to their original world positions and rotations
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null) // Check if child still exists
                {
                    children[i].position = childWorldPositions[i];
                    children[i].rotation = childWorldRotations[i];
                }
            }
            
            // Ping effect
            EditorGUIUtility.PingObject(obj);
            
            Logger.Message($"Aligned '{obj.name}' Y axis to world up without moving children.", null);
        }
        
        // Legacy IMGUI support for backward compatibility with SimulationQuickEdit
        private void AddLegacyIMGUISupport()
        {
            if (OnQuickEditDraw != null)
            {
                var imguiContainer = new IMGUIContainer(() =>
                {
                    OnQuickEditDraw?.Invoke();
                });
                
                imguiContainer.style.paddingTop = 5;
                imguiContainer.style.paddingBottom = 5;
                buttonContainer.Add(imguiContainer);
            }
        }
    }
}
#endif

