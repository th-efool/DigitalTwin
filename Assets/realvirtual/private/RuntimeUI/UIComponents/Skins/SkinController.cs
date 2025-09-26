using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    public class SkinController : MonoBehaviour
    {
        
        public RealvirtualUISkin LightSkin;
        public RealvirtualUISkin DarkSkin;
        [Header("Selected Skin")]
        public RealvirtualUISkin ActiveSkin;
        
        private static RealvirtualUISkin currentSkin;

        private void OnValidate()
        {
            CheckApplySkin();
        }

        void Start()
        {
            CheckApplySkin();
        }
        
        private void CheckApplySkin()
        {
            if (PlayerPrefs.HasKey("SkinName") && ActiveSkin == null)
            {
                string skinName = PlayerPrefs.GetString("SkinName");
                if (skinName == LightSkin.name)
                {
                    ApplySkin(LightSkin);
                }
                else if (skinName == DarkSkin.name)
                {
                    ApplySkin(DarkSkin);
                }
            }
            else
            {
                if(ActiveSkin != null)
                    ApplySkin(ActiveSkin);
                else
                {
                    ApplySkin(LightSkin);
                }
                
            }
        }
        
        public static bool IsDarkSkin()
        {
            return GetCurrentSkin().name == FindFirstObjectByType<SkinController>(FindObjectsInactive.Include).DarkSkin.name;
        } 
        
        public static void ApplySkin(RealvirtualUISkin skin)
        {
            currentSkin = skin;
            PlayerPrefs.SetString("SkinName", skin.name);
            PlayerPrefs.Save();
            
            List<IUISkinEdit> targets = GetAllObjectsWithInterface<IUISkinEdit>();
            
            foreach (IUISkinEdit t in targets)
            {
                t.UpdateUISkinParameter(skin);
            }

        }

        public static void ApplyLightSkin()
        {
            RealvirtualUISkin skin = FindAnyObjectByType<SkinController>(FindObjectsInactive.Include).LightSkin;
            ApplySkin(skin);
        }
        
        public static void ApplyDarkSkin()
        {
            RealvirtualUISkin skin = FindAnyObjectByType<SkinController>(FindObjectsInactive.Include).DarkSkin;
            ApplySkin(skin);
        }
        
        public static RealvirtualUISkin GetCurrentSkin()
        {
            if (SkinController.currentSkin == null)
            {
                SkinController controller = FindAnyObjectByType<SkinController>(FindObjectsInactive.Include);
                controller?.CheckApplySkin();
            }

            return SkinController.currentSkin;
        }
        private static List<T> GetAllObjectsWithInterface<T>() where T : class
        {
            List<T> results = new List<T>();
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
            foreach (GameObject go in allObjects)
            {
                T component = go.GetComponent(typeof(T)) as T;
                if (component != null)
                {
                    results.Add(component);
                }
            }

            return results;
        }
    }
}
