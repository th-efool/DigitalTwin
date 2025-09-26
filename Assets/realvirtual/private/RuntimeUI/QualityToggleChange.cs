using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace realvirtual
{
    public class QualityToggleChange : MonoBehaviour
    {

        public QualityController settingscontroller,IUISkinEdit;
        private Toggle toggle;
        private realvirtualController _controller;
        public int qualitylevel;
        // Start is called before the first frame update

        void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnQualityToggleChanged);
            _controller = FindAnyObjectByType<realvirtualController>();
        }
        private void Start()
        {

        }
        public void SetQualityStatus(int quality)
        {
            if (quality == qualitylevel)
                toggle.isOn = true;
        }

        public void OnQualityToggleChanged(bool ison)
        {
            //if (ison)
                //settingscontroller.OnQualityToggleChanged(qualitylevel);
        }


    }
}
    
