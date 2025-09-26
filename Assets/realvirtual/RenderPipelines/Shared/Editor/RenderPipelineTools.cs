// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;


namespace RenderPipeLineSetup{

    public enum RenderPipeline{
            Standard,
            Universal,
            HighDefinition,
            Unknown
    }

    public enum HDRPRenderMode{
            None,
            RayTracing,
            PathTracing
    }

    public static class RenderPipelineTools
    {
        public static bool AutoInstallPackages = true;

        private static ListRequest listRequest;

        

        private static RenderPipeline activePipeline;

        [MenuItem("realvirtual/Settings/Render Pipelines/Switch to Standard Render Pipeline (SRP)",false,502)]
        public static void SwitchToSRP(){
            if (!EditorUtility.DisplayDialog("Switch to Standard Render Pipeline", "Are you sure you want to switch to Standard Render Pipeline, this will change all your project and scene materials, please make a backup before processing ?", "Yes", "No"))
                return;
            
            EditorUtility.DisplayProgressBar("Switching to Standard Render Pipeline", "Please wait...", 0f);

            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();


            RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
            status.pipeline = RenderPipeline.Standard;
            status.mode = HDRPRenderMode.None;
            status.state = -1;

            SetDefaultRenderPipelineAsset(RenderPipeline.Standard);
            UpgradeMaterials(RenderPipeline.Standard);
            UpgradeEnvironment(RenderPipeline.Standard, HDRPRenderMode.None);


            EnvironmentUpgrade.RestoreRealvirtualController();


            
        }

        [MenuItem("realvirtual/Settings/Render Pipelines/Switch to Universal Render Pipeline (URP)", false, 503)]
        public static void SwitchToURP(){

            RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
            status.mode = HDRPRenderMode.None;
            
            // Write a global message box and asking if the user is sure to switch render pipleine
            // If the user clicks yes, then switch to URP
            if (!EditorUtility.DisplayDialog("Switch to URP Render Pipline", "Are you sure you want to switch to URP, this will install URP package if needed and change all your project and scene materials, please make a backup before processing ?", "Yes", "No"))
                return;
            EditorUtility.DisplayProgressBar("Looking for URP Package", "Please wait...", 0f);

            if(!HasPackage(RenderPipeline.Universal)){
                if(AutoInstallPackages){
                    RenderPipelineInstaller.InstallPackageURP();
                }else{
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(
                        "Install Universal Render Pipeline",
                        "Universal Render Pipeline not installed. Please install the Universal Render Pipeline package via the Package Manager.",
                        "OK"
                    );
                }

            }else{
                EditorUtility.ClearProgressBar();
                SetDefaultRenderPipelineAsset(RenderPipeline.Universal);
                UpgradeMaterials(RenderPipeline.Universal);
                UpgradeEnvironment(RenderPipeline.Universal, HDRPRenderMode.None);
            }
        }

        [MenuItem("realvirtual/Settings/Render Pipelines/Switch to HD render Pipeline (HDRP)/Path Tracing",false, 504)]
        public static void SwitchToHDRPPathTracing(){
            SwitchToHDRP(HDRPRenderMode.PathTracing);
        }

        [MenuItem("realvirtual/Settings/Render Pipelines/Switch to HD render Pipeline (HDRP)/Ray Tracing",false, 505)]
        public static void SwitchToHDRPRayTracing(){
            SwitchToHDRP(HDRPRenderMode.RayTracing);
        }

        public static void SwitchToHDRP(HDRPRenderMode mode){

            RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
            status.mode = mode;

            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();


            if (!EditorUtility.DisplayDialog("Switch to HDRP Render Pipeline", "Are you sure you want to switch to HDRP, this will install HDRP package if needed and change all your project and scene materials, please make a backup before processing ?", "Yes", "No"))
                return;

            EditorUtility.DisplayProgressBar("Installing Denoising Package", "Please wait...", 0f);

            if(mode == HDRPRenderMode.PathTracing){
                DenoisingInstaller.InstallPackage();
            }

            EditorUtility.DisplayProgressBar("Looking for HDRP Package", "Please wait...", 0f);

            if(!HasPackage(RenderPipeline.HighDefinition)){
                if(AutoInstallPackages){
                    RenderPipelineInstaller.InstallPackageHDRP();
                }else{
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(
                        "Install High Definition Render Pipeline",
                        "High Definition Render Pipeline not installed. Please install the High Definition Render Pipeline package via the Package Manager.",
                        "OK"
                    );
                }

            }else{
                EditorUtility.ClearProgressBar();
                SetDefaultRenderPipelineAsset(RenderPipeline.HighDefinition);
                UpgradeMaterials(RenderPipeline.HighDefinition);
                UpgradeEnvironment(RenderPipeline.HighDefinition, mode);
            }
        }

        public static void SetDefaultRenderPipelineAsset(RenderPipeline pipeline){
           
            if(pipeline == RenderPipeline.Standard){
                GraphicsSettings.defaultRenderPipeline = null;
            }

            if(pipeline == RenderPipeline.Universal){
                GraphicsSettings.defaultRenderPipeline = Resources.Load<RenderPipelineAsset>("URP/URP-Default");
            }

            if(pipeline == RenderPipeline.HighDefinition){
                GraphicsSettings.defaultRenderPipeline = Resources.Load<RenderPipelineAsset>("HDRP/HDRP-Default");
            }
        }

        public static void UpgradeMaterials(RenderPipeline pipeline){
            MaterialUpgrade.Apply(pipeline);
        }

        public static void UpgradeEnvironment(RenderPipeline pipeline, HDRPRenderMode mode){
            Debug.Log("[RenderPipelineTools] Upgrading Environment " + pipeline + " ,  " + mode);
            EnvironmentUpgrade.Apply(pipeline, mode);
            EnvironmentUpgrade.RestoreLightIntensities();
        }

        private static bool HasPackage(RenderPipeline pipeline){
            if(pipeline == RenderPipeline.Standard){
                return true;
            }
            if(pipeline == RenderPipeline.Universal){


                listRequest = Client.List(); // This starts the asynchronous operation
                while (!listRequest.IsCompleted) {} // Wait for the operation to complete

                if (listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name.Contains("render-pipelines.universal"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            if(pipeline == RenderPipeline.HighDefinition){
                listRequest = Client.List(); // This starts the asynchronous operation
                while (!listRequest.IsCompleted) {} // Wait for the operation to complete

                if (listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name.Contains("render-pipelines.high-definition"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            return true;
        }

        public static RenderPipeline GetActivePipeline(){

            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Debug.Log("Standard (Built-in) Render Pipeline is active.");
                return RenderPipeline.Standard;
            }
            else if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("Universal"))
            {
                Debug.Log("Universal Render Pipeline (URP) is active.");
                return RenderPipeline.Universal;
            }
            else if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HDRenderPipeline"))
            {
                Debug.Log("High Definition Render Pipeline (HDRP) is active.");
                return RenderPipeline.HighDefinition;
            }
            else
            {
                Debug.Log("Unknown Render Pipeline is active.");
                return RenderPipeline.Unknown;
            }
            

        }
    }
}
#endif