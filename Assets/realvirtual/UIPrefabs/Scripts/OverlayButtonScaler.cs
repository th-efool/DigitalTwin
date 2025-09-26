
using System.Collections.Generic;

using UnityEngine;

namespace realvirtual
{



    public class OverlayButtonScaler : MonoBehaviour
    {
        
        public float SmallScreenScale = 2f;
        public float BigScreenScale = 1f;
  
        public float SmallScreenRectWidth = 70;
        public float BigScreenRectWidth = 140;
        public bool IsSmallOnAndroid = true;
        public bool IsSmallOnIOS = true;
        
        public float SmallIsWidthSmallerThen = 13;
        public float SmallIsHeightSmallerThen = 13;
        public RectTransform RectTransform;


        [ReadOnly] public bool IsSmall;
        [ReadOnly]public float CurrentScreenWidht;
        [ReadOnly]public float CurrentScreenHeight;
    
        public List<GameObject> DeactivateWhenSmall;
        public List<GameObject> DeactivateWhenWebGL;
        public List<GameObject> DeactivateWhenNoInterface;
        private bool _issmallbefore;
        
        public void SizeChanged()
        {
            // Is it WebGL?
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                foreach (var obj in DeactivateWhenWebGL)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                foreach (var obj in DeactivateWhenWebGL)
                {
                    obj.SetActive(true);
                }
            }
            // Settings for small screen sizes
            if (IsSmall)
            {
             

                foreach (var obj in DeactivateWhenSmall)
                {
                    obj.SetActive(false);
                }
                RectTransform.localScale = new Vector3(SmallScreenScale, SmallScreenScale, 1);
                RectTransform.sizeDelta = new Vector2(SmallScreenRectWidth, RectTransform.sizeDelta.y);
             
            }
            // Settings for big screen sizes
            else
            {
                foreach (var obj in DeactivateWhenSmall)
                {
                    obj.SetActive(true);
                }
                RectTransform.localScale = new Vector3(BigScreenScale, BigScreenScale, 1);
                // set the width of the recttransform 
                RectTransform.sizeDelta = new Vector2(BigScreenRectWidth, RectTransform.sizeDelta.y);
                
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // check if in the scene is an interfacebaseclass children
            var interfaces = FindObjectsByType<InterfaceBaseClass>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (interfaces.Length == 0)
            {
                foreach (var obj in DeactivateWhenNoInterface)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                foreach (var obj in DeactivateWhenNoInterface)
                {
                    obj.SetActive(true);
                }
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

            if (IsSmall != _issmallbefore)
                SizeChanged();
            
            if (Application.platform == RuntimePlatform.Android)
            {
                IsSmall = IsSmallOnAndroid;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                IsSmall = IsSmallOnIOS;
            }

            _issmallbefore = IsSmall;

        }
    }
}
