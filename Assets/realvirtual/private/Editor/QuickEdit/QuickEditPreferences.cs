// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    // Optional: Add a preference window for QuickEdit settings
    public class QuickEditPreferences : SettingsProvider
    {
        private const string k_PreferencesPath = "Preferences/realvirtual/Quick Edit";
        
        public QuickEditPreferences(string path, SettingsScope scope)
            : base(path, scope) { }
        
        [SettingsProvider]
        public static SettingsProvider CreateQuickEditPreferences()
        {
            return new QuickEditPreferences(k_PreferencesPath, SettingsScope.User)
            {
                label = "Quick Edit",
                guiHandler = (searchContext) => {
                    EditorGUILayout.LabelField("Quick Edit Display Mode", EditorStyles.boldLabel);
                    
                    bool useOverlay = EditorPrefs.GetBool("realvirtual_UseQuickEditOverlay", true);
                    bool newUseOverlay = EditorGUILayout.Toggle("Use Overlay Mode", useOverlay);
                    
                    if (newUseOverlay != useOverlay)
                    {
                        EditorPrefs.SetBool("realvirtual_UseQuickEditOverlay", newUseOverlay);
                        
                        if (newUseOverlay)
                        {
                            // Switch to overlay
                            EditorPrefs.SetBool("realvirtual_QuickEditVisible", true);
                            var window = EditorWindow.GetWindow<QuickEditWindow>(false);
                            if (window != null) window.Close();
                        }
                        else
                        {
                            // Switch to window
                            EditorPrefs.SetBool("realvirtual_QuickEditVisible", false);
                            QuickEditWindow.ShowWindow();
                        }
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(
                        "Overlay Mode: Appears as a floating panel in the Scene view\n" +
                        "Window Mode: Traditional dockable window that can be tabbed with other windows",
                        MessageType.Info);
                    
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("Reset to Default"))
                    {
                        EditorPrefs.SetBool("realvirtual_UseQuickEditOverlay", true);
                        EditorPrefs.SetBool("realvirtual_QuickEditVisible", true);
                    }
                }
            };
        }
    }
}
#endif