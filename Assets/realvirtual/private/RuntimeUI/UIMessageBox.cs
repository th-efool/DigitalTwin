// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;
using UnityEngine.UI;


namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/ui-components")]
    //! Displays a message box with a text field in the middle of the gameview
    public class UIMessageBox : MonoBehaviour
    {
        public Text TextBox; //!< A pointer to the Unity UI text box

        
        private void DestroyMessage()
        {
            Destroy(gameObject);
        }

        //! Displays the message on the middle of the gameview
        public void DisplayMessage(string message, bool autoclose, float closeafterseconds)
        {
            
            TextBox.text = message;
            if (autoclose)
            {
                Invoke("DestroyMessage", closeafterseconds);
            }
        }
    }
}
