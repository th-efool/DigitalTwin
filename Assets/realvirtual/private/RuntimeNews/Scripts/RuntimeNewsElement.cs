// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace realvirtual
{
    public class RuntimeNewsElement : MonoBehaviour
    {
        public TMPro.TMP_Text title;
        public TMPro.TMP_Text text;
        public Button button;
        public TMPro.TMP_Text buttonText;
        public int buttonSpacing = 40;

        public void SetNews(string title, string text)
        {
            this.title.text = title;
            this.text.text = text;
        }

        public void SetButton(string text, UnityEngine.Events.UnityAction action)
        {
            buttonText.text = text;
            button.onClick.AddListener(action);
            button.gameObject.SetActive(true);

            if (this.text is TextMeshProUGUI uiText)
            {
                // Set margins: left, top, right, bottom
                uiText.margin = new Vector4(uiText.margin.x, uiText.margin.y, uiText.margin.z,
                    uiText.margin.w + buttonSpacing);
                uiText.ForceMeshUpdate();
            }
        }


    }
}