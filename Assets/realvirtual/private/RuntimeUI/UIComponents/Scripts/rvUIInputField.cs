// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{

    using realvirtual;
    using TMPro;
    using UnityEngine;


#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class rvUIInputField : MonoBehaviour
    {
        public TextMeshProUGUI labelText;

        // valueInputField;
        public TMP_InputField valueInputField;

        public TMP_InputField.SubmitEvent GetSubmitEvent()
        {
            return valueInputField.onEndEdit;
        }

        public void MakeSelected()
        {
            labelText.gameObject.GetComponent<rvUIColorizer>().Colorize(rvUIColorizer.SkinColor.Selected);
            valueInputField.gameObject.GetComponent<rvUIColorizer>().Colorize(rvUIColorizer.SkinColor.Selected);
        }

        public void MakeUnselected()
        {
            labelText.gameObject.GetComponent<rvUIColorizer>().Colorize(rvUIColorizer.SkinColor.Font);
            valueInputField.gameObject.GetComponent<rvUIColorizer>().Colorize(rvUIColorizer.SkinColor.Font);
        }

        public void SetColor(Color color)
        {
            // TODO: Implement set color in colorizer
        }

        public void ChangeLabelText(string label)
        {
            labelText.text = label;
        }

        public void ChangeValueText(string value)
        {
            valueInputField.text = value;
        }


#pragma warning restore CS3009 // Base type is not CLS-compliant
    }
}