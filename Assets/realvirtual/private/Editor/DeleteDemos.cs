using UnityEditor;
using UnityEngine;
using System.IO;

namespace realvirtual
{
    public class DeleteScenesWindow : EditorWindow
    {
        [MenuItem("realvirtual/Settings/Delete Demodata", priority = 504)]
        public static void ShowWindow()
        {
            GetWindow<DeleteScenesWindow>("Delete Demo Data");
        }

        private void OnGUI()
        {
            GUILayout.Label("Warning: If your are using some of the demo data the references in your own model are lost, you need to import realvirtual.io again to restore the data", EditorStyles.wordWrappedLabel);
            GUILayout.Label("Delete all Scenes in realvirtual Folder", EditorStyles.boldLabel);

            if (GUILayout.Button("Delete Scenes"))
            {
                DeleteAllScenesInRealvirtualFolder();
            }
            
            GUILayout.Label("Delete all Meshes and FBX in realvirtual Folder, this will also deleta all 3D Prefabs, including 3D Buttons, Robots and so on", EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Delete Meshes and FBX"))
            {
                DeleteAllMeshesAndFBXInRealvirtualFolder();
            }
        }

        private void DeleteAllMeshesAndFBXInRealvirtualFolder()
        {
            string realvirtualPath = "Assets/realvirtual";
            string[] meshFiles = Directory.GetFiles(realvirtualPath, "*.mesh", SearchOption.AllDirectories);
            string[] fbxFiles = Directory.GetFiles(realvirtualPath, "*.fbx", SearchOption.AllDirectories);

            foreach (string meshFile in meshFiles)
            {
                if (File.Exists(meshFile))
                {
                    File.Delete(meshFile);
                    File.Delete(meshFile + ".meta");
                    Debug.Log("Deleted mesh: " + meshFile);
                }
            }

            foreach (string fbxFile in fbxFiles)
            {
                if (File.Exists(fbxFile))
                {
                    File.Delete(fbxFile);
                    File.Delete(fbxFile + ".meta");
                    Debug.Log("Deleted FBX: " + fbxFile);
                }
            }

            AssetDatabase.Refresh();
        }

        private void DeleteAllScenesInRealvirtualFolder()
        {
            string realvirtualPath = "Assets/realvirtual";
            string[] sceneFiles = Directory.GetFiles(realvirtualPath, "*.unity", SearchOption.AllDirectories);

            foreach (string sceneFile in sceneFiles)
            {
                if (File.Exists(sceneFile))
                {
                    File.Delete(sceneFile);
                    File.Delete(sceneFile + ".meta");
                    Debug.Log("Deleted scene: " + sceneFile);
                }
            }

            AssetDatabase.Refresh();
        }
    }
}

