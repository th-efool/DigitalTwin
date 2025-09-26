using UnityEngine;

namespace realvirtual
{
    public class QualityController : MonoBehaviour
    {
        
        public void Awake()
        {
            int quality = PlayerPrefs.GetInt("Quality", -1); 
            if (quality!=-1)
                QualitySettings.SetQualityLevel(quality, true);
            
        }

        public static void SetQuality(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
            PlayerPrefs.SetInt("Quality", level);
            PlayerPrefs.Save();
        }

        public static string GetQualityName()
        {
            return QualitySettings.names[QualitySettings.GetQualityLevel()];
        }
        
    }
}
    
