// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEditor;

namespace realvirtual
{
    [InitializeOnLoad]
    public class ProjectPathMenuItem
    {
        public const string MenuName = "realvirtual/Settings/Show Project Path in Toolbar";
        private static bool isToggled;
        
        static ProjectPathMenuItem()
        {
            EditorApplication.delayCall += () =>
            {
                isToggled = EditorPrefs.GetBool(MenuName, true);
                UnityEditor.Menu.SetChecked(MenuName, isToggled);
            };
        }
        
        [MenuItem(MenuName, false, 501)]
        private static void ToggleProjectPath()
        {
            isToggled = !isToggled;
            UnityEditor.Menu.SetChecked(MenuName, isToggled);
            EditorPrefs.SetBool(MenuName, isToggled);
            
            // Force the toolbar to refresh
            QuickEditToolbarIMGUI.ForceRefresh();
        }
        
        public static bool IsProjectPathEnabled()
        {
            return EditorPrefs.GetBool(MenuName, true);
        }
    }
}