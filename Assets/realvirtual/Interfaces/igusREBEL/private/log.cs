using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    public static class log {
    
    
        public static void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public static void DebugFormat(string format, params object[] args)
        {
            UnityEngine.Debug.Log(string.Format(format, args));
        }
        
        public static void InfoFormat(string format, params object[] args)
        {
            UnityEngine.Debug.Log(string.Format(format, args));
        }
        
        public static void ErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogError(string.Format(format, args));
        }
    }

}

