// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  


using System.Linq;
using UnityEditor;
using UnityEngine;

namespace realvirtual
{
    public static class SelectMaterialsInScene
    {
        [MenuItem("realvirtual/Select Materials in Scene", false, 400)]
        private static void SelectMaterials()
        {
            foreach (var selected in Selection.objects)
            {
                // check if all selected are materials
                if (!(selected is Material))
                {
                    // prompt a message that materials should be selected in assets 
                    EditorUtility.DisplayDialog("Error", "Please select Materials in Asset folder first", "OK");
                    break;
                }
            }

            // get all renderers in the scene
            var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var oneselected = false;
            // check if the material is containted in the renderers materials list
            foreach (var renderer in renderers)
            {
                foreach (var selected in Selection.objects)
                {
                    // check if selected material is contained in Meshrenderer Materials
                    if (renderer.sharedMaterials.Contains(selected))
                    {
                        oneselected = true;
                        Selection.activeGameObject = renderer.gameObject;
                    }
                }
            }

            // prompt a message if material is not in current scene
            if (!oneselected)
            {
                EditorUtility.DisplayDialog("Message", "Material is not in the current scene", "OK");
            }


        }
    }
}