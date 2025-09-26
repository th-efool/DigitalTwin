// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
    using TMPro;

    using UnityEngine;
    using UnityEngine.UI;

#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class rvUIButton : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI text;

        public void SetText(string s)
        {
            text.text = s;
        }

    }
}
#pragma warning restore CS3009 // Base type is not CLS-compliant