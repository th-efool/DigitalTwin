// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
#if UNITY_EDITOR
using UnityEngine;

namespace RenderPipeLineSetup
{
    [CreateAssetMenu(fileName = "RenderPipelineStatus", menuName = "RenderPipelineStatus", order = 1)]
    public class RenderPipelineStatus : ScriptableObject
    {
        public RenderPipeline pipeline;
        public HDRPRenderMode mode;
        public int state;

    }
}
#endif
