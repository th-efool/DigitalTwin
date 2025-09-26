// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace realvirtual
{
    [ExecuteAlways]
    #region doc
    //! Kinematic component provides runtime hierarchy manipulation and pivot point adjustment for imported CAD models and assemblies.
    
    //! The Kinematic component is a powerful tool for adapting imported 3D models to work with realvirtual's
    //! automation components. It allows you to modify object hierarchies, reposition pivot points, and reorganize
    //! component relationships without changing the original CAD data. This is essential when working with
    //! imported models that have incorrect pivot points, wrong parent-child relationships, or need to be
    //! dynamically grouped for animation purposes.
    //!
    //! Key features:
    //! - Reposition objects to new locations while maintaining child relationships
    //! - Move pivot points without affecting visual geometry position
    //! - Integrate multiple objects from Groups into a single kinematic assembly
    //! - Change parent-child relationships at runtime for kinematic chains
    //! - Preview changes in Editor mode before applying them
    //! - Support for complex hierarchy simplification
    //! - Visual gizmos for grouped objects in Scene view
    //! - Maintains proper scaling and rotation during transformations
    //!
    //! Common applications in industrial automation:
    //! - Correcting pivot points for imported robot arms and mechanisms
    //! - Grouping conveyor sections for synchronized movement
    //! - Creating kinematic chains for multi-axis machines
    //! - Adapting CAD assemblies for use with Drive components
    //! - Reorganizing imported factory layouts for simulation
    //! - Fixing rotation centers for doors, gates, and rotating equipment
    //! - Combining multiple parts into single controllable units
    //! - Setting up parent-child relationships for tooling and fixtures
    //!
    //! The Kinematic component solves common CAD import issues:
    //! - Wrong pivot points from CAD systems using different conventions
    //! - Flat hierarchies that need proper parent-child structure
    //! - Components that need to move together but are separate in CAD
    //! - Rotation axes that don't align with mechanical joints
    //! - Objects that need different parents during different operations
    //!
    //! Repositioning capabilities:
    //! - Align objects to reference GameObjects or empties
    //! - Apply additional rotation offsets for fine-tuning
    //! - Update positions in Editor mode for immediate feedback
    //! - Maintain child object positions during parent movement
    //!
    //! Pivot adjustment features:
    //! - Move pivot point by delta position in millimeters
    //! - Rotate pivot orientation without affecting geometry
    //! - Keep all child objects in their world positions
    //! - Essential for drives that rotate around specific axes
    //!
    //! Group integration system:
    //! - Collect all objects with matching Group components
    //! - Support for prefab-based group name prefixes
    //! - Optional hierarchy simplification for performance
    //! - Visual preview with wireframe gizmos
    //! - Focus commands for easy navigation to grouped objects
    //!
    //! Integration with other components:
    //! - Essential preparation for Drive components
    //! - Works with Group components for object collection
    //! - Compatible with all movement and physics components
    //! - Supports TransportSurface collision detection
    //! - Maintains proper transform relationships for sensors
    //!
    //! Performance considerations:
    //! - Hierarchy changes happen once at Start, minimal runtime overhead
    //! - Simplified hierarchies reduce transform calculation costs
    //! - Group integration can optimize draw calls for renderers
    //! - Proper pivot points improve animation performance
    //!
    //! The Kinematic component bridges the gap between CAD design and simulation requirements,
    //! making it possible to use industrial CAD models directly in virtual commissioning
    //! without time-consuming manual corrections.
    //!
    //! For detailed documentation and examples, see:
    //! https://doc.realvirtual.io/components-and-scripts/motion/kinematic
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/kinematic")]
    public class Kinematic : realvirtualBehavior
    {
        [Tooltip("Enables repositioning of this object and its children")]
        [BoxGroup("Reposition (including children)")] [Label("Enable")] [OnValueChanged("UpdateValues")]
        public bool RepositionEnable = false; //!<  Enables Repositioning

        [Tooltip("Updates position in Editor mode for preview")]
        [BoxGroup("Reposition (including children)")] [ShowIf("RepositionEnable")] [OnValueChanged("UpdateValues")]
        public bool UpdateInEditorMode; //!<  Also Update in Editor Mode

        [Tooltip("Target GameObject to align this object's position and rotation with")]
        [BoxGroup("Reposition (including children)")] [ShowIf("RepositionEnable")] [OnValueChanged("UpdateValues")]
        public GameObject MoveTo; //!<  Reposition and Move the Pivot of this object to the defined Pivot

        [Tooltip("Additional rotation applied after repositioning in degrees")]
        [BoxGroup("Reposition (including children)")] [ShowIf("RepositionEnable")] [OnValueChanged("UpdateValues")]
        public Vector3 AdditionalRotation; //!<  Gives an additional rotation when repositioning

        [Tooltip("Moves the pivot point without affecting child positions")]
        [BoxGroup("Move Center (keep children)")] [Label("Enable")]
        public bool MoveCenterEnable = false; //!<  Enables to move the Pivot Point without moving the part itself

        [Tooltip("Position offset for the pivot point in mm (x, y, z)")]
        [BoxGroup("Move Center (keep children)")] [ShowIf("MoveCenterEnable")]
        public Vector3 DeltaPosOrigin; //!<  Vector to move the Pivot Point in x,y,z

        [Tooltip("Rotation offset for the pivot point in degrees")]
        [BoxGroup("Move Center (keep children)")] [ShowIf("MoveCenterEnable")]
        public Vector3 DeltaRotOrigin; //!<  Rotation to move the Pivot Point

        [Tooltip("Integrates all objects from a Group as children of this GameObject")]
        [BoxGroup("Integrate Group")] [Label("Enable")] 
        public bool IntegrateGroupEnable = false; //!<  Integrate a Group as children of this component

        [Tooltip("Name of the Group to integrate as children")]
        [BoxGroup("Integrate Group")] [ShowIf("IntegrateGroupEnable")]
        [Dropdown("GetGroupNames")]
        public string GroupName = ""; //!<  The name of the group to integrate

        [Tooltip("Optional GameObject whose name is used as prefix for the Group name (for prefabs)")]
        [BoxGroup("Integrate Group")] [ShowIf("IntegrateGroupEnable")]
        public GameObject
            GroupNamePrefix; //!<  Optional reference to a Part which name is defining a Prefix for the Groupname. Needs to be used with Prefabs which are using Group function

        [Tooltip("Simplifies hierarchy by integrating only mesh objects from the Group")]
        [BoxGroup("Integrate Group")] [ShowIf("IntegrateGroupEnable")]
        public Boolean SimplifyHierarchy; //!< Simplify the Hierarchy for the integrated parts

        [Tooltip("Shows wireframe visualization of Group objects in Scene view")]
        [BoxGroup("Integrate Group")] [ShowIf("IntegrateGroupEnable")]
        public Boolean ShowGroupGizmo = true; //!< Shows Gizomo for the Group parts
        [Tooltip("Moves this GameObject to a new parent at simulation start")]
        [BoxGroup("New Kinematic Parent")] [Label("Enable")]
        public bool
            KinematicParentEnable =
                false; //!< Defines a new kinematic parent for this component (moves it and all children during simulation start to a new parent)

        [Tooltip("The GameObject that will become the new parent")]
        [BoxGroup("New Kinematic Parent")] [ShowIf("KinematicParentEnable")]
        public GameObject Parent; //!< The new kinematic parent

        // The information text in the hierarchy view
        public string GetVisuText()
        {
            var groupname = GroupName;
            if (GroupNamePrefix != null)
                groupname = GroupNamePrefix.name + GroupName;
            var text = "";
            if (IntegrateGroupEnable)
                text = text + "<" + groupname;

            if (Parent == null)
                return text;

            if (KinematicParentEnable)
                if (text != "")
                    text = text + " ";
                else
                    text = text + "^" + Parent.name;

            return text;
        }

        public void UpdateValues()
        {
            if (UpdateInEditorMode)
                MoveAndRotate(true);
        }

        
        public void MoveAndRotate()
        {
            MoveAndRotate(true);
        }

        public void DisplayGroupMeshes(Color color, bool forcecolor = false)
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject != gameObject && !forcecolor)
            {
                if (Global.g4acontrollernotnull)
                    Gizmos.color = Global.realvirtualcontroller.GetGizmoOptions().AxisColor;
                else
                {
                    Gizmos.color = Color.yellow;
                }
            }
              
            var groupname = GroupName;
            if (GroupNamePrefix != null)
                groupname = GroupNamePrefix.name + GroupName;
            var objs = GetAllMeshesWithGroup(groupname);
            
            // Make the gizmos lighter and more subtle by reducing color intensity
            Color lightColor = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, color.a);
            Gizmos.color = lightColor;
            
            foreach (var obj in objs)
            {
                var meshes = obj.GetComponentsInChildren<MeshFilter>();
                foreach (var mesh in meshes)
                {
                    if(!Global.realvirtualcontroller.CheckIfMeshIsHovered(mesh.gameObject))
                        Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.transform.position, mesh.transform.rotation,
                        obj.transform.lossyScale);
                }
            }
