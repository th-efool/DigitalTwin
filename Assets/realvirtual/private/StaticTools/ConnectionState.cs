// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    //! Helper class that determines if components will be active based on their ActiveOnly settings.
    //! Checks the realvirtualController connection state and component enabled states.
    public static class ConnectionState
    {
        //! Checks if a realvirtualBehavior component will be active based on its ActiveOnly setting
        public static bool IsActive(realvirtualBehavior behavior, bool assumeConnected = true)
        {
            if (behavior == null) return false;
            
            switch (behavior.Active)
            {
                case realvirtualBehavior.ActiveOnly.Always:
                    return true;
                    
                case realvirtualBehavior.ActiveOnly.Never:
                    return false;
                    
                case realvirtualBehavior.ActiveOnly.DontChange:
                    return behavior.enabled;
                    
                case realvirtualBehavior.ActiveOnly.Connected:
                case realvirtualBehavior.ActiveOnly.Disconnected:
                    if (Global.realvirtualcontroller != null)
                    {
                        bool isConnected = Global.realvirtualcontroller.Connected;
                        return behavior.Active == realvirtualBehavior.ActiveOnly.Connected ? isConnected : !isConnected;
                    }
                    return behavior.Active == realvirtualBehavior.ActiveOnly.Connected ? assumeConnected : !assumeConnected;
                    
                default:
                    return true;
            }
        }
        
    }
}