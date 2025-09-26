using UnityEngine;
using UnityEngine.Events;
using TMPro;


namespace realvirtual
{

    public class rvUIToggle : MonoBehaviour
    {
        public string label = "Toggle";
        public UnityEvent OnToggleOn;
        public UnityEvent OnToggleOff;
        public UnityEvent<bool> OnToggle;
        public GameObject handleOn;
        public GameObject handleOff;
        private bool isOn;

        private void OnValidate()
        {
            ChangeLabelText(label);
        }
        public void Toggle()
        {
            if (isOn)
                ToggleOff();
            else
                ToggleOn();
        }
        public void ChangeLabelText(string label)
        {
            var text = GetComponentInChildren<TMP_Text>(true);
            this.label = label;
            text.text = label;
        }
        public void ToggleTo(bool state)
        {
            if (state)
                ToggleOn();
            else
                ToggleOff();
        }
        public void ToggleOn()
        {
            isOn = true;
            handleOff.gameObject.SetActive(false);
            handleOn.gameObject.SetActive(true);
            OnToggleOn.Invoke();
            OnToggle.Invoke(isOn);
        }
        public void ToggleOff()
        {
            isOn = false;
            handleOff.gameObject.SetActive(true);
            handleOn.gameObject.SetActive(false);
            OnToggleOff.Invoke();
            OnToggle.Invoke(isOn);
        }

    }
}