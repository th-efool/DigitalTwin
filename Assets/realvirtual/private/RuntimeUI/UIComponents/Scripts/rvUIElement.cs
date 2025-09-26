using UnityEngine;
using UnityEngine.Events;
using TMPro;


namespace realvirtual
{
    public class rvUIElement : MonoBehaviour
    {
        public string label = "Element";
        public string description = "Description";
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;

        public GameObject HandleSelected;
        public GameObject HandleDeselected;
        public GameObject ActivateButton;
        public GameObject DeactivateButton;
        
        private void OnValidate()
        {
            var texts = GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = label;

            if (texts.Length > 1) texts[1].text = description;
        }

        public void SetTexts(string label, string description)
        {
            this.label = label;
            this.description = description;
            OnValidate();
        }


        public void Select()
        {
            HandleSelected.SetActive(true);
            HandleDeselected.SetActive(false);
            ActivateButton.SetActive(false);
            DeactivateButton.SetActive(true);
            OnSelected.Invoke();
        }

        public void Deselect()
        {
            HandleSelected.SetActive(false);
            HandleDeselected.SetActive(true);
            ActivateButton.SetActive(true);
            DeactivateButton.SetActive(false);
            OnDeselected.Invoke();
        }

    }
}