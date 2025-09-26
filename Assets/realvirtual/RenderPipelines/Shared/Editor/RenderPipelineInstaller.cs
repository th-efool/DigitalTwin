// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#pragma warning disable 0414
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace RenderPipeLineSetup{

    public static class RenderPipelineInstaller
    {
        private static AddRequest addRequest;

        private static RenderPipeline targetPipeline;

        private static bool succeeded;
        
        public static void InstallPackageURP()
        {
            targetPipeline = RenderPipeline.Universal;
            EditorUtility.DisplayProgressBar("Installing URP Package", "Please wait...", 0f);

            // Specify the package name and version.
            // You can omit the version to get the latest compatible version.
            string packageName = "com.unity.render-pipelines.universal";

            RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
            status.pipeline = RenderPipeline.Universal;
            status.state = 0;

            succeeded = false;
            
            // Starts the package installation process.

            addRequest = Client.Add(packageName);
            EditorApplication.update += ProgressAddRequest;
        }

        public static void InstallPackageHDRP()
        {
            targetPipeline = RenderPipeline.HighDefinition;
            EditorUtility.DisplayProgressBar("Installing HDRP Package", "Please wait...", 0f);

            // Specify the package name and version.
            // You can omit the version to get the latest compatible version.
            string packageName = "com.unity.render-pipelines.high-definition";

            RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
            status.pipeline = RenderPipeline.HighDefinition;
            status.state = 0;
            succeeded = false;
            
            // Starts the package installation process.
            addRequest = Client.Add(packageName);
            
            //queuedPackage = "com.unity.rendering.denoising";

            EditorApplication.update += ProgressAddRequest;
        }

        private static string PipeLineString(RenderPipeline pipeline){
            if(pipeline == RenderPipeline.Universal){
                return "Universal";
            }
            if(pipeline == RenderPipeline.HighDefinition){
                return "High Definition";
            }

            return "UNKNOWN";
        }

        private static void ProgressAddRequest()
        {
            


            if (addRequest.IsCompleted)
            {
                EditorUtility.ClearProgressBar();

                if (addRequest.Status == StatusCode.Success)
                {
                    

                    EditorApplication.update -= ProgressAddRequest;
                    EditorUtility.DisplayDialog("Package Installer", PipeLineString(targetPipeline) + " RP Package installed successfully.", "OK");
                    

                    

                }
                else if (addRequest.Status >= StatusCode.Failure)
                {
                    EditorUtility.DisplayDialog("Package Installer", $"Failed to install "+PipeLineString(targetPipeline)+" RP Package: {addRequest.Error.message}", "OK");
                    EditorUtility.DisplayDialog(
                        "Install "+PipeLineString(targetPipeline)+" Render Pipeline",
                        PipeLineString(targetPipeline) + " Render Pipeline not installed. Please install the "+PipeLineString(targetPipeline)+" Render Pipeline package via the Package Manager.",
                        "OK"
                    );

                    EditorApplication.update -= ProgressAddRequest;
                }

                

                
            }
        }

       
    }

}
#endif
