
using UnityEngine;

namespace realvirtual
{
    public class ToolbarLogoButton : MonoBehaviour
    {

        public GameObject Infobox;
    
        // Start is called before the first frame update
        public void ToggleInfobox(bool val)
        {
            if (Infobox.activeSelf)
            {
                Infobox.SetActive(false);
            }
            else
            {
                Infobox.SetActive(true);
            }
        }

  
    }
}
