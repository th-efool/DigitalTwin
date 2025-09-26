// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! Prevents multiple Drive components on the same GameObject.
    //! Automatically removes duplicate Drive components when detected.
    public class PreventMultipleDrivesRule : ComponentAddedRule<Drive>
    {
        public override string RuleName => "Single Drive";
        
        public override bool Validate(Drive drive)
        {
            var drives = drive.GetComponents<Drive>();
            if (drives.Length > 1)
            {
                LogWarning("Only one Drive component allowed per GameObject. Removing duplicate Drive component.", drive);
                // Remove the newest one (the one we just added)
                Undo.DestroyObjectImmediate(drive);
                return false;
            }
            return true;
        }
    }
}
#endif