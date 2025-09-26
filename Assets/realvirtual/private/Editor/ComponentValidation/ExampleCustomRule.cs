// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! Example validation rule that checks if sensors are placed on moving parts.
    //! This rule runs in all validation contexts.
    public class ExampleValidationRule : ValidationRule<Sensor>
    {
        public override string RuleName => "Example Rule";
        
        public override bool Validate(Sensor sensor)
        {
            // Example: Check if sensor is on a moving part
            var drive = sensor.GetComponentInParent<Drive>();
            if (drive != null)
            {
                LogWarning("Sensor placed on moving part - this may cause inaccurate readings", sensor);
                return false;
            }
            
            return true;
        }
    }
    
    //! Example rule that validates sensor signal configuration when a sensor is added.
    //! Only runs when Sensor components are added to GameObjects.
    public class ExampleComponentAddedRule : ComponentAddedRule<Sensor>
    {
        public override string RuleName => "Sensor Setup";
        
        public override bool Validate(Sensor sensor)
        {
            // Example: Check if sensor has occupied signal configured
            if (sensor.SensorOccupied == null && sensor.SensorNotOccupied == null)
            {
                // Both signals are null - sensor won't communicate its state
                LogWarning("No signals configured on sensor - it won't communicate occupation state", sensor);
                return false;
            }
            return true;
        }
    }
    
    //! Example rule that validates drive configuration before entering play mode.
    //! Checks if drives have movement configured through speed or jogging settings.
    public class ExamplePrePlayRule : PrePlayRule<Drive>
    {
        public override string RuleName => "Drive Ready Check";
        
        public override bool Validate(Drive drive)
        {
            // Example: Check if drive is properly configured for play mode
            if (drive.TargetSpeed == 0 && drive.JogForward == false && drive.JogBackward == false)
            {
                LogWarning("Drive has no movement configured", drive);
                return false;
            }
            return true;
        }
    }
    
    // No registration needed! Rules are automatically discovered.
    // To disable a rule, simply comment it out or delete it.
}
#endif