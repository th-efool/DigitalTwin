// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace realvirtual
{
    // Traditional dockable window version
    public class QuickEditWindow : EditorWindow
    {
        private VisualElement root;
        private VisualElement buttonContainer;
        
        // Reuse the same static variables and methods from QuickEditOverlay
        private static QuickEditOverlay sharedLogic = new QuickEditOverlay();
        
        // [MenuItem("Window/realvirtual/Quick Edit Window")] // Moved to main realvirtual menu
        public static void ShowWindow()
        {
            var window = GetWindow<QuickEditWindow>("Quick Edit");
            window.minSize = new Vector2(250, 300);
            window.Show();
        }
        
        private void CreateGUI()
        {
            // Get the root visual element
            root = rootVisualElement;
            root.style.paddingTop = 5;
            root.style.paddingBottom = 5;
            root.style.paddingLeft = 5;
            root.style.paddingRight = 5;
            
            // Create the content similar to overlay
            CreateWindowContent();
            
            // Subscribe to events
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnSelectionChanged()
        {
            UpdateWindowContent();
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                QuickEditOverlay.drives = Global.GetAllSceneComponents<Drive>();
            }
            UpdateWindowContent();
        }
        
        private void OnEditorUpdate()
        {
            // Update displays similar to overlay
            if (Application.isPlaying && buttonContainer != null)
            {
                // Find and update time/speed displays
                var labels = buttonContainer.Query<Label>().ToList();
                foreach (var label in labels)
                {
                    if (label.name == "timeLabel")
                        label.text = Time.time.ToString("0.0");
                }
            }
            else if (buttonContainer != null)
            {
                // Update jog button states in edit mode
                sharedLogic.buttonContainer = buttonContainer;
                sharedLogic.UpdateJogButtonStates();
            }
        }
        
        private void CreateWindowContent()
        {
            root.Clear();
            
            // Set black background for window mode
            root.style.backgroundColor = new Color(0, 0, 0, 1);
            
            // Load the stylesheet
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/realvirtual/private/Editor/QuickEdit/QuickEdit.uss");
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);
            
            // Create centered container
            var centerContainer = new VisualElement();
            centerContainer.style.flexGrow = 1;
            centerContainer.style.alignItems = Align.Center;
            centerContainer.style.justifyContent = Justify.FlexStart;
            root.Add(centerContainer);
            
            // Create main content container with fixed width
            var contentWrapper = new VisualElement();
            contentWrapper.style.width = 251; // Same as overlay width
            contentWrapper.AddToClassList("quickedit-root");
            contentWrapper.AddToClassList("quickedit-window-mode");
            centerContainer.Add(contentWrapper);
            
            // Create button container
            buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("quickedit-button-container");
            contentWrapper.Add(buttonContainer);
            
            // Call shared logic to create content
            sharedLogic.LoadIcons();
            sharedLogic.buttonContainer = buttonContainer;
            
            // Create modified content with overlay button
            UpdateWindowContentWithOverlayButton();
        }
        
        private void UpdateWindowContentWithOverlayButton()
        {
            // Just update content - the buttons already have the correct text from IsWindowMode()
            sharedLogic.UpdateContent();
        }
        
        private void UpdateWindowContent()
        {
            if (buttonContainer != null && sharedLogic != null)
            {
                sharedLogic.buttonContainer = buttonContainer;
                UpdateWindowContentWithOverlayButton();
            }
        }
    }
}
#endif