// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.UIElements;
using System.Linq;

namespace realvirtual
{
    [InitializeOnLoad]
    public static class QuickEditToolbarIMGUI
    {
        private static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject m_currentToolbar;
        private static Texture2D iconTexture;
        private static GUIStyle buttonStyle;
        private static IMGUIContainer m_container;
        private static int initializationAttempts = 0;
        private const int maxInitializationAttempts = 30; // Increased for Unity 6
        private static bool debugMode = false;
        private static bool isInitialized = false;
        private static float lastInitTime = 0f;
        private static bool isInitializing = false;
        
        public static void ForceRefresh()
        {
            // Force the IMGUI to repaint by triggering Unity's internal repaint
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        
        private static void ForceOverlayUpdate(bool visible)
        {
            // Multiple approaches to ensure the overlay updates
            
            // 1. First try the static event approach (most reliable)
#if UNITY_2021_2_OR_NEWER
            try
            {
                // Use reflection to invoke the static event
                var overlayType = Type.GetType("realvirtual.QuickEdit, Assembly-CSharp-Editor");
                if (overlayType != null)
                {
                    var eventInfo = overlayType.GetEvent("OnVisibilityChangeRequested", BindingFlags.Public | BindingFlags.Static);
                    if (eventInfo != null)
                    {
                        var eventDelegate = eventInfo.EventHandlerType;
                        var invokeMethod = eventDelegate.GetMethod("Invoke");
                        var handler = Delegate.CreateDelegate(eventDelegate, null, invokeMethod);
                        eventInfo.GetRaiseMethod()?.Invoke(null, new object[] { visible });
                        
                        // Alternative: directly invoke if the event has subscribers
                        var field = overlayType.GetField("OnVisibilityChangeRequested", BindingFlags.NonPublic | BindingFlags.Static);
                        if (field != null)
                        {
                            var eventDelegate2 = field.GetValue(null) as System.Action<bool>;
                            eventDelegate2?.Invoke(visible);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (debugMode) Debug.LogError($"[realvirtual] Error invoking overlay event: {e.Message}");
            }
#endif
            
            // 2. Ensure Scene view exists
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                sceneView = EditorWindow.GetWindow<SceneView>();
            }
            
            if (sceneView != null)
            {
                // 3. Force immediate repaint
                sceneView.Repaint();
                
                // 4. Try to find and update the overlay directly
#if UNITY_2021_2_OR_NEWER
                try
                {
                    // Get all overlays in the scene view
#if UNITY_6000_0_OR_NEWER
                    var overlays = sceneView.overlayCanvas.overlays;
#else
                    // Unity 2022 compatibility - overlays property doesn't exist, use reflection
                    var overlaysField = sceneView.overlayCanvas.GetType().GetField("m_Overlays", BindingFlags.NonPublic | BindingFlags.Instance);
                    var overlaysList = overlaysField?.GetValue(sceneView.overlayCanvas) as System.Collections.IEnumerable;
                    var overlays = overlaysList?.Cast<object>();
#endif
#if UNITY_6000_0_OR_NEWER
                    foreach (var overlay in overlays)
                    {
                        if (overlay != null && overlay.GetType().Name == "QuickEdit")
                        {
                            // Force the overlay to update its displayed state
                            overlay.displayed = visible;
                            
                            // If showing the overlay, also expand it
                            if (visible && overlay.collapsed)
                            {
                                overlay.collapsed = false;
                            }
                            
                            if (debugMode) Debug.Log($"[realvirtual] Found and updated QuickEdit directly: {visible}");
                            break;
                        }
                    }
#else
                    foreach (var overlayObj in overlays)
                    {
                        if (overlayObj != null && overlayObj.GetType().Name == "QuickEdit")
                        {
                            // Use reflection for Unity 2022 property access
                            var displayedProperty = overlayObj.GetType().GetProperty("displayed");
                            var collapsedProperty = overlayObj.GetType().GetProperty("collapsed");
                            
                            // Force the overlay to update its displayed state
                            displayedProperty?.SetValue(overlayObj, visible);
                            
                            // If showing the overlay, also expand it
                            if (visible)
                            {
                                var currentCollapsed = (bool)(collapsedProperty?.GetValue(overlayObj) ?? false);
                                if (currentCollapsed)
                                {
                                    collapsedProperty?.SetValue(overlayObj, false);
                                }
                            }
                            
                            if (debugMode) Debug.Log($"[realvirtual] Found and updated QuickEdit directly: {visible}");
                            break;
                        }
                    }
#endif
                }
                catch (Exception e)
                {
                    if (debugMode) Debug.LogError($"[realvirtual] Error updating overlay: {e.Message}");
                }
#endif
                
                // 5. Schedule multiple delayed repaints to ensure update
                for (int i = 0; i < 3; i++)
                {
                    int delay = i;
                    EditorApplication.delayCall += () =>
                    {
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.Repaint();
                        }
                    };
                }
                
                // 6. Force all views to repaint
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
        
        static QuickEditToolbarIMGUI()
        {
            debugMode = EditorPrefs.GetBool("realvirtual_ToolbarDebugMode", false);
            
            // Load icon with multiple fallback paths
            LoadIconWithFallbacks();
            
            // Use delayed initialization for better Unity 6 compatibility
            EditorApplication.delayCall += DelayedInitialization;
            
            // Re-initialize on domain reload and play mode changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            
            // Also hook into hierarchy change for additional initialization attempts
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
        
        static void LoadIconWithFallbacks()
        {
            // First try direct asset loading
            string[] assetPaths = new string[]
            {
                "Assets/realvirtual/private/Resources/Icons/Icon48.png",
                "Assets/realvirtual/private/Resources/Icons/button-0local.png",
                "Assets/realvirtual/Resources/Icons/Icon48.png",
                "Assets/realvirtual/Resources/Icons/button-0local.png"
            };
            
            foreach (var path in assetPaths)
            {
                iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (iconTexture != null)
                {
                    if (debugMode) Debug.Log($"[realvirtual] Icon loaded from asset: {path}");
                    return;
                }
            }
            
            // Then try Resources.Load
            string[] resourcePaths = new string[]
            {
                "Icons/Icon48",
                "Icons/button-0local",
                "realvirtual/Icons/Icon48",
                "realvirtual/Icons/button-0local"
            };
            
            foreach (var path in resourcePaths)
            {
                iconTexture = UnityEngine.Resources.Load<Texture2D>(path);
                if (iconTexture != null)
                {
                    if (debugMode) Debug.Log($"[realvirtual] Icon loaded from resources: {path}");
                    return;
                }
            }
            
            // Try finding any realvirtual icon
            var iconGuids = AssetDatabase.FindAssets("Icon48 t:Texture2D");
            if (iconGuids.Length > 0)
            {
                var iconPath = AssetDatabase.GUIDToAssetPath(iconGuids[0]);
                iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                if (iconTexture != null && debugMode)
                {
                    Debug.Log($"[realvirtual] Icon found via search: {iconPath}");
                }
            }
            
            if (iconTexture == null)
            {
                // Only show warning if Unity has been running for a reasonable time
                // to avoid warnings during initial asset import on clean installs
                if (EditorApplication.timeSinceStartup > 10.0)
                {
                    Debug.LogWarning("[realvirtual] Toolbar icon could not be loaded from any known path");
                }
            }
            else if (debugMode)
            {
                Debug.Log($"[realvirtual] Icon successfully loaded: {iconTexture.name} ({iconTexture.width}x{iconTexture.height})");
            }
        }
        
        static void DelayedInitialization()
        {
            EditorApplication.update -= OnUpdate; // Remove old update handler
            EditorApplication.update += OnDelayedUpdate;
        }
        
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                // Reset initialization
                isInitialized = false;
                m_currentToolbar = null;
                m_container = null;
                initializationAttempts = 0;
                lastInitTime = (float)EditorApplication.timeSinceStartup;
                
                // Single delayed initialization attempt
                EditorApplication.delayCall += DelayedInitialization;
            }
        }
        
        static void OnHierarchyChanged()
        {
            // If not initialized and enough time has passed, try again
            if (!isInitialized && EditorApplication.timeSinceStartup - lastInitTime > 2.0f)
            {
                lastInitTime = (float)EditorApplication.timeSinceStartup;
                EditorApplication.delayCall += DelayedInitialization;
            }
        }
        
        static void OnAfterAssemblyReload()
        {
            // Reset and retry after assembly reload
            isInitialized = false;
            m_currentToolbar = null;
            m_container = null;
            initializationAttempts = 0;
            lastInitTime = (float)EditorApplication.timeSinceStartup;
            LoadIconWithFallbacks();
            
            // Single initialization attempt
            EditorApplication.delayCall += DelayedInitialization;
        }
        
        static void OnDelayedUpdate()
        {
            // Check if already properly initialized
            if (isInitialized && m_currentToolbar != null && m_container != null && m_container.parent != null)
            {
                // Verify container is still in the hierarchy
                if (m_container.parent.panel != null)
                {
                    return;
                }
                else
                {
                    // Container lost, need to reinitialize
                    isInitialized = false;
                    if (debugMode) Debug.Log("[realvirtual] Container lost, reinitializing...");
                }
            }
            
            if (initializationAttempts >= maxInitializationAttempts)
            {
                EditorApplication.update -= OnDelayedUpdate;
                if (debugMode) Debug.LogWarning($"[realvirtual] Stopped trying to initialize toolbar after {maxInitializationAttempts} attempts");
                return;
            }
            
            initializationAttempts++;
            
            if (TryInitializeToolbar())
            {
                isInitialized = true;
                EditorApplication.update -= OnDelayedUpdate;
                if (debugMode) Debug.Log($"[realvirtual] Toolbar initialized successfully after {initializationAttempts} attempts");
                
                // Schedule periodic verification
                EditorApplication.delayCall += () => {
                    EditorApplication.delayCall += VerifyToolbarIntegrity;
                };
            }
        }
        
        static bool TryInitializeToolbar()
        {
            // Prevent concurrent initialization attempts
            if (isInitializing)
            {
                if (debugMode) Debug.Log("[realvirtual] Already initializing, skipping");
                return false;
            }
            
            isInitializing = true;
            
            try
            {
                // Find toolbar
                var toolbars = UnityEngine.Resources.FindObjectsOfTypeAll(m_toolbarType);
                if (toolbars.Length == 0)
                {
                    if (debugMode) Debug.Log("[realvirtual] No toolbar found yet");
                    return false;
                }
                
                m_currentToolbar = (ScriptableObject)toolbars[0];
                
                // Get root element
                var root = GetToolbarRoot();
                if (root == null)
                {
                    if (debugMode) Debug.Log("[realvirtual] Toolbar root not found");
                    return false;
                }
                
                // Register our UI
                return RegisterToolbarUI(root);
            }
            catch (Exception e)
            {
                if (debugMode) Debug.LogError($"[realvirtual] Error initializing toolbar: {e.Message}\n{e.StackTrace}");
                return false;
            }
            finally
            {
                isInitializing = false;
            }
        }
        
        static VisualElement GetToolbarRoot()
        {
            if (m_currentToolbar == null) return null;
            
            try
            {
                // Try different methods to get the root element
                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                
                // Method 1: m_Root field (Unity 2021 and earlier)
                var rootField = m_currentToolbar.GetType().GetField("m_Root", bindingFlags);
                if (rootField != null)
                {
                    var rawRoot = rootField.GetValue(m_currentToolbar);
                    if (rawRoot is VisualElement root)
                    {
                        return root;
                    }
                }
                
                // Method 2: rootVisualElement property (Unity 2022+)
                var rootProperty = m_currentToolbar.GetType().GetProperty("rootVisualElement", bindingFlags);
                if (rootProperty != null)
                {
                    var visualElement = rootProperty.GetValue(m_currentToolbar) as VisualElement;
                    if (visualElement != null)
                    {
                        return visualElement;
                    }
                }
                
                // Method 3: Try to find through the toolbar window
                var windows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var window in windows)
                {
                    if (window.GetType().Name == "Toolbar")
                    {
                        return window.rootVisualElement;
                    }
                }
            }
            catch (Exception e)
            {
                if (debugMode) Debug.LogError($"[realvirtual] Error getting toolbar root: {e.Message}");
            }
            
            return null;
        }
        
        public static void OnUpdate()
        {
            // Keep for compatibility but redirect to new handler
            OnDelayedUpdate();
        }
        
        static bool RegisterToolbarUI(VisualElement root)
        {
            // First, remove ALL existing RealVirtualToolbar elements globally to prevent duplicates
            var allWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window != null && window.rootVisualElement != null)
                {
                    var existingInWindow = window.rootVisualElement.Query<IMGUIContainer>("RealVirtualToolbar").ToList();
                    foreach (var existing in existingInWindow)
                    {
                        existing.parent?.Remove(existing);
                    }
                }
            }
            
            // Also remove from the current root
            var existingToolbars = root.Query<IMGUIContainer>("RealVirtualToolbar").ToList();
            foreach (var existing in existingToolbars)
            {
                existing.parent?.Remove(existing);
            }
            
            // Remove existing container if any
            if (m_container != null && m_container.parent != null)
            {
                m_container.parent.Remove(m_container);
                m_container = null;
            }
            
            // Create new container with proper styling
            m_container = new IMGUIContainer(OnGUI);
            m_container.style.height = 22;
            m_container.style.flexShrink = 0;
            m_container.style.flexGrow = 0;
            m_container.name = "RealVirtualToolbar";
            
            // Try multiple locations for Unity 6 compatibility
            VisualElement targetParent = FindBestToolbarLocation(root);
            
            if (targetParent == null)
            {
                if (debugMode) Debug.LogWarning("[realvirtual] No suitable toolbar location found, trying fallback approach");
                
                // Fallback: try to insert before specific elements
                var accountButton = root.Q("AccountToolbarButton");
                if (accountButton != null && accountButton.parent != null)
                {
                    targetParent = accountButton.parent;
                    var index = targetParent.IndexOf(accountButton);
                    if (index > 0)
                    {
                        targetParent.Insert(index, m_container);
                        if (debugMode) Debug.Log("[realvirtual] Inserted before account button");
                        ForceRefresh();
                        return true;
                    }
                }
                
                // Last resort
                targetParent = root;
            }
            
            // Add our container
            targetParent.Add(m_container);
            
            // Force refresh multiple times
            ForceRefresh();
            EditorApplication.delayCall += ForceRefresh;
            EditorApplication.delayCall += () => { EditorApplication.delayCall += ForceRefresh; };
            
            return true;
        }
        
