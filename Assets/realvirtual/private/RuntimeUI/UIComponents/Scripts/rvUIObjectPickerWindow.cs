// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace realvirtual
{
    public class rvUIObjectPickerWindow : MonoBehaviour
    {
        public GameObject Container;
        public static void Open(Action<Object> onObjectSelected, Type type)
        {
        
            rvUIObjectPickerWindow w = FindFirstObjectByType<rvUIObjectPickerWindow>(FindObjectsInactive.Include);

            w.ClearElements();
        
            w.gameObject.SetActive(true);
        
            w.FillWithElements(onObjectSelected, type);
        
        
        }
        public void ClearElements()
        {
            foreach (Transform child in Container.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void ApplyObjectAndClose(Action<Object> onObjectSelected, Object obj)
        {
            onObjectSelected(obj);
            gameObject.SetActive(false);
        }

        private void FillWithElements(Action<Object> onObjectSelected, Type type)
        {
        
            Object[] objects = FindObjectsByType(type, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
            GameObject prefab = UnityEngine.Resources.Load<GameObject>("rvUIElement");

        
            foreach (Object obj in objects)
            {
                // Instantiate a new element
            
                GameObject element = Instantiate(prefab, Container.transform);


                // Set up the element (assuming it has a component to handle this)
                rvUIElement pickerElement = element.GetComponentInChildren<rvUIElement>();
            
                pickerElement.SetTexts(obj.name, type.ToString().Replace("realvirtual.", ""));
            
                pickerElement.ActivateButton.GetComponent<Button>().onClick.AddListener(() => 
                    ApplyObjectAndClose(onObjectSelected, obj)
                );
            }
        }
    }

}
