using NaughtyAttributes;
using UnityEngine;

namespace realvirtual{

    public class EvironmentSetup : MonoBehaviour
    {
        public bool advanced = false;

        [Header("Floor")]
        public float size = 10;
        public bool fade = false;
        
        
        [Button]
        public void Activate(){            
        
            gameObject.SetActive(true);
            
            GameObject go = GameObject.Find("realvirtual");

            if(go == null){
                return;
            }
            
            SetupFloor();
            
            SkyboxSetup skyboxSetup = GetComponentInChildren<SkyboxSetup>();
            if(skyboxSetup != null){
                skyboxSetup.advanced = advanced;
                skyboxSetup.Apply();
            }
            
            
        }

        public void SetupFloor()
        {
            FloorSetup floorSetup = GetComponentInChildren<FloorSetup>();
            if(floorSetup != null){
                floorSetup.size = size;
                floorSetup.useFade = fade;
                floorSetup.Apply();
            }
        }

        [Button]
        public void Deactivate()
        { 
            gameObject.SetActive(false);
        }
    }



}