        static void VerifyToolbarIntegrity()
        {
            if (!isInitialized) return;
            
            // Check if container is still valid
            if (m_container == null || m_container.parent == null || m_container.panel == null)
            {
                if (debugMode) Debug.Log("[realvirtual] Toolbar integrity check failed, reinitializing...");
                isInitialized = false;
                initializationAttempts = 0;
                EditorApplication.delayCall += DelayedInitialization;
            }
        }
        
        static VisualElement FindBestToolbarLocation(VisualElement root)
        {
            // Unity 6 specific element names and class names
            string[] possibleLocations = new string[]
            {
                // Unity 6 specific
                "unity-toolbar-contents",
                "toolbar-contents",
                "ToolbarContents",
                // Right side locations
                "ToolbarZoneRightAlign",
                "toolbar-zone-right-align",
                "ToolbarRightAlign",
                "RightToolbar",
                "toolbar-zone-right",
                // Spacer locations
                "ToolbarFlexSpacer",
                "toolbar-flex-spacer",
                "toolbar-spacer",
                // Play mode area
                "ToolbarZonePlayMode",
                "toolbar-zone-play-mode",
                "PlayModeToolbar",
                "play-mode-toolbar",
                // Left side as fallback
                "ToolbarZoneLeftAlign",
                "toolbar-zone-left-align",
                "toolbar-zone-left"
            };
            
            // First try direct name matching
            foreach (var locationName in possibleLocations)
            {
                var element = root.Q(locationName);
                if (element != null)
                {
                    if (debugMode) Debug.Log($"[realvirtual] Found toolbar location by name: {locationName}");
                    
                    // For flex spacer, we want to add after it
                    if (locationName.Contains("FlexSpacer") || locationName.Contains("spacer"))
                    {
                        if (element.parent != null)
                        {
                            return element.parent;
                        }
                    }
                    
                    return element;
                }
            }
            
            // Try by class names (Unity 6 uses different class naming)
            string[] classNames = new string[]
            {
                "unity-toolbar__contents",
                "unity-toolbar__navigation",
                "unity-toolbar__right-align",
                "unity-toolbar-contents",
                "toolbar-contents"
            };
            
            foreach (var className in classNames)
            {
                var byClass = root.Q(className: className);
                if (byClass != null)
                {
                    if (debugMode) Debug.Log($"[realvirtual] Found toolbar by class: {className}");
                    
                    // For navigation elements, use parent
                    if (className.Contains("navigation") && byClass.parent != null)
                    {
                        return byClass.parent;
                    }
                    
                    return byClass;
                }
            }
            
            // Deep search for toolbar elements
            var allElements = root.Query<VisualElement>().ToList();
            
            // Look for elements with toolbar in name or class
            foreach (var element in allElements)
            {
                if (element.name != null && element.name.ToLower().Contains("toolbar"))
                {
                    // Prefer right-aligned or contents areas
                    if (element.name.ToLower().Contains("right") || 
                        element.name.ToLower().Contains("contents") ||
                        element.name.ToLower().Contains("zone"))
                    {
                        if (debugMode) Debug.Log($"[realvirtual] Found toolbar by deep search: {element.name}");
                        return element;
                    }
                }
                
                // Check classes
                foreach (var cls in element.GetClasses())
                {
                    if (cls.ToLower().Contains("toolbar") && 
                        (cls.ToLower().Contains("right") || cls.ToLower().Contains("contents")))
                    {
                        if (debugMode) Debug.Log($"[realvirtual] Found toolbar by class search: {cls}");
                        return element;
                    }
                }
            }
            
            // Last resort - find any toolbar element
            var anyToolbar = root.Query<VisualElement>().Where(e => 
                (e.name != null && e.name.ToLower().Contains("toolbar")) ||
                e.GetClasses().Any(c => c.ToLower().Contains("toolbar"))
            ).First();
            
            if (anyToolbar != null && debugMode)
            {
                Debug.Log($"[realvirtual] Using fallback toolbar element: {anyToolbar.name}");
            }
            
            return anyToolbar;
        }
        
