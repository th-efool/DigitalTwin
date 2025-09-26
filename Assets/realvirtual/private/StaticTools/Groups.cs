// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace realvirtual
{
    //! Static utility class providing centralized functionality for working with Groups in realvirtual.
    //! Offers methods for querying, bounds calculation, collider management, and mesh operations on grouped objects.
    public static class Groups
    {
        #region Group Query Methods
        
        //! Gets all unique group names in the scene.
        public static List<string> GetAllGroupNames()
        {
            var groupSet = new HashSet<string>();
            var groupComponents = Object.FindObjectsByType<Group>(FindObjectsSortMode.None);
            
            foreach (var group in groupComponents)
            {
                if (group != null)
                {
                    var groupName = group.GetGroupName();
                    if (!string.IsNullOrEmpty(groupName))
                        groupSet.Add(groupName);
                }
            }
            
            return groupSet.ToList();
        }
        
        //! Gets all GameObjects that have a Group component with the specified group name.
        public static List<GameObject> GetGameObjectsWithGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return new List<GameObject>();
                
            var resultSet = new HashSet<GameObject>();
            var allGroups = Object.FindObjectsByType<Group>(FindObjectsSortMode.None);
            
            foreach (var group in allGroups)
            {
                if (group != null && group.GetGroupName() == groupName)
                    resultSet.Add(group.gameObject);
            }
            
            return resultSet.ToList();
        }
        
        //! Gets all GameObjects with the specified group name including all their child objects.
        public static List<GameObject> GetGameObjectsWithGroupIncludingChildren(string groupName)
        {
            var groupObjects = GetGameObjectsWithGroup(groupName);
            if (groupObjects.Count == 0)
                return groupObjects;
                
            var resultSet = new HashSet<GameObject>();
            
            foreach (var obj in groupObjects)
            {
                resultSet.Add(obj);
                
                // More efficient than GetComponentsInChildren<Transform>
                var transforms = obj.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                {
                    resultSet.Add(transform.gameObject);
                }
            }
            
            return resultSet.ToList();
        }
        
        //! Gets GameObjects that belong to ALL specified groups.
        public static List<GameObject> GetGameObjectsWithAllGroups(List<string> groupNames)
        {
            if (groupNames == null || groupNames.Count == 0)
                return new List<GameObject>();
                
            var result = GetGameObjectsWithGroup(groupNames[0]);
            
            for (int i = 1; i < groupNames.Count; i++)
            {
                var groupObjects = GetGameObjectsWithGroup(groupNames[i]);
                result = result.Intersect(groupObjects).ToList();
            }
            
            return result;
        }
        
        //! Gets GameObjects with the specified group that also have MeshFilter components.
        public static List<GameObject> GetMeshesWithGroup(string groupName)
        {
            var groupObjects = GetGameObjectsWithGroup(groupName);
            if (groupObjects.Count == 0)
                return groupObjects;
                
            // Use LINQ for more concise filtering
            return groupObjects.Where(obj => obj.GetComponent<MeshFilter>() != null).ToList();
        }
        
        //! Gets GameObjects with all specified groups that also have MeshFilter components.
        public static List<GameObject> GetMeshesWithAllGroups(List<string> groupNames)
        {
            var groupObjects = GetGameObjectsWithAllGroups(groupNames);
            if (groupObjects.Count == 0)
                return groupObjects;
                
            // Use LINQ for more concise filtering
            return groupObjects.Where(obj => obj.GetComponent<MeshFilter>() != null).ToList();
        }
        
        #endregion
        
        #region Bounds Calculation Methods
        
        //! Calculates the combined bounds of all renderers in objects belonging to the specified group.
        public static Bounds GetGroupBounds(string groupName)
        {
            var groupObjects = GetGameObjectsWithGroupIncludingChildren(groupName);
            return GetGroupBounds(groupObjects);
        }
        
        //! Calculates the combined bounds of all renderers in the provided list of GameObjects.
        public static Bounds GetGroupBounds(List<GameObject> groupObjects)
        {
            if (groupObjects == null || groupObjects.Count == 0)
                return new Bounds(Vector3.zero, Vector3.one);
                
            Bounds? combinedBounds = null;
            
            // Collect all renderers first to avoid repeated GetComponentsInChildren calls
            var allRenderers = new List<Renderer>();
            foreach (var obj in groupObjects)
            {
                if (obj != null)
                    allRenderers.AddRange(obj.GetComponentsInChildren<Renderer>(true));
            }
            
            // Calculate bounds
            foreach (var renderer in allRenderers)
            {
                if (renderer != null)
                {
                    if (!combinedBounds.HasValue)
                        combinedBounds = renderer.bounds;
                    else
                    {
                        var bounds = combinedBounds.Value;
                        bounds.Encapsulate(renderer.bounds);
                        combinedBounds = bounds;
                    }
                }
            }
            
            return combinedBounds ?? new Bounds(Vector3.zero, Vector3.one);
        }
        
        //! Gets all renderers from objects in the specified group.
        public static List<Renderer> GetRenderersFromGroup(string groupName)
        {
            var groupObjects = GetGameObjectsWithGroupIncludingChildren(groupName);
            return GetRenderersFromGroup(groupObjects);
        }
        
        //! Gets all renderers from the provided list of GameObjects.
        public static List<Renderer> GetRenderersFromGroup(List<GameObject> groupObjects)
        {
            if (groupObjects == null || groupObjects.Count == 0)
                return new List<Renderer>();
                
            var renderers = new List<Renderer>();
            
            foreach (var obj in groupObjects)
            {
                if (obj != null)
                    renderers.AddRange(obj.GetComponentsInChildren<Renderer>(true));
            }
            
            return renderers;
        }
        
        #endregion
        
        #region Collider Methods
        
        //! Finds the first collider in objects belonging to the specified group.
        public static Collider GetFirstColliderInGroup(string groupName)
        {
            var groupObjects = GetGameObjectsWithGroupIncludingChildren(groupName);
            return GetFirstColliderInGroup(groupObjects);
        }
        
        //! Finds the first collider in the provided list of GameObjects.
        public static Collider GetFirstColliderInGroup(List<GameObject> groupObjects)
        {
            if (groupObjects == null || groupObjects.Count == 0)
                return null;
                
            foreach (var obj in groupObjects)
            {
                if (obj != null)
                {
                    var collider = obj.GetComponent<Collider>();
                    if (collider != null)
                        return collider;
                }
            }
            
            return null;
        }
        
        //! Gets all colliders from objects in the specified group.
        public static List<Collider> GetAllCollidersInGroup(string groupName)
        {
            var groupObjects = GetGameObjectsWithGroupIncludingChildren(groupName);
            return GetAllCollidersInGroup(groupObjects);
        }
        
        //! Gets all colliders from the provided list of GameObjects.
        public static List<Collider> GetAllCollidersInGroup(List<GameObject> groupObjects)
        {
            if (groupObjects == null || groupObjects.Count == 0)
                return new List<Collider>();
                
            var colliders = new List<Collider>();
            
            foreach (var obj in groupObjects)
            {
                if (obj != null)
                    colliders.AddRange(obj.GetComponentsInChildren<Collider>(true));
            }
            
            return colliders;
        }
        
        //! Creates a MeshCollider on the target GameObject that encompasses all meshes in the specified group.
        public static MeshCollider CreateMeshColliderForGroup(GameObject target, string groupName)
        {
            var groupObjects = GetGameObjectsWithGroupIncludingChildren(groupName);
            return CreateMeshColliderForGroup(target, groupObjects);
        }
        
        //! Creates a MeshCollider on the target GameObject that encompasses all meshes in the provided GameObjects.
        public static MeshCollider CreateMeshColliderForGroup(GameObject target, List<GameObject> groupObjects)
        {
            if (target == null || groupObjects == null || groupObjects.Count == 0)
                return null;
                
            var meshCollider = target.GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = target.AddComponent<MeshCollider>();
                
            var combinedMesh = CombineGroupMeshes(groupObjects, target.transform);
            if (combinedMesh != null)
            {
                meshCollider.sharedMesh = combinedMesh;
                return meshCollider;
            }
            
            return null;
        }
        
        //! Combines all meshes from the specified group objects into a single mesh.
        public static Mesh CombineGroupMeshes(List<GameObject> groupObjects, Transform targetTransform = null)
        {
            if (groupObjects == null || groupObjects.Count == 0)
                return null;
                
            // Pre-calculate capacity for better memory allocation
            var meshFilters = new List<MeshFilter>(groupObjects.Count * 2);
            
            foreach (var obj in groupObjects)
            {
                if (obj != null)
                    meshFilters.AddRange(obj.GetComponentsInChildren<MeshFilter>(true));
            }
            
            // Count valid meshes first
            int validMeshCount = 0;
            foreach (var filter in meshFilters)
            {
                if (filter != null && filter.sharedMesh != null)
                    validMeshCount++;
            }
            
            if (validMeshCount == 0)
                return null;
                
            // Save target transform state if provided
            Vector3 originalPos = Vector3.zero;
            Quaternion originalRot = Quaternion.identity;
            if (targetTransform != null)
            {
                originalPos = targetTransform.position;
                originalRot = targetTransform.rotation;
                targetTransform.position = Vector3.zero;
                targetTransform.rotation = Quaternion.identity;
            }
            
            // Combine only valid meshes
            var combine = new CombineInstance[validMeshCount];
            int index = 0;
            
            foreach (var filter in meshFilters)
            {
                if (filter != null && filter.sharedMesh != null)
                {
                    combine[index].mesh = filter.sharedMesh;
                    combine[index].transform = filter.transform.localToWorldMatrix;
                    index++;
                }
            }
            
            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine, true, true);
            
            // Restore target transform
            if (targetTransform != null)
            {
                targetTransform.position = originalPos;
                targetTransform.rotation = originalRot;
            }
            
            return combinedMesh;
        }
        
        #endregion
        
        #region Utility Methods
        
        //! Gets all group names from a specific GameObject.
        public static List<string> GetGroupNamesFromGameObject(GameObject obj)
        {
            if (obj == null)
                return new List<string>();
                
            var groups = obj.GetComponents<Group>();
            var groupNames = new List<string>();
            
            foreach (var group in groups)
            {
                var name = group.GetGroupName();
                if (!string.IsNullOrEmpty(name))
                    groupNames.Add(name);
            }
            
            return groupNames;
        }
        
        //! Checks if a GameObject belongs to the specified group.
        public static bool HasGroup(GameObject obj, string groupName)
        {
            if (obj == null || string.IsNullOrEmpty(groupName))
                return false;
                
            var groups = obj.GetComponents<Group>();
            // Use LINQ Any for early exit
            return groups.Any(group => group != null && group.GetGroupName() == groupName);
        }
        
        //! Checks if a GameObject belongs to ALL specified groups.
        public static bool HasAllGroups(GameObject obj, List<string> groupNames)
        {
            if (obj == null || groupNames == null || groupNames.Count == 0)
                return false;
                
            var objGroupNames = new HashSet<string>(GetGroupNamesFromGameObject(obj));
            
            // Use LINQ All for more concise check
            return groupNames.All(requiredGroup => objGroupNames.Contains(requiredGroup));
        }
        
        #endregion
    }
}