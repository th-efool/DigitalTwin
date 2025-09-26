// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! Automatic component validation system that discovers and executes validation rules.
    //! Validation rules are automatically discovered through reflection and executed when components
    //! are added, removed, or before entering play mode.
    [InitializeOnLoad]
    public static class ComponentValidation
    {
        private static Dictionary<Type, List<object>> _allRules = new Dictionary<Type, List<object>>();
        private static bool _initialized = false;
        
        static ComponentValidation()
        {
            Initialize();
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            
            // Subscribe to component removal events
            ComponentAdditionEvents.OnComponentRemovedEvent += OnComponentRemoved;
        }
        
        //! Discovers and registers all validation rules in the project through reflection
        public static void Initialize()
        {
            if (_initialized) return;
            
            _allRules.Clear();
            
            // Find all validation rule types
            var baseType = typeof(ValidationRule<>);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || !type.IsClass) continue;
                        
                        // Check if it inherits from ValidationRule<T>
                        var current = type.BaseType;
                        while (current != null)
                        {
                            if (current.IsGenericType && current.GetGenericTypeDefinition() == baseType)
                            {
                                // Get the component type T
                                var componentType = current.GetGenericArguments()[0];
                                
                                // Create instance
                                var instance = Activator.CreateInstance(type);
                                
                                // Add to dictionary
                                if (!_allRules.ContainsKey(componentType))
                                {
                                    _allRules[componentType] = new List<object>();
                                }
                                _allRules[componentType].Add(instance);
                                
                                break;
                            }
                            current = current.BaseType;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"[ComponentValidation] Failed to process assembly {assembly.GetName().Name}: {e.Message}", null, false);
                }
            }
            
            _initialized = true;
        }
        
        private static GameObject _lastProcessedObject;
        
        //! Handles hierarchy changes to detect newly added components
        private static void OnHierarchyChanged()
        {
            if (Selection.activeGameObject == null || Selection.activeGameObject == _lastProcessedObject)
                return;
                
            _lastProcessedObject = Selection.activeGameObject;
            var components = Selection.activeGameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component != null)
                    OnComponentAdded(component);
            }
        }
        
        private static void OnComponentAdded(Component component)
        {
            if (component == null) return;
            
            // Check if component validation is enabled
            var controller = UnityEngine.Object.FindFirstObjectByType<realvirtualController>();
            if (controller != null && !controller.ValidationOnComponentsAdded)
                return;
            
            var componentType = component.GetType();
            
            // Check all registered rules for this component type
            foreach (var kvp in _allRules)
            {
                if (kvp.Key.IsAssignableFrom(componentType))
                {
                    foreach (var rule in kvp.Value)
                    {
                        // Only run ComponentAddedRule instances
                        var ruleType = rule.GetType();
                        if (IsComponentAddedRule(ruleType))
                        {
                            InvokeValidate(rule, component);
                        }
                    }
                }
            }
        }
        
        private static void OnComponentRemoved(GameObject gameObject, Type removedComponentType)
        {
            if (gameObject == null) return;
            
            // Check if component validation is enabled
            var controller = UnityEngine.Object.FindFirstObjectByType<realvirtualController>();
            if (controller != null && !controller.ValidationOnComponentsAdded)
                return;
            
            // When a component is removed, validate all remaining components
            // as they might have dependencies on the removed component
            var remainingComponents = gameObject.GetComponents<Component>();
            foreach (var component in remainingComponents)
            {
                if (component == null) continue;
                
                var componentType = component.GetType();
                
                // Run all validation rules (not just ComponentAddedRule)
                foreach (var kvp in _allRules)
                {
                    if (kvp.Key.IsAssignableFrom(componentType))
                    {
                        foreach (var rule in kvp.Value)
                        {
                            // Run all rules except PrePlayRule
                            var ruleType = rule.GetType();
                            if (!IsPrePlayRule(ruleType))
                            {
                                InvokeValidate(rule, component);
                            }
                        }
                    }
                }
            }
        }
        
        //! Validates all components in the scene, running PrePlayRule validations
        public static void ValidateAllComponents()
        {
            // Note: Validation enabled check is already done in PlayModeValidation
            // This method is called after that check passes
            
            if (!_initialized)
            {
                Initialize();
            }
                
            var allComponents = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsSortMode.None);
            int behaviorInterfaceCount = 0;
            
            foreach (var component in allComponents)
            {
                if (component == null) continue;
                
                var componentType = component.GetType();
                
                // Count BehaviorInterfaces for debugging
                if (component is BehaviorInterface)
                    behaviorInterfaceCount++;
                
                // Check all registered rules for this component type
                foreach (var kvp in _allRules)
                {
                    if (kvp.Key.IsAssignableFrom(componentType))
                    {
                        foreach (var rule in kvp.Value)
                        {
                            // Only run PrePlayRule instances
                            var ruleType = rule.GetType();
                            if (IsPrePlayRule(ruleType))
                            {
                                InvokeValidate(rule, component);
                            }
                        }
                    }
                }
            }
        }
        
        //! Checks if a rule type inherits from ComponentAddedRule<T>
        private static bool IsComponentAddedRule(Type ruleType)
        {
            var current = ruleType.BaseType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ComponentAddedRule<>))
                    return true;
                current = current.BaseType;
            }
            return false;
        }
        
        //! Checks if a rule type inherits from PrePlayRule<T>
        private static bool IsPrePlayRule(Type ruleType)
        {
            var current = ruleType.BaseType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(PrePlayRule<>))
                    return true;
                current = current.BaseType;
            }
            return false;
        }
        
        //! Invokes the Validate method on a rule instance
        private static void InvokeValidate(object rule, Component component)
        {
            try
            {
                var validateMethod = rule.GetType().GetMethod("Validate");
                validateMethod?.Invoke(rule, new object[] { component });
            }
            catch (Exception e)
            {
                Logger.Error($"[ComponentValidation] Failed to run rule: {e.Message}", component, false);
            }
        }
    }
}
#endif