// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace realvirtual
{
    public static class ProjectSettingsTools
    {
        private const string REALVIRTUAL_FIRST_INSTALL_KEY = "realvirtual_FirstInstallCompleted";
        private const string REALVIRTUAL_VERSION_KEY = "realvirtual_InstalledVersion";
        private const string REALVIRTUAL_CURRENT_VERSION = "6.0.0"; // Update this with each release

        private const string REALVIRTUAL_SETTINGS_CALL_KEY = "realvirtual_SetStandardSettingsCalled";
        private const string REALVIRTUAL_SETTINGS_SUCCESS_KEY = "realvirtual_SetStandardSettingsSuccess";
        
        static ProjectSettingsTools()
        {
            // Silent initialization tracking for verification purposes only
            EditorPrefs.SetString("realvirtual_ProjectSettingsTools_InitTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }
        
        /// <summary>
        /// Checks if this is the first realvirtual installation in this project
        /// </summary>
        /// <returns>True if this is the first installation, false if realvirtual has been installed before</returns>
        public static bool IsFirstTimeInstallation()
        {
            // Check if realvirtual layers exist
            // If NO layers exist, it's a first-time installation (clean project)
            // If layers exist, it's an old project (not first-time)
            return !HasExistingRealvirtualLayers();
        }
        
        /// <summary>
        /// Checks if realvirtual layers already exist in the project
        /// </summary>
        /// <returns>True if realvirtual layers are found</returns>
        private static bool HasExistingRealvirtualLayers()
        {
            string[] realvirtualLayers = { "rvMUSensor", "rvMUTransport", "rvSensor", "rvTransport", "rvMU", "rvSnapping" };
            
            foreach (string layerName in realvirtualLayers)
            {
                // LayerMask.NameToLayer returns -1 if layer doesn't exist
                if (LayerMask.NameToLayer(layerName) != -1)
                {
                    return true; // Found at least one realvirtual layer
                }
            }
            
            return false; // No realvirtual layers found
        }
        
        /// <summary>
        /// Marks the installation as completed and saves the current version
        /// </summary>
        private static void MarkInstallationCompleted()
        {
            EditorPrefs.SetBool(REALVIRTUAL_FIRST_INSTALL_KEY, true);
            EditorPrefs.SetString(REALVIRTUAL_VERSION_KEY, REALVIRTUAL_CURRENT_VERSION);
        }
        
        /// <summary>
        /// Shows a dialog asking user if they want to apply standard settings for an existing installation
        /// </summary>
        /// <returns>True if user wants to apply settings, false otherwise</returns>
        private static bool ShowSettingsPromptDialog()
        {
            return EditorUtility.DisplayDialog(
                "realvirtual Settings", 
                "realvirtual has detected an existing installation or update.\n\n" +
                "Would you like to apply the standard realvirtual project settings?\n\n" +
                "This will configure layers, collision matrix, scene view, and other Unity settings optimized for industrial automation.\n\n" +
                "You can always apply these settings later via:\n" +
                "realvirtual → Development → Set Standard Settings", 
                "Apply Settings", 
                "Skip"
            );
        }
        
        /// <summary>
        /// Sets standard realvirtual project settings with optional first-time detection
        /// </summary>
        /// <param name="message">Show success message after applying settings</param>
        /// <param name="forceApply">Force apply settings without prompting (for menu commands)</param>
        public static void SetStandardSettings(bool message)
        {
            #if UNITY_EDITOR
            // Silent tracking that SetStandardSettings was called
            EditorPrefs.SetString(REALVIRTUAL_SETTINGS_CALL_KEY, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            // Don't apply settings during play mode
            if (Application.isPlaying)
            {
                Logger.Warning("Cannot apply standard settings during play mode. Please exit play mode first.", null);
                return;
            }
            
            // Check Unity version compatibility
            if (!CheckUnityVersionCompatibility())
            {
                return; // User chose to cancel due to version warning
            }
            
            // Check if this is first installation or an update
            bool isFirstTime = IsFirstTimeInstallation();
            
            if (!isFirstTime)
            {
                // Not first time - show prompt
                bool applySettings = EditorUtility.DisplayDialog(
                    "realvirtual Settings",
                    "realvirtual has detected an existing installation.\n\n" +
                    "Would you like to apply the standard realvirtual project settings?\n\n" +
                    "This will configure layers, collision matrix, scene view, and other Unity settings optimized for industrial automation.",
                    "Apply Settings",
                    "Skip"
                );
                
                if (!applySettings)
                {
                    Logger.Message("realvirtual settings application skipped by user choice.", null);
                    return;
                }
            }
            
            try
            {
                List<string> layerstoconsider = new List<string>
                {
                    "rvMUSensor", "rvMUTransport", "rvSensor",
                    "rvTransport", "rvMU", "rvSnapping", "rvSimDynamic","rvSimStatic","rvSelection","rvSceneMesh"
                };
                List<string> collission = new List<string>
                {
                    "rvMUSensor-rvSensor",
                    "rvMUTransport/rvTransport",
                    "rvMUTransport/rvMUTransport",
                    "rvMUSensor/rvSensor",
                    "rvSensor/rvMU",
                    "rvTransport/rvMUTransport",
                    "rvTransport/rvMU",
                    "rvTransport/rvTransport",
                    "rvMU/rvMU",
                    "rvSimDynamic/rvSimStatic",
                    "rvSnapping/rvSnapping",
                    "rvSceneMesh/rvSceneMesh",
                    "rvSceneMesh/Default"
                };
                

                var layernumber = 13;
                foreach (var layer in layerstoconsider)
                {
                    if (layernumber <= 31) // Unity supports layers 0-31
                    {
                        CreateLayer(layer, layernumber, false); // Silent layer creation
                        layernumber++;
                    }
                    else
                    {
                        Logger.Warning($"Cannot create layer '{layer}' - exceeded maximum layer number 31", null);
                    }
                }

                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        string layerx = LayerMask.LayerToName(x);
                        string layery = LayerMask.LayerToName(y);
                        string index1 = layerx + "/" + layery;
                        string index2 = layery + "/" + layerx;
                        if (layerstoconsider.Contains(layerx) || layerstoconsider.Contains(layery))
                        {
                            if (collission.Contains(index1) || collission.Contains(index2))
                            {
                                UnityEngine.Physics.IgnoreLayerCollision(x, y, false);
                            }
                            else
                            {
                                UnityEngine.Physics.IgnoreLayerCollision(x, y, true);
                            }
                        }
                    }
                }

                // Gizmo configuration removed - letting Unity use default gizmo settings
                
                // Force refresh scene view to apply 3D icon changes
                SceneView.RepaintAll();
                EditorApplication.RepaintHierarchyWindow();
                
                // Set handle position to pivot mode and rotation to local
                Tools.pivotMode = PivotMode.Pivot;
                Tools.pivotRotation = PivotRotation.Local;
                
                // Set to Move tool as the active tool
                Tools.current = Tool.Move;
                
                // Toolbar hiding removed - keeping Unity standard overlays
                
                // Collapse all items in the hierarchy
                CollapseHierarchy();
                
                // Clean up the scene (unlock hidden objects, etc.) - only on existing installations
                if (!isFirstTime)
                {
                    SceneTools.CleanupScene(false);
                }
                
                // Turn on shaded mode for Scene view
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null)
                {
                    sceneView = EditorWindow.GetWindow<SceneView>();
                }
                if (sceneView != null)
                {
                    sceneView.drawGizmos = true; // Keep gizmos enabled in scene view
                    
                    // Enable orientation gizmo (navigation cube)
                    sceneView.isRotationLocked = false;
                    
                    // Try to expand the orientation gizmo
                    try
                    {
                        var sceneViewType = sceneView.GetType();
                        
                        // Try different property names that Unity might use
                        string[] possiblePropertyNames = new string[] 
                        { 
                            "viewIsLockedToObject",
                            "m_OrientationGizmoSize",
                            "orientationGizmoSize",
                            "navigationGizmoSize"
                        };
                        
                        foreach (var propName in possiblePropertyNames)
                        {
                            var property = sceneViewType.GetField(propName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            if (property != null && property.FieldType == typeof(int))
                            {
                                // Set to expanded size (typically 1 for expanded, 0 for collapsed)
                                property.SetValue(sceneView, 1);
                                break;
                            }
                        }
                        
                        // Also try setting through EditorPrefs if available
                        EditorPrefs.SetInt("Scene/OrientationGizmoSize", 1);
                    }
                    catch { }
                    
                    // Overlay configuration removed - keeping Unity defaults
                    
                    // Configure specific scene view visibility flags based on screenshot requirements
                    if (sceneView != null)
                    {
                        // Enable the specific items shown in the screenshot
                        sceneView.drawGizmos = true; // Keep gizmos enabled
                        sceneView.showGrid = true; // Grid toggle
                        
                        // Configure SceneViewState to disable unwanted visual effects
                        try
                        {
                            var sceneViewType = sceneView.GetType();
                            var sceneViewStateField = sceneViewType.GetField("m_SceneViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                            
                            if (sceneViewStateField != null)
                            {
                                var sceneViewState = sceneViewStateField.GetValue(sceneView);
                                if (sceneViewState != null)
                                {
                                    var stateType = sceneViewState.GetType();
                                    
                                    // Disable all visual effects that clutter the industrial simulation view
                                    SetSceneViewStateField(stateType, sceneViewState, "showFog", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "showSkybox", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "showFlares", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "showImageEffects", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "showParticleSystems", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "showVisualEffectGraphs", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "m_ShowClouds", false);
                                    SetSceneViewStateField(stateType, sceneViewState, "showClouds", false);
                                    
                                    // Also try to set properties
                                    SetSceneViewStateProperty(stateType, sceneViewState, "fogEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "skyboxEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "cloudsEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "flaresEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "imageEffectsEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "particleSystemsEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "visualEffectGraphsEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "fxEnabled", false);
                                    SetSceneViewStateProperty(stateType, sceneViewState, "m_FxEnabled", false);
                                }
                            }
                        }
                        catch { }
                    }
                    
                    // Set shading mode to Shaded
                    var drawMode = SceneView.GetBuiltinCameraMode(DrawCameraMode.Textured);
                    sceneView.cameraMode = drawMode;
                    
                    // Also try to set renderMode for older Unity versions
                    var renderModeProperty = typeof(SceneView).GetProperty("renderMode", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (renderModeProperty != null)
                    {
                        renderModeProperty.SetValue(sceneView, DrawCameraMode.Textured);
                    }
                    
                    // Force repaint to apply the changes
                    sceneView.Repaint();
                    
                    // Frame non-realvirtual objects
                    FrameNonRealvirtualObjects(sceneView);
                }
                
                PlayerSettings.colorSpace = ColorSpace.Linear;
                
                // Setup realvirtual toolbar with delay to ensure assets are loaded
                EditorApplication.delayCall += () => {
                    SetupRealvirtualToolbar();
                };
                
                ProjectSettingsUnity2022.SetDefine("REALVIRTUAL");
                
                // Time settings for industrial simulation
                Time.fixedDeltaTime = 0.02f; // 50 Hz physics update rate (standard for industrial simulation)
                Time.maximumDeltaTime = 0.1f; // Prevent large time steps
                
                // Physics settings for accurate simulation
                Physics.defaultSolverIterations = 10; // Increased from default 6 for more accurate physics
                Physics.defaultSolverVelocityIterations = 2; // Increased from default 1
                Physics.autoSyncTransforms = true; // Important for drive movements
                Physics.defaultContactOffset = 0.01f; // Standard contact offset
                
                // Quality settings for editor performance
                QualitySettings.vSyncCount = 0; // Disable VSync for maximum performance in editor
                Application.targetFrameRate = -1; // Unlimited framerate in editor
                
                // Additional scene view settings
                if (sceneView != null)
                {
                    // Set scene view camera settings
                    var sceneCamera = sceneView.camera;
                    if (sceneCamera != null)
                    {
                        sceneCamera.clearFlags = CameraClearFlags.Skybox;
                        sceneCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                    }
                }
                
                // Configure player settings using Unity 2022 compatibility layer
                ProjectSettingsUnity2022.ConfigurePlayerSettings();
                
                // Set scene view camera to reasonable position with Y pointing up
                SetSceneViewCameraOrientation();

                // Check if Professional Version and set define
                if (AssetDatabase.IsValidFolder("Assets/Playmaker"))
                {
                    ProjectSettingsUnity2022.SetDefine("REALVIRTUAL_PLAYMAKER");
                }

                CreateTag("Align");
                var alllayers = ~0;
                Tools.visibleLayers = alllayers & ~(1 << LayerMask.NameToLayer("UI"));

                // Check if Playmaker  and set define
                if (AssetDatabase.IsValidFolder("Assets/realvirtual/Professional"))
                {
                    ProjectSettingsUnity2022.SetDefine("REALVIRTUAL_PROFESSIONAL");
                }

                if (!message)
                {
                    string scenePath = "Assets/realvirtual/Scenes/DemoRealvirtual.unity";
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
                    {
                        EditorSceneManager.OpenScene(scenePath);
                    }
                    else
                    {
                        Logger.Warning($"Demo scene not found at {scenePath}", null);
                    }
                }
                
                if (message)
                {
                    RealvirtualMessageWindow.ShowSuccess(
                        "realvirtual Standard Settings Applied",
                        new string[] {
                            "Layers and collision matrix configured for automation components",
                            "Scene view optimized: hierarchy collapsed",
                            "UI layer hidden from Tools visibility",
                            "Transform tools: Move tool active, Pivot/Local mode enabled",
                            "Build settings: Linear color space, Mono backend, .NET Standard 2.0",
                            "Physics optimized: 50Hz update rate, improved solver accuracy",
                            "Performance: VSync off, run in background enabled",
                            "QuickEdit toolbar and overlay enabled for quick access",
                            "Script icons configured in Gizmos folder"
                        },
                        windowTitle: "Settings applied",
                        animationDuration: 2f
                    );
                    
                    // Log nice summary to console with green checkmarks matching window messages
                    Logger.Message("<color=#00ff00><b>realvirtual Standard Settings Applied</b></color>\n", null);
                    Logger.Message("<color=#00ff00>✓</color> Layers created (Edit > Project Settings > Tags and Layers)", null);
                    Logger.Message("<color=#00ff00>✓</color> UI Layer hidden from view (Layers dropdown in Scene view)", null);
                    Logger.Message("<color=#00ff00>✓</color> Gizmos kept at Unity default settings", null);
                    Logger.Message("<color=#00ff00>✓</color> Handle position set to Pivot mode", null);
                    Logger.Message("<color=#00ff00>✓</color> Handle rotation set to Local mode", null);
                    Logger.Message("<color=#00ff00>✓</color> Move tool selected as default", null);
                    Logger.Message("<color=#00ff00>✓</color> Scene view overlays kept at Unity defaults", null);
                    Logger.Message("<color=#00ff00>✓</color> Hierarchy collapsed for clean view", null);
                    Logger.Message("<color=#00ff00>✓</color> Scene view camera oriented with Y pointing up", null);
                    Logger.Message("<color=#00ff00>✓</color> Scene view set to Shaded mode", null);
                    Logger.Message("<color=#00ff00>✓</color> Framed non-realvirtual objects in scene", null);
                    Logger.Message("<color=#00ff00>✓</color> Linear color space enabled (Edit > Project Settings > Player > Other Settings)", null);
                    Logger.Message("<color=#00ff00>✓</color> Script icons configured (Gizmos folder)", null);
                    Logger.Message("<color=#00ff00>✓</color> Scripting backend: Mono (Edit > Project Settings > Player > Configuration)", null);
                    Logger.Message("<color=#00ff00>✓</color> API compatibility: .NET Standard 2.0 (Edit > Project Settings > Player > Configuration)", null);
                    Logger.Message("<color=#00ff00>✓</color> Run in background enabled (Edit > Project Settings > Player > Resolution)", null);
                    Logger.Message("<color=#00ff00>✓</color> Visible in background enabled (Edit > Project Settings > Player > Resolution)", null);
                    Logger.Message("<color=#00ff00>✓</color> Physics update rate: 50 Hz (Time.fixedDeltaTime = 0.02)", null);
                    Logger.Message("<color=#00ff00>✓</color> Physics solver iterations increased for accuracy", null);
                    Logger.Message("<color=#00ff00>✓</color> VSync disabled in editor for maximum performance", null);
                    Logger.Message("<color=#00ff00>✓</color> QuickEdit overlay enabled (Scene view)", null);
                    Logger.Message("<color=#00ff00>✓</color> QuickEdit button added to main toolbar (next to Play button)", null);
                }

                /// Move the Script Icons to the folder
                try
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Gizmos"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Gizmos");
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/Gizmos/realvirtual"))
                    {
                        AssetDatabase.CreateFolder("Assets/Gizmos", "realvirtual");
                    }
                    else
                    {
                        FileUtil.DeleteFileOrDirectory("Assets/Gizmos/realvirtual");
                        AssetDatabase.CreateFolder("Assets/Gizmos", "realvirtual");
                    }

                    string sourcePath = "Assets/realvirtual/private/Resources/Icons/EditorScriptIcons";
                    if (AssetDatabase.IsValidFolder(sourcePath))
                    {
                        Copy(sourcePath, "Assets/Gizmos/realvirtual");
                    }
                    else
                    {
                        Logger.Warning($"Source icon folder not found at {sourcePath}", null);
                    }
                    
                    AssetDatabase.Refresh();
                }
                catch (Exception e)
                {
                    Logger.Error($"Error setting up script icons: {e.Message}", null);
                }
                
                // Setup URP renderer based on Unity version
                ProjectSettingsUnity2022.SetupURPRenderer();
                
                // Remove Unity 6-only components from prefabs in Unity 2022
                ProjectSettingsUnity2022.CleanupUnity6OnlyComponents();
                
                // Mark installation as completed
                EditorPrefs.SetBool(REALVIRTUAL_FIRST_INSTALL_KEY, true);
                
                // Silent tracking of successful completion
                EditorPrefs.SetString(REALVIRTUAL_SETTINGS_SUCCESS_KEY, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            catch (Exception e)
            {
                Logger.Error($"Error applying standard settings: {e.Message}", null);
                if (message)
                {
                    RealvirtualMessageWindow.ShowError(
                        $"Failed to apply standard settings: {e.Message}",
                        windowTitle: "Error",
                        headerTitle: "Settings Error"
                    );
                }
            }
            #endif
        }
        
        /// <summary>
        /// Switches to Unity 2022 compatible renderer (without Unity 6-only features)
        /// </summary>
        public static void SwitchToUnity2022Renderer()
        {
            ProjectSettingsUnity2022.SwitchToUnity2022Renderer();
        }
        
        /// <summary>
        /// Switches to full Unity 6 renderer (with all features)
        /// </summary>
        public static void SwitchToUnity6Renderer()
        {
            ProjectSettingsUnity2022.SwitchToUnity6Renderer();
        }
        
        /// <summary>
        /// Checks Unity version compatibility and shows warnings for unsupported versions
        /// </summary>
        /// <returns>True if user wants to continue, false if they want to cancel</returns>
        private static bool CheckUnityVersionCompatibility()
        {
            var unityVersion = Application.unityVersion;
            var versionParts = unityVersion.Split('.');
            
            if (versionParts.Length < 2)
            {
                Logger.Warning($"Could not parse Unity version: {unityVersion}", null);
                return true; // Continue anyway
            }
            
            int majorVersion = 0;
            int minorVersion = 0;
            
            if (!int.TryParse(versionParts[0], out majorVersion) || 
                !int.TryParse(versionParts[1], out minorVersion))
            {
                Logger.Warning($"Could not parse Unity version numbers: {unityVersion}", null);
                return true; // Continue anyway
            }
            
            // Check for supported versions
            bool isUnity2022 = majorVersion == 2022;
            bool isUnity6_0 = majorVersion == 6000 && minorVersion == 0; // Unity 6 uses 6000.0.x format
            bool isUnity6_1OrNewer = majorVersion == 6000 && minorVersion >= 1;
            bool isNewerThanUnity6 = majorVersion > 6000;
            
            if (isUnity2022 || isUnity6_0)
            {
                // Supported versions: Unity 2022.x or Unity 6.0.x (LTS)
                return true;
            }
            
            // Unsupported version - show warning dialog
            string warningMessage;
            string title;
            
            if (majorVersion < 2022)
            {
                // Too old
                warningMessage = $"Unity {unityVersion} is not supported by realvirtual.\n\n" +
                    "Supported Unity versions:\n" +
                    "• Unity 2022.3 LTS or newer\n" +
                    "• Unity 6.0 or newer\n\n" +
                    "Some features may not work correctly or at all in older Unity versions.\n\n" +
                    "Do you want to continue anyway?";
                title = "Unsupported Unity Version";
            }
            else if (isUnity6_1OrNewer || isNewerThanUnity6)
            {
                // Newer than supported Unity 6.0 LTS
                warningMessage = $"Unity {unityVersion} is newer than the supported versions for realvirtual.\n\n" +
                    "Supported Unity versions:\n" +
                    "• Unity 2022.3 LTS\n" +
                    "• Unity 6.0 LTS only\n\n" +
                    "Unity 6.1 and newer versions are not yet supported. " +
                    "This version might work but usage is at your own risk. " +
                    "Some features may behave unexpectedly or require updates.\n\n" +
                    "Do you want to continue anyway?";
                title = "Unsupported Unity Version";
            }
            else
            {
                // Other unsupported versions (like Unity 2023, Unity 5, etc.)
                warningMessage = $"Unity {unityVersion} has not been tested with realvirtual.\n\n" +
                    "Supported Unity versions:\n" +
                    "• Unity 2022.3 LTS\n" +
                    "• Unity 6.0 LTS only\n\n" +
                    "This version might work but usage is at your own risk.\n\n" +
                    "Do you want to continue anyway?";
                title = "Unsupported Unity Version";
            }
            
            bool continueAnyway = EditorUtility.DisplayDialog(
                title,
                warningMessage,
                "Continue Anyway",
                "Cancel"
            );
            
            if (continueAnyway)
            {
                Logger.Warning($"User chose to continue with Unity {unityVersion} despite compatibility warning.", null);
            }
            else
            {
                Logger.Message($"User cancelled setup due to Unity {unityVersion} compatibility warning.", null);
            }
            
            return continueAnyway;
        }
        

        public static void SetDefine(string mydefine)
        {
            ProjectSettingsUnity2022.SetDefine(mydefine);
        }

        public static void DeleteDefine(string mydefine)
        {
            ProjectSettingsUnity2022.DeleteDefine(mydefine);
        }

        public static bool CreateTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                Logger.Warning("Cannot create tag with empty or null name", null);
                return false;
            }

            try
            {
                // Load tag manager
                var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
                if (tagManagerAssets == null || tagManagerAssets.Length == 0)
                {
                    Logger.Error("Could not load TagManager.asset", null);
                    return false;
                }

                SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");
                
                if (tagsProp == null)
                {
                    Logger.Error("Could not find 'tags' property in TagManager", null);
                    return false;
                }

                if (tagsProp.arraySize >= 10000)
                {
                Logger.Warning($"Maximum number of tags reached ({tagsProp.arraySize}). Cannot add '{tagName}'", null);
                    return false;
                }

                // Check if tag already exists
                if (!PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName))
                {
                    int index = tagsProp.arraySize;
                    tagsProp.InsertArrayElementAtIndex(index);
                    SerializedProperty sp = tagsProp.GetArrayElementAtIndex(index);
                    sp.stringValue = tagName;
                    tagManager.ApplyModifiedProperties();
                    return true;
                }
                
                return false; // Tag already exists
            }
            catch (Exception e)
            {
                Logger.Error($"Error creating tag '{tagName}': {e.Message}", null);
                return false;
            }
        }

        private static bool PropertyExists(SerializedProperty property, int start, int end, string value)
        {
            for (int i = start; i < end; i++)
            {
                SerializedProperty t = property.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TagExists(string tagName)
        {
            var ret = false;
#if UNITY_EDITOR
            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Layers Property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            ret = PropertyExists(tagsProp, 0, 1000, tagName);
#endif
            return ret;
        }

        private static void CreateLayer(string name, int number, bool logMessage = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Logger.Warning("Cannot create layer with empty or null name", null);
                return;
            }

            if (number < 0 || number > 31)
            {
                Logger.Error($"Layer number {number} is out of range (0-31)", null);
                return;
            }

            // Layers 0-7 are reserved by Unity
            if (number < 8)
            {
                Logger.Warning($"Layer {number} is reserved by Unity. Use layers 8-31 for custom layers.", null);
                return;
            }

            try
            {
                var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
                if (tagManagerAssets == null || tagManagerAssets.Length == 0)
                {
                    Logger.Error("Could not load TagManager.asset", null);
                    return;
                }

                var tagManager = new SerializedObject(tagManagerAssets[0]);
                var layerProps = tagManager.FindProperty("layers");
                
                if (layerProps == null)
                {
                    Logger.Error("Could not find 'layers' property in TagManager", null);
                    return;
                }

                var propCount = layerProps.arraySize;
                if (number >= propCount)
                {
                    Logger.Error($"Layer index {number} is out of bounds (max: {propCount - 1})", null);
                    return;
                }

                // Clear any existing layer with the same name at a different index
                for (var i = 0; i < propCount; i++)
                {
                    var layerProp = layerProps.GetArrayElementAtIndex(i);
                    if (layerProp.stringValue == name && i != number)
                    {
                        layerProp.stringValue = string.Empty;
                    }
                }

                // Set the layer at the specified index
                var targetProp = layerProps.GetArrayElementAtIndex(number);
                if (targetProp != null)
                {
                    targetProp.stringValue = name;
                    tagManager.ApplyModifiedProperties();
                    if (logMessage)
                    {
                        Logger.Message($"Layer '{name}' created at index {number}", null);
                    }
                }
                else
                {
                    Logger.Error($"Could not access layer property at index {number}", null);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error creating layer '{name}' at index {number}: {e.Message}", null);
            }
        }
        
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        
        
        private static bool AnnotationExists(Type annotationUtilityType, int classId, string className)
        {
            if (annotationUtilityType == null) return false;
            
            try
            {
                // Get all annotations to check if the specific one exists
                var getAnnotationsMethod = annotationUtilityType.GetMethod("GetAnnotations", 
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                
                if (getAnnotationsMethod != null)
                {
                    var annotations = getAnnotationsMethod.Invoke(null, null) as Array;
                    if (annotations != null)
                    {
                        foreach (var annotation in annotations)
                        {
                            if (annotation != null)
                            {
                                var annotationType = annotation.GetType();
                                var classIdField = annotationType.GetField("classID");
                                var scriptClassField = annotationType.GetField("scriptClass");
                                
                                if (classIdField != null && scriptClassField != null)
                                {
                                    var annotationClassId = (int)classIdField.GetValue(annotation);
                                    var annotationScriptClass = (string)scriptClassField.GetValue(annotation);
                                    
                                    // Match by classId and className (allow null/empty className matches)
                                    if (annotationClassId == classId && 
                                        (string.IsNullOrEmpty(className) || 
                                         string.IsNullOrEmpty(annotationScriptClass) ||
                                         annotationScriptClass == className))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // If we can't check, assume it doesn't exist to be safe
                return false;
            }
            
            return false;
        }

        private static MethodInfo FindMethodWithParameters(Type type, string methodName, Type[] parameterTypes)
        {
            if (type == null || string.IsNullOrEmpty(methodName) || parameterTypes == null)
                return null;
                
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            
            foreach (var method in methods)
            {
                if (method.Name != methodName) continue;
                
                var parameters = method.GetParameters();
                if (parameters.Length != parameterTypes.Length) continue;
                
                bool match = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                    {
                        match = false;
                        break;
                    }
                }
                
                if (match)
                {
                    return method;
                }
            }
            
            return null;
        }

        private static void Copy(string sourceDirectory, string targetDirectory)
        {
#if UNITY_EDITOR
            try
            {
                if (string.IsNullOrWhiteSpace(sourceDirectory) || string.IsNullOrWhiteSpace(targetDirectory))
                {
                    Logger.Error("Source or target directory path is empty", null);
                    return;
                }

                if (!Directory.Exists(sourceDirectory))
                {
                    Logger.Error($"Source directory does not exist: {sourceDirectory}", null);
                    return;
                }

                var source = new DirectoryInfo(sourceDirectory);
                var diTarget = new DirectoryInfo(targetDirectory);

                CopyAll(source, diTarget);
            }
            catch (Exception e)
            {
                Logger.Error($"Error copying directory from '{sourceDirectory}' to '{targetDirectory}': {e.Message}", null);
            }
#endif
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
#if UNITY_EDITOR
            try
            {
                // Create target directory if it doesn't exist
                if (!Directory.Exists(target.FullName))
                {
                    Directory.CreateDirectory(target.FullName);
                }

                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    if (fi.Extension != ".meta")
                    {
                        try
                        {
                            string destFile = Path.Combine(target.FullName, fi.Name);
                            fi.CopyTo(destFile, true);
                        }
                        catch (Exception e)
                        {
                            Logger.Warning($"Failed to copy file '{fi.Name}': {e.Message}", null);
                        }
                    }
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    try
                    {
                        DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                        CopyAll(diSourceSubDir, nextTargetSubDir);
                    }
                    catch (Exception e)
                    {
                        Logger.Warning($"Failed to copy subdirectory '{diSourceSubDir.Name}': {e.Message}", null);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CopyAll: {e.Message}", null);
            }
#endif
        }
        
        private static void FrameNonRealvirtualObjects(SceneView sceneView)
        {
            try
            {
                // Get all root objects in the scene
                var rootObjects = new List<GameObject>();
                var scene = SceneManager.GetActiveScene();
                scene.GetRootGameObjects(rootObjects);
                
                var objectsToFrame = new List<GameObject>();
                
                // Check each root object
                foreach (var rootObj in rootObjects)
                {
                    if (rootObj != null && rootObj.activeInHierarchy)
                    {
                        // Check if this root object or any of its parents/children have realvirtualController
                        bool hasController = HasRealvirtualControllerInHierarchy(rootObj.transform);
                        
                        if (!hasController)
                        {
                            // This root object and its entire hierarchy are free of realvirtualController
                            objectsToFrame.Add(rootObj);
                        }
                    }
                }
                
                // Frame the objects using bounds calculation
                if (objectsToFrame.Count > 0)
                {
                    // Calculate combined bounds
                    Bounds? combinedBounds = null;
                    foreach (var obj in objectsToFrame)
                    {
                        var renderers = obj.GetComponentsInChildren<Renderer>();
                        foreach (var renderer in renderers)
                        {
                            if (renderer.enabled)
                            {
                                if (combinedBounds == null)
                                {
                                    combinedBounds = renderer.bounds;
                                }
                                else
                                {
                                    var bounds = combinedBounds.Value;
                                    bounds.Encapsulate(renderer.bounds);
                                    combinedBounds = bounds;
                                }
                            }
                        }
                    }
                    
                    // Frame the calculated bounds
                    if (combinedBounds.HasValue && combinedBounds.Value.size.magnitude > 0.01f)
                    {
                        // Use Frame with instant=true to avoid animation
                        sceneView.Frame(combinedBounds.Value, true);
                        
                        // Also try LookAt as alternative
                        sceneView.LookAt(combinedBounds.Value.center, sceneView.rotation, combinedBounds.Value.size.magnitude * 0.5f, false, true);
                    }
                    else
                    {
                        // Fallback to selection-based framing
                        Selection.objects = objectsToFrame.ToArray();
                        sceneView.FrameSelected();
                        Selection.activeObject = null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Could not frame non-realvirtual objects: {e.Message}", null);
            }
        }
        
        private static bool HasRealvirtualControllerInHierarchy(Transform transform)
        {
            // Check this object
            if (transform.GetComponent<realvirtualController>() != null)
                return true;
            
            // Check all children recursively
            foreach (Transform child in transform)
            {
                if (HasRealvirtualControllerInHierarchy(child))
                    return true;
            }
            
            return false;
        }
        
        private static void SetSceneViewCameraOrientation()
        {
            try
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null)
                {
                    sceneView = EditorWindow.GetWindow<SceneView>();
                }
                
                if (sceneView != null)
                {
                    // Set scene view camera with Y pointing up and X pointing to the back
                    // Rotation: looking from front-right with X axis going away from camera
                    sceneView.rotation = Quaternion.Euler(30f, 45f, 0f);
                    
                    // Set a reasonable pivot point and size if the scene is empty or small
                    if (sceneView.pivot.magnitude < 0.1f || float.IsNaN(sceneView.size))
                    {
                        sceneView.pivot = new Vector3(0, 2, 0);
                        sceneView.size = 10f;
                    }
                    
                    // Ensure orthographic mode is off for better 3D visualization
                    sceneView.orthographic = false;
                    
                    // Force a repaint to apply changes
                    sceneView.Repaint();
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Could not set scene view camera orientation: {e.Message}", null);
            }
        }
        
        private static void CollapseHierarchy()
        {
            try
            {
                // Use reflection to access the SceneHierarchyWindow
                var hierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                if (hierarchyWindowType == null)
                {
                    // Try alternative name
                    hierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchy");
                }
                
                if (hierarchyWindowType != null)
                {
                    // Get the hierarchy window
                    var hierarchyWindow = EditorWindow.GetWindow(hierarchyWindowType);
                    if (hierarchyWindow != null)
                    {
                        // Try to find the method to collapse all
                        var collapseAllMethod = hierarchyWindowType.GetMethod("CollapseAll", 
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        
                        if (collapseAllMethod != null)
                        {
                            collapseAllMethod.Invoke(hierarchyWindow, null);
                        }
                        else
                        {
                            // Alternative approach: manipulate the tree view state
                            var sceneHierarchyField = hierarchyWindowType.GetField("m_SceneHierarchy", 
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            
                            if (sceneHierarchyField != null)
                            {
                                var sceneHierarchy = sceneHierarchyField.GetValue(hierarchyWindow);
                                if (sceneHierarchy != null)
                                {
                                    var sceneHierarchyType = sceneHierarchy.GetType();
                                    var treeViewField = sceneHierarchyType.GetField("m_TreeView", 
                                        BindingFlags.Instance | BindingFlags.NonPublic);
                                    
                                    if (treeViewField != null)
                                    {
                                        var treeView = treeViewField.GetValue(sceneHierarchy);
                                        if (treeView != null)
                                        {
                                            var collapseAllTreeMethod = treeView.GetType().GetMethod("CollapseAll", 
                                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                            
                                            if (collapseAllTreeMethod != null)
                                            {
                                                collapseAllTreeMethod.Invoke(treeView, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Force repaint
                        hierarchyWindow.Repaint();
                    }
                }
                
                // Force a refresh of the hierarchy
                EditorApplication.RepaintHierarchyWindow();
            }
            catch (Exception e)
            {
                Logger.Warning($"Could not collapse hierarchy: {e.Message}", null);
            }
        }
        
        public static void SetupRealvirtualToolbar()
        {
            try
            {
                // Enable QuickEdit overlay and make it visible
                EditorPrefs.SetBool("realvirtual_QuickEditVisible", true);
                
                // Show QuickToggle
                QuickToggle.ShowQuickToggle(true);
                
                // Force the QuickEdit overlay to be displayed (unhidden)
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null)
                {
                    sceneView = EditorWindow.GetWindow<SceneView>();
                }
                
                if (sceneView != null)
                {
#if UNITY_2021_2_OR_NEWER
                    // Configure overlay visibility states
                    try
                    {
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
                            if (overlay != null)
                            {
                                // Open realvirtual Toolbar overlay
                                if (overlay.GetType().Name == "RealvirtualToolbarOverlay" || 
                                    overlay.id.Contains("realvirtual Toolbar") ||
                                    overlay.displayName.Contains("realvirtual Toolbar"))
                                {
                                    overlay.displayed = true;
                                    overlay.collapsed = false;
                                }
                                // Open realvirtual Quick Edit overlay
                                else if (overlay.GetType().Name == "QuickEditOverlay" || 
                                         overlay.GetType().Name == "QuickEdit" ||
                                         overlay.id.Contains("QuickEdit") || 
                                         overlay.displayName.Contains("Quick Edit"))
                                {
                                    overlay.displayed = true;
                                    overlay.collapsed = false;
                                }
                                // Keep MovePivot overlay closed
                                else if (overlay.GetType().Name == "MovePivotOverlay" ||
                                         overlay.id.Contains("MovePivot") || 
                                         overlay.displayName.Contains("Move Pivot"))
                                {
                                    overlay.displayed = false;
                                }
                            }
                        }
#else
                        if (overlays != null)
                        {
                            foreach (var overlayObj in overlays)
                            {
                                if (overlayObj != null)
                                {
                                    // Use reflection for Unity 2022 property access
                                    var idProperty = overlayObj.GetType().GetProperty("id");
                                    var displayNameProperty = overlayObj.GetType().GetProperty("displayName");
                                    var displayedProperty = overlayObj.GetType().GetProperty("displayed");
                                    var collapsedProperty = overlayObj.GetType().GetProperty("collapsed");
                                    
                                    var id = idProperty?.GetValue(overlayObj)?.ToString() ?? "";
                                    var displayName = displayNameProperty?.GetValue(overlayObj)?.ToString() ?? "";
                                    var typeName = overlayObj.GetType().Name;
                                    
                                    // Open realvirtual Toolbar overlay
                                    if (typeName == "RealvirtualToolbarOverlay" || 
                                        id.Contains("realvirtual Toolbar") ||
                                        displayName.Contains("realvirtual Toolbar"))
                                    {
                                        displayedProperty?.SetValue(overlayObj, true);
                                        collapsedProperty?.SetValue(overlayObj, false);
                                    }
                                    // Open realvirtual Quick Edit overlay
                                    else if (typeName == "QuickEditOverlay" || 
                                             typeName == "QuickEdit" ||
                                             id.Contains("QuickEdit") || 
                                             displayName.Contains("Quick Edit"))
                                    {
                                        displayedProperty?.SetValue(overlayObj, true);
                                        collapsedProperty?.SetValue(overlayObj, false);
                                    }
                                    // Keep MovePivot overlay closed
                                    else if (typeName == "MovePivotOverlay" ||
                                             id.Contains("MovePivot") || 
                                             displayName.Contains("Move Pivot"))
                                    {
                                        displayedProperty?.SetValue(overlayObj, false);
                                    }
                                }
                            }
                        }
#endif
                    }
                    catch { }
#endif
                    sceneView.Repaint();
                }
                
                // Force initialization of the toolbar IMGUI if needed
                var quickEditToolbarType = typeof(ProjectSettingsTools).Assembly.GetType("realvirtual.QuickEditToolbarIMGUI");
                if (quickEditToolbarType != null)
                {
                    var forceRefreshMethod = quickEditToolbarType.GetMethod("ForceRefresh", BindingFlags.Public | BindingFlags.Static);
                    if (forceRefreshMethod != null)
                    {
                        forceRefreshMethod.Invoke(null, null);
                    }
                    
                    // Also try to trigger initialization
                    var onUpdateMethod = quickEditToolbarType.GetMethod("OnUpdate", BindingFlags.Public | BindingFlags.Static);
                    if (onUpdateMethod != null)
                    {
                        onUpdateMethod.Invoke(null, null);
                    }
                }
                
                // Ensure project path visibility setting is applied
                if (EditorPrefs.GetBool("realvirtual_ShowProjectPath", true))
                {
                    // Force a toolbar refresh
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Could not fully setup realvirtual toolbar: {e.Message}", null);
            }
        }
        
        
        
        private static void SetSceneViewStateField(Type stateType, object sceneViewState, string fieldName, object value)
        {
            try
            {
                var field = stateType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(sceneViewState, value);
                }
            }
            catch { }
        }
        
        private static void SetSceneViewStateProperty(Type stateType, object sceneViewState, string propertyName, object value)
        {
            try
            {
                var property = stateType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(sceneViewState, value);
                }
            }
            catch { }
        }
        
    }
}