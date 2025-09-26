/// <summary>
/// SURGE FRAMEWORK
/// Author: Bob Berkebile
/// Email: bobb@pixelplacement.com
/// 
/// Forces pivot mode to center so an anchor's pivot is always correct while adjusting a spline.
/// 
/// </summary>

#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Pixelplacement
{
#pragma warning disable CS3009
    [CustomEditor(typeof(SplineAnchor))]
    public class SplineAnchorEditor : Editor
    {
        //Scene GUI:
        void OnSceneGUI ()
        {
            //ensure pivot is used so anchor selection has a proper transform origin:
            if (Tools.pivotMode == PivotMode.Center)
            {
                Tools.pivotMode = PivotMode.Pivot;
            }
        }

        //Gizmos:
        [DrawGizmo(GizmoType.Selected)]
        static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType)
        {
            if (objectTransform.parent != null)
            {
                SplineEditor.RenderCustomGizmo(objectTransform.parent, gizmoType);
            }
        }
    }
#pragma warning restore CS3009
}
#endif