        static void OnGUI()
        {
            // Ensure icon is loaded
            if (iconTexture == null)
            {
                LoadIconWithFallbacks();
            }
            
            // Get the current window width to detect if we need to hide elements
            float windowWidth = EditorGUIUtility.currentViewWidth;
            
            if (debugMode && Event.current.type == EventType.Repaint)
            {
                Debug.Log($"[realvirtual] OnGUI called - Window width: {windowWidth}, Icon loaded: {iconTexture != null}");
            }
            
            // Dynamic width thresholds based on toolbar content
            const float minWidthForFullDisplay = 900f;   // Show everything (path, dropdown, icon) - reduced
            const float minWidthForIconOnly = 200f;      // Show only dropdown and icon - reduced
            const float absoluteMinWidth = 100f;         // Hide everything - reduced
            
            // Don't display anything if window is extremely small
            if (windowWidth < absoluteMinWidth)
            {
                return;
            }
            
            // Create button style if needed
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle();
                buttonStyle.padding = new RectOffset(2, 2, 2, 2);
                buttonStyle.margin = new RectOffset(4, 0, 0, 0);  // Remove right margin
                buttonStyle.imagePosition = ImagePosition.ImageOnly;
                buttonStyle.fixedWidth = 24;
                buttonStyle.fixedHeight = 19;
            }
            
