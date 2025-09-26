// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    [Serializable]
    public class SceneDescription
    {
        [HideInInspector]public string sceneName;
        [TextArea]public string DisplayName;
        [TextArea]public string Description;
        public Texture2D SceneIcon;
        [HideInInspector]public string assetBundle;
    }
    public class SceneDescriptionComponent : MonoBehaviour
    {
        public SceneDescription sceneDescription=new SceneDescription();
       
        public void OnValidate()
        {
            sceneDescription.sceneName = gameObject.scene.name;
        }
    }
}

