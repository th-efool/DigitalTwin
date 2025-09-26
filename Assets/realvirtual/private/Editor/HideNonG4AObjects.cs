// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2024 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  


using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace realvirtual
{
    [InitializeOnLoad]
    public static class HideNonG4AObjects
    {
        static HideNonG4AObjects()
        {
            Selection.selectionChanged += OnSelectionChange;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnSelectionChange()
        {
        }

        private static void OnHierarchyChanged()
        {
        }


        private static void Hide(GameObject gameobject, bool hide)
        {
            var components = gameobject.GetComponents<Component>();
            foreach (var component in components)
                if (component != null)
                {
                    var ctype = component.GetType();
                    var subclass = ctype.IsSubclassOf(typeof(realvirtualBehavior));
                    if (!subclass)
                    {
                        if (ctype != typeof(Transform))
                        {
                            if (hide)
                                component.hideFlags = HideFlags.HideInInspector;
                            else
                                component.hideFlags = HideFlags.None;
                        }
                    }
                    else
                    {
                        ((realvirtualBehavior)component).HideNonG44Components = hide;
                    }
                }

            if (hide)
            {
                var texture = (Texture2D)UnityEngine.Resources.Load("Icons/Icon48");
                SetIcon(gameobject, texture);
            }
            else
            {
                ClearIcon(gameobject);
            }
        }

        public static void ClearIcon(GameObject gObj)
        {
            SetIcon(gObj, null);
        }

        private static void SetIcon(GameObject gObj, Texture2D texture)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { gObj, texture });
        }

        [MenuItem("CONTEXT/Component/realvirtual/Only G4A")]
        public static void ShowOnlyG4AComponents(MenuCommand command)
        {
            var gameobject = command.context;
            var obj = (Component)gameobject;
            Hide(obj.gameObject, true);
        }

        [MenuItem("CONTEXT/Component/realvirtual/Show all")]
        public static void ShowAllComponents(MenuCommand command)
        {
            var gameobject = command.context;
            var obj = (Component)gameobject;
            Hide(obj.gameObject, false);
        }
    }
}