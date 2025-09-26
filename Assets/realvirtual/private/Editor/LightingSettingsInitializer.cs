// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace realvirtual
{
    [InitializeOnLoad]
    public static class LightingSettingsInitializer
    {
        static LightingSettingsInitializer()
        {
            SetLightingSettingsAsset();
        }

        private static void SetLightingSettingsAsset()
        {
            // Reference the LightingSettings asset here
            /*var lightingSettingsAsset = AssetDatabase.LoadAssetAtPath<LightingSettings>("Assets/realvirtual/Settings/Realvirtual Lighting Settings.lighting");

            if (lightingSettingsAsset != null)
            {
                // Apply the lighting settings to the currently open scene
                var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                Lightmapping.lightingSettings = lightingSettingsAsset;

                Debug.Log($"Lighting settings for scene '{currentScene.name}' have been set to {lightingSettingsAsset.name}.");

                EnvironmentController envController = GameObject.Find("Environments").GetComponent<EnvironmentController>();

                envController.UpdateEnvironment();
            }
            else
            {
                Debug.LogError("Lighting settings asset not found. Please check the asset path.");
            }*/
        }
    }
}
