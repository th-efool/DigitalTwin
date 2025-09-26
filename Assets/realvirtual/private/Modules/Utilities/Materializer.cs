using UnityEngine;

namespace realvirtual
{
    public class Materializer : MonoBehaviour
    {
        public new MeshRenderer renderer;
        public Material[] materials;
        public int submeshIndex;


        public void SetMaterial(int index)
        {
            if (index < 0 || index >= materials.Length)
            {
                Debug.LogError("Material index out of bounds");
                return;
            }

            Material[] sharedMaterials = renderer.sharedMaterials;
            sharedMaterials[submeshIndex] = materials[index];
            renderer.sharedMaterials = sharedMaterials;

        }

    }

}