            // Add some right margin to avoid overlap with Unity's standard toolbar buttons
            GUILayout.BeginHorizontal(GUILayout.Height(19));
            
            // Add flexible space to push our content to the right
            GUILayout.FlexibleSpace();
            
            if (ProjectPathMenuItem.IsProjectPathEnabled() && windowWidth >= minWidthForFullDisplay)
            {
                // Get full project path
                string projectPath = System.IO.Path.GetDirectoryName(Application.dataPath);
                
                // Progressive path truncation based on available width
                if (windowWidth < 1400f)
                {
                    var pathParts = projectPath.Split(System.IO.Path.DirectorySeparatorChar);
                    if (windowWidth < 1200f && pathParts.Length > 1)
                    {
                        // Very narrow - show only last directory
                        projectPath = ".../" + pathParts[pathParts.Length - 1];
                    }
                    else if (pathParts.Length > 2)
                    {
                        // Medium width - show last two directories
                        projectPath = ".../" + pathParts[pathParts.Length - 2] + "/" + pathParts[pathParts.Length - 1];
                    }
                }
                
                // Create button style that looks like a label
                var pathStyle = new GUIStyle(EditorStyles.label);
                pathStyle.alignment = TextAnchor.MiddleRight;
                pathStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                pathStyle.fontSize = 11;
                pathStyle.margin = new RectOffset(0, 4, 0, 0);
                pathStyle.fixedHeight = 19;
                
                // Change cursor on hover
                pathStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                
                // Draw project path as clickable button
                if (GUILayout.Button(projectPath, pathStyle, GUILayout.Height(19)))
                {
                    // Get full path for opening
                    string fullPath = System.IO.Path.GetDirectoryName(Application.dataPath);
                    EditorUtility.RevealInFinder(fullPath);
                }
            }
            
