// // realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// // (c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace realvirtual
{
    public static class Persistence
    {
        
        [System.Serializable]
        private class DataField
        {
            public string value;
        }

        public static void Save<T>(T obj, string component, string propname)
        {
            string name = GetName(component) + "-" + propname;
            string value;

            var type = obj.GetType();
            
            if (type.IsPrimitive || type == typeof(string))
            {
                DataField dataField = new DataField { value = obj.ToString() };
                value = JsonUtility.ToJson(dataField);
            }
            else
            {
                value = JsonUtility.ToJson(obj);
            }

            PlayerPrefs.SetString(name, value);
            PlayerPrefs.Save();
            
            if (CheckDebugMode())
            {
                Debug.Log($"Saved: {name} - {value}");
            }
        }
        
        public static bool Load<T>(ref T obj, string component, string propname)
        {
            // Generate a unique key using component and property name
            string name = GetName(component) + "-" + propname;

            // Retrieve JSON from PlayerPrefs
            string json = PlayerPrefs.GetString(name, string.Empty);
            
            if (CheckDebugMode())
            {
                Debug.Log($"Loaded: {name} - {json}");
            }
            // Check if the JSON is valid
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    // get type of obj
                    var type = obj.GetType();
                    
                    if (type.IsPrimitive || type == typeof(string))
                    {
                        DataField dataField = JsonUtility.FromJson<DataField>(json);
                        //set value of obj to the value of the datafield
                        obj = (T)Convert.ChangeType(dataField.value, type);
                    }
                    else
                    {
                        JsonUtility.FromJsonOverwrite(json, obj); // Overwrite for objects
                    }

                    return true;
                }
            }
            catch
            {
                PlayerPrefs.DeleteKey(name);
            }

            return false; // No saved data found
        }

      

        public static void Delete(string component, string propname)
        {
            var name = GetName(component) + "-" + propname;
            PlayerPrefs.DeleteKey(name);
            var debugmode = true;
            if (Global.realvirtualcontroller != null)
            {
                debugmode = Global.realvirtualcontroller.DebugMode;
            }
            if (debugmode)
            {
                Debug.Log("Deleted: " + name);
            }
        }
        private static string GetName(string component)
        {
            return "realvirtual" + "-" + SceneManager.GetActiveScene().name + "-" + component;
        }
        
        private static bool CheckDebugMode()
        {
            return false;
        }
    }
}