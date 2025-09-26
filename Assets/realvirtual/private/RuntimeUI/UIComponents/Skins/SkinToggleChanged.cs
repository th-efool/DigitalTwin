using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.UI;


namespace realvirtual
{
    public class SkinToggleChanged : realvirtualBehavior,IUISkinEdit
    {
        public SkinController SkinController;
        public RealvirtualUISkin Skin;
        public UISkin FileBrowserSkin;
        public RuntimeInspectorNamespace.UISkin InspectorSkin;
        
        
        private Toggle toggle;
        private Text textObj;
        private realvirtualController controller;
        
        // Start is called before the first frame update
        protected new void Awake()
        {
            controller = FindFirstObjectByType<realvirtualController>();
            toggle = GetComponent<Toggle>();
            //toggle.onValueChanged.AddListener(OnSkinToggleChanged);
            textObj= GetComponentInChildren<Text>();
            textObj.text = toggle.name;
        }

        

       public void UpdateUISkinParameter(RealvirtualUISkin skin)
       {
           //toggle.graphic.color = skin.WindowHoverColor;
       }
    }
}
