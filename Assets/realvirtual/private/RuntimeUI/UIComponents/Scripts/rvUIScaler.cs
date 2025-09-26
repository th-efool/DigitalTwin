
// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;
using UnityEngine.UI;

namespace realvirtual
{
    public class rvUIScaler : MonoBehaviour
    {
        
        public float SmallScreenScale = 2f;
        public float BigScreenScale = 1f;
        
        public bool IsSmallOnAndroid = true;
        public bool IsSmallOnIOS = true;
        public bool IsSmallOnWebGLOnMobile = true;
        public bool IsSmallOnWebGL = false;
        public bool IsSmallOnWithSmallerHeight = true;
        public bool DeactivateOnSmall = false;
     
        public RectTransform RectTransform;
        [ReadOnly] public bool IsSmall;
      
        private bool _issmallbefore;
        
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
        
        public void UpdaterScale()
        {
            IsSmall = false;
            
            // is width of screen smaller height
            if (IsSmallOnWithSmallerHeight)
                if (Screen.width < Screen.height)
                {
                        IsSmall = true;
                }
           
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (IsSmallOnWebGLOnMobile)
                {
                    if (IsWebGLOnMobile())
                    {
                        IsSmall = true;
                    }
                }
                else
                {
                    if (IsSmallOnWebGL)
                    {
                        IsSmall = true;
                    }
                }
               
            }
            
            if (IsSmallOnAndroid)
                if (Application.platform == RuntimePlatform.Android)
                    IsSmall = true;
                
                
            if (IsSmallOnIOS) 
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    IsSmall = true;


            if (RectTransform != null)
            {
                if (IsSmall )
                {
                    RectTransform.localScale = new Vector3(SmallScreenScale, SmallScreenScale, 1);
                }
                // Settings for big screen sizes
                else
                {
                    RectTransform.localScale = new Vector3(BigScreenScale, BigScreenScale, 1);
                }
            }
            
            if (DeactivateOnSmall )
            {
                this.gameObject.SetActive(!IsSmall);
            }
            
            //LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
        
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            UpdaterScale();
        }

       
    }
}
