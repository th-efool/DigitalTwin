// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! ScriptableObject that stores validation messages across play mode transitions
    public class ValidationMessageStorage : ScriptableObject
    {
        [System.Serializable]
        public class ValidationMessage
        {
            public string message;
            public LogType logType;
            public string timestamp;
            public Object context;  // Store the GameObject/Component reference
            
            public ValidationMessage(string msg, LogType type, Object ctx = null)
            {
                message = msg;
                logType = type;
                timestamp = System.DateTime.Now.ToString("HH:mm:ss");
                context = ctx;
            }
        }
        
        public List<ValidationMessage> messages = new List<ValidationMessage>();
        public bool hasNewMessages = false;
        
        private static ValidationMessageStorage _instance;
        
        //! Gets or creates the singleton instance
        public static ValidationMessageStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to load existing instance
                    string[] guids = AssetDatabase.FindAssets("t:ValidationMessageStorage");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<ValidationMessageStorage>(path);
                    }
                    
                    // Create new instance if none exists
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<ValidationMessageStorage>();
                        
                        // Ensure directory exists
                        string directory = "Assets/realvirtual/private/Editor/ComponentValidation/Resources";
                        if (!AssetDatabase.IsValidFolder(directory))
                        {
                            System.IO.Directory.CreateDirectory(directory);
                            AssetDatabase.Refresh();
                        }
                        
                        // Save as asset
                        string assetPath = $"{directory}/ValidationMessageStorage.asset";
                        AssetDatabase.CreateAsset(_instance, assetPath);
                        AssetDatabase.SaveAssets();
                    }
                }
                return _instance;
            }
        }
        
        //! Adds a validation message
        public void AddMessage(string message, LogType logType = LogType.Log, Object context = null)
        {
            messages.Add(new ValidationMessage(message, logType, context));
            hasNewMessages = true;
            EditorUtility.SetDirty(this);
        }
        
        //! Clears all messages
        public void Clear()
        {
            messages.Clear();
            hasNewMessages = false;
            EditorUtility.SetDirty(this);
        }
        
        //! Displays stored messages and clears them
        public void DisplayAndClear()
        {
            if (!hasNewMessages || messages.Count == 0)
                return;
            
            // Count actual issues (not status messages)
            int warningCount = 0;
            int errorCount = 0;
            foreach (var msg in messages)
            {
                if (msg.logType == LogType.Warning) warningCount++;
                else if (msg.logType == LogType.Error) errorCount++;
            }
            
            // Only show if there are actual warnings or errors
            if (warningCount == 0 && errorCount == 0)
            {
                Clear();
                return;
            }
            
            Logger.Warning("=============== PRE-PLAY VALIDATION ISSUES ===============", null, false);
            Logger.Warning($"Found {warningCount} warning(s) and {errorCount} error(s) at {messages[0].timestamp}", null, false);
            
            // Only show warning/error messages, skip status messages
            foreach (var msg in messages)
            {
                if (msg.logType == LogType.Warning)
                {
                    Logger.Warning(msg.message, msg.context, false);
                }
                else if (msg.logType == LogType.Error)
                {
                    Logger.Error(msg.message, msg.context, false);
                }
            }
            
            Logger.Warning("⚠ These issues should be fixed in Edit Mode before running.", null, false);
            Logger.Warning("To disable validation: realvirtualController > ValidateBeforeStart = false", null, false);
            Logger.Warning("=========================================================", null, false);
            
            Clear();
        }
        
    }
    
    //! Displays validation messages after entering play mode
    [InitializeOnLoad]
    public static class ValidationMessageDisplayer
    {
        static ValidationMessageDisplayer()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Display messages after a short delay to ensure console is ready
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        ValidationMessageStorage.Instance.DisplayAndClear();
                    };
                };
            }
        }
    }
}
#endif