// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  


using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;

namespace realvirtual
{
    public class InstalledPackages : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var inPackages = importedAssets.Any(path => path.StartsWith("realvirtual/")) ||
                             deletedAssets.Any(path => path.StartsWith("realvirtual/")) ||
                             movedAssets.Any(path => path.StartsWith("realvirtual/")) ||
                             movedFromAssetPaths.Any(path => path.StartsWith("realvirtual/"));
 
            if (inPackages)
            {
                InitializeOnLoad();
            }
        }
   
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
                Thread.Sleep(100);
 
            if (listRequest.Error != null)
            {
                Debug.Log("Error: " + listRequest.Error.message);
                return;
            }
 
            var packages = listRequest.Result;
            var text = new StringBuilder("Packages:\n");
            foreach (var package in packages)
            {
                if (package.source == PackageSource.Registry)
                    text.AppendLine($"{package.name}: {package.version} [{package.resolvedPath}]");
            }
       
            // Debug.Log(text.ToString());
        }
    }
}
