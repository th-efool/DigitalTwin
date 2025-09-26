// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEditor;
using UnityEngine;
using System.IO;


namespace realvirtual
{
    class Prepare202215Update : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool realvirtualimport = false;

            foreach (string str in importedAssets)
            {
                if (str.Contains("Update202215"))
                {

                    realvirtualimport = true;
                }

            }


            if (realvirtualimport)
            {
               
                // Delete old Planner if existant
                if (Directory.Exists("Assets/realvirtual/private/Planner"))
                {
                    Directory.Delete("Assets/realvirtual/private/Planner",true);
                    // Write Modal Message
                    EditorUtility.DisplayDialog("realvirtual Update 2022-15", "Update preparation successful - you are now ready to update to 2022.15!", "OK");
                }
               
            }


        }
    }
}