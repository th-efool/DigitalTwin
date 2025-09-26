// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
  #if REALVIRTUAL_INTERACT
      using XdeEngine.Core;
#endif


  namespace realvirtual
{
    [CustomEditor(typeof(Drive))]
    //! Class for displaying the drive handle in Unity Editor in Scene View during edit and play mode
    //! With the drive handle the drive directions can be defined and the drive can moved manually during playmode
    public class DriveHandle : NaughtyAttributes.Editor.NaughtyInspector
    {
        private static float sizecones = 0.36f; // 20% bigger cone size
        private static float sizecubes = 0.24f; // 20% bigger cube size  
        private static float distancecenter = 0.18f; // 20% bigger offset
        private static float sizearc = 0.96f; // 20% bigger arc radius
        private static float fontsize = 14; // Slightly bigger font
        // Enhanced modern color scheme with better contrast
        private static Color colordir = new Color(0f, 0.7f, 1f, 1f); // Bright blue for main direction
        private static Color colorinactive = new Color(0.7f, 0.7f, 0.7f, 0.4f); // Light gray for inactive
        private static Color colorrunning = new Color(0f, 1f, 0.3f, 1f); // Bright green for running forward
        private static Color colorrunningreverse = new Color(1f, 0.3f, 0f, 1f); // Orange-red for running reverse
        private static Color colorrunningstopped = new Color(1f, 0.85f, 0f, 1f); // Golden yellow for ready
        private static Color colorcirclelimits = new Color(0.5f, 0.8f, 1f, 0.5f); // Stronger blue for arc
        private static Color colorlimitwarning = new Color(1f, 0.4f, 0.1f, 1f); // Bright orange warning
        private static Color labelcolor = Color.white; // White text
        private static Color labelbackground = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Darker background
        
        private float distanceclick = 0.2f;
   
        private int idactive, idnonactive1, idnonactive2, idrevert, idposmin;
        private Drive drive;
        private Kinematic _kinematic;
        private float size;
        private Vector3 posactive, posinactive1, posinactive2, posrevert, posmin;
        private DIRECTION dirnotused1, dirnotused2;
        private bool _istranslation;
        private float _globalscale = 1000;
        #if REALVIRTUAL_INTERACT
        private XdeHingeJoint _xdehingejoint;
        #endif
        private bool _isinit = false;
        private realvirtualController _settings;
        private float _scalehandle = 1;
        private float _offset;
        private float pulseTime = 0f; // For animated effects
        
        // Cached objects for performance
        private static GUIStyle guiStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle shadowStyle;
        private static realvirtualController cachedSettings;
        private static double lastSettingsCheck;
        private List<TransportSurface> cachedSurfaces;
        private int lastSurfacesFrame = -1;

        private void InitializeGUIStyles()
        {
            if (guiStyle == null)
            {
                guiStyle = new GUIStyle();
                guiStyle.fontStyle = FontStyle.Bold;
                guiStyle.alignment = TextAnchor.MiddleCenter;
            }
            
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.fontSize = 18;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleCenter;
            }
            
