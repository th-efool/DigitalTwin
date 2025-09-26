// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;

namespace realvirtual
{
    //! Base attribute for runtime persistence metadata
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class RuntimePersistenceAttribute : Attribute { }

    //! Validation range for numeric fields
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RuntimePersistenceRangeAttribute : RuntimePersistenceAttribute
    {
        public float Min { get; set; }
        public float Max { get; set; }
        
        public RuntimePersistenceRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    //! Custom label for settings display
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RuntimePersistenceLabelAttribute : RuntimePersistenceAttribute
    {
        public string Label { get; set; }
        
        public RuntimePersistenceLabelAttribute(string label)
        {
            Label = label;
        }
    }

    //! Format string for display (e.g., "F2" for 2 decimal places)
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RuntimePersistenceFormatAttribute : RuntimePersistenceAttribute
    {
        public string Format { get; set; }
        
        public RuntimePersistenceFormatAttribute(string format)
        {
            Format = format;
        }
    }

    //! Step value for increment/decrement operations
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RuntimePersistenceStepAttribute : RuntimePersistenceAttribute
    {
        public float Step { get; set; }
        
        public RuntimePersistenceStepAttribute(float step)
        {
            Step = step;
        }
    }

    //! Tooltip/hint text to display alongside the field
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RuntimePersistenceHintAttribute : RuntimePersistenceAttribute
    {
        public string Hint { get; set; }
        
        public RuntimePersistenceHintAttribute(string hint)
        {
            Hint = hint;
        }
    }
}