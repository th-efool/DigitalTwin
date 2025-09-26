// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
    using UnityEngine;

#pragma warning disable CS3009 // Base type is not CLS-compliant
#pragma warning disable CS3003 // Base type is not CLS-compliant
    public class rvUISizeLink : MonoBehaviour
    {
        public RectTransform target;
        public Vector2 padding;

        private void OnValidate()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (target == null) return;

            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = target.sizeDelta + padding * 2;
            rt.anchoredPosition = -padding;
        }
    }
}
#pragma warning restore CS3009 // Base type is not CLS-compliant
#pragma warning restore CS3003 // Base type is not CLS-compliant