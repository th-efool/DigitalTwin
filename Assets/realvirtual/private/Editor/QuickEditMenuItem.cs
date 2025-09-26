// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEditor;

namespace realvirtual
{
    
    [InitializeOnLoad]
    //! The class is automatically saving the scene when run is started in the Unity editor. It can be turned off by the toggle in the realvirtual menu
    public class QuickEditMenuItem
    {
        
        public const string MenuName = "realvirtual/Settings/Quick Edit Overlay";
        private static bool isToggled;

        static QuickEditMenuItem()
        {
            EditorApplication.delayCall += () =>
            {
                // Initialize from menu preference, defaulting to overlay visibility
                bool overlayVisible = EditorPrefs.GetBool("realvirtual_QuickEditVisible", true);
                isToggled = EditorPrefs.GetBool(MenuName, overlayVisible);
                UnityEditor.Menu.SetChecked(MenuName, isToggled);
                SetMode();
            };
        }

        [MenuItem(MenuName, false, 500)]
        private static void ToggleMode()
        {
            // Simple on/off toggle for QuickEdit visibility
            isToggled = !isToggled;
            UnityEditor.Menu.SetChecked(MenuName, isToggled);
            EditorPrefs.SetBool(MenuName, isToggled);
            SetMode();
        }

        private static void SetMode()
        {
           
            if (isToggled)
            {
                Global.QuickEditDisplay = true;
            }
            else
            {
                Global.QuickEditDisplay = false;
            }
            
            // Synchronize with overlay system preference
            EditorPrefs.SetBool("realvirtual_QuickEditVisible", Global.QuickEditDisplay);
            
            // Trigger the overlay visibility change event
            // This is needed because the overlay no longer polls the EditorPrefs
            QuickEditOverlay.RequestVisibilityChange(Global.QuickEditDisplay);
        }

    }
}