#endif
        }

        public string GetGroupName()
        {
            var groupname = GroupName;
            if (GroupNamePrefix != null)
                groupname = GroupNamePrefix.name + GroupName;
            return groupname;
        }

        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (IntegrateGroupEnable && ShowGroupGizmo)
            {
                Color color;
                if (Global.g4acontrollernotnull)
                    if(Selection.activeGameObject== gameObject)
                        color = Global.realvirtualcontroller.GetGizmoOptions().KT_SelectedMeshColor;
                    else
                        color = Global.realvirtualcontroller.GetGizmoOptions().MeshColorConnectedAxis;
                else
                {
                    // Use a much darker, more subtle magenta color
                    color = new Color(0.3f, 0.06f, 0.3f, 1f);
                }
                DisplayGroupMeshes(color,true);
            }
#endif
        }
        private void MoveAndRotate(bool silent)
        {
            if (MoveTo != null)
            {
                var newrot = MoveTo.transform.rotation * Quaternion.Euler(AdditionalRotation);
                if ((transform.position != MoveTo.transform.position) || (transform.rotation != newrot))
                {
                    bool ok = true;
                    if (!silent)
                    {
#if UNITY_EDITOR
                        ok = EditorUtility.DisplayDialog("Warning",
                            "Repositioning Object " + name + "because " + MoveTo.name + " changed position", "OK",
                            "CANCEL");
#endif
                    }

                    if (ok)
                    {
                        transform.position = MoveTo.transform.position;
                        transform.rotation = MoveTo.transform.rotation * Quaternion.Euler(AdditionalRotation);
                    }
                }
            }
        }
        
