// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using realvirtual;

namespace RenderPipeLineSetup{

    public static class EnvironmentUpgrade
    {
        
        public static void Apply(RenderPipeline targetPipeline, HDRPRenderMode mode)
        {
            GameObject hdrp_ray = GameObject.Find("Environment Ray Tracing");
            GameObject hdrp_path = GameObject.Find("Environment Path Tracing");
            GameObject urp = GameObject.Find("Environment Universal");
            GameObject urpnew = GameObject.Find("Environments");

            if(targetPipeline == RenderPipeline.Standard){
                if(hdrp_ray != null){
                    GameObject.DestroyImmediate(hdrp_ray);
                }
                if(hdrp_path != null){
                    GameObject.DestroyImmediate(hdrp_path);
                }
                if(urp != null){
                    GameObject.DestroyImmediate(urp);
                }

                return;
                
            }
            
            
            if(targetPipeline == RenderPipeline.Universal){
                if(hdrp_ray != null){
                    GameObject.DestroyImmediate(hdrp_ray);
                }
                if(hdrp_path != null){
                    GameObject.DestroyImmediate(hdrp_path);
                }
                if(urp == null && urpnew == null){ 
                    urp = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("URP/Prefabs/Environment Universal")) as GameObject;
                }

            }else if(targetPipeline == RenderPipeline.HighDefinition && mode == HDRPRenderMode.RayTracing){
                if(urp != null){
                    GameObject.DestroyImmediate(urp);
                }
                if(hdrp_path != null){
                    GameObject.DestroyImmediate(hdrp_path);
                }
                if(hdrp_ray == null){
                    hdrp_ray = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("HDRP/Prefabs/Environment Ray Tracing")) as GameObject;
                }

            }else if(targetPipeline == RenderPipeline.HighDefinition && mode == HDRPRenderMode.PathTracing){
                if(urp != null){
                    GameObject.DestroyImmediate(urp);
                }
                if(hdrp_ray != null){
                    GameObject.DestroyImmediate(hdrp_ray);
                }
                if(hdrp_path == null){
                    hdrp_path = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("HDRP/Prefabs/Environment Path Tracing")) as GameObject;
                }
            }

            
        }

        public static void DisableRealvirtualVisualSettings()
        {
            realvirtualController controller = GameObject.Find("realvirtual").GetComponent<realvirtualController>();
            if(controller != null){
                foreach (Light light in controller.gameObject.GetComponentsInChildren<Light>()){
                    light.gameObject.SetActive(false);
                    
                }

                // camera setup
                
                foreach (Camera camera in controller.gameObject.GetComponentsInChildren<Camera>()){
                    camera.clearFlags = CameraClearFlags.Skybox;
                 
                }
            }
        }

        public static void RestoreLightIntensities(){
            GameObject go = GameObject.Find("realvirtual");


            if(go == null){
                return;
            }


            realvirtualController controller = go.GetComponent<realvirtualController>();
            if(controller != null){
                foreach (Light light in controller.gameObject.GetComponentsInChildren<Light>(true)){
                    light.gameObject.SetActive(true);
                    Debug.Log("[EnvironmentSetup] Restoring light :  " + light.gameObject.name);


                    float intensity = 1;
                    if(light.gameObject.name == "Main Light"){
                        intensity = 2;
                    }else if(light.gameObject.name == "SecondLight"){
                        intensity = 1.5f;
                    }

                    light.intensity = intensity;
                }

            
               
            }
        }

        public static void RestoreRealvirtualController(){
            
            GameObject go = GameObject.Find("realvirtual");
            
            if(go == null){
                return;
            }


            realvirtualController controller = go.GetComponent<realvirtualController>();
            if(controller != null){
                foreach (Light light in controller.gameObject.GetComponentsInChildren<Light>(true)){
                    light.gameObject.SetActive(true);
                    Debug.Log("[EnvironmentSetup] Enabling light :  " + light.gameObject.name);


                }

                // camera setup
                
                foreach (Camera camera in controller.gameObject.GetComponentsInChildren<Camera>(true)){
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    Debug.Log("[EnvironmentSetup] Initializing camera :  " + camera.gameObject.name);

                    

                    
                }
                
               
            }
        }

        
    }
}
#endif
