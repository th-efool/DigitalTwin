// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Collections.Generic;

using UnityEngine;

namespace realvirtual
{



    public class ToolbarScaler : MonoBehaviour
    {
        public bool AdjustHeight;
        public float SmallScreenHeight = 120;
        public float BigScreenHeight = 60;

        public bool Scale;
        public float SmallScreenScale = 2f;
        public float BigScreenScale = 1f;
        public float CurrentScreenWidht;

        public float CurrentScreenHeight;

        public bool IsSmallOnAndroid = true;
        public bool IsSmallOnIOS = true;
        public bool IsSmallOnWebGLOnMobile = true;
        
        public float SmallIsWidthSmallerThen = 13;
        public float SmallIsHeightSmallerThen = 13;

        public bool IsSmall;
        private RectTransform _rect;
        
        public List<GameObject> DeactivateWhenLinuxWindows;
        public List<GameObject> DeactivateWhenWebGL;
        public List<GameObject> DeactivateWhenSmall;
     
        public List<GameObject> DeactivateWhenNoInterface;
        private bool _issmallbefore;
        private bool IsSmallSystem;
        public bool IsWebGLOnMobile()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string os = SystemInfo.operatingSystem.ToLower();
                if (os.Contains("android") || os.Contains("ios"))
                {
                    return true;
                }
            }
            return false;
        }
    
        public void SizeChanged()
        {
           
            // Settings for small screen sizes
            if (IsSmall)
            {
                if (AdjustHeight)
                {
                    _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, SmallScreenHeight);
                }

                foreach (var obj in DeactivateWhenSmall)
                {
                    obj.SetActive(false);
                }

                if (Scale)
                {
                    _rect.localScale = new Vector3(SmallScreenScale, SmallScreenScale, 1);
                }
            }
            // Settings for big screen sizes
            else
            {
                if (AdjustHeight)
                {
                    _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, BigScreenHeight);
                }

                foreach (var obj in DeactivateWhenSmall)
                {
                    obj.SetActive(true);
                }

                if (Scale)
                {
                    _rect.localScale = new Vector3(BigScreenScale, BigScreenScale, 1);
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            var listdeactivate = new List<GameObject>();
            
            _rect = GetComponent<RectTransform>();
            // check if in the scene is an interfacebaseclass children
            var interfaces = FindObjectsByType<InterfaceBaseClass>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (interfaces.Length == 0)
            {
                foreach (var obj in DeactivateWhenNoInterface)
                {
                    listdeactivate.Add(obj);
                }
            }
          
            
            // Is it WebGL?
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                foreach (var obj in DeactivateWhenWebGL)
                {
                    listdeactivate.Add(obj);
                }
            }
         
            // Is it Linux or Windows?
            if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor)
            {
                foreach (var obj in DeactivateWhenLinuxWindows)
                {
                    listdeactivate.Add(obj);
                }
            }
            
            // now deactivate all objects in the list
            foreach (var obj in listdeactivate)
            {
                obj.SetActive(false);
            }
            
            if (Application.platform == RuntimePlatform.Android && IsSmallOnAndroid)
            {
                IsSmallSystem = true;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer && IsSmallOnIOS)
            {
                IsSmallSystem = true;
            }
            if (IsWebGLOnMobile() && IsSmallOnWebGLOnMobile)
            {
                IsSmallSystem = true;
            }
            
            
        }

        // Update is called once per frame
        void Update()
        {
            CurrentScreenWidht = Screen.width / Screen.dpi * 2.54f;
            CurrentScreenHeight = Screen.height / Screen.dpi * 2.54f;

            if ((CurrentScreenWidht < SmallIsWidthSmallerThen) || (CurrentScreenHeight < SmallIsHeightSmallerThen))
            {
                IsSmall = true;
            }
            else
            {
                IsSmall = false;
            }
            
            // if height is greater than with, it is small
            if (CurrentScreenHeight > CurrentScreenWidht)
            {
                IsSmall = true;
            }
            
            if (IsSmallSystem)
            {
                IsSmall = true;
            }
            
            if (IsSmall != _issmallbefore)
                SizeChanged();

            _issmallbefore = IsSmall;

        }
    }
}
