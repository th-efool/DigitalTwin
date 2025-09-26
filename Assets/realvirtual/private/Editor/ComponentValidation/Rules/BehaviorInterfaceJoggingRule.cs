// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;

namespace realvirtual
{
    //! Validates and fixes conflicts between BehaviorInterface and Drive jogging settings.
    //! When a BehaviorInterface will be active, it disables Drive jogging to prevent conflicts.
    public class BehaviorInterfaceJoggingRule : PrePlayRule<BehaviorInterface>
    {
        public override string RuleName => "Behavior Jogging";
        
        public override bool Validate(BehaviorInterface behaviorInterface)
        {
            // Check if BehaviorInterface will be active during play mode
            // Assume connected state (true) as it's the most common scenario
            bool willBeActive = ConnectionState.IsActive(behaviorInterface);
            
            // Only check for conflicts if BehaviorInterface will be active
            if (willBeActive)
            {
                var drive = behaviorInterface.GetComponent<Drive>();
                if (drive != null && (drive.JogForward || drive.JogBackward))
                {
                    SetValue(behaviorInterface, "JogForward", false, drive);
                    SetValue(behaviorInterface, "JogBackward", false, drive);
                    LogWarning("Disabled jogging on Drive. Active BehaviorInterface will control the drive position and conflicts with manual jogging.", behaviorInterface);
                    return false;
                }
            }
            
            return true;
        }
    }
}
#endif