// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using System;
using UnityEngine;

namespace realvirtual
{
    /// <summary>
    /// Internal interface for the validation system to work with generic rules
    /// </summary>
    internal interface IValidationRuleBase
    {
        string RuleName { get; }
        Type ComponentType { get; }
        bool ValidateComponent(Component component);
        bool IsComponentAddedRule { get; }
        bool IsPrePlayRule { get; }
    }
    
    /// <summary>
    /// Wrapper to make generic rules work with the validation system
    /// </summary>
    internal class ValidationRuleWrapper<T> : IValidationRuleBase where T : Component
    {
        private readonly ValidationRule<T> rule;
        
        public ValidationRuleWrapper(ValidationRule<T> rule)
        {
            this.rule = rule;
        }
        
        public string RuleName => rule.RuleName;
        public Type ComponentType => typeof(T);
        public bool IsComponentAddedRule => rule is ComponentAddedRule<T>;
        public bool IsPrePlayRule => rule is PrePlayRule<T>;
        
        public bool ValidateComponent(Component component)
        {
            if (component is T typedComponent)
            {
                return rule.Validate(typedComponent);
            }
            return true;
        }
    }
}
#endif