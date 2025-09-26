// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;


    public class rvUIURLField : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public UnityEvent<string> OnLoad;

        public delegate void OnLoadURLDelegate(string path);

        public static OnLoadURLDelegate OnLoadURLClicked;

        public void Load()
        {
            Debug.Log("Loading collection from url " + text.text);
            OnLoad.Invoke(text.text);
            if (OnLoadURLClicked != null)
            {
                OnLoadURLClicked(text.text);
            }
        }
    }
}
