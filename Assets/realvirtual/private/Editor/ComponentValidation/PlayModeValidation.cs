// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! Handles validation before entering play mode to ensure scene components are properly configured.
    //! Automatically runs validation rules when transitioning from edit to play mode.
    [InitializeOnLoad]
    public static class PlayModeValidation
    {
        private static bool validationEnabled = true;
        
        static PlayModeValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Check if validation is enabled in realvirtualController
                var controller = Object.FindFirstObjectByType<realvirtualController>();
                
                if (controller != null && controller.ValidateBeforeStart && validationEnabled)
                {
                    // Clear any previous validation messages
                    ValidationMessageStorage.Instance.Clear();
                    
                    // Run validation
                    RunValidation();
                }
            }
        }
        
        //! Executes pre-play validation by running all PrePlayRule validations
        public static void RunValidation()
        {
            // Use the ValidateAllComponents method that checks PrePlayRules
            ComponentValidation.ValidateAllComponents();
        }
        
        //! Enables or disables validation programmatically
        public static bool ValidationEnabled
        {
            get => validationEnabled;
            set => validationEnabled = value;
        }
    }
}
#endif