using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using System.Linq;

namespace realvirtual
{
#if UNITY_2021_2_OR_NEWER
    [Overlay(typeof(SceneView), "realvirtual Toolbar", true)]
    [Icon("Assets/realvirtual/Icons/realvirtual.png")]
    public class RealvirtualToolbarOverlay : ToolbarOverlay
    {
        RealvirtualToolbarOverlay() : base(
            QuickEditButton.id,
            MovePivotButton.id,
            DrawModeDropdown.id,
            GizmoToggle.id)
        { }
        
        public override void OnCreated()
        {
            base.OnCreated();
            // Position at top of scene view
            floatingPosition = new Vector2(10, 10);
            collapsed = false;
            displayed = true;
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class GizmoToggle : EditorToolbarToggle
    {
        public const string id = "RealvirtualGizmoToggle";
        
        public GizmoToggle()
        {
            text = "Gizmos";
            tooltip = "Toggle gizmo visibility";
            // Use the standard gizmos icon
            var gizmoIcon = EditorGUIUtility.IconContent("d_SceneViewVisibility");
            if (gizmoIcon != null && gizmoIcon.image != null)
            {
                icon = gizmoIcon.image as Texture2D;
            }
            
            UpdateState();
            
            // Handle toggle
            this.RegisterValueChangedCallback(evt =>
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    sceneView.drawGizmos = evt.newValue;
                    sceneView.Repaint();
                }
            });
        }
        
        void UpdateState()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                value = sceneView.drawGizmos;
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class DrawModeDropdown : EditorToolbarDropdown
    {
        public const string id = "RealvirtualDrawMode";
        
        public DrawModeDropdown()
        {
            text = "Shaded";
            tooltip = "Select draw mode";
            // Use a valid Unity icon for draw modes
            var iconContent = EditorGUIUtility.IconContent("ViewToolOrbit");
            if (iconContent != null && iconContent.image != null)
            {
                icon = iconContent.image as Texture2D;
            }
            
            clicked += ShowDrawModeMenu;
            
            // Update text based on current mode
            UpdateCurrentMode();
        }
        
        void UpdateCurrentMode()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                var cameraMode = sceneView.cameraMode;
                if (cameraMode.drawMode == DrawCameraMode.Textured)
                {
                    text = "Shaded";
                }
                else if (cameraMode.drawMode == DrawCameraMode.TexturedWire)
                {
                    text = "Shaded Wireframe";
                }
                // If any other mode, default to Shaded
                else
                {
                    text = "Shaded";
                }
            }
        }
        
        void ShowDrawModeMenu()
        {
            var menu = new GenericMenu();
            var sceneView = SceneView.lastActiveSceneView;
            
            if (sceneView == null) return;
            
            // Shaded
            menu.AddItem(new GUIContent("Shaded"), 
                sceneView.cameraMode.drawMode == DrawCameraMode.Textured, 
                () => SetDrawMode(DrawCameraMode.Textured));
            
            // Shaded Wireframe
            menu.AddItem(new GUIContent("Shaded Wireframe"), 
                sceneView.cameraMode.drawMode == DrawCameraMode.TexturedWire, 
                () => SetDrawMode(DrawCameraMode.TexturedWire));
            
            menu.ShowAsContext();
        }
        
