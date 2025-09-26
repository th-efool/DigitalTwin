using System;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    
    [Serializable]
    public class SceneDescriptionSO
    {
        public string sceneName;
        [TextArea]public string DisplayName;
        [TextArea]public string Description;
        public Texture2D SceneIcon;
        public string assetBundle;
    }  
    [CreateAssetMenu(fileName = "SceneDescriptions", menuName = "realvirtual/SceneDescriptionList", order = 1)]
    public class rvSceneDescriptions : ScriptableObject
    {
        public List<SceneDescriptionSO> SceneDescriptionsList = new List<SceneDescriptionSO>();
    }
}
