// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_6000_0_OR_NEWER

using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace realvirtual.RendererFeatures
{
    public class MultiObjectOverlayRendererFeature : ScriptableRendererFeature
    {
        public Color color = Color.white;

        public enum Mode
        {
            Default,
            XRay
        }

        public Mode mode;
        [SerializeField] private RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingTransparents;


        public Material overlayMaterialHighlight;
        public Material overlayMaterialXRay;
        
        private Material overlayMaterial; 


        private MultiObjectOverlayPass _overlayPass;

        private Renderer[] _targetRenderers;


        private Mode _currentMode = Mode.Default;

        public void SetRenderers(Renderer[] targetRenderers)
        {
            _targetRenderers = targetRenderers;

            if (_overlayPass != null)
                _overlayPass.Renderers = _targetRenderers;
        }

        public override void Create()
        {
            // Pass in constructor variables which don't/shouldn't need to be updated every frame.
            _overlayPass = new MultiObjectOverlayPass();

            _currentMode = mode;

            overlayMaterial = GetMaterial();
        }

        Material GetMaterial()
        {
            switch (_currentMode)
            {
                case Mode.Default:
                    return overlayMaterialHighlight;
                case Mode.XRay:
                    return overlayMaterialXRay;
                default:
                    return overlayMaterialHighlight;
            }
        }

        void CheckSwitchShader()
        {
            if (mode != _currentMode)
            {
                _currentMode = mode;
                overlayMaterial = GetMaterial();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_overlayPass == null)
                return;


            if (!overlayMaterial ||
                _targetRenderers == null ||
                _targetRenderers.Length == 0)
            {
                // Don't render the effect if there's nothing to render
                return;
            }

            // Any variables you may want to update every frame should be set here.
            _overlayPass.RenderEvent = renderEvent;

            CheckSwitchShader();
            overlayMaterial.SetColor("_BaseColor", color);

            _overlayPass.OverlayMaterial = overlayMaterial;

            _overlayPass.Renderers = _targetRenderers;

            renderer.EnqueuePass(_overlayPass);
        }
    }
}

#endif // UNITY_6000_0_OR_NEWER