        void SetDrawMode(DrawCameraMode mode)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.cameraMode = SceneView.GetBuiltinCameraMode(mode);
                sceneView.Repaint();
                UpdateCurrentMode();
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class QuickEditButton : EditorToolbarButton
    {
        public const string id = "RealvirtualQuickEdit";
        
        public QuickEditButton()
        {
            text = "Quick Edit";
            tooltip = "Toggle QuickEdit overlay (F1)";
            
            // Try to load realvirtual icon from the correct path
            var iconPath = "Assets/realvirtual/private/Resources/Icons/Icon64.png";
            var realvirtualIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (realvirtualIcon != null)
            {
                icon = realvirtualIcon;
            }
            else
            {
                // Fallback icon if realvirtual icon not found
                icon = EditorGUIUtility.IconContent("d_Settings").image as Texture2D;
            }
            
            clicked += ShowQuickEdit;
        }
        
        void ShowQuickEdit()
        {
            // Toggle QuickEdit overlay visibility
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.overlayCanvas != null)
            {
#if UNITY_6000_0_OR_NEWER
                foreach (var overlay in sceneView.overlayCanvas.overlays)
#else
                // Unity 2022 compatibility - overlays property doesn't exist, use reflection
                var overlaysField = sceneView.overlayCanvas.GetType().GetField("m_Overlays", BindingFlags.NonPublic | BindingFlags.Instance);
                var overlaysList = overlaysField?.GetValue(sceneView.overlayCanvas) as System.Collections.IEnumerable;
                foreach (var overlayObj in overlaysList?.Cast<object>() ?? new object[0])
                {
                    var overlay = overlayObj; // For Unity 2022, we'll need to use reflection for properties too
#endif
                {
#if UNITY_6000_0_OR_NEWER
                    // Check for QuickEdit overlay with various possible IDs
                    if (overlay.id.Contains("QuickEdit") || 
                        overlay.displayName.Contains("QuickEdit") ||
                        overlay.id.Contains("realvirtual.QuickEdit") ||
                        overlay.GetType().Name.Contains("QuickEdit"))
                    {
                        overlay.displayed = !overlay.displayed;
                        if (overlay.displayed)
                        {
                            overlay.collapsed = false;
                        }
                        break;
                    }
#else
                    // Unity 2022 compatibility - use reflection for property access
                    var idProperty = overlay.GetType().GetProperty("id");
                    var displayNameProperty = overlay.GetType().GetProperty("displayName");
                    var displayedProperty = overlay.GetType().GetProperty("displayed");
                    var collapsedProperty = overlay.GetType().GetProperty("collapsed");
                    
                    var id = idProperty?.GetValue(overlay)?.ToString() ?? "";
                    var displayName = displayNameProperty?.GetValue(overlay)?.ToString() ?? "";
                    var typeName = overlay.GetType().Name;
                    
                    if (id.Contains("QuickEdit") || 
                        displayName.Contains("QuickEdit") ||
                        id.Contains("realvirtual.QuickEdit") ||
                        typeName.Contains("QuickEdit"))
                    {
                        var currentDisplayed = (bool)(displayedProperty?.GetValue(overlay) ?? false);
                        displayedProperty?.SetValue(overlay, !currentDisplayed);
                        if (!currentDisplayed) // If we're now showing it
                        {
                            collapsedProperty?.SetValue(overlay, false);
                        }
                        break;
                    }
#endif
                }
#if !UNITY_6000_0_OR_NEWER
                } // Close Unity 2022 foreach loop
#endif
                
                // Note: QuickEdit overlay may not be available if not in use
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class MovePivotButton : EditorToolbarButton
    {
        public const string id = "RealvirtualMovePivot";
        
        public MovePivotButton()
        {
            text = "Move Pivot";
            tooltip = "Open Move Pivot overlay";
            icon = EditorGUIUtility.IconContent("d_ToolHandlePivot").image as Texture2D;
            
            clicked += ShowMovePivot;
        }
        
        void ShowMovePivot()
        {
            // Toggle MovePivot overlay visibility
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.overlayCanvas != null)
            {
#if UNITY_6000_0_OR_NEWER
                foreach (var overlay in sceneView.overlayCanvas.overlays)
#else
                // Unity 2022 compatibility - overlays property doesn't exist, use reflection
                var overlaysField = sceneView.overlayCanvas.GetType().GetField("m_Overlays", BindingFlags.NonPublic | BindingFlags.Instance);
                var overlaysList = overlaysField?.GetValue(sceneView.overlayCanvas) as System.Collections.IEnumerable;
                foreach (var overlayObj in overlaysList?.Cast<object>() ?? new object[0])
                {
                    var overlay = overlayObj; // For Unity 2022, we'll need to use reflection for properties too
#endif
                {
#if UNITY_6000_0_OR_NEWER
                    if (overlay.id.Contains("MovePivot") || overlay.displayName.Contains("Move Pivot"))
                    {
                        overlay.displayed = !overlay.displayed;
                        if (overlay.displayed)
                        {
                            overlay.collapsed = false;
                        }
                        break;
                    }
#else
                    // Unity 2022 compatibility - use reflection for property access
                    var idProperty = overlay.GetType().GetProperty("id");
                    var displayNameProperty = overlay.GetType().GetProperty("displayName");
                    var displayedProperty = overlay.GetType().GetProperty("displayed");
                    var collapsedProperty = overlay.GetType().GetProperty("collapsed");
                    
                    var id = idProperty?.GetValue(overlay)?.ToString() ?? "";
                    var displayName = displayNameProperty?.GetValue(overlay)?.ToString() ?? "";
                    
                    if (id.Contains("MovePivot") || displayName.Contains("Move Pivot"))
                    {
                        var currentDisplayed = (bool)(displayedProperty?.GetValue(overlay) ?? false);
                        displayedProperty?.SetValue(overlay, !currentDisplayed);
                        if (!currentDisplayed) // If we're now showing it
                        {
                            collapsedProperty?.SetValue(overlay, false);
                        }
                        break;
                    }
#endif
                }
#if !UNITY_6000_0_OR_NEWER
                } // Close Unity 2022 foreach loop
#endif
            }
        }
    }
#endif
}