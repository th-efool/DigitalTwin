using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace realvirtual
{
    //! class for highlighing objects in runtime (game mode) by changing materials and saving the original materials
    public class ObjectSelection : MonoBehaviour
    {
        public List<Material> OriginalMaterial; //!< List of original materials
        //! Sets the new highlight material
        
        public void SetNewMaterial(Material material)
        {
            var meshrenderer = GetComponentInChildren<MeshRenderer>();
            if (meshrenderer != null)
            {
                if (OriginalMaterial == null)
                    OriginalMaterial = meshrenderer.materials.ToList();
                Material[] sharedMaterialsCopy = meshrenderer.materials;
                for (int i = 0; i < meshrenderer.materials.Length; i++)
                {
                    sharedMaterialsCopy[i] = material;
                }

                meshrenderer.materials = sharedMaterialsCopy;
            }
        }

        //! Sets the original material
        public void ResetMaterial()
        {
            var meshrenderer = GetComponentInChildren<MeshRenderer>();
            if (meshrenderer != null)
            {
                meshrenderer.materials = OriginalMaterial.ToArray();
            }
    
            DestroyImmediate(this);
        }
    }
}