            // Draw dropdown arrow button BEFORE the icon (show when no path is displayed)
            if (windowWidth >= minWidthForIconOnly)
            {
                var dropdownStyle = new GUIStyle();
                dropdownStyle.padding = new RectOffset(0, 0, 6, 0);  // Move text down with top padding
                dropdownStyle.margin = new RectOffset(2, 2, 0, 0);  // Small margins on both sides
                dropdownStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                dropdownStyle.hover.textColor = Color.white;
                dropdownStyle.fontSize = 8;  // Smaller font size
                dropdownStyle.fixedWidth = 10;  // Narrower width
                dropdownStyle.fixedHeight = 19;
                dropdownStyle.alignment = TextAnchor.UpperCenter;
                
                if (GUILayout.Button("▼", dropdownStyle))
            {
                // Create dropdown menu
                GenericMenu menu = new GenericMenu();
                
                menu.AddItem(new GUIContent("Apply Standard Settings"), false, () => {
                    ProjectSettingsTools.SetStandardSettings(true);
                });
                
                menu.AddItem(new GUIContent("Show Project Path in Toolbar"), 
                    ProjectPathMenuItem.IsProjectPathEnabled(), 
                    () => {
                        EditorApplication.ExecuteMenuItem(ProjectPathMenuItem.MenuName);
                    });
                
                // Show the dropdown menu
                menu.ShowAsContext();
                }
            }
            
            bool isVisible = EditorPrefs.GetBool("realvirtual_QuickEditVisible", true);
            
            // Draw button - just the icon, no background
            GUIContent content = iconTexture != null 
                ? new GUIContent(iconTexture, "realvirtual quickedit")
                : new GUIContent("RV", "realvirtual quickedit");
            
            // Use alpha to indicate state - full opacity when active, slightly faded when inactive
            var oldColor = GUI.color;
            GUI.color = isVisible ? Color.white : new Color(1f, 1f, 1f, 0.6f);
            
            if (GUILayout.Button(content, buttonStyle, GUILayout.Width(24), GUILayout.Height(19)))
            {
                // Toggle QuickEdit
                bool newState = !isVisible;
                EditorPrefs.SetBool("realvirtual_QuickEditVisible", newState);
                
                // Force overlay to update immediately
                ForceOverlayUpdate(newState);
            }
            
            GUI.color = oldColor;
            
            // Add right margin to avoid overlap with Unity's toolbar buttons (account, cloud, etc.)
            GUILayout.Space(10); // Reduced space for Unity's right-side toolbar buttons
            
            GUILayout.EndHorizontal();
        }
    }
}