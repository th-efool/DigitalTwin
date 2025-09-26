using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    public class SettingsController : MonoBehaviour
    {

        public GameObject window;
        // Start is called before the first frame update

        public void Start()
        {
            int quality = PlayerPrefs.GetInt("Quality", -1); 
            if (quality!=-1)
                QualitySettings.SetQualityLevel(quality, true);
            quality = QualitySettings.GetQualityLevel();
            window.SetActive(true);
            var tog = GetComponentsInChildren<QualityToggleChange>();
            foreach (var to in tog)
            {
                to.SetQualityStatus(quality);
            }
            window.SetActive(false);
        }

        public void OnQualityToggleChanged(int qualitylevel)
        {
            QualitySettings.SetQualityLevel(qualitylevel, true);
            PlayerPrefs.SetInt("Quality", qualitylevel);
            PlayerPrefs.Save();
        }

        public void CloseSettingsWindow()
        {
            window.SetActive(false);
        }
        

    }
}
    
