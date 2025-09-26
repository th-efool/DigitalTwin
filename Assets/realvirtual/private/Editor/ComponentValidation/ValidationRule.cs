// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! Base class for validation rules that check component configurations.
    //! Inherit from this class to create custom validation rules that run in all contexts.
    public abstract class ValidationRule<T> where T : Component
    {
        //! Gets the display name of this validation rule
        public abstract string RuleName { get; }
        
        //! Validates the component and returns true if valid, false if issues were found
        public abstract bool Validate(T component);
        
        //! Logs a validation warning with consistent formatting
        protected void LogWarning(string message, T component)
        {
            var warningMessage = $"[Validation: {RuleName}] {message}";
            
            // Store in ScriptableObject for display after play mode starts (with object reference)
            ValidationMessageStorage.Instance.AddMessage(warningMessage, LogType.Warning, component.gameObject);
            
            // Also log immediately for those who have console history open
            Logger.Warning(warningMessage, component.gameObject, false);
        }
        
        //! Logs a validation error with consistent formatting
        protected void LogError(string message, T component)
        {
            var errorMessage = $"[Validation: {RuleName}] {message}";
            
            // Store in ScriptableObject for display after play mode starts (with object reference)
            ValidationMessageStorage.Instance.AddMessage(errorMessage, LogType.Error, component.gameObject);
            
            // Also log immediately for those who have console history open
            Logger.Error(errorMessage, component.gameObject, false);
        }
        
        //! Removes a component with undo support
        protected void RemoveComponent(Component component)
        {
            Undo.DestroyObjectImmediate(component);
        }
        
        //! Sets a component property value with undo support
        protected void SetValue(T component, string propertyName, object value, Component targetComponent = null)
        {
            var target = targetComponent ?? (Component)component;
            Undo.RecordObject(target, $"{RuleName} - Set {propertyName}");
            var property = target.GetType().GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(target, value);
            }
        }
    }
    
    //! Base class for validation rules that only run when components are added to GameObjects
    public abstract class ComponentAddedRule<T> : ValidationRule<T> where T : Component
    {
    }
    
    //! Base class for validation rules that only run before entering play mode
    public abstract class PrePlayRule<T> : ValidationRule<T> where T : Component
    {
    }
}
#endif