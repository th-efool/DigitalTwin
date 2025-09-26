// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if !UNITY_6000_0_OR_NEWER
using NaughtyAttributes;
#endif

namespace realvirtual.RendererFeatures
{
    [ExecuteInEditMode]
    public class OverlaySelectionManager : 
#if UNITY_6000_0_OR_NEWER
        AbstractSelectionManager
#else
        MonoBehaviour
#endif
    {
#if UNITY_6000_0_OR_NEWER
        [SerializeField] private MultiObjectOverlayRendererFeature overlayRendererFeature;
        [SerializeField] private List<Renderer> selectedRenderers = new List<Renderer>();

        [ContextMenu("Reassign Renderers Now")]
        private void OnValidate()
        {
            if (overlayRendererFeature != null)
                overlayRendererFeature.SetRenderers(selectedRenderers.ToArray());
        }

        public override void Select(Renderer renderer)
        {
            if (selectedRenderers.Contains(renderer))
            {
                return;
            }

            selectedRenderers.Add(renderer);
            overlayRendererFeature.SetRenderers(selectedRenderers.ToArray());
        }

        public override void Deselect(Renderer renderer)
        {
            if (!selectedRenderers.Contains(renderer))
            {
                return;
            }

            selectedRenderers.Remove(renderer);
            overlayRendererFeature.SetRenderers(selectedRenderers.ToArray());
        }

        public override void DeselectAll()
        {
            selectedRenderers.Clear();
            overlayRendererFeature.SetRenderers(selectedRenderers.ToArray());
        }
#else
        [InfoBox("⚠️ UNITY 6 ONLY FEATURE ⚠️\n\nThis Overlay Selection Manager component requires Unity 6.0 or newer to function.\n\nUpgrade to Unity 6 to enable advanced render features.", EInfoBoxType.Warning)]
        [SerializeField] private bool unity6OnlyWarning;

        // Dummy methods for Unity 2022 compatibility
        public virtual void Select(Renderer renderer)
        {
            Debug.LogWarning("OverlaySelectionManager: This feature requires Unity 6.0 or newer.", this);
        }

        public virtual void Deselect(Renderer renderer)
        {
            Debug.LogWarning("OverlaySelectionManager: This feature requires Unity 6.0 or newer.", this);
        }

        public virtual void DeselectAll()
        {
            Debug.LogWarning("OverlaySelectionManager: This feature requires Unity 6.0 or newer.", this);
        }
#endif
    }
}