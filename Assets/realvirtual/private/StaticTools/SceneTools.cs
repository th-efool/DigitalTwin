// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

namespace realvirtual
{
    //! Static utility class for scene cleanup and management operations
    public static class SceneTools
    {
#if UNITY_EDITOR
        [MenuItem("realvirtual/Settings/Cleanup Scene", false, 510)]
        public static void CleanupSceneMenu()
        {
            CleanupScene(true);
        }
        
        //! Cleans up the scene by unlocking hidden GameObjects and removing highlight system artifacts
        public static void CleanupScene(bool showDialog = false)
        {
            int objectsUnlocked = 0;
            bool highlightCleaned = false;
            System.Collections.Generic.List<string> unlockedObjectNames = new System.Collections.Generic.List<string>();
            
            // 1. Try to clean up highlight system objects if available
            try
            {
                var highlightType = System.Type.GetType("realvirtual.Highlight, Assembly-CSharp");
                if (highlightType != null)
                {
                    var forceCleanupMethod = highlightType.GetMethod("ForceCleanupAllHighlightObjects", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (forceCleanupMethod != null)
                    {
                        forceCleanupMethod.Invoke(null, null);
                        highlightCleaned = true;
                    }
                }
            }
            catch { }
            
            // 2. Find all GameObjects in the scene and unlock/unhide them
            GameObject[] allObjects = UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                // Skip if object is not in scene (could be prefab asset)
                if (!obj.scene.IsValid() || EditorUtility.IsPersistent(obj))
                    continue;
                
                // Skip if object is under a realvirtual hierarchy (has realvirtualController)
                if (IsUnderRealvirtualController(obj))
                    continue;
                
                // Unlock objects (remove HideFlags that prevent selection)
                if ((obj.hideFlags & HideFlags.NotEditable) != 0 || 
                    (obj.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    obj.hideFlags &= ~(HideFlags.NotEditable | HideFlags.HideInHierarchy);
                    objectsUnlocked++;
                    unlockedObjectNames.Add(GetObjectPath(obj));
                    EditorUtility.SetDirty(obj);
                }
            }
            
            // Refresh the hierarchy to show changes
            EditorApplication.RepaintHierarchyWindow();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
            // Show summary dialog only if requested
            if (showDialog)
            {
                string message = "Scene cleanup completed:\n\n";
                if (highlightCleaned)
                    message += "• Highlight system cleaned\n";
                message += $"• {objectsUnlocked} objects unlocked\n";
                
                // Add detailed list of unlocked objects if any
                if (unlockedObjectNames.Count > 0)
                {
                    message += "\nUnlocked objects:\n";
                    // Limit display to first 20 objects to avoid huge dialog
                    int displayCount = System.Math.Min(unlockedObjectNames.Count, 20);
                    for (int i = 0; i < displayCount; i++)
                    {
                        message += $"  - {unlockedObjectNames[i]}\n";
                    }
                    if (unlockedObjectNames.Count > 20)
                    {
                        message += $"  ... and {unlockedObjectNames.Count - 20} more\n";
                    }
                }
                
                EditorUtility.DisplayDialog("Scene Cleanup", message, "OK");
            }
            
            // Always log summary
            Logger.Message($"Scene cleanup completed - {objectsUnlocked} objects unlocked");
            
            // Log detailed information to console
            if (unlockedObjectNames.Count > 0)
            {
                Logger.Message("Unlocked objects:");
                foreach (string objPath in unlockedObjectNames)
                {
                    Logger.Message($"  - {objPath}");
                }
            }
        }
#endif
        
        //! Checks if a GameObject is under a realvirtual hierarchy (has realvirtualController in its parent chain)
        private static bool IsUnderRealvirtualController(GameObject obj)
        {
            Transform current = obj.transform;
            
            while (current != null)
            {
                // Check if this GameObject has a realvirtualController component
                if (current.GetComponent<realvirtualController>() != null)
                {
                    return true;
                }
                current = current.parent;
            }
            
            return false;
        }
        
        //! Gets the full hierarchy path of a GameObject
        public static string GetObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
    }
}