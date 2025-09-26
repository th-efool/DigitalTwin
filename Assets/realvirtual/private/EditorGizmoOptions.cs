using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    [Serializable]
    public class meshGizmo
    {
        public List<MeshFilter> meshFilterList=new List<MeshFilter>();
        public float pivotSize;
        public bool DrawMeshPivot;
        public bool DrawMeshCenter;
        public Color MeshColor;
        public GameObject mainGO;

    }
    [CreateAssetMenu(fileName = "EditorGizmoOptions", menuName = "realvirtual/Add EditorGizmoOptions", order = 1)]
    public class EditorGizmoOptions : ScriptableObject
    {
        [Header("Move Pivot Settings")]
        public Color HoverMeshColor;
        public Color FirstSelectedMeshColor;
        public Color SecondSelectedMeshColor;
        public Color DefaultColorSelectionSphere;
        
        [Header("Kinematic Tool Settings")]
        [Header("Dialog setting")]
        [Tooltip("Button background active Button")]public Color ActiveButtonBackground;
        [Header("Scene setting")]
        [Tooltip("Color currently hovered mesh.")]public Color KT_HoverMeshColor;
        [Tooltip("Color selected mesh.")]public Color KT_SelectedMeshColor;
        [Tooltip("Color of axis direction line.")]public Color AxisColor;
        [Tooltip("Color of axis direction line of sub objects.")]public Color AxisColorSecondaryAxis;
        [Tooltip("Mesh color of the connected axis.")]public Color MeshColorConnectedAxis;
        [Tooltip("Mesh color of the upper axis.")]public Color MeshColorUpperAxis;

        [HideInInspector] public List<meshGizmo> SelectedMeshes = new List<meshGizmo>();
       
    }
}
