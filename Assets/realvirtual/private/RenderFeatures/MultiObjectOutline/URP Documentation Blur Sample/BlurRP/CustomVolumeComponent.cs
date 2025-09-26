// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using UnityEngine.Rendering;

namespace realvirtual
{
    [Serializable]
    public class CustomVolumeComponent : VolumeComponent
    {
        public BoolParameter isActive = new BoolParameter(true);

        public ClampedFloatParameter horizontalBlur =
            new ClampedFloatParameter(0.05f, 0, 0.5f);

        public ClampedFloatParameter verticalBlur =
            new ClampedFloatParameter(0.05f, 0, 0.5f);
    }
}