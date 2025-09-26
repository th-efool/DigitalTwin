using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    public class FloorSetup : MonoBehaviour
    {
        [OnValueChanged("Apply")] public float size = 10;
        [OnValueChanged("Apply")] public bool useFade;

        public GameObject standardRoot;
        public GameObject fadeRoot;
        public GameObject logo;
        
        [OnValueChanged("Apply")]
        public Vector2 logoOffset;


        void OnValidate()
        {
            if (useFade)
            {
                ApplyFade();
            }
        }
        
        public void Apply()
        {
            standardRoot.SetActive(!useFade);
            fadeRoot.SetActive(useFade);

            if (useFade)
            {
                ApplyFade();
            }
            else
            {
                standardRoot.transform.localScale = new Vector3(size, 1, size);

            }
            
            float offset = size / 2f;
            
            logo.transform.localPosition = new Vector3(-offset+logoOffset.x, 0.01f, -offset+logoOffset.y);
        }

        void ApplyFade()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetFloat("_FadeStart", size / 2f);
            
            GameObject near = fadeRoot.transform.Find("Plane Near").gameObject;
            GameObject far = fadeRoot.transform.Find("Plane Far").gameObject;
            
            near.GetComponent<MeshRenderer>().SetPropertyBlock(block);
            far.GetComponent<MeshRenderer>().SetPropertyBlock(block);
            
            
        }

    }
}