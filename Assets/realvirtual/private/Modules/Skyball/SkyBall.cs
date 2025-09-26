// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

namespace realvirtual
{


    using UnityEngine;

    public class SkyBall : MonoBehaviour
    {
        public Material reflectionMaterial;

        public void Apply()
        {
            RenderSettings.skybox = reflectionMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }
}
