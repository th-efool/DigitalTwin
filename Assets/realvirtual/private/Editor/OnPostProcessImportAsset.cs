// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

#if !UNITY_CLOUD_BUILD
namespace realvirtual
{
    class MyAllPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Sets scripting define symbols with Unity version compatibility (duplicate of ProjectSettingsUnity2022.SetDefine)
        /// </summary>
        /// <param name="defineSymbol">Define symbol to set</param>
        private static void SetDefineSymbol(string defineSymbol)
        {
            if (string.IsNullOrWhiteSpace(defineSymbol))
            {
                Logger.Warning($"Cannot set empty or null define symbol", null);
                return;
            }

            try
            {
#if UNITY_2021_2_OR_NEWER
                var currtarget = EditorUserBuildSettings.selectedBuildTargetGroup;
                var nametarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(currtarget);
                string symbols = PlayerSettings.GetScriptingDefineSymbols(nametarget);
#else
                var currtarget = EditorUserBuildSettings.selectedBuildTargetGroup;
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currtarget);
#endif
                
                // Split existing symbols and check if already exists
                var symbolList = symbols.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                
                if (!symbolList.Contains(defineSymbol))
                {
                    symbolList.Add(defineSymbol);
                    string newSymbols = string.Join(";", symbolList);
#if UNITY_2021_2_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(nametarget, newSymbols);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(currtarget, newSymbols);
#endif
                    // Define symbol added silently
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error setting define symbol '{defineSymbol}': {e.Message}", null);
            }
        }
        
        /// <summary>
        /// Verifies that ProjectSettingsTools.SetStandardSettings was actually executed
        /// </summary>
        private static void VerifyProjectSettingsExecution()
        {
            try
            {
                string callTime = EditorPrefs.GetString("realvirtual_SetStandardSettingsCalled", "");
                string successTime = EditorPrefs.GetString("realvirtual_SetStandardSettingsSuccess", "");
                string initTime = EditorPrefs.GetString("realvirtual_ProjectSettingsTools_InitTime", "");
                
                // Silent verification - only show problems
                if (string.IsNullOrEmpty(callTime))
                {
                    Logger.Warning("ProjectSettingsTools.SetStandardSettings was never called!");
                    TriggerFallbackSettings();
                }
                else if (string.IsNullOrEmpty(successTime))
                {
                    Logger.Warning("ProjectSettingsTools.SetStandardSettings was called but did not complete successfully!");
                    TriggerFallbackSettings();
                }
                // Success is silent - no logging needed
            }
            catch (System.Exception e)
            {
                Logger.Error($"Error verifying ProjectSettingsTools execution: {e.Message}");
                TriggerFallbackSettings();
            }
        }
        
        /// <summary>
        /// Triggers fallback settings application when ProjectSettingsTools fails
        /// </summary>
        private static void TriggerFallbackSettings()
        {
            try
            {
                // Apply essential fallback settings silently
                PlayerSettings.colorSpace = ColorSpace.Linear;
                
                // Time settings for industrial simulation
                Time.fixedDeltaTime = 0.02f; // 50 Hz physics update rate
                Time.maximumDeltaTime = 0.1f;
                
                // Physics settings
                Physics.defaultSolverIterations = 10;
                Physics.defaultSolverVelocityIterations = 2;
                Physics.autoSyncTransforms = true;
                Physics.defaultContactOffset = 0.01f;
                
                // Quality settings
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
            }
            catch (System.Exception e)
            {
                Logger.Error($"Error applying fallback settings: {e.Message}");
            }
        }
        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool Game4AutomationImport = false;
            bool GlobalCsImported = false;

            foreach (string str in importedAssets)
            {
                // Only trigger on specific important files
                if (str.Contains("Assets/realvirtual/private/Global.cs"))
                {
                    GlobalCsImported = true;
                    Game4AutomationImport = true;
                    break;
                }
                // Or if it's the initial import (checking for a core file)
                else if (str.Contains("Assets/realvirtual/private/Editor/realvirtual.editor.asmdef"))
                {
                    Game4AutomationImport = true;
                }
            }

#if !DEV
            if (Game4AutomationImport && !Application.isPlaying)
            {
                if (GlobalCsImported)
                    Logger.Message("Updating realvirtual");
                else
                    Logger.Message("Updating realvirtual - Initial import detected");
                
                // Disable Interact
                string MenuName = "realvirtual/Enable Interact (Pro)";
                EditorPrefs.SetBool(MenuName, false);
                
                // Safe call to ProjectSettingsTools with comprehensive verification
                try
                {
                    // Verify ProjectSettingsTools is available before calling
                    var projectSettingsType = typeof(ProjectSettingsTools);
                    if (projectSettingsType != null)
                    {
                        // Call ProjectSettingsTools silently
                        ProjectSettingsTools.SetStandardSettings(false);
                        
                        // Verify execution with delay (allow time for completion)
                        EditorApplication.delayCall += () =>
                        {
                            VerifyProjectSettingsExecution();
                        };
                    }
                    else
                    {
                        Logger.Warning("ProjectSettingsTools type not found during OnPostprocessAllAssets");
                        TriggerFallbackSettings();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error($"Error calling ProjectSettingsTools.SetStandardSettings: {e.Message}");
                    Logger.Message("Retrying with delay and fallback...");
                    
                    // Retry with delay if initial call fails
                    EditorApplication.delayCall += () =>
                    {
                        try
                        {
                            ProjectSettingsTools.SetStandardSettings(false);
                            // Settings applied successfully after retry
                            
                            // Verify after retry
                            EditorApplication.delayCall += () =>
                            {
                                VerifyProjectSettingsExecution();
                            };
                        }
                        catch (System.Exception retryException)
                        {
                            Logger.Error($"Failed to apply standard settings even after retry: {retryException.Message}");
                            Logger.Message("Triggering fallback settings application");
                            TriggerFallbackSettings();
                        }
                    };
                }
                
                // Set essential compiler defines as backup (duplicate to ensure they're set)
                SetDefineSymbol("REALVIRTUAL");
                
                // Check and set Professional version define
                if (AssetDatabase.IsValidFolder("Assets/realvirtual/Professional"))
                {
                    SetDefineSymbol("REALVIRTUAL_PROFESSIONAL");
                }
                
                // Check and set Playmaker define
                if (AssetDatabase.IsValidFolder("Assets/Playmaker"))
                {
                    SetDefineSymbol("REALVIRTUAL_PLAYMAKER");
                }

                EditorApplication.delayCall += () =>
                {
                    var window = ScriptableObject.CreateInstance<HelloWindow>();
                    window.Open();
                };
                
                // Delete old QuickToggle Location if existant
                if (Directory.Exists("Assets/realvirtual/private/Editor/QuickToggle"))
                {
                    Directory.Delete("Assets/realvirtual/private/Editor/QuickToggle",true);
                }
                
                // Delete old Planner if existant
                if (Directory.Exists("Assets/realvirtual/private/Planner"))
                {
                    Directory.Delete("Assets/realvirtual/private/Planner",true);
                }
               
            }
#endif

        }
    }
}
#endif