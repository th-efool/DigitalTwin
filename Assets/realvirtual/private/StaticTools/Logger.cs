// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    //! Static logging class for realvirtual framework with automatic hierarchy path inclusion
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class Logger
    {
        private const string PINK = "#FF1493";
        private const string ICON = "◆";
        private const string WARNING_ICON = "⚠";
        private const string ERROR_ICON = "✖";
        
        //! Formats the message with component path if context is provided
        private static string FormatMessageWithPath(string message, Object context)
        {
            if (context == null) return message;
            
            GameObject go = null;
            
            // Handle different context types
            if (context is GameObject)
            {
                go = context as GameObject;
            }
            else if (context is Component)
            {
                go = (context as Component).gameObject;
            }
            
            if (go != null)
            {
                string path = SceneTools.GetObjectPath(go);
                return $"{message} [{path}]";
            }
            
            return message;
        }
        
        //! Logs a message without stack trace
        [HideInCallstack]
        public static void Message(string message, Object context = null)
        {
            var messageWithPath = FormatMessageWithPath(message, context);
            var formatted = $"<color={PINK}>{ICON}</color> <b>realvirtual:</b> {messageWithPath}";
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, context, formatted);
        }
        
        //! Logs a message with stack trace (excluding Logger from stack)
        [HideInCallstack]
        public static void Log(string message, Object context = null, bool showStackTrace = true)
        {
            var messageWithPath = FormatMessageWithPath(message, context);
            var formatted = $"<color={PINK}>{ICON}</color> <b>realvirtual:</b> {messageWithPath}";
            if (showStackTrace)
                Debug.Log(formatted, context);
            else
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, context, formatted);
        }
        
        //! Logs a warning with stack trace (excluding Logger from stack)
        [HideInCallstack]
        public static void Warning(string message, Object context = null, bool showStackTrace = true)
        {
            var messageWithPath = FormatMessageWithPath(message, context);
            var formatted = $"<color={PINK}>{WARNING_ICON}</color> <b>realvirtual:</b> {messageWithPath}";
            if (showStackTrace)
                Debug.LogWarning(formatted, context);
            else
                Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, context, formatted);
        }
        
        //! Logs an error with stack trace (excluding Logger from stack)
        [HideInCallstack]
        public static void Error(string message, Object context = null, bool showStackTrace = true)
        {
            var messageWithPath = FormatMessageWithPath(message, context);
            var formatted = $"<color={PINK}>{ERROR_ICON}</color> <b>realvirtual:</b> {messageWithPath}";
            if (showStackTrace)
                Debug.LogError(formatted, context);
            else
                Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, context, formatted);
        }
    }
}