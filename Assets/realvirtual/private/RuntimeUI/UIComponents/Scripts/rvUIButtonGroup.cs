// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace realvirtual
{
#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class rvUIButtonGroup : MonoBehaviour
    {
        private Button[] buttons;

        private void Start()
        {
            CheckInitButtons();
        }

        private void CheckInitButtons()
        {
            if (buttons == null)
            {
                buttons = GetComponentsInChildren<Button>(true);
                foreach (var button in buttons)
                    button.onClick.AddListener(() => SetActiveButton(GetButtonName(button)));
            }
        }

        private string GetButtonName(Button button)
        {
            if (button == null) return null;
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null) return null;
            return button.GetComponentInChildren<TextMeshProUGUI>().text;
        }

        public void SetActiveButton(string buttonName)
        {
            CheckInitButtons();
            foreach (var button in buttons)
                if (GetButtonName(button) == buttonName)
                    button.GetComponent<rvUIColorizer>().Colorize(rvUIColorizer.SkinColor.Selected);
                else
                    button.GetComponent<rvUIColorizer>().Colorize(rvUIColorizer.SkinColor.Button);
        }
    }
}
#pragma warning restore CS3009 // Base type is not CLS-compliant