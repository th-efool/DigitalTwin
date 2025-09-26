
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RenderPipeLineSetup{

    public static class MaterialUpgrade
    {
        
        public static void Apply(RenderPipeline targetPipeline)
        {
            RenderPipelineShaderMapping mapping = Resources.Load<RenderPipelineShaderMapping>("ShaderMapping");
            RenderPipeline currentPipeline = RenderPipelineTools.GetActivePipeline();
            mapping.BuildMap(currentPipeline, targetPipeline);
            UpgradeAllMaterials(mapping);

            
        }

        private static void UpgradeAllMaterials(RenderPipelineShaderMapping mapping)
        {
            UpgradeProjectMaterials(mapping);
            UpgradeSceneMaterials(mapping);
        }


        private static void UpdateMaterial(Material material, RenderPipelineShaderMapping mapping){

            if(!mapping.map.ContainsKey(material.shader.name)){
                return;
            }

            // Fetch Properties

            Color color = GetColor(material);
            Texture texture = GetTexture(material);


            
            Shader newShader = Shader.Find(mapping.map[material.shader.name]);

            if (newShader == null)
            {
                return;
            }
            
            if (newShader.name == material.shader.name)
            {
                return;
            }
           
           
           
           // Check if the material is a variant
           if (material.shader != newShader)
           {
               // Update shader only if it's not a variant
               material.shader = newShader;
           }


            // Standard
            material.SetColor("_Color", color);
            material.SetTexture("_MainTex", texture);

            // Universal
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", texture);
            
            // High Definition
            material.SetTexture("_BaseColorMap", texture);

            
        }

        private static Color GetColor(Material material){

            string[] definitions = new string[]{
                "_Color",
                "_BaseColor"
            };

            for (int i = 0; i < definitions.Length; i++)
            {
                if(material.HasProperty(definitions[i])){
                    return material.GetColor(definitions[i]);
                }
            }

            return Color.white;

           
        }

        private static Texture GetTexture(Material material)
        {
            string[] definitions = new string[]
            {
                "_MainTex",     // Standard naming convention for main texture in Unity's standard shader
                "_BaseMap",     // Commonly used in URP for the base (albedo) texture
                "_BaseColorMap" // Sometimes used in HDRP or custom shaders for the base (albedo) texture
            };

            for (int i = 0; i < definitions.Length; i++)
            {
                if (material.HasProperty(definitions[i]))
                {
                    return material.GetTexture(definitions[i]);
                }
            }

            return null;
        }

    
        private static void UpgradeProjectMaterials(RenderPipelineShaderMapping mapping)
        {
           
            // Find all Material assets in the project
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");
            int i = 0;
            foreach (string guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

                

                if (material != null)
                {
                    UpdateMaterial(material, mapping);
                }

                EditorUtility.DisplayProgressBar("Upgrading Project Materials", $"Processing {path}", (float)i / materialGuids.Length);
                i +=1;

            }

            EditorUtility.ClearProgressBar();

            Debug.Log("[MaterialUpgrade] Project material upgrade process complete.");
        }



        

        private static void UpgradeSceneMaterials(RenderPipelineShaderMapping mapping)
        {
            

            EditorUtility.DisplayProgressBar("Upgrading Scene Materials", "",1);

            // Iterate through all GameObjects in the active scene
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.sharedMaterials) // Use sharedMaterials to affect prefabs too
                    {
                        if (material != null)
                        {
                            UpdateMaterial(material, mapping);
                        }
                    }


                }

                

            }

            // Mark the scene as dirty to ensure changes are saved
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            EditorUtility.ClearProgressBar();


            Debug.Log("[MaterialUpgrade] Scene material upgrade process complete.");
        }
    }
}
#endif
