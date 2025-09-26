// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{

    public enum LightGroupEnum {Sun,FirstLight,SecondLight}
    
    [RequireComponent(typeof(Light))]
    //! The LightGroup is used to be able to set centralized multiple lights. You can attach a LightGroup to any light in the scene.
    //! realvirtualController will use the LightGroup to set the light intensity.
    public class LightGroup : MonoBehaviour
    {
        [Tooltip("Light group for centralized intensity control")]
        public LightGroupEnum Group;  //!< The group of the light

        //! Sets the intensity if the light is assigned to the LightGroup group
        public void SetIntensity(LightGroupEnum group, float intensity)
        {
            if (group == Group)
            {
                var light = GetComponent<Light>();
                light.intensity = intensity;
            }
        }
        
    }
}

