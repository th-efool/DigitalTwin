#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace RenderPipeLineSetup{


// This attribute makes the static constructor of the class to be called as soon as the Unity editor loads.
[InitializeOnLoad]
public class RenderPipelineInitializer
{

    // Static constructor
    static RenderPipelineInitializer()
    {
        // Code here will be executed when Unity starts or when scripts are recompiled.
        Initialize();
    }

    private static void Initialize()
    {
        // Initialization code here
        
        RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
        if (status != null)
        {
            if(status.state == 0){
                RenderPipelineTools.SetDefaultRenderPipelineAsset(status.pipeline);
                EditorApplication.update += ProgressMaterialUpdate;
            }
        }
       
    }

    private static void ProgressMaterialUpdate(){
        RenderPipelineStatus status = Resources.Load<RenderPipelineStatus>("RenderPipelineStatus");
        if(status.state < 1){

            status.state += 1;
        }else{

            


            EditorApplication.update -= ProgressMaterialUpdate;
            status.state = -1;
            RenderPipelineTools.UpgradeMaterials(status.pipeline);
            RenderPipelineTools.UpgradeEnvironment(status.pipeline, status.mode);
        }
        
    }
}

}
#endif
