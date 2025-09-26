// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using System.Linq;

namespace realvirtual
{
    //! Validates that only one BehaviorInterface is active on a GameObject at play time.
    //! Multiple active BehaviorInterfaces can cause conflicts and unexpected behavior.
    public class MultipleBehaviorInterfacesRule : PrePlayRule<BehaviorInterface>
    {
        public override string RuleName => "Multiple BehaviorInterfaces";
        
        public override bool Validate(BehaviorInterface behaviorInterface)
        {
            // Get all BehaviorInterface components on the same GameObject
            var allBehaviorInterfaces = behaviorInterface.GetComponents<BehaviorInterface>();
            
            // Check if multiple will be active during play mode
            var activeInterfaces = allBehaviorInterfaces
                .Where(bi => bi != null && bi.enabled && ConnectionState.IsActive(bi))
                .ToList();
            
            // If more than one active BehaviorInterface found
            if (activeInterfaces.Count > 1)
            {
                // Find which ones to disable (keep the first one active)
                foreach (var bi in activeInterfaces.Skip(1))
                {
                    if (bi != behaviorInterface)
                        continue;
                    
                    // Disable this BehaviorInterface
                    SetValue(behaviorInterface, "enabled", false);
                    
                    var interfaceNames = string.Join(", ", activeInterfaces.Select(b => b.GetType().Name));
                    LogWarning($"Multiple active BehaviorInterfaces detected ({interfaceNames}). Disabled {behaviorInterface.GetType().Name} to prevent conflicts. Only one BehaviorInterface should be active per GameObject.", behaviorInterface);
                    return false;
                }
            }
            
            return true;
        }
    }
}
#endif