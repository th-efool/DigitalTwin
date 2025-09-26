// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_6000_0_OR_NEWER

using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace realvirtual.RendererFeatures
{
    public class MultiObjectOverlayPass : ScriptableRenderPass
    {
        private const string DrawOutlineOverlayPassName = "DrawOverlayObjectsPass";

        public RenderPassEvent RenderEvent { private get; set; }
        public Material OverlayMaterial { private get; set; }
        public Renderer[] Renderers { get; set; }


        public MultiObjectOverlayPass()
        {
        }

        public override void RecordRenderGraph(RenderGraph renderGraph,
            ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            // The following line ensures that the render pass doesn't blit
            // from the back buffer.
            if (resourceData.isActiveTargetBackBuffer)
                return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Update Settings
            renderPassEvent = RenderEvent;

            var screenColorHandle = resourceData.activeColorTexture;
            var screenDepthStencilHandle = resourceData.activeDepthTexture;

            // This check is to avoid an error from the material preview in the scene
            if (!screenColorHandle.IsValid() ||
                !screenDepthStencilHandle.IsValid())
                return;

            // Draw objects-to-outline pass
            using (var builder = renderGraph.AddRasterRenderPass<RenderObjectsPassData>(DrawOutlineOverlayPassName,
                       out var passData))
            {
                // Configure pass data
                passData.Renderers = Renderers;
                passData.Material = OverlayMaterial;

                // Draw to the screen color texture
                builder.SetRenderAttachment(screenColorHandle, 0);

                // Blit from the source color to destination color,
                // using the first shader pass.
                builder.SetRenderFunc((RenderObjectsPassData data, RasterGraphContext context) =>
                    ExecuteDrawOverlayObjects(data, context));
            }
        }

        private static void ExecuteDrawOverlayObjects(
            RenderObjectsPassData data,
            RasterGraphContext context)
        {
            // Render all the outlined objects to the temp texture
            foreach (Renderer objectRenderer in data.Renderers)
            {
                // Skip null renderers
                if (objectRenderer)
                {
                    int materialCount = objectRenderer.sharedMaterials.Length;
                    for (int i = 0; i < materialCount; i++)
                    {
                        context.cmd.DrawRenderer(objectRenderer, data.Material, i, 0);
                    }
                }
            }
        }

        private static void ExecuteBlit(BlitPassData data, RasterGraphContext context, int pass)
        {
            Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1f, 1f, 0f, 0f), data.Material, pass);
        }

        private static void ExecuteDoubleBlit(BlitPassData data, RasterGraphContext context, int pass)
        {
            Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1f, 1f, 0f, 0f), data.Material, pass);
            Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1f, 1f, 0f, 0f), data.Material, pass + 1);
        }

        private class RenderObjectsPassData
        {
            internal Renderer[] Renderers;
            internal Material Material;
        }

        private class BlitPassData
        {
            internal TextureHandle Source;
            internal Material Material;
        }
    }
}

#endif // UNITY_6000_0_OR_NEWER