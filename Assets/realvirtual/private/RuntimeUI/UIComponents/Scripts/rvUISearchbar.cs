// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
    using System.Collections.Generic;
    using TMPro;

    using UnityEngine;

#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class rvUISearchbar : MonoBehaviour
    {
        public TMP_InputField inputField;
        public GameObject contentRoot;

        public void FilterElements()
        {
            var filter = inputField.text;

            var children = new List<Transform>();
            foreach (Transform child in contentRoot.transform) children.Add(child);


            // activate all elements if filter is ""
            if (filter == "")
            {
                foreach (var child in children) child.gameObject.SetActive(true);
                return;
            }

            foreach (var child in children)
                if (child.gameObject.name.ToLower().Contains(filter.ToLower()))
                    child.gameObject.SetActive(true);
                else
                    child.gameObject.SetActive(false);
        }
    }
}
#pragma warning restore CS3009 // Base type is not CLS-compliant
