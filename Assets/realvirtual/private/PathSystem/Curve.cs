// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2024 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using UnityEngine;
using NaughtyAttributes;
 using UnityEditor;
 using UnityEngine.Serialization;
 namespace realvirtual
 {
 // realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
 [RequireComponent(typeof(LineRenderer))]
 [ExecuteAlways]
 [HelpURL("https://doc.realvirtual.io/extensions/page-4/realvirtual.io-path-system/curve")]

    //! Curve implements curved path segments for smooth material flow transitions in automated transport systems.
    //! This component creates arc-shaped path sections with configurable radius and angle, enabling realistic
    //! cornering behavior for AGVs, conveyor transfers, and material handling equipment. Supports both clockwise
    //! and counter-clockwise curves with precise angular control, automatic endpoint snapping for seamless network
    //! integration, and smooth interpolation for natural movement patterns. Critical for designing efficient
    //! factory layouts with space-optimized routing, turntables, and curved conveyor sections in modern
    //! intralogistics and flexible manufacturing systems.
    public class Curve : SimulationPath
    {
 

     #region PublicProperties

     [Header("Curve Properties")] [OnValueChanged("DrawPath")]
    
     public bool Clockwise = true;

     [OnValueChanged("DrawPath")] public float Radius = 0.5f;//!< Radius of the curve
     [OnValueChanged("DrawPath")] public float StartAngle;//!< Start angle of the curve
     [OnValueChanged("DrawPath")] public float Degrees = 90;//!< Angle covered by the curve
     [FormerlySerializedAs("DirectionArrowActive")] [OnValueChanged("DrawPath")] public bool DirectionArrow = false;//!< Boolean which activate/ deactivate the direction arrow in the middle of the line
     public GameObject CenterDebug;
     public GameObject PosDebug;
     public GameObject TanDebug;
    // [HideInInspector] public LineRenderer Linerenderer;
       
     
     #endregion

     #region PrivateProperties

     private Vector3 center;
     private Vector3 startdir;
     private Vector3 enddir;
     
     #endregion

     #region PublicOverrides

     // Return the length of the curve
     public override float GetLength()
     {
         return Length;
     }

     // Return the direction as vector3 at a certain point (parameter normalizedposition) of the line (Linerenderer functionality)
     public override Vector3 GetDirection(float normalizedposition)
     {
         var degrees = Degrees * normalizedposition;
         var circlepos = GetGlobalCirclePos(degrees);
         var vectorfromcenter = circlepos - GetGlobalCenter();
         if (Clockwise)
             return Vector3.Normalize(Vector3.Cross(Vector3.up, vectorfromcenter));
         else
             return Vector3.Normalize(Vector3.Cross(Vector3.down, vectorfromcenter));
     }

     // Return the position as vector3 at a certain point (parameter normalizedposition) of the line (Linerenderer functionality)
     public override Vector3 GetPosition(float normalizedposition, ref BasePath currentpath)
     {

         var relativeangle = Degrees * normalizedposition;
         var circlepos = GetPathAnglePos(relativeangle);
         return transform.TransformPoint(circlepos);
     }

     // Rotation around a given pivot point with a defined angle
     public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
     {
         Vector3 dir = point - pivot; // get point direction relative to pivot
         dir = Quaternion.Euler(angle) * dir; // rotate it
         point = dir + pivot; // calculate rotated point
         return point; // return it
     }

     
     public Vector3 GetPathAnglePos(float angle)
     {
         Vector3 startvector = GetCirclePos(0);
         Vector3 circlepos = GetCirclePos(angle);
         Vector3 point = circlepos - startvector + LocalStartPos;
         if (!Clockwise)
             point = new Vector3(point.x, -point.y, point.z);
         // rotate Point by startangle
         Vector3 rotatepoint = RotatePointAroundPivot(point, StartPoint.transform.localPosition, new Vector3(0, 0, -StartAngle));
         return rotatepoint;
     }

     // Drawing the path
     public override void DrawPath()
     {
         if (blockdraw || name == "blockdraw")
             return;
         ClearConnections();
         
         // Debug.Log("Draw Path Curve");
         var segments = (int) Degrees;
         float delta = Degrees / segments;
         Linerenderer.useWorldSpace = false;
         Linerenderer.startWidth = Thickness;
         Linerenderer.endWidth = Thickness;
         var pointCount = segments + 1;
         Linerenderer.positionCount = pointCount;
         var points = new Vector3[pointCount + 1];
         float angle = 0;
         float arcLength = Degrees;


         for (int i = 0; i <= pointCount; i++)
         {

             Vector3 rotatepoint = GetPathAnglePos(angle);
             points[i] = rotatepoint;
             angle += (Degrees / segments);
         }

         Linerenderer.SetPositions(points);
         BasePath path = null;
         EndPoint.transform.position = GetPosition(1, ref path);
         StartPoint.transform.localPosition = new Vector3(0, 0, 0);
         StartPoint.transform.localRotation= Quaternion.identity;
         
         var EndSnap = GetChildByName("End");

         EndSnap.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), GetDirection(1));
         EndSnap.transform.localRotation = EndSnap.transform.localRotation * Quaternion.Euler(90, 0, 0);
         
         Length = (float) (2 * Math.PI * Radius) * Degrees / 360;

         
         var center = GetGlobalCenter();
         if (CenterDebug != null)
             CenterDebug.transform.position = center;
         var start = GetGlobalCirclePos(90);
         if (PosDebug != null)
             PosDebug.transform.position = start;
         var tan = GetDirection(1f);
         if (TanDebug != null)
             TanDebug.transform.position = StartPoint.transform.position + tan;
         
         
         var MArrow = this.GetComponentInChildren<MidArrow>();
         if (MArrow != null)
         {
             MArrow.Size = SizeDirectionArrow;
             if (DirectionArrow == true)
             {
                 MArrow.transform.position = GetPosition(0.5f, ref path);
                    
                 MArrow.Draw();
             }
             else
             {
                 MArrow.Hide();
             }
         }

     }

     public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
     {
         return Mathf.Atan2(
             Vector3.Dot(n, Vector3.Cross(v1, v2)),
             Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
     }

     // The method is used when another segment is attached to the line by using the buttons "Attach Line" or "Attach Curve".
     public override void AttachTo(SimulationPath path)
     {
         Drive = path.Drive;
         transform.rotation = path.EndPoint.transform.rotation;
         transform.parent = path.transform.parent;
         transform.position = path.EndPos;
         transform.localScale = new Vector3(1, 1, 1);
         if (path.GetType() == typeof(Curve))
         {
             Curve cur = (Curve) path;
             Clockwise = cur.Clockwise;
         }

        
         var dir = GetLocalDirection(0);
         var dirattachto = path.GetLocalDirection(1);
         var angle = Vector3.SignedAngle(dirattachto, dir, transform.up);
        // StartAngle = -angle;
        DrawPath();
        CheckSnapping();
     }


     #endregion

     #region PublicMethods

     [Button("Init")]
     
     // Reset the curve to the parent parameter.
     public void Reset()
     {
         // Debug.Log("Reset Curve");
         EnableSnap = true;
         if (transform.childCount < 1)
         {
             var obj = new GameObject();
             obj.transform.parent = transform;
             obj.transform.localPosition = new Vector3(0, 0, 0);
             obj.transform.localRotation = Quaternion.identity;
             obj.name = "Start";
         }
         if (transform.childCount < 2)
         {
             var obj = new GameObject();
             obj.transform.parent = transform;
             obj.transform.localPosition = new Vector3(1, 0, 0);
             obj.transform.localRotation = Quaternion.identity;
             obj.name = "End";
         }
         StartPoint = transform.GetChild(0).gameObject;
         EndPoint = transform.GetChild(1).gameObject;
         BaseReset();
         DrawPath();
     }

     [Button("SetStartToZero")]
     // set start point
     public void setStart()
     {
         SetStartTo0();
     }

     [Button("Rotate")]
     // rotate curve
     public void Rotate()
     {
         StartAngle = StartAngle + 90;
         DrawPath();
     }

     [Button("Attach Line")]
     // attache line to the current object
     public void AttachLine()
     {

         AttachPathLine("Assets/realvirtual-Simulation/PathSystems/Line.prefab", this.gameObject);

#if UNITY_EDITOR
        
#endif
     }

     [Button("Attach Curve")]
     // attache curve to the current object
     public void AttachCurve()
     {
         // Create new Gameobject
         AttachPathCurve("Assets/realvirtual-Simulation/PathSystems/Curve.prefab", this.gameObject);
#if UNITY_EDITOR
        
#endif
     }

     #endregion

     #region PrivateMethods

     private Vector3 GetGlobalCirclePos(float angle)
     {
         var point = GetPathAnglePos(angle);
         return transform.TransformPoint(point);
     }

     private Vector3 GetGlobalCenter()
     {
         Vector3 start = GetPathAnglePos(0);
         Vector3 end = GetPathAnglePos(180);
         var half = (end - start) / 2;
         var localpos = StartPoint.transform.localPosition + half;
         return transform.TransformPoint(localpos);
     }

     private Vector3 GetCirclePos(float angle)
     {

         float x = Mathf.Sin(Mathf.Deg2Rad * angle) * Radius;
         float y = Mathf.Cos(Mathf.Deg2Rad * angle) * Radius;
         float z = 0;
         var vec = new Vector3(x, y, z);
         return vec;

     }

     #endregion
   
 }
 }
 