#if UNITY_EDITOR
        private List<string> GetGroupNames()
        {
            var Groups = UnityEngine.Resources.FindObjectsOfTypeAll<Group>()
                .Where(group => !EditorUtility.IsPersistent(group.transform.root.gameObject))
                .Select(group => group.GroupName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();
            if (Groups == null)
                Groups = new List<string>();
            if (!Groups.Contains(GroupName))
            {
                Groups.Insert(0,GroupName);
            }
            return Groups;
        }
#endif

        [Button("Focus Group")]
        private void Focus()
        {
#if UNITY_EDITOR
            var groupname = GroupName;
            if (GroupNamePrefix != null)
                groupname = GroupNamePrefix.name + GroupName;
            var objs = GetAllMeshesWithGroup(groupname);
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var obj in objs)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            
            }
            SceneView.lastActiveSceneView.Frame(bounds, true);
#endif
        }

        new void Awake()
        {
            base.Awake();
            if (Application.IsPlaying(gameObject))
            {
                List<GameObject> objs;
                if (IntegrateGroupEnable)
                {
                    var groupname = GroupName;
                    if (GroupNamePrefix != null)
                        groupname = GroupNamePrefix.name + GroupName;
                    if (!SimplifyHierarchy)
                        objs = GetAllWithGroup(groupname);
                    else
                        objs = GetAllMeshesWithGroup(groupname);

                    foreach (var obj in objs)
                    {
                        obj.transform.parent = transform;
                    }
                }

                if (KinematicParentEnable)
                {
                    gameObject.transform.parent = Parent.transform;
                }

                if (RepositionEnable)
                {
                    if (MoveTo != null)
                    {
                        MoveAndRotate(true);
                    }
                }

                if (MoveCenterEnable)
                {
                    var deltapos = new Vector3(DeltaPosOrigin.x / realvirtualController.Scale / transform.lossyScale.x,
                        DeltaPosOrigin.y / realvirtualController.Scale / transform.lossyScale.y,
                        DeltaPosOrigin.z / realvirtualController.Scale  / transform.lossyScale.z);

                    Global.MovePositionKeepChildren(gameObject, deltapos);
                    Global.MoveRotationKeepChildren(gameObject, Quaternion.Euler(DeltaRotOrigin));
                }
            }
            else
            {
                if (UpdateInEditorMode)
                    MoveAndRotate(false);
            }
        }
    }
}