#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace RenderPipeLineSetup{

    public static class DenoisingInstaller
    {
        private static AddRequest addRequest;

        public static bool isInstalled;
        public static bool isInstalling;



        public static void InstallPackage()
        {
            EditorUtility.DisplayProgressBar("Installing Denoising Package", "Please wait...", 0f);

            // Specify the package name and version.
            // You can omit the version to get the latest compatible version.
            string packageName = "com.unity.rendering.denoising";

            
            // Starts the package installation process.

            addRequest = Client.Add(packageName);
            EditorApplication.update += ProgressAddRequest;
            isInstalling = true;
        }

        private static void ProgressAddRequest()
        {
            


            if (addRequest.IsCompleted)
            {
                EditorUtility.ClearProgressBar();

                if (addRequest.Status == StatusCode.Success)
                {
                    

                    EditorApplication.update -= ProgressAddRequest;
                        //EditorUtility.DisplayDialog("Package Installer", PipeLineString(targetPipeline) + " RP Package installed successfully.", "OK");
                    
                    isInstalling = false;
                    isInstalled = true;
                    

                }
                else if (addRequest.Status >= StatusCode.Failure)
                {
                    EditorUtility.DisplayDialog("Package Installer", $"Failed to install Denoising Package: {addRequest.Error.message}", "OK");
                    

                    EditorApplication.update -= ProgressAddRequest;
                }

                

                
            }
        }

       
    }

}
#endif
