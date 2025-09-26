// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    


using UnityEngine;
using System;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using System.Collections.Generic;
using System.Linq;


namespace realvirtual
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class RuntimeSelection
    {
        public static void Highlight (bool highlight, GameObject obj , Material material)
        {
            if (highlight)
            {
                var sel = obj.AddComponent<ObjectSelection>();
                sel.SetNewMaterial(material);
            }
            else
            {
                var sel = obj.GetComponent<ObjectSelection>();
                sel.ResetMaterial();
            }
        }
        
        
    }
}