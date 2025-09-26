// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;



namespace realvirtual
{
    [ExecuteInEditMode]
    //! MeasureRaycast performs distance measurements using Unity's raycast system for sensor simulation.
    //! Provides real-time distance measurement to colliders on specified layers with visual feedback.
    //! Ideal for simulating laser distance sensors, ultrasonic sensors, or collision detection systems with PLC integration.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/measureraycast")]
    public class MeasureRaycast : BehaviorInterface 
    {
        [Foldout("Settings")]  
        [Tooltip("Direction of the raycast in local space")]
        public Vector3 RaycastDirection = new Vector3(1,0,0);
        
        [Foldout("Settings")] 
        [Tooltip("When enabled, measures the distance between two colliders on opposite sides")]
        public bool MeasureBetweenColliders = false;
        
        [Foldout("Settings")]  
        [Tooltip("Maximum raycast distance in Unity units (or mm if UseMillimeters is enabled)")]
        public float RaycastLength = 1000;
        
        [Foldout("Settings")]  
        [Tooltip("Shows the raycast line in Scene view (red when hitting, yellow when not)")]
        public bool DisplayRaycast = true;
        
        [Foldout("Settings")]  
        [Tooltip("Layer name to detect collisions with")]
        public string RayCastToLayer = "Default";
        
        [Foldout("Settings")]  
        [Tooltip("Use millimeters for measurements (1 Unity unit = 1000mm)")]
        public bool UseMillimeters = true;
        
        [Foldout("Settings")]  
        [Tooltip("Offset value added to the PLC signal output")]
        public float PLCSignalOffset=0;  
        
        [Foldout("Settings")]  
        [Tooltip("Shows the measured distance value in Scene view")]
        public bool DisplayDistance=true;  
        
        [ReadOnly] public float DistanceAbs; //!< The measured distance (read only) as absolute value
        [ReadOnly] public bool Hit;   //!< true if ray is colliding with an collider
        
        [Foldout("PLC IOs")] 
        [Tooltip("PLC input signal for the measured distance")]
        public PLCInputFloat MeasuredDistance; //!< The absolute distance as PLCInputFloat
        
        [Foldout("PLC IOs")] 
        [Tooltip("PLC input signal indicating if the raycast hits a collider")]
        public PLCInputBool RaycastHit; //!< True if raycast is hitting an object
        
        private bool measureddistancenotnull = false;
        private bool raycasthitnotnull = false;
        private int layermask;
        private Vector3 display;
     
        private void Start()
        {
            measureddistancenotnull = MeasuredDistance != null;
            raycasthitnotnull = RaycastHit != null;

            int layer = LayerMask.NameToLayer(RayCastToLayer);
            if (layer == -1)
            {
                Debug.LogWarning("Invalid layer name: " + RayCastToLayer);
            }
            else
            {
                layermask = 1 << layer;
            }
       
        }

        private void Measure()
        {
            float scale = 1;
            RaycastHit hit;
            RaycastHit hit2;
            bool Hit2;
            float dist2;
            if (UseMillimeters)
                scale = 1000;
             var globaldir = transform.TransformDirection(RaycastDirection);
            display = Vector3.Normalize(globaldir) * RaycastLength / scale;
        
            if (Physics.Raycast(transform.position, globaldir, out hit, RaycastLength/scale, layermask))
            {
                if (DisplayRaycast) Debug.DrawRay(transform.position, Vector3.Normalize(globaldir)*hit.distance, Color.red);
                Hit = true;
                DistanceAbs = hit.distance * scale;
            }
            else
            {
                if (DisplayRaycast) Debug.DrawRay(transform.position, display, Color.yellow);
                Hit = false;
                DistanceAbs = 0;
            }

            if (MeasureBetweenColliders)
            {
                if (Physics.Raycast(transform.position, -globaldir, out hit2, RaycastLength / scale, layermask))
                {
                    if (DisplayRaycast) Debug.DrawRay(transform.position, -Vector3.Normalize(globaldir)*hit2.distance, Color.red);
                    Hit2 = true;
                    dist2 = hit2.distance * scale;
                }
                else
                {
                    if (DisplayRaycast) Debug.DrawRay(transform.position, -display, Color.yellow);
                    Hit2 = false;
                    dist2 = 0;
                }

                if (Hit && Hit2)
                {
                    Hit = true;
                    DistanceAbs = DistanceAbs + dist2;
                }
                else
                {
                    Hit = false;
                    DistanceAbs = 0;
                }
            }
            
        }
        
        public void DrawString(string text, Vector3 worldPos, Color? textColor = null, Color? backColor = null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();
            var restoreTextColor = GUI.color;
            var restoreBackColor = GUI.backgroundColor;
            GUI.color = textColor ?? Color.white;
            GUI.backgroundColor = backColor ?? Color.white;
            var style = EditorStyles.numberField;
            var restoresize = style.fontSize;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            if (view != null && view.camera != null)
            {
                Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
                {
                    GUI.color = restoreTextColor;
                    UnityEditor.Handles.EndGUI();
                    return;
                }
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));          
                var r = new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x+3, size.y);
                GUI.Box(r, text, style);
           
                GUI.color = restoreTextColor;
                GUI.backgroundColor = restoreBackColor;
              
            }
            UnityEditor.Handles.EndGUI();
#endif
        }
        
        void OnDrawGizmos()
        {
            if (DisplayDistance)
                DrawString(DistanceAbs.ToString(),transform.position,Color.yellow,Color.white);
        }
        
        private void FixedUpdate()
        {
            if (Application.isPlaying)
                Measure();
            if (measureddistancenotnull)
                MeasuredDistance.Value = DistanceAbs + PLCSignalOffset;
            if (raycasthitnotnull)
                RaycastHit.Value = Hit;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                Measure();
          
            }
        }


    }
}