// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;

using UnityEditor;
using NaughtyAttributes;
using UnityEngine.Events;


namespace realvirtual
{
    using System.Collections.Generic;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    [Serializable]
    public class DataField
    {
      
        public string value; // Store everything as a string to keep flexibility

       
        public DataField(object obj)
        {
           
            if (obj is string || obj is int || obj is float || obj is bool) 
            {
                this.value = obj.ToString(); // Store as string
            }
            else
            {
                this.value = JsonUtility.ToJson(obj); // Serialize objects
            }
        }
    }

    public class RuntimePersistence : realvirtualBehavior, IBeforeAwake, IInitAwake 
    {
        // class containing the component and the name of the property
        [Serializable]
        public class Property
        {
            public Component component;
            public string name;
            public bool ShowInOptions;
            public UnityEvent OnValueChanged;
        }
        
        public bool LoadBeforeAwake = true;
        public bool SaveOnDisable = true;

        // A list of components and objects that should be saved
        public List<Property> PropertiesToSave = new List<Property>();
        
        // Event that is called when optionswindow is closing
        public UnityEvent OnOptionsWindowClosing;

        private bool onstartloaded = false;
        
        private string GetName()
        {
            return "realvirtual" + "-" + SceneManager.GetActiveScene().name + "-" + this.name;
        }

        [Button("Save Persistences")]
        public void SavePersistences()
        {
            foreach (var property in PropertiesToSave)
            {
                var component = property.component;
                var fieldName = property.name;
                var type = component.GetType();
                var field = type.GetField(fieldName);

                if (field == null)
                {
                    Debug.LogError($"Persistence Error: Component [{component.name}], Field [{fieldName}] not found.");
                }
                else
                {
                    var value = field.GetValue(component);
                    DataField dataField = new DataField(value);
                    Persistence.Save(dataField, this.gameObject.name, fieldName);
                }
            }
        }

        [Button("Load Persistences")]
        public void LoadPersistences()
        {
            onstartloaded = true;
            foreach (var property in PropertiesToSave)
            {
                var component = property.component;
                var fieldName = property.name;
                var type = component.GetType();
                var field = type.GetField(fieldName);

                if (field == null)
                {
                    Debug.LogError($"Persistence Error: Component [{component.name}], Field [{fieldName}] not found.");
                }
                else
                {
                    var value = field.GetValue(component);
                    var fieldType = field.FieldType;
                    value = Convert.ChangeType(value, fieldType);
                    
                    if (Persistence.Load(ref value, this.gameObject.name, fieldName))
                    {
                        field.SetValue(component, value);
                    }

                    if (CheckDebugMode())
                    {
                        Debug.Log($"Loaded: {component.name} {fieldName} = {value}");
                    }
                }
            }

            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        [Button("Delete Persistences")]
        public void DeletePersistences()
        {
            foreach (var property in PropertiesToSave)
            {
                var component = property.component;
                var fieldName = property.name;
                Persistence.Delete(component.name, fieldName);
                Debug.Log($"Deleted: {component.name} {fieldName}");
            }
        } 
        public void CallbackSettingsWindow(Component component,Property property, object value)
        {
            var type = component.GetType();
            var field = type.GetField(property.name);
            var fieldType = field.FieldType;

            if (fieldType == typeof(int))
            {
                value = Convert.ToInt32(value);
            }
            else if (fieldType == typeof(uint))
            {
                value = Convert.ToUInt32(value);
            }
            else if (fieldType == typeof(bool)) // Toggle with only one callback
            {
                var currentValue = (bool)field.GetValue(component);
                if(currentValue)
                    value = false;
                else
                    value = true;
            }
            else if (fieldType == typeof(float))
            {
                value = Convert.ToSingle(value);
            }
            else
            {
                value = Convert.ChangeType(value, fieldType);
            }

            field.SetValue(component, value);
            property.OnValueChanged?.Invoke();
        }
        
        public void OnBeforeAwake()
        {

           if (LoadBeforeAwake) LoadPersistences();

}

        private static bool CheckDebugMode()
        {
            // Get realvirtual controller in the current scene
            var controller = Object.FindAnyObjectByType<realvirtualController>(FindObjectsInactive.Include);
            var debugmode = false;

            if (controller != null)
            {
                debugmode = controller.DebugMode;
            }

            return debugmode;
        }
        
        void OnDisable()
        {

            if (SaveOnDisable) SavePersistences();

        }

        public void InitAwake()
        {
            // if scene is loaded a second time
            if (!onstartloaded)
            {
                LoadPersistences();
            }
        }
    }
}
