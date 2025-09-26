// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace realvirtual
{
    /// <summary>
    /// Unity 2022-specific project settings and compatibility utilities
    /// </summary>
    public static class ProjectSettingsUnity2022
    {
        /// <summary>
        /// Sets up the appropriate URP renderer based on Unity version for Unity 2022 compatibility
        /// </summary>
        public static void SetupURPRenderer()
        {
            try
            {
#if !UNITY_6000_0_OR_NEWER
                // Unity 2022 - use compatible renderer
                Logger.Message("Unity 2022 detected: Switching to Unity 2022 compatible renderer...", null);
                SwitchToUnity2022Renderer();
#else
                // Unity 6 - can use full renderer, but respect user preference
                // Note: Removed informational message about Unity 6 URP features to reduce console noise
#endif
            }
            catch (Exception e)
            {
                Logger.Warning($"Could not setup URP renderer: {e.Message}", null);
            }
        }
        
        /// <summary>
        /// Switches to Unity 2022 compatible renderer (without Unity 6-only features)
        /// </summary>
        public static void SwitchToUnity2022Renderer()
        {
            Logger.Message("Switching to Unity 2022 compatible URP pipeline and renderer...", null);
            
            // Step 1: Load Unity 2022 pipeline asset
            var unity2022Pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                "Assets/realvirtual/RenderPipelines/Resources/URP/URP-DefaultUnity2022.asset");
                
            if (unity2022Pipeline == null)
            {
                Logger.Warning("Unity 2022 compatible pipeline not found. Please ensure URP-DefaultUnity2022.asset exists.", null);
                return;
            }
            
            // Step 2: Load Unity 2022 renderer asset
            var unity2022Renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(
                "Assets/realvirtual/RenderPipelines/Resources/URP/Settings/URP-Default-Renderer2022.asset");
                
            if (unity2022Renderer == null)
            {
                Logger.Warning("Unity 2022 compatible renderer not found. Please ensure URP-Default-Renderer2022.asset exists.", null);
                return;
            }
            
            // Step 3: Set the Unity 2022 pipeline in Graphics Settings
            Logger.Message("Setting Unity 2022 URP pipeline in Graphics Settings...", null);
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = unity2022Pipeline;
            
            // Step 4: Ensure the Unity 2022 renderer is set in the pipeline
            Logger.Message("Setting Unity 2022 renderer in the pipeline...", null);
            SetRendererInPipeline(unity2022Renderer, "Unity 2022 compatible renderer (without Unity 6-only features)");
            
            Logger.Message("Successfully switched to Unity 2022 compatible URP pipeline and renderer", null);
        }
        
        /// <summary>
        /// Switches to full Unity 6 renderer (with all features)
        /// </summary>
        public static void SwitchToUnity6Renderer()
        {
            Logger.Message("Switching to Unity 6 URP pipeline and renderer...", null);
            
            // Step 1: Load Unity 6 pipeline asset
            var unity6Pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                "Assets/realvirtual/RenderPipelines/Resources/URP/URP-Default.asset");
                
            if (unity6Pipeline == null)
            {
                Logger.Warning("Unity 6 pipeline not found. Please ensure URP-Default.asset exists.", null);
                return;
            }
            
            // Step 2: Load Unity 6 renderer asset
            var unity6Renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(
                "Assets/realvirtual/RenderPipelines/Resources/URP/Settings/URP-Default-Renderer.asset");
                
            if (unity6Renderer == null)
            {
                Logger.Warning("Unity 6 renderer not found. Please ensure URP-Default-Renderer.asset exists.", null);
                return;
            }
            
            // Step 3: Set the Unity 6 pipeline in Graphics Settings
            Logger.Message("Setting Unity 6 URP pipeline in Graphics Settings...", null);
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = unity6Pipeline;
            
            // Step 4: Ensure the Unity 6 renderer is set in the pipeline
            Logger.Message("Setting Unity 6 renderer in the pipeline...", null);
            SetRendererInPipeline(unity6Renderer, "Full Unity 6 renderer (with all features)");
            
            Logger.Message("Successfully switched to Unity 6 URP pipeline and renderer", null);
        }
        
        /// <summary>
        /// Sets the specified renderer in the URP pipeline
        /// </summary>
        private static void SetRendererInPipeline(UniversalRendererData rendererData, string description)
        {
            Logger.Message("Attempting to set renderer in URP pipeline...", null);
            var urpAsset = UniversalRenderPipeline.asset;
            if (urpAsset == null)
            {
                Logger.Warning("No URP asset found in Graphics Settings. Please ensure URP is set as the render pipeline.", null);
                return;
            }
            
            Logger.Message($"URP asset found: {urpAsset.name}", null);
            
            // Use reflection to access the rendererDataList since it's internal
            var rendererDataListField = urpAsset.GetType().GetField("m_RendererDataList", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (rendererDataListField != null)
            {
                var rendererDataList = rendererDataListField.GetValue(urpAsset) as ScriptableRendererData[];
                if (rendererDataList != null && rendererDataList.Length > 0)
                {
                    Logger.Message($"Current renderer count: {rendererDataList.Length}", null);
                    Logger.Message($"Current renderer at index 0: {(rendererDataList[0] != null ? rendererDataList[0].name : "null")}", null);
                    
                    // Replace the first renderer (index 0)
                    rendererDataList[0] = rendererData;
                    rendererDataListField.SetValue(urpAsset, rendererDataList);
                    
                    EditorUtility.SetDirty(urpAsset);
                    AssetDatabase.SaveAssets();
                    
                    Logger.Message($"Successfully switched to {description}", null);
                    Logger.Message($"New renderer at index 0: {rendererData.name}", null);
                }
                else
                {
                    Logger.Warning("URP renderer data list is empty or null.", null);
                }
            }
            else
            {
                Logger.Warning("Could not access URP renderer data list via reflection.", null);
            }
        }
        
        /// <summary>
        /// Logs information about Unity 6-only components when running Unity 2022
        /// </summary>
        public static void CleanupUnity6OnlyComponents()
        {
#if !UNITY_6000_0_OR_NEWER
            Logger.Message("Unity 2022 detected: Some advanced render features are only available in Unity 6.", null);
            Logger.Message("Unity 6-only features will show warning messages in the Inspector.", null);
#endif
        }
        
        /// <summary>
        /// Sets scripting define symbols with Unity 2022/2021 compatibility
        /// </summary>
        /// <param name="mydefine">Define symbol to set</param>
        public static void SetDefine(string mydefine)
        {
            if (string.IsNullOrWhiteSpace(mydefine))
            {
                Logger.Warning("Cannot set empty or null define symbol", null);
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
                
                if (!symbolList.Contains(mydefine))
                {
                    symbolList.Add(mydefine);
                    string newSymbols = string.Join(";", symbolList);
#if UNITY_2021_2_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(nametarget, newSymbols);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(currtarget, newSymbols);
#endif
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error setting define symbol '{mydefine}': {e.Message}", null);
            }
        }

        /// <summary>
        /// Deletes scripting define symbols with Unity 2022/2021 compatibility
        /// </summary>
        /// <param name="mydefine">Define symbol to delete</param>
        public static void DeleteDefine(string mydefine)
        {
            if (string.IsNullOrWhiteSpace(mydefine))
            {
                Logger.Warning("Cannot delete empty or null define symbol", null);
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
                
                // Split existing symbols and remove the target
                var symbolList = symbols.Split(';').Where(s => !string.IsNullOrWhiteSpace(s) && s != mydefine).ToList();
                
                string newSymbols = string.Join(";", symbolList);
                if (newSymbols != symbols)
                {
#if UNITY_2021_2_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(nametarget, newSymbols);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(currtarget, newSymbols);
#endif
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error deleting define symbol '{mydefine}': {e.Message}", null);
            }
        }
        
        /// <summary>
        /// Sets player settings with Unity 2022 compatibility (scripting backend and API compatibility)
        /// </summary>
        public static void ConfigurePlayerSettings()
        {
            try
            {
                // Set scripting backend to Mono
#if UNITY_2021_2_OR_NEWER
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                PlayerSettings.SetScriptingBackend(namedBuildTarget, ScriptingImplementation.Mono2x);
                
                // Set API compatibility level to .NET Standard 2.0
                PlayerSettings.SetApiCompatibilityLevel(namedBuildTarget, ApiCompatibilityLevel.NET_Standard_2_0);
#else
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, ScriptingImplementation.Mono2x);
                
                // Set API compatibility level to .NET Standard 2.0
                PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_Standard_2_0);
#endif
                
                // Allow application to run in background
                PlayerSettings.runInBackground = true;
                
                // Make application visible in background
                PlayerSettings.visibleInBackground = true;
            }
            catch (Exception e)
            {
                Logger.Error($"Error configuring player settings: {e.Message}", null);
            }
        }
        
        /// <summary>
        /// Test method to debug URP renderer switching
        /// </summary>
        [UnityEditor.MenuItem("realvirtual DEV/Debug/Test Unity 2022 Renderer Switch")]
        public static void TestUnity2022RendererSwitch()
        {
            Logger.Message("=== Testing Unity 2022 Renderer Switch ===", null);
            SetupURPRenderer();
            Logger.Message("=== Test Complete ===", null);
        }
        
    }
}