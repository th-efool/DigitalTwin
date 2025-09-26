// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace realvirtual
{
    public class SettingsWindowCtrl : MonoBehaviour
    {
        public rvUIButtonGroup QualityButtonGroup;
        public GameObject Content;
        public GameObject InputFieldOptions;
        public GameObject ToggleElement;
        public GameObject propertyContentArea;
        
        private List<GameObject> inputFieldOptions = new List<GameObject>();
        private float hightHeader = 60f;
        private float hightButtons= 40f;
        private float currenthightElements = 360f;

        public static void Open()
        {
            var settingsWindow = FindFirstObjectByType<SettingsWindowCtrl>(FindObjectsInactive.Include);
            settingsWindow.gameObject.SetActive(true);
        }

        public static void Close()
        {
            var settingsWindow = FindFirstObjectByType<SettingsWindowCtrl>(FindObjectsInactive.Include);
            settingsWindow.gameObject.SetActive(false);
        }

        private void SetActiveButtons()
        {
            QualityButtonGroup.SetActiveButton(QualityController.GetQualityName());
        }
        public void OnToolbarButtonClicked()
        {
            if (Content.activeSelf)
            {
                Content.SetActive(false);
                OnWindowsClose();
            }
            
            else
            {
                SetActiveButtons();
                SetAdditionalOptions();
                SetWindowhight();
                Content.SetActive(true);
            }
        }
        private void OnEnable()
        {
           
            SetActiveButtons();
            SetAdditionalOptions();
            SetWindowhight();
        }

        private void OnWindowsClose()
        {
            // Save Runtime Persistances
            List<RuntimePersistence> runtimePersistences = Object.FindObjectsByType<RuntimePersistence>(FindObjectsInactive.Include,FindObjectsSortMode.None).ToList();
            if (runtimePersistences.Count > 0)
            {
                foreach (var objvar in runtimePersistences)
                {
                    objvar.OnOptionsWindowClosing.Invoke();
                }
            }
        }
        private void Awake()
        {
            hightButtons = GethightButtonGroup();
            var header = Content.transform.Find("Header");
            if (header != null)
            {
                hightHeader = header.GetComponent<LayoutElement>().preferredHeight;
            }
            currenthightElements = hightHeader + hightButtons + 100;
            SetActiveButtons();
            SetWindowhight();
        }

        private void SetWindowhight()
        {
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currenthightElements);
        }
        private void SetAdditionalOptions()
        {
            if(inputFieldOptions.Count>0)
            {
                foreach (var go in inputFieldOptions)
                {
                    Destroy(go);
                }
                inputFieldOptions.Clear();
            }
            List<RuntimePersistence> runtimePersistences = Object.FindObjectsByType<RuntimePersistence>(FindObjectsInactive.Include,FindObjectsSortMode.None).ToList();
            if (runtimePersistences.Count > 0)
            {
                foreach (var objvar in runtimePersistences)
                {
                    foreach (var property in objvar.PropertiesToSave)
                    {
                        if (property.ShowInOptions)
                        { 
                            var type = property.component.GetType();
                            var field = type.GetField(property.name);
                            var value = field.GetValue(property.component);
                            var fieldType = field.FieldType;
                            if(fieldType==typeof(bool))
                            {
                                 // instanciate the gameobject row below content
                                 var go = Instantiate(ToggleElement, propertyContentArea.transform);
                                 inputFieldOptions.Add(go);
                                 rvUIToggle toggle = go.GetComponentInChildren<rvUIToggle>();
                                 string label = property.component.name + "." + property.name;
                                 toggle.ChangeLabelText(label);
                                 toggle.ToggleTo((bool)value);
                                 toggle.OnToggleOn.AddListener(() =>
                                 {
                                     objvar.CallbackSettingsWindow(property.component, property,value); ;
                                 });
                                toggle.OnToggleOff.AddListener(() =>
                                {
                                    objvar.CallbackSettingsWindow(property.component, property, value); ;
                                });
                                currenthightElements += go.GetComponent<LayoutElement>().preferredHeight;
                            }
                            else if (fieldType == typeof(float))
                            {
                                // Use input field for all float properties
                                var go = Instantiate(InputFieldOptions, propertyContentArea.transform);
                                inputFieldOptions.Add(go);
                                currenthightElements += go.GetComponent<LayoutElement>().preferredHeight;
                                rvUIInputField inputField = go.GetComponentInChildren<rvUIInputField>();
                                
                                // Read attributes using reflection
                                var rangeAttr = field.GetCustomAttribute<RuntimePersistenceRangeAttribute>();
                                var labelAttr = field.GetCustomAttribute<RuntimePersistenceLabelAttribute>();
                                var hintAttr = field.GetCustomAttribute<RuntimePersistenceHintAttribute>();
                                var formatAttr = field.GetCustomAttribute<RuntimePersistenceFormatAttribute>();
                                
                                // Determine label
                                string label = property.component.name + "." + property.name;
                                if (labelAttr != null)
                                {
                                    label = labelAttr.Label;
                                    if (hintAttr != null)
                                    {
                                        label += " " + hintAttr.Hint;
                                    }
                                }
                                
                                // Determine format
                                string format = formatAttr != null ? formatAttr.Format : "F2";
                                
                                inputField.ChangeLabelText(label);
                                inputField.ChangeValueText(((float)value).ToString(format));
                                
                                inputField.GetSubmitEvent().AddListener((newValue) =>
                                {
                                    if (float.TryParse(newValue, out float floatValue))
                                    {
                                        // Apply validation if attribute exists
                                        if (rangeAttr != null)
                                        {
                                            floatValue = Mathf.Clamp(floatValue, rangeAttr.Min, rangeAttr.Max);
                                        }
                                        
                                        field.SetValue(property.component, floatValue);
                                        objvar.CallbackSettingsWindow(property.component, property, floatValue);
                                        
                                        // Update display with formatted value
                                        inputField.ChangeValueText(floatValue.ToString(format));
                                    }
                                    
                                    // Deselect the input field after processing
                                    inputField.valueInputField.DeactivateInputField();
                                });
                            }
                            else
                            {
                               // instanciate the gameobject row below content for other types (strings, etc.)
                               var go = Instantiate(InputFieldOptions, propertyContentArea.transform);
                               inputFieldOptions.Add(go);
                               currenthightElements += go.GetComponent<LayoutElement>().preferredHeight;
                               rvUIInputField inputField = go.GetComponentInChildren<rvUIInputField>();
                               string label = property.component.name + "." + property.name;
                               inputField.ChangeLabelText(label);
                               inputField.ChangeValueText(value.ToString());
                               inputField.GetSubmitEvent().AddListener((value) =>
                               {
                                   objvar.CallbackSettingsWindow(property.component, property, value);
                                   // Deselect the input field after processing
                                   inputField.valueInputField.DeactivateInputField();
                               });
                            }
                           
                        }
                    }
                }
            }
           
        }

        private float GethightButtonGroup()
        {
            foreach (Transform child in QualityButtonGroup.gameObject.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    if(child.GetComponent<LayoutElement>())
                        return child.GetComponent<LayoutElement>().preferredHeight;
                    else
                        return child.GetComponent<RectTransform>().rect.height;
                }
            }
            return 0;
        }

        // Public method to change the quality settings where 0 is lowest and 5 is highest
        public void ChangeQualitySettings(int level)
        {
            QualityController.SetQuality(level);
            QualityButtonGroup.SetActiveButton(QualityController.GetQualityName());
        }
    }
}