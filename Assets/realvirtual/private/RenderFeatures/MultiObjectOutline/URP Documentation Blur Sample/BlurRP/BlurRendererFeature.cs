// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_6000_0_OR_NEWER

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace realvirtual
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private BlurSettings settings;
        [SerializeField] private Shader shader;
        private Material material;
        private BlurRenderPass blurRenderPass;
    
        public override void Create()
        {
            if (shader == null)
            {
                return;
            }
            material = new Material(shader);
            blurRenderPass = new BlurRenderPass(material, settings);
            
            blurRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
    
        public override void AddRenderPasses(ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                renderer.EnqueuePass(blurRenderPass);
            }
        }
    
        protected override void Dispose(bool disposing)
        {
    #if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
    #else
                    Destroy(material);
    #endif
        }
    }
    
    [Serializable]
    public class BlurSettings
    {
        [Range(0, 0.4f)] public float horizontalBlur;
        [Range(0, 0.4f)] public float verticalBlur;
    }
}

#endif // UNITY_6000_0_OR_NEWER
