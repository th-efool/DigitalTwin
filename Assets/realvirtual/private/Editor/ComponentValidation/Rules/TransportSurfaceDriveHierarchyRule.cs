// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  


using UnityEngine;
using System.Linq;

namespace realvirtual
{
    //! Validates that TransportSurface components don't have more than one Drive component above them in the hierarchy.
    //! This prevents Unity physics violations that can occur when multiple drives control transform surfaces from above.
    public class TransportSurfaceDriveHierarchyRule : PrePlayRule<TransportSurface>
    {
        public override string RuleName => "TransportSurface Drive Hierarchy";
        
        public override bool Validate(TransportSurface transportSurface)
        {
            // GetComponentsInParent includes the current GameObject and all parents
            var drivesInHierarchy = transportSurface.GetComponentsInParent<Drive>();
            int driveCount = drivesInHierarchy.Length;
            
            // Check for no Drive (but allow DriveReference as alternative)
            if (driveCount == 0)
            {
                // If DriveReference is set, that's acceptable - no hierarchy Drive needed
                if (transportSurface.DriveReference != null)
                {
                    return true; // DriveReference satisfies the Drive requirement
                }
                
                LogWarning("TransportSurface requires a Drive component in the hierarchy to function properly. Please add a Drive component to this GameObject or a parent GameObject, or assign DriveReference.", transportSurface);
                return false;
            }
            
            // More than one Drive above the TransportSurface is not allowed
            if (driveCount > 1)
            {
                LogWarning("To not violate Unity physics a Drive is not allowed to be above a TransportSurface in GameObject hierarchy. Please check https://doc.realvirtual.io/components-and-scripts/motion/transportsurface#transport-surfaces-and-unity-physics", transportSurface);
                return false;
            }
            
            return true;
        }
    }
}
