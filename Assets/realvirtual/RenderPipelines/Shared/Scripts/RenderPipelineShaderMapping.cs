// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace RenderPipeLineSetup
{
    [CreateAssetMenu(fileName = "ShaderMapping", menuName = "ShaderMapping", order = 1)]
    public class RenderPipelineShaderMapping : ScriptableObject
    {
        public ShaderMap[] maps;

        public Dictionary<string, string> map;

        public void BuildMap(RenderPipeline currentPipeline, RenderPipeline targetPipeline)
        {
            map = new Dictionary<string, string>();

            foreach (var shaderMap in maps)
            {
                string currentShaderName = "";
                string targetShaderName = "";

                // Get the shader name for the current pipeline
                switch (currentPipeline)
                {
                    case RenderPipeline.Standard:
                        currentShaderName = shaderMap.standard;
                        break;
                    case RenderPipeline.Universal:
                        currentShaderName = shaderMap.universal;
                        break;
                    case RenderPipeline.HighDefinition:
                        currentShaderName = shaderMap.highDefinition;
                        break;
                }

                // Get the shader name for the target pipeline
                switch (targetPipeline)
                {
                    case RenderPipeline.Standard:
                        targetShaderName = shaderMap.standard;
                        break;
                    case RenderPipeline.Universal:
                        targetShaderName = shaderMap.universal;
                        break;
                    case RenderPipeline.HighDefinition:
                        targetShaderName = shaderMap.highDefinition;
                        break;
                }
                
                if(!map.ContainsKey(shaderMap.standard)){
                    map[shaderMap.standard] = targetShaderName;
                }
                if(!map.ContainsKey(shaderMap.universal)){
                    map[shaderMap.universal] = targetShaderName;
                }
                if(!map.ContainsKey(shaderMap.highDefinition)){
                    map[shaderMap.highDefinition] = targetShaderName;
                }
               
            }

        }
    }

    [System.Serializable]
    public class ShaderMap
    {
        public string standard;
        public string universal;
        public string highDefinition;
    }
}
#endif
