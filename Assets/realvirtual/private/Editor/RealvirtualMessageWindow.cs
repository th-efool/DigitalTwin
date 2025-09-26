// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace realvirtual
{
    //! Standardized message window for realvirtual operations with consistent header and styling.
    //! Provides a reusable window framework with animated logo and customizable content area.
    //! 
    //! Usage:
    //! var window = RealvirtualMessageWindow.Show("My Title", 500, 300, true, (win) => {
    //!     EditorGUILayout.LabelField("Custom content here");
    //!     if (GUILayout.Button("Close")) win.Close();
    //! });
    public class RealvirtualMessageWindow : EditorWindow
    {
        private static RealvirtualMessageWindow window;
        private Action<RealvirtualMessageWindow> onGUICallback;
        private Texture2D logo;
        private string windowTitle = "realvirtual";
        private string headerTitle = "";
        private bool animateLogo = false;
        private float animationTime;
        private float animationDuration = 0f; // 0 = infinite
        private float rotationSpeed = 720f; // degrees per second - full rotation per second
        private float autoCloseTime = 0f; // 0 = no auto close
        private double windowStartTime = 0;
        private double lastUpdateTime = 0;
        
        //! Shows a new realvirtual message window with standardized header and custom content.
        //! @param windowTitle The window title displayed in the Unity window title bar
        //! @param width Window width in pixels (default: 500)
        //! @param height Initial window height in pixels (default: 300)
        //! @param animateIcon If true, the logo rotates continuously
        //! @param onGUI Callback to draw custom content below the header
        //! @param animationDuration Duration in seconds for logo rotation (0 = infinite)
        //! @param autoCloseTime Time in seconds before window auto-closes (0 = no auto close)
        //! @param headerTitle Optional title displayed next to the logo (if empty, uses windowTitle)
        //! @return The created window instance
        public static RealvirtualMessageWindow Show(string windowTitle, int width = 500, int height = 300, bool animateIcon = false, Action<RealvirtualMessageWindow> onGUI = null, float animationDuration = 0f, float autoCloseTime = 0f, string headerTitle = null)
        {
            if (window != null)
                window.Close();
            
            window = GetWindow<RealvirtualMessageWindow>(true, windowTitle, true);
            window.windowTitle = windowTitle;
            window.headerTitle = string.IsNullOrEmpty(headerTitle) ? windowTitle : headerTitle;
            window.onGUICallback = onGUI;
            window.animateLogo = animateIcon;
            window.animationDuration = animationDuration;
            window.autoCloseTime = autoCloseTime;
            window.windowStartTime = EditorApplication.timeSinceStartup;
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width, 800);
            
            // Center the window
            var main = EditorGUIUtility.GetMainWindowPosition();
            var pos = window.position;
            float centerX = (main.x + (main.width - pos.width) * 0.5f);
            float centerY = (main.y + (main.height - pos.height) * 0.5f);
            window.position = new Rect(centerX, centerY, pos.width, pos.height);
            
            window.Show();
            return window;
        }
        
        //! Updates the content drawing callback for dynamic content changes.
        public void SetContent(Action<RealvirtualMessageWindow> onGUI)
        {
            onGUICallback = onGUI;
            Repaint();
        }
        
        //! Enables or disables the logo rotation animation.
        public void SetLogoAnimation(bool animate)
        {
            animateLogo = animate;
            Repaint();
        }
        
        //! Resets the auto-close timer to keep window open longer
        public void ResetAutoCloseTimer(float newAutoCloseTime = 0f)
        {
            if (newAutoCloseTime > 0)
            {
                autoCloseTime = newAutoCloseTime;
            }
            windowStartTime = EditorApplication.timeSinceStartup;
        }
        
        //! Automatically resizes the window height to fit content.
        public void AutoResize(float height)
        {
            var currentPos = position;
            if (Mathf.Abs(currentPos.height - height) > 5)
            {
                position = new Rect(currentPos.x, currentPos.y, currentPos.width, height);
            }
        }
        
        private void OnEnable()
        {
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/realvirtual/private/Resources/Icons/Icon256.png");
            animationTime = 0f;
            windowStartTime = EditorApplication.timeSinceStartup;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }
        
        private void Update()
        {
            if (animateLogo)
            {
                // Use editor time instead of Time.deltaTime
                double currentTime = EditorApplication.timeSinceStartup;
                float deltaTime = (float)(currentTime - lastUpdateTime);
                lastUpdateTime = currentTime;
                
                animationTime += deltaTime;
                
                // Stop animation after duration if specified
                if (animationDuration > 0 && animationTime >= animationDuration)
                {
                    animateLogo = false;
                    animationTime = animationDuration;
                }
                
                Repaint();
            }
            
            // Auto close window after specified time
            if (autoCloseTime > 0)
            {
                double elapsedTime = EditorApplication.timeSinceStartup - windowStartTime;
                if (elapsedTime >= autoCloseTime)
                {
                    Debug.Log($"[RealvirtualMessageWindow] Auto-closing after {elapsedTime:F1} seconds (target: {autoCloseTime})");
                    Close();
                }
            }
        }
        
        private void OnGUI()
        {
            // Always draw header with logo - it's standard
            DrawHeader();
            EditorGUILayout.Space(15);
            
            // Draw custom content
            onGUICallback?.Invoke(this);
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10); // Add top margin
            EditorGUILayout.BeginHorizontal(GUILayout.Height(50));
            GUILayout.Space(10);
            
            if (logo != null)
            {
                if (animateLogo)
                {
                    float rotation = animationTime * rotationSpeed;
                    var matrix = GUI.matrix;
                    var iconRect = GUILayoutUtility.GetRect(50, 50, GUILayout.Width(50), GUILayout.Height(50));
                    var pivotPoint = new Vector2(iconRect.center.x, iconRect.center.y);
                    GUIUtility.RotateAroundPivot(rotation, pivotPoint);
                    GUI.DrawTexture(iconRect, logo);
                    GUI.matrix = matrix;
                }
                else
                {
                    GUILayout.Label(logo, GUILayout.Width(50), GUILayout.Height(50));
                }
                GUILayout.Space(10);
            }
            
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label(headerTitle, titleStyle);
            
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        //! Shows a simple OK dialog with a message.
        //! @param message The message to display
        //! @param windowTitle The window title (default: "realvirtual")
        //! @param headerTitle Optional header title (if null, uses windowTitle)
        //! @param animationDuration Duration for logo animation in seconds (0 = no animation)
        //! @return The created window instance
        public static RealvirtualMessageWindow ShowOK(string message, string windowTitle = "realvirtual", string headerTitle = null, float animationDuration = 0f)
        {
            return Show(windowTitle, 500, 150, animationDuration > 0, (window) =>
            {
                EditorGUILayout.Space(10);
                
                var messageStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
                
                EditorGUILayout.LabelField(message, messageStyle);
                
                EditorGUILayout.Space(20);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("OK", GUILayout.Width(100), GUILayout.Height(35)))
                {
                    window.Close();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Auto-resize window based on actual content
                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    float contentHeight = lastRect.yMax + 5;
                    window.AutoResize(contentHeight);
                }
            }, animationDuration: animationDuration, autoCloseTime: 0f, headerTitle: headerTitle);
        }
        
        //! Shows a success dialog with green checkmarks for each success item.
        //! @param headerTitle The header title to display
        //! @param successItems Array of success messages to show with checkmarks
        //! @param windowTitle The window title (default: "realvirtual")
        //! @param animationDuration Duration for logo animation in seconds (0 = no animation)
        //! @return The created window instance
        public static RealvirtualMessageWindow ShowSuccess(string headerTitle, string[] successItems, string windowTitle = "realvirtual", float animationDuration = 0f)
        {
            // Start with a reasonable initial height
            int initialHeight = 200;
            var win = Show(windowTitle, 600, initialHeight, animationDuration > 0, (window) =>
            {
                EditorGUILayout.Space(10);
                
                var messageStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
                
                var greenStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    normal = { textColor = new Color(0.2f, 0.8f, 0.2f) }
                };
                
                foreach (var item in successItems)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20); // Left indent
                    EditorGUILayout.LabelField("✓", greenStyle, GUILayout.Width(20));
                    EditorGUILayout.LabelField(item, messageStyle);
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(20);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("OK", GUILayout.Width(100), GUILayout.Height(35)))
                {
                    window.Close();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Auto-resize window based on actual content
                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    float contentHeight = lastRect.yMax + 5; // Add small padding
                    window.AutoResize(contentHeight);
                }
            }, animationDuration: animationDuration, autoCloseTime: 0f, headerTitle: headerTitle);
            
            return win;
        }
        
        //! Shows an error dialog with red-colored error message.
        //! @param errorMessage The error message to display
        //! @param windowTitle The window title (default: "Error")
        //! @param headerTitle Optional header title (if null, uses windowTitle)
        //! @param animationDuration Duration for logo animation in seconds (0 = no animation)
        //! @return The created window instance
        public static RealvirtualMessageWindow ShowError(string errorMessage, string windowTitle = "Error", string headerTitle = null, float animationDuration = 0f)
        {
            return Show(windowTitle, 500, 150, animationDuration > 0, (window) =>
            {
                EditorGUILayout.Space(10);
                
                var errorStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new Color(0.8f, 0.2f, 0.2f) }
                };
                
                EditorGUILayout.LabelField("Error:", errorStyle);
                EditorGUILayout.Space(5);
                
                var messageStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 11
                };
                EditorGUILayout.LabelField(errorMessage, messageStyle);
                
                EditorGUILayout.Space(20);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("OK", GUILayout.Width(100), GUILayout.Height(35)))
                {
                    window.Close();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Auto-resize window based on actual content
                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    float contentHeight = lastRect.yMax + 5;
                    window.AutoResize(contentHeight);
                }
            }, animationDuration: animationDuration, autoCloseTime: 0f, headerTitle: headerTitle);
        }
        
        //! Shows an information dialog with a message.
        //! @param message The information message to display
        //! @param windowTitle The window title (default: "Information")
        //! @param headerTitle Optional header title (if null, uses windowTitle)
        //! @param animationDuration Duration for logo animation in seconds (0 = no animation)
        //! @return The created window instance
        public static RealvirtualMessageWindow ShowInfo(string message, string windowTitle = "Information", string headerTitle = null, float animationDuration = 0f)
        {
            return Show(windowTitle, 500, 150, animationDuration > 0, (window) =>
            {
                EditorGUILayout.Space(10);
                
                var infoStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
                
                EditorGUILayout.LabelField(message, infoStyle);
                
                EditorGUILayout.Space(20);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("OK", GUILayout.Width(100), GUILayout.Height(35)))
                {
                    window.Close();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Auto-resize window based on actual content
                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    float contentHeight = lastRect.yMax + 5;
                    window.AutoResize(contentHeight);
                }
            }, animationDuration: animationDuration, autoCloseTime: 0f, headerTitle: headerTitle);
        }
    }
}
#endif