            if (shadowStyle == null)
            {
                shadowStyle = new GUIStyle();
                shadowStyle.fontSize = 18;
                shadowStyle.fontStyle = FontStyle.Bold;
                shadowStyle.alignment = TextAnchor.MiddleCenter;
                shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.7f);
            }
        }
        
        private realvirtualController GetCachedSettings()
        {
            var currentTime = EditorApplication.timeSinceStartup;
            if (cachedSettings == null || currentTime - lastSettingsCheck > 1.0)
            {
                cachedSettings = UnityEngine.Object.FindAnyObjectByType<realvirtualController>();
                lastSettingsCheck = currentTime;
            }
            return cachedSettings;
        }
        
        private List<TransportSurface> GetCachedTransportSurfaces(Drive drive)
        {
            int currentFrame = Time.frameCount;
            if (cachedSurfaces == null || lastSurfacesFrame != currentFrame)
            {
                cachedSurfaces = drive.GetTransportSurfaces();
                lastSurfacesFrame = currentFrame;
            }
            return cachedSurfaces;
        }

        protected virtual void OnSceneGUI()
        {
            // Update animation timers
            if (Application.isPlaying)
            {
                pulseTime += 0.03f; // Increment for smooth animation
            }
            
            // Initialize GUI styles once
            InitializeGUIStyles();
         
            if (!_isinit)
            {
                drive = (Drive) target;
                if (drive.HideGizmos)
                    return;
                idactive = GUIUtility.GetControlID(FocusType.Passive);
                idnonactive1 = GUIUtility.GetControlID(FocusType.Passive);
                idnonactive2 = GUIUtility.GetControlID(FocusType.Passive);
                idrevert = GUIUtility.GetControlID(FocusType.Passive);
                idposmin = GUIUtility.GetControlID(FocusType.Passive);
#if REALVIRTUAL_INTERACT
                _xdehingejoint = drive.GetComponent<XdeHingeJoint>();
#endif

                _kinematic = drive.GetComponent<Kinematic>();
                _isinit = true;
                _settings = GetCachedSettings();
                if (_settings != null)
                {
                    _globalscale = _settings.Scale;
                    _scalehandle = _settings.ScaleHandles;
                }
                

            }

            if (drive.Direction == DIRECTION.Virtual)
                return;

            // Switching Directions with Keys - only Editor mode
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space && !Application.isPlaying)
            {
                drive.ReverseDirection = !drive.ReverseDirection;
            }
            
          
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab && !Application.isPlaying)
            {
                if ((int)drive.Direction < Enum.GetNames(typeof(DIRECTION)).Length-2)
                {
                    drive.Direction = drive.Direction + 1;
                }
                else
                {
                    drive.Direction = (DIRECTION)0;
                }
              
            }

            // Jogging with Keys - only Runmode           
            if (Event.current.type == EventType.KeyDown && Application.isPlaying)
            {
                if (Event.current.keyCode == KeyCode.Alpha3 || Event.current.keyCode == KeyCode.Keypad3)
                {
                    drive.JogForward = true;
                    drive.JogBackward = false;
                }
                
                if ( Event.current.keyCode == KeyCode.Alpha1  || Event.current.keyCode == KeyCode.Keypad1)
                {
        
                    drive.JogForward = false;
                    drive.JogBackward = true;
                }
            }           
            if (Event.current.type == EventType.KeyUp && Application.isPlaying)
            {
                if (Event.current.keyCode == KeyCode.Alpha3 || Event.current.keyCode == KeyCode.Alpha1  || Event.current.keyCode == KeyCode.Keypad1 || Event.current.keyCode == KeyCode.Keypad3)
                {
                    drive.JogForward = false;
                    drive.JogBackward = false;
                }
            }
     


            if (drive.Direction == DIRECTION.LinearX || drive.Direction == DIRECTION.LinearY ||
                drive.Direction == DIRECTION.LinearZ)
                _istranslation = true;
            else
                _istranslation = false;

            switch (Event.current.GetTypeForControl(idactive))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(idactive, HandleUtility.DistanceToCircle(posactive, size * distanceclick));
                    break;
            }

            switch (Event.current.GetTypeForControl(idnonactive1))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(idnonactive1,
                        HandleUtility.DistanceToCircle(posinactive1, size * distanceclick));
                    break;
            }

            switch (Event.current.GetTypeForControl(idnonactive2))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(idnonactive2,
                        HandleUtility.DistanceToCircle(posinactive2, size * distanceclick));
                    break;
            }

            switch (Event.current.GetTypeForControl(idrevert))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(idrevert, HandleUtility.DistanceToCircle(posrevert, size * distanceclick));
                    break;
            }

            switch (Event.current.GetTypeForControl(idposmin))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(idposmin, HandleUtility.DistanceToCircle(posmin, size * distanceclick));
                    break;
            }

          
            
            if (Event.current.type == EventType.Repaint)
            {
                Transform transform = ((Drive) target).transform;
            
                var driveposition = transform.position;

                var dirdrive = DirectionToVector(drive,true);
                var dirdrivelocal = DirectionToVector(drive, false);
                // Make cones symmetrical and further from pivot for better visibility
                var direction = dirdrive * distancecenter * 1.5f; // Moderate distance from pivot
                var directionmin = new Vector3(0, 0, 0);
                var rotationquat = Quaternion.LookRotation(dirdrive);
                var rotationquatrevert = Quaternion.LookRotation(dirdrive * -1);
                
                // Calculate size with aggressive limits to prevent oversizing at distance
                float rawSize = HandleUtility.GetHandleSize(driveposition);
                
                // Use logarithmic scaling to compress the size range
                // This prevents exponential growth at distance
                float logSize = Mathf.Log10(Mathf.Max(rawSize, 1f) + 1f);
                
                // Size limits (20% bigger)
                float minSize = 0.24f * _scalehandle;
                float maxSize = 0.96f * _scalehandle;
                
                // Apply the scaling
                size = Mathf.Clamp(logSize * 0.5f, minSize, maxSize);
                
                // Additional distance-based reduction using Scene camera
                Camera sceneCamera = null;
                if (SceneView.lastActiveSceneView != null)
                    sceneCamera = SceneView.lastActiveSceneView.camera;
                else if (SceneView.currentDrawingSceneView != null)
                    sceneCamera = SceneView.currentDrawingSceneView.camera;
                    
                if (sceneCamera != null)
                {
                    float distanceToCamera = Vector3.Distance(driveposition, sceneCamera.transform.position);
                    
                    // Progressive size reduction based on distance
                    if (distanceToCamera > 15f)
                    {
                        float reduction = 15f / distanceToCamera;
                        size *= Mathf.Clamp(reduction, 0.15f, 1f);
                    }
                }

                
                    var surfaces = GetCachedTransportSurfaces(drive);
                    if (surfaces != null && surfaces.Count > 0)
                    {
                        // get first transportsurface
                        var trans = surfaces[0];
                        if (trans != null)
                            driveposition = trans.GetMiddleTopPoint();
                        // get middle top position of transportsurface
                        if (trans != null)
                            if (trans.Radial)
                            {
                                driveposition = trans.gameObject.transform.position;
                            }
                    }
                    

                if (Application.isPlaying)
                    _offset = 0;
                else
                    _offset = drive.Offset;
                

                if (_istranslation)
                {
                    /// Linear Movement /////////////////////////////////////
                   
                    // Keep cones at standard positions in both edit and play mode
                    // Cones no longer move to limits


                    if (drive.ReverseDirection)
                    {
                        direction = direction * -1;
                        directionmin = directionmin * -1;
                        rotationquat = Quaternion.LookRotation(dirdrive * -1);
                        rotationquatrevert = Quaternion.LookRotation(dirdrive);
                    }

                    posactive = driveposition + direction;
                    posrevert = driveposition - direction; // Symmetrical position on opposite side
                    posmin = driveposition - directionmin;


                    // Draw Direction Handle with enhanced visuals
                    Color forwardConeColor = colordir; // Store the final color for the cone
                    
                    if (Application.isPlaying)
                    {
                        // Check if at limits
                        bool atUpperLimit = false;
                        bool atLowerLimit = false;
                        if (drive.UseLimits)
                        {
                            atUpperLimit = Mathf.Abs(drive.CurrentPosition - drive.UpperLimit) < 1f; // Within 1mm of upper limit
                            atLowerLimit = Mathf.Abs(drive.CurrentPosition - drive.LowerLimit) < 1f; // Within 1mm of lower limit
                        }
                        
                        // Determine forward cone color
                        if (atUpperLimit)
                        {
                            // At upper limit - no pulsing, show warning color
                            forwardConeColor = colorlimitwarning;
                        }
                        else if (drive.IsSpeed > 0.01f) // Forward movement - pulse
                        {
                            var pulse = Mathf.Sin(pulseTime * 3f) * 0.5f + 0.5f;
                            forwardConeColor = new Color(colorrunning.r * pulse, colorrunning.g * pulse, colorrunning.b * pulse, colorrunning.a);
                        }
                        else if (drive.IsSpeed < -0.01f) // Backward movement - dim the forward cone
                        {
                            forwardConeColor = new Color(colorrunningstopped.r, colorrunningstopped.g, colorrunningstopped.b, 0.3f);
                        }
                        else // Stopped
                        {
                            forwardConeColor = colorrunningstopped;
                        }
                    }

                    // Draw main cone with the stored color - ensure color is set
                    if (!Application.isPlaying)
                    {
                        Handles.color = colordir;
                        Handles.ConeHandleCap(idactive, posactive, rotationquat, size * sizecones,
                            EventType.Repaint);
                    }
                    else
                    {
                        // Apply color and immediately draw cone (matching backward cone pattern)
                        Handles.color = forwardConeColor;
                        Handles.ConeHandleCap(idactive, posactive, rotationquat, size * sizecones,
                            EventType.Repaint);
                        
                        // Draw motion trail lines AFTER cone to prevent color interference
                        if (drive.IsSpeed > 0.01f) // Forward movement
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                var offset = (pulseTime + i * 0.3f) % 1.0f;
                                var lineStart = driveposition + (posactive - driveposition) * offset * 0.8f;
                                var lineEnd = driveposition + (posactive - driveposition) * (offset * 0.8f + 0.1f);
                                var trailColor = new Color(colorrunning.r, colorrunning.g, colorrunning.b, (1f - offset) * 0.3f);
                                Handles.color = trailColor;
                                Handles.DrawLine(lineStart, lineEnd);
                            }
                        }
                    }
                    
                    // Add direction arrow line for clarity (only in edit mode)
                    if (!Application.isPlaying)
                    {
                        Handles.color = new Color(colordir.r, colordir.g, colordir.b, 0.5f);
                        Handles.DrawDottedLine(driveposition, posactive, 2f);
                    }
                    
                    // Draw center sphere to show pivot point - always gray color
                    Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    Handles.SphereHandleCap(0, driveposition, Quaternion.identity, Mathf.Min(size * 0.08f, 0.1f), EventType.Repaint);
                    
                    // Add axis label for linear drives only (disabled to avoid transform gizmo interference)

                    // Enhanced Limit Handles with better visualization
                    if (drive.UseLimits)
                    {
                        // Show limit spheres and values in both edit and play modes
                        // Calculate actual limit positions in world space (limits don't move with the object)
                        // In edit mode, show relative to current position; in play mode, show absolute world positions
                        Vector3 upperLimitPos, lowerLimitPos;
                        
                        if (Application.isPlaying)
                        {
                            // During play, limits are fixed world positions
                            // Calculate based on the initial/home position plus the limit offsets
                            var worldLimitOrigin = transform.position - dirdrive * (drive.CurrentPosition / 1000);
                            upperLimitPos = worldLimitOrigin + dirdrive * ((drive.UpperLimit + _offset) / 1000);
                            lowerLimitPos = worldLimitOrigin + dirdrive * ((drive.LowerLimit + _offset) / 1000);
                        }
                        else
                        {
                            // In edit mode, show relative to current position for easier setup
                            upperLimitPos = driveposition + dirdrive * ((drive.UpperLimit + _offset - drive.CurrentPosition) / 1000);
                            lowerLimitPos = driveposition + dirdrive * ((drive.LowerLimit + _offset - drive.CurrentPosition) / 1000);
                        }
                        
                        // Draw limit indicator spheres at both limits
                        Handles.color = colorcirclelimits;
                        
                        // Lower limit sphere
                        Handles.SphereHandleCap(0, lowerLimitPos, Quaternion.identity,
                            size * 0.12f, EventType.Repaint);
                        
                        // Upper limit sphere
                        Handles.SphereHandleCap(0, upperLimitPos, Quaternion.identity,
                            size * 0.12f, EventType.Repaint);
                        
                        // Display limit values
                        var limitLabelStyle = new GUIStyle();
                        limitLabelStyle.fontSize = 11;
                        limitLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 0.9f);
                        limitLabelStyle.alignment = TextAnchor.MiddleCenter;
                        
                        // Position labels slightly offset from limit positions
                        var perpDir = Vector3.Cross(dirdrive, Vector3.up).normalized;
                        if (perpDir.magnitude < 0.1f) // If drive is vertical
                            perpDir = Vector3.Cross(dirdrive, Vector3.forward).normalized;
                        
                        var lowerLabelPos = lowerLimitPos + perpDir * size * 0.5f;
                        Handles.Label(lowerLabelPos, drive.LowerLimit.ToString("F0") + " mm", limitLabelStyle);
                        
                        var upperLabelPos = upperLimitPos + perpDir * size * 0.5f;
                        Handles.Label(upperLabelPos, drive.UpperLimit.ToString("F0") + " mm", limitLabelStyle);
                        
                        if (Application.isPlaying)
                        {
                            // Determine color for lower limit cone
                            Color limitConeColor;
                            
                            // Check if at lower limit
                            bool atLowerLimit = Mathf.Abs(drive.CurrentPosition - drive.LowerLimit) < 1f; // Within 1mm of limit
                            
                            if (atLowerLimit)
                            {
                                // At limit - no pulsing, show warning color
                                limitConeColor = colorlimitwarning;
                            }
                            else if (drive.IsSpeed < -0.01f) // Moving backward - apply pulsing
                            {
                                var pulse = Mathf.Sin(pulseTime * 3f) * 0.5f + 0.5f;
                                limitConeColor = new Color(colorrunningreverse.r * pulse, colorrunningreverse.g * pulse, colorrunningreverse.b * pulse, colorrunningreverse.a);
                            }
                            else
                            {
                                // Not moving backward - dimmed
                                limitConeColor = new Color(colorrunningstopped.r, colorrunningstopped.g, colorrunningstopped.b, 0.3f);
                            }
                            Handles.color = limitConeColor;
                            
                            // Draw with outline
                            var limitColor = Handles.color;
                            
                            // Draw outline at symmetrical position (not at limit)
                            Handles.color = new Color(0f, 0f, 0f, 0.5f);
                            Handles.ConeHandleCap(idrevert, posrevert, rotationquatrevert,
                                size * sizecones * 1.1f,
                                EventType.Repaint);
                            
                            // Draw main cone at symmetrical position (not at limit)
                            Handles.color = limitColor;
                            Handles.ConeHandleCap(idrevert, posrevert, rotationquatrevert,
                                size * sizecones,
                                EventType.Repaint);
                        }

                        // Draw limit range line between actual limit positions
                        Handles.color = colorcirclelimits;
                        Handles.DrawAAPolyLine(3f, lowerLimitPos, upperLimitPos);
                        
                    }

                    if (Application.isPlaying)
                    {
                        /// Display info offset from linear gizmos
                        Handles.BeginGUI();
                        // Calculate offset perpendicular to drive direction (to the right side)
                        Vector3 perpOffset = Vector3.Cross(dirdrive, Vector3.up).normalized;
                        if (perpOffset.magnitude < 0.1f) // If drive is vertical, use different perpendicular
                            perpOffset = Vector3.Cross(dirdrive, Vector3.forward).normalized;
                        
                        // Offset further to the side to avoid overlapping with gizmos
                        Vector3 offsetPos = driveposition + perpOffset * size * 3.5f; // More offset to the side
                        Vector2 offsetPos2D = HandleUtility.WorldToGUIPoint(offsetPos);
                        
                        // Create simplified info content with proper decimal formatting
                        string posText = drive.IsPosition.ToString("F1") + " mm";
                        string speedText = Mathf.Abs(drive.IsSpeed) > 0.01f ? 
                            drive.IsSpeed.ToString("F1") + " mm/s" : "";
                        
                        // Setup text style
                        float scaledFontSize = Mathf.Clamp(fontsize * _scalehandle, 12f, 18f);
                        guiStyle.fontSize = (int)scaledFontSize;
                        guiStyle.fontStyle = FontStyle.Bold;
                        guiStyle.alignment = TextAnchor.MiddleCenter;
                        
                        // Draw background panel offset from gizmos
                        var bgRect = new Rect(offsetPos2D.x - 45, offsetPos2D.y - 15, 90, 30);
                        EditorGUI.DrawRect(bgRect, new Color(0.1f, 0.1f, 0.1f, 0.8f));
                        
                        // Draw position on top line
                        guiStyle.normal.textColor = Color.white;
                        GUI.Label(new Rect(bgRect.x, bgRect.y + 2, bgRect.width, 15), 
                            posText, guiStyle);
                        
                        // Draw speed on bottom line (smaller, colored)
                        if (!string.IsNullOrEmpty(speedText))
                        {
                            guiStyle.fontSize = (int)(scaledFontSize * 0.8f);
                            guiStyle.normal.textColor = Mathf.Abs(drive.IsSpeed) > 0.01f ? 
                                new Color(0.4f, 1f, 0.4f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);
                            GUI.Label(new Rect(bgRect.x, bgRect.y + 15, bgRect.width, 15), 
                                speedText, guiStyle);
                        }
                        
                        Handles.EndGUI();
                      
                        
                        
                        // Show backward cone consistently
                        Color backwardConeColor;
                        if (drive.IsSpeed < -0.01f) // Moving backward (negative speed)
                        {
                            // Apply pulsing effect for backward movement
                            var pulse = Mathf.Sin(pulseTime * 3f) * 0.5f + 0.5f; // More pronounced pulsing: 0.0 to 1.0
                            backwardConeColor = new Color(colorrunningreverse.r * pulse, colorrunningreverse.g * pulse, colorrunningreverse.b * pulse, colorrunningreverse.a);
                        }
                        else
                        {
                            // Dimmed when not moving backward
                            backwardConeColor = new Color(colorrunningstopped.r, colorrunningstopped.g, colorrunningstopped.b, 0.3f);
                        }
                        
                        Handles.color = backwardConeColor;
                        if (!drive.UseLimits)
                            Handles.ConeHandleCap(idrevert, posrevert,  rotationquatrevert,
                                size * sizecones,
                                EventType.Repaint);
                    }

                    // Draw Handles in the Directions not used
                    if (!Application.isPlaying)
                    {
                        var directionnotused1 = DirectionNotUsed1(drive,false);
                        var dir1 = transform.rotation*directionnotused1 * distancecenter;
                        posinactive1 = driveposition + dir1;
                        var rotationquat1 = Quaternion.LookRotation(dir1);

                        var directionnotused2 = DirectionNotUsed2(drive,false);
                        var dir2 = transform.rotation*directionnotused2 * distancecenter;
                        posinactive2 = driveposition + dir2;
                        var rotationquat2 = Quaternion.LookRotation(dir2);

                        if (drive.ReverseDirection)
                        {
                            posinactive1 = driveposition - dir1;
                            rotationquat1 = Quaternion.LookRotation(dir1 * -1);
                            posinactive2 = driveposition - dir2;
                            rotationquat2 = Quaternion.LookRotation(dir2 * -1);
                        }


                        // Draw inactive direction handles with better visibility
                        Handles.color = new Color(0.6f, 0.6f, 0.6f, 0.3f);
                        
                        // First inactive direction
                        Handles.ConeHandleCap(idnonactive1, posinactive1, rotationquat1,
                            size * sizecubes * 0.8f, // Slightly bigger
                            EventType.Repaint);
                        
                        // Second inactive direction
                        Handles.ConeHandleCap(idnonactive2, posinactive2,  rotationquat2,
                            size * sizecubes * 0.8f, // Slightly bigger
                            EventType.Repaint);

                        // Main direction line
                        Handles.color = colordir;
                        Handles.DrawAAPolyLine(2f, driveposition, posactive);
                        
                        // Inactive direction lines as dotted
                        Handles.color = new Color(colorinactive.r, colorinactive.g, colorinactive.b, 0.2f);
                        Handles.DrawDottedLine(driveposition, posinactive1, 4f);
                        Handles.DrawDottedLine(driveposition, posinactive2, 4f);
                    }
                }
                else
                {

                    if (_kinematic != null)
                    {
                        if (_kinematic.MoveCenterEnable && !Application.isPlaying)
                        {
                            var center = _kinematic.DeltaPosOrigin;
                            var scale = transform.lossyScale;
                            var centerpos = new Vector3(center.x/Global.realvirtualcontroller.Scale,center.y/Global.realvirtualcontroller.Scale,center.z/Global.realvirtualcontroller.Scale);
                            driveposition = transform.position +
                                            drive.gameObject.transform.TransformDirection(centerpos);
                        }
                    }
                    
                    var dirarc = DirectionNotUsed1(drive, true);
                    var dir1 = dirarc;
                    var dirrot = DirectionNotUsed2(drive, true);

                    // Calculate limited radius for rotation gizmos (20% bigger)
                    var maxArcSize = Application.isPlaying ? 0.96f : 1.2f; // 20% bigger limits
                    var radius = Mathf.Min(size * sizearc, maxArcSize);
                    var startpos = driveposition;
                    var drivreverse = 1;
                    if (drive.ReverseDirection)
                    {
                        drivreverse = -1;
                    }

                    var dirrotquat = Quaternion.LookRotation(dirrot * drivreverse);
                    var dirrotrevert = Quaternion.LookRotation(-dirrot * drivreverse);

                    posactive = startpos + (dirarc) * size * sizearc + (drivreverse * dirrot * radius * 0.5f);
                    posrevert = startpos + (dirarc) * size * sizearc + (-drivreverse * dirrot * radius * 0.5f);
                    var poszeroline = startpos + (dirarc) * size * sizearc;
                    var poslable = startpos + (dirarc) * size * sizearc;
                    var angle = 360f;
                    if (drive.UseLimits)
                    {
                        angle = drive.UpperLimit - drive.LowerLimit;
                        Vector3 rotatedVector =
                            Quaternion.AngleAxis(drive.LowerLimit + _offset - drive.CurrentPosition, drivreverse * dirdrive) *
                            dirarc;
                        dirarc = rotatedVector;
                    }

                    // Axis lines - simpler in play mode
                    if (!Application.isPlaying)
                    {
                        // Draw axis line with gradient effect in edit mode
                        Handles.color = new Color(1f, 1f, 1f, 0.2f);
                        Handles.DrawAAPolyLine(5f, driveposition, startpos+drivreverse*dirdrive*radius);
                        
                        Handles.color = colordir;
                        Handles.DrawAAPolyLine(3f, driveposition, startpos+drivreverse*dirdrive*radius);
                        
                        // Zero position line
                        Handles.color = new Color(1f, 1f, 1f, 0.3f);
                        Handles.DrawDottedLine(driveposition, poszeroline, 3f);
                    }
                    else
                    {
                        // Just a simple axis indicator in play mode
                        Handles.color = new Color(colordir.r, colordir.g, colordir.b, 0.3f);
                        Handles.DrawLine(driveposition, startpos+drivreverse*dirdrive*radius);
                    }
                    
                    // Removed axis label for rotational drives to avoid interference with transform gizmo
                    
                    // Draw center sphere to show rotation point
                    Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    Handles.SphereHandleCap(0, driveposition, Quaternion.identity, Mathf.Min(size * 0.08f, 0.1f), EventType.Repaint);

                    // Simple arc visualization - no double circles
                    // Draw filled arc
                    Handles.color = new Color(colorcirclelimits.r, colorcirclelimits.g, colorcirclelimits.b, 0.25f);
                    Handles.DrawSolidArc(driveposition, drivreverse * dirdrive, dirarc, angle, radius);
                    
                    // Single clean outline
                    Handles.color = new Color(0.3f, 0.7f, 1f, 0.6f);
                    Handles.DrawWireArc(driveposition, drivreverse * dirdrive, dirarc, angle, radius, 2f);
                    
                    // Draw current position indicator on arc using IsPosition
                    if (Application.isPlaying)
                    {
                        float currentAngle;
                        if (drive.UseLimits)
                        {
                            currentAngle = drive.IsPosition - drive.LowerLimit;
                        }
                        else
                        {
                            currentAngle = drive.IsPosition;
                        }
                        
                        Vector3 currentPosVector = Quaternion.AngleAxis(currentAngle, drivreverse * dirdrive) * dirarc;
                        var currentPosOnArc = driveposition + currentPosVector.normalized * radius;
                        
                        // Draw position marker
                        Handles.color = Color.white;
                        Handles.SphereHandleCap(0, currentPosOnArc, Quaternion.identity, Mathf.Min(size * 0.12f, 0.15f), EventType.Repaint);
                        
                        // Draw line from center to current position (thinner in play mode)
                        Handles.color = new Color(1f, 1f, 1f, 0.2f);
                        Handles.DrawLine(driveposition, currentPosOnArc);
                    }

                    // Determine and apply cone color for rotational movement
                    Color rotationalConeColor = colordir;
                    if (Application.isPlaying)
                    {
                        if (drive.IsSpeed > 0.01f) // Forward rotation
                        {
                            // Pulsing effect for forward rotating drive
                            var pulse = Mathf.Sin(pulseTime * 3f) * 0.5f + 0.5f; // More pronounced pulsing: 0.0 to 1.0
                            rotationalConeColor = new Color(colorrunning.r * pulse, colorrunning.g * pulse, colorrunning.b * pulse, colorrunning.a);
                        }
                        else if (drive.IsSpeed < -0.01f) // Backward rotation - dim the forward cone
                        {
                            // Dimmed when rotating backward
                            rotationalConeColor = new Color(colorrunningstopped.r, colorrunningstopped.g, colorrunningstopped.b, 0.3f);
                        }
                        else // Stopped
                        {
                            rotationalConeColor = colorrunningstopped;
                        }
                    }

                    // Apply the color and draw cone
                    Handles.color = rotationalConeColor;
                    Handles.ConeHandleCap(idactive, posactive, dirrotquat, size * sizecones,
                        EventType.Repaint);
                    if (Application.isPlaying)
                    {
                        /// Display rotational info offset from gizmos
                        Handles.BeginGUI();
                        // Offset to the side of the rotation arc to avoid overlap
                        Vector3 offsetPos = driveposition + Vector3.right * radius * 1.8f; 
                        Vector2 offsetPos2D = HandleUtility.WorldToGUIPoint(offsetPos);
                        
                        // Create simplified info for rotation with proper decimal formatting
                        string angleText = drive.IsPosition.ToString("F1") + "°";
                        string speedText = Mathf.Abs(drive.IsSpeed) > 0.01f ? 
                            drive.IsSpeed.ToString("F1") + "°/s" : "";
                        
                        // Setup text style
                        float scaledFontSize = Mathf.Clamp(fontsize * _scalehandle, 12f, 18f);
                        guiStyle.fontSize = (int)scaledFontSize;
                        guiStyle.fontStyle = FontStyle.Bold;
                        guiStyle.alignment = TextAnchor.MiddleCenter;
                        
                        // Draw background panel below gizmos
                        var bgRect = new Rect(offsetPos2D.x - 45, offsetPos2D.y - 15, 90, 30);
                        EditorGUI.DrawRect(bgRect, new Color(0.1f, 0.1f, 0.1f, 0.8f));
                        
                        // Draw angle on top line
                        guiStyle.normal.textColor = Color.white;
                        GUI.Label(new Rect(bgRect.x, bgRect.y + 2, bgRect.width, 15), 
                            angleText, guiStyle);
                        
                        // Draw speed on bottom line (smaller, colored)
                        if (!string.IsNullOrEmpty(speedText))
                        {
                            guiStyle.fontSize = (int)(scaledFontSize * 0.8f);
                            guiStyle.normal.textColor = Mathf.Abs(drive.IsSpeed) > 0.01f ? 
                                new Color(0.4f, 1f, 0.4f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);
                            GUI.Label(new Rect(bgRect.x, bgRect.y + 15, bgRect.width, 15), 
                                speedText, guiStyle);
                        }
                        
                        Handles.EndGUI();
                        
                        
                        Handles.color = colorrunningstopped;
                        if (drive.IsSpeed < 0)
                        {
                            // Pulsing effect for reverse rotation
                            var pulse = Mathf.Sin(pulseTime * 3f) * 0.5f + 0.5f; // More pronounced pulsing: 0.0 to 1.0
                            Handles.color = new Color(colorrunningreverse.r * pulse, colorrunningreverse.g * pulse, colorrunningreverse.b * pulse, colorrunningreverse.a);
                        }
                        // Draw cone without double outline
                        Handles.ConeHandleCap(idrevert, posrevert, dirrotrevert, size * sizecones,
                            EventType.Repaint);
                    }

                    // Draw Handles in the Directions not used
                    if (!Application.isPlaying)
                    {
                        var dirr1 = transform.rotation * dirrot;
                        var dirr2 = transform.rotation * dirdrive;
                        posinactive1 = startpos + (dirr1) * size * sizearc + (dirdrive * radius * 0.5f);
                        posinactive2 = startpos + (dirr2) * size * sizearc + (dir1 * radius * 0.5f);
                        Handles.color = colorinactive;
                        dirrotquat = Quaternion.LookRotation(dirdrive);
                        Handles.ConeHandleCap(idnonactive1, posinactive1, dirrotquat, size * sizecones,
                            EventType.Repaint);

                        dirrotquat = Quaternion.Inverse(Quaternion.LookRotation(-dir1));
                        Handles.ConeHandleCap(idnonactive2, posinactive2, dirrotquat, size * sizecones,
                            EventType.Repaint);
                    }
                }
            }

                
            if (Event.current.type == EventType.MouseDown)
            {
                int id = HandleUtility.nearestControl;
                if (!Application.isPlaying)
                {
                    if (id == idactive)
                    {
                        drive.ReverseDirection = !drive.ReverseDirection;
                    }

                    if (id == idnonactive1)
                    {
                        drive.Direction = dirnotused1;
                    }

                    if (id == idnonactive2)
                    {
                        drive.Direction = dirnotused2;
                    }
                }
                else
                {
                    {
                        if (id == idactive)
                        {
                            if (!drive.JogForward)
                            {
                                drive.JogForward = true;
                                drive.JogBackward = false;
                            }
                            else
                            {
                                drive.JogForward = false;
                                drive.JogBackward = false;
                            }
                        }

                        if (id == idrevert || id == idposmin)
                        {
                            if (!drive.JogBackward)
                            {
                                drive.JogBackward = true;
                                drive.JogForward = false;
                            }
                            else
                            {
                                drive.JogForward = false;
                                drive.JogBackward = false;
                            }
                        }
                    }
                }
            }
        }


        private Vector3 DirectionToVector(Drive drive, bool global)
        {
            Vector3 result = Vector3.up;
            switch (drive.Direction)
            {
                case DIRECTION.LinearX:
                    result = Vector3.right;
                    break;
                case DIRECTION.LinearY:
                    result = Vector3.up;
                    break;
                case DIRECTION.LinearZ:
                    result = Vector3.forward;
                    break;
                case DIRECTION.RotationX:
                    result = Vector3.right;
                    break;
                case DIRECTION.RotationY:
                    result = Vector3.up;
                    break;
                case DIRECTION.RotationZ:
                    result = Vector3.forward;
                    break;
            }

            if (global)
                return drive.transform.TransformDirection(result);
            else
                return result;
        }

        private Vector3 DirectionToVector(Drive drive, DIRECTION dir, bool global)
        {
            Vector3 result = Vector3.up;
            switch (dir)
            {
                case DIRECTION.LinearX:
                    result = Vector3.right;
                    break;
                case DIRECTION.LinearY:
                    result = Vector3.up;
                    break;
                case DIRECTION.LinearZ:
                    result = Vector3.forward;
                    break;
                case DIRECTION.RotationX:
                    result = Vector3.right;
                    break;
                case DIRECTION.RotationY:
                    result = Vector3.up;
                    break;
                case DIRECTION.RotationZ:
                    result = Vector3.forward;
                    break;
            }

            if (global)
                return drive.transform.TransformDirection(result);
            else
                return result;
        }

        private Vector3 DirectionNotUsed1(Drive drive, bool global)
        {
            DIRECTION result = DIRECTION.LinearX;
            switch (drive.Direction)
            {
                case DIRECTION.LinearX:
                    result = DIRECTION.LinearY;
                    break;
                case DIRECTION.LinearY:
                    result = DIRECTION.LinearZ;
                    break;
                case DIRECTION.LinearZ:
                    result = DIRECTION.LinearX;
                    break;
                case DIRECTION.RotationX:
                    result = DIRECTION.RotationY;
                    break;
                case DIRECTION.RotationY:
                    result = DIRECTION.RotationZ;
                    break;
                case DIRECTION.RotationZ:
                    result = DIRECTION.RotationX;
                    break;
            }

            dirnotused1 = result;
            return DirectionToVector(drive, result,global);
        }

        private Vector3 DirectionNotUsed2(Drive drive, bool global)
        {
            DIRECTION result = DIRECTION.LinearX;
            switch (drive.Direction)
            {
                case DIRECTION.LinearX:
                    result = DIRECTION.LinearZ;
                    break;
                case DIRECTION.LinearY:
                    result = DIRECTION.LinearX;
                    break;
                case DIRECTION.LinearZ:
                    result = DIRECTION.LinearY;
                    break;
                case DIRECTION.RotationX:
                    result = DIRECTION.RotationZ;
                    break;
                case DIRECTION.RotationY:
                    result = DIRECTION.RotationX;
                    break;
                case DIRECTION.RotationZ:
                    result = DIRECTION.RotationY;
                    break;
            }

            dirnotused2 = result;
            return DirectionToVector(drive, result,global);
        }
    }
}