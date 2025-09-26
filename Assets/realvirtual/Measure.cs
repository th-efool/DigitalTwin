// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;



namespace realvirtual
{
    [ExecuteInEditMode]
    //! Measure component for calculating and monitoring distances between GameObjects in real-time.
    //! Provides continuous distance measurement between pivot points with visual feedback in the Scene view.
    //! Supports both absolute and vector distance output, automatic position maintenance, and PLC signal integration for position feedback control.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/measure")]
    public class Measure : BehaviorInterface 
    {
        [Tooltip("Source GameObject to measure distance from. Distance is calculated between pivot points")]
        public GameObject MeasureFrom; //!< The object from which the Distance between Pivot Points should be measured to this objects Pivot Point.

        [ReadOnly] public Vector3 Distance; //!< The measured distance (read only) as Vector in World coordinates
        [ReadOnly] public float DistanceAbs; //!< The measured distance (read only) as absolute value in World coordinates

        [Tooltip("Target distance vector to apply when 'Set Distance' button is clicked")]
        public Vector3 SetDistance; //!< The distance that should be set when button Set Distance is pushed
        
        [Tooltip("When enabled, maintains the object at the specified SetDistance from MeasureFrom")]
        [OnValueChanged("CurrentToSet")]public bool KeepSetDistance; //!< True if the distance should be always kept as the value in SetDistance
        
        [Foldout("Options")] 
        [Tooltip("Shows measurement visualization in Scene view when this object is selected")]
        public bool DisplayOnSelected=true;  //!< Display measurement in Scene window when selected
        
        [Foldout("Options")] 
        [Tooltip("Always shows measurement visualization in Scene view")]
        public bool DisplayAlways=true;  //!< Display measurement in Scene window always
        
        [Foldout("Options")] 
        [Tooltip("Draws a line between the measurement points in Scene view")]
        public bool DisplayLine=true;  //!< Display a line between points
        
        [Foldout("Options")] 
        [Tooltip("Shows the absolute distance value in Scene view")]
        public bool DisplayAbs=true;  //!< Display the absolute value in Scene window
        
        [Foldout("Options")] 
        [Tooltip("Shows the distance vector (X,Y,Z) in world coordinates in Scene view")]
        public bool DisplayVector=true;  //!< Display the vector in World coordinates in Scene window
        
        [Foldout("Options")] 
        [Tooltip("Use millimeters for display and PLC signals (1 Unity unit = 1000mm)")]
        public bool UseMillimeters=true;  //!< Use Millimeters on display and PLCSignal (1 Unity scale = 1000mm)
        
        [Foldout("Options")] 
        [Tooltip("Offset value added to the PLC signal output")]
        public float PLCSignalOffset=0;  //!< Offset to the PLCSignal

        [Foldout("PLC IOs")] 
        [Tooltip("PLC input signal for the absolute measured distance")]
        public PLCInputFloat MeasuredDistance; //!< The absolute distance as PLCInputFloat
        
        [Foldout("PLC IOs")] 
        [Tooltip("PLC input signal for the X component of the distance in world coordinates")]
        public PLCInputFloat MeasuredDistanceX; //!< The distance in World X coordinates as PLCInputFloat
        
        [Foldout("PLC IOs")] 
        [Tooltip("PLC input signal for the Y component of the distance in world coordinates")]
        public PLCInputFloat MeasuredDistanceY; //!< The distance in World Y coordinates as PLCInputFloat
        
        [Foldout("PLC IOs")] 
        [Tooltip("PLC input signal for the Z component of the distance in world coordinates")]
        public PLCInputFloat MeasuredDistanceZ; //!< The distance in World Z coordinates as PLCInputFloat
        
        private bool measureddistancenotnull = false;
        private bool measureddistancexnotnull = false;
        private bool measureddistanceynotnull = false;
        private bool measureddistanceznotnull = false;
        
        [Button("Current to Set Distance")]
        void CurrentToSet()
        {
            SetDistance = this.transform.position-MeasureFrom.transform.position;
            if (UseMillimeters)
                SetDistance = SetDistance * 1000;
        }

        [Button("Set Distance")]
        void SetDistanceTo()
        {
            if (!UseMillimeters)
                this.transform.position= MeasureFrom.transform.position + SetDistance;
            else
                this.transform.position= MeasureFrom.transform.position + SetDistance/1000;
            
        }
        
        
        public void DrawString(string text, Vector3 worldPos, Color? textColor = null, Color? backColor = null)
        {
         #if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();
            var restoreTextColor = GUI.color;
            var restoreBackColor = GUI.backgroundColor;
            GUI.color = textColor ?? Color.white;
            GUI.backgroundColor = backColor ?? Color.black;
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

        void Draw()
        {
   #if UNITY_EDITOR
            Gizmos.color = Color.yellow;
            if (DisplayLine)
                 Gizmos.DrawLine(transform.position, MeasureFrom.transform.position);
    
            var display = "";
            var posmedium =  transform.position - (transform.position- MeasureFrom.transform.position) / 2;
            if (!DisplayLine)
                posmedium = transform.position;
            

            if (DisplayVector)
                 display = Distance.ToString();
            if (DisplayAbs && DisplayVector)
                display = display + "\r\n";
            if (DisplayAbs)
                display = display + DistanceAbs;
            if (DisplayAbs || DisplayVector)
                DrawString(display,posmedium,Color.yellow,Color.white);
            #endif
        }
        
        void OnDrawGizmos()
        {
            if (MeasureFrom != null && DisplayAlways )
            {
                Draw();
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (MeasureFrom != null && DisplayOnSelected && !DisplayAlways)
            {
                Draw();
            }
        }

        private void Start()
        {
            measureddistancenotnull = MeasuredDistance != null;
            measureddistancexnotnull = MeasuredDistanceX != null;
            measureddistanceynotnull = MeasuredDistanceY != null;
            measureddistanceznotnull = MeasuredDistanceZ != null;
        }
        
        private void FixedUpdate()
        {
            if (measureddistancenotnull)
                MeasuredDistance.Value = DistanceAbs + PLCSignalOffset;
            if (measureddistancexnotnull)
                MeasuredDistanceX.Value = Distance.x;
            if (measureddistanceynotnull)
                MeasuredDistanceY.Value = Distance.y;
            if (measureddistanceznotnull)
                MeasuredDistanceZ.Value = Distance.z;
        }

        void Update()
        {
            if (MeasureFrom == null)
                return;
            if (KeepSetDistance)
            {
                SetDistanceTo();
            }

            Distance = this.transform.position-MeasureFrom.transform.position;
        
            DistanceAbs = Vector3.Distance(this.transform.position, MeasureFrom.transform.position);
            if (UseMillimeters)
            {
                Distance = Distance * 1000;
                DistanceAbs = DistanceAbs * 1000;
            }
        }
        
    }
}