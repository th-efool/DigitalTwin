// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    //! Monitors component additions and removals on GameObjects and fires events for external systems.
    //! Tracks component changes using Unity's ObjectChangeEvents API to detect when components are added or removed.
    [InitializeOnLoad]
    public static class ComponentAdditionEvents
    {
        // Track components to detect additions
        private static Dictionary<int, HashSet<System.Type>> gameObjectComponents = new Dictionary<int, HashSet<System.Type>>();
        
        // Events for external systems
        public static event Action OnSystemInitialized;
        public static event Action<GameObject, Type> OnComponentAddedEvent;
        public static event Action<GameObject, Type> OnComponentRemovedEvent;
        #pragma warning disable CS0067 // The event is never used
        public static event Action<GameObject, Type, string> OnValidationPerformed;
        #pragma warning restore CS0067
        
        static ComponentAdditionEvents()
        {
            // Initialize the validation system with default rules
            ComponentValidation.Initialize();
            
            // Subscribe to object change events
            ObjectChangeEvents.changesPublished += OnObjectChanged;
            
            // Initial scan of scene
            RefreshComponentTracking();
            
            // Component validation system initialized
            
            // Allow external code to add custom rules after initialization
            EditorApplication.delayCall += () => {
                OnSystemInitialized?.Invoke();
            };
        }
        
        private static void OnObjectChanged(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                var eventType = stream.GetEventType(i);
                
                if (eventType == ObjectChangeKind.ChangeGameObjectStructure)
                {
                    stream.GetChangeGameObjectStructureEvent(i, out var changeEvent);
                    HandleStructureChange(changeEvent.instanceId);
                }
                else if (eventType == ObjectChangeKind.CreateGameObjectHierarchy)
                {
                    stream.GetCreateGameObjectHierarchyEvent(i, out var createEvent);
                    HandleNewGameObject(createEvent.instanceId);
                }
                else if (eventType == ObjectChangeKind.DestroyGameObjectHierarchy)
                {
                    stream.GetDestroyGameObjectHierarchyEvent(i, out var destroyEvent);
                    HandleDestroyedGameObject(destroyEvent.instanceId);
                }
            }
        }
        
        //! Handles structure changes to detect component additions and removals
        private static void HandleStructureChange(int instanceId)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;
            
            // Get current components
            var currentComponents = new HashSet<System.Type>();
            var components = go.GetComponents<Component>();
            
            foreach (var comp in components)
            {
                if (comp != null)
                    currentComponents.Add(comp.GetType());
            }
            
            // Check if we have previous state
            if (gameObjectComponents.TryGetValue(instanceId, out var previousComponents))
            {
                // Find newly added components
                foreach (var compType in currentComponents.Where(c => !previousComponents.Contains(c)))
                {
                    CheckAndLogComponent(go, compType);
                }
                
                // Find removed components
                foreach (var compType in previousComponents.Where(p => !currentComponents.Contains(p)))
                {
                    OnComponentRemoved(go, compType);
                }
            }
            
            gameObjectComponents[instanceId] = currentComponents;
        }
        
        //! Handles newly created GameObjects to track their initial components
        private static void HandleNewGameObject(int instanceId)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;
            
            var components = go.GetComponents<Component>();
            var componentTypes = new HashSet<System.Type>();
            
            foreach (var comp in components)
            {
                if (comp != null)
                {
                    componentTypes.Add(comp.GetType());
                    CheckAndLogComponent(go, comp.GetType());
                }
            }
            
            gameObjectComponents[instanceId] = componentTypes;
        }
        
        //! Cleans up tracking when a GameObject is destroyed
        private static void HandleDestroyedGameObject(int instanceId)
        {
            gameObjectComponents.Remove(instanceId);
        }
        
        //! Processes component addition events
        private static void CheckAndLogComponent(GameObject go, System.Type componentType)
        {
            if (go.tag == "NoValidation") return;
            
            OnComponentAddedEvent?.Invoke(go, componentType);
        }
        
        //! Processes component removal events
        private static void OnComponentRemoved(GameObject go, System.Type componentType)
        {
            if (go.tag == "NoValidation") return;
            
            OnComponentRemovedEvent?.Invoke(go, componentType);
        }
        
        
        //! Refreshes component tracking for all GameObjects in the scene
        private static void RefreshComponentTracking()
        {
            gameObjectComponents.Clear();
            
            var allGameObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foreach (var go in allGameObjects)
            {
                var instanceId = go.GetInstanceID();
                var components = go.GetComponents<Component>();
                var componentTypes = new HashSet<System.Type>();
                
                foreach (var comp in components)
                {
                    if (comp != null)
                        componentTypes.Add(comp.GetType());
                }
                
                gameObjectComponents[instanceId] = componentTypes;
            }
        }
        
        private static string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
        
        // Public method to manually refresh tracking (useful after scene changes)
        public static void RefreshTracking()
        {
            RefreshComponentTracking();
            Debug.Log($"[KinematicChangedEvent] Component tracking refreshed. Tracking {gameObjectComponents.Count} GameObjects.");
        }
    }
}
#endif