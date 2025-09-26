// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2024 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEditor;

namespace realvirtual
{
    public static class SetGroupPrefix
    {
        // Add menu item named "Custom Window" to the Window menu
        [MenuItem("realvirtual/Set Group Prefix", false, 401)]
        [MenuItem("GameObject/realvirtual/Set Group Prefix", false, 0)]
        public static void SetGroupPrefixToGameobject()
        {
            // if not gameobject is selected prompt a message
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No GameObject Selected",
                    "Please select a GameObject which will be the prefix for the underlying components", "OK");
                return;
            }

            // get all kinematic of all childrend in all selected gameobjects
            var go = Selection.activeGameObject;
            var kinematics = go.GetComponentsInChildren<Kinematic>(true);
            foreach (var kinematic in kinematics) kinematic.GroupNamePrefix = go;

            // do the same for all group objects

            var groups = go.GetComponentsInChildren<Group>(true);
            foreach (var group in groups) group.GroupNamePrefix = go;
        }

        [MenuItem("realvirtual/Remove Group Prefix", false, 402)]
        [MenuItem("GameObject/realvirtual/Remove Group Prefix", false, 0)]
        public static void RemoveGroupPrefixFromGameobjects()
        {
            // if not gameobject is selected prompt a message
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No GameObject Selected",
                    "Please select a GameObject which will be the prefix for the underlying components", "OK");
                return;
            }

            // get all kinematic of all childrend in all selected gameobjects
            foreach (var go in Selection.gameObjects)
            {
                var kinematics = go.GetComponentsInChildren<Kinematic>(true);
                foreach (var kinematic in kinematics) kinematic.GroupNamePrefix = null;
            }

            // do the same for all group objects
            foreach (var go in Selection.gameObjects)
            {
                var groups = go.GetComponentsInChildren<Group>(true);
                foreach (var group in groups) group.GroupNamePrefix = null;
            }
        }
    }
}