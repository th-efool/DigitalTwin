// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    


using UnityEngine;
using System;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using System.Collections.Generic;
using System.Linq;


namespace realvirtual
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class Global
    {
        // Global Variables
        public static bool RuntimeInspectorEnabled = true;

        public static string Version = "";
        public static string Release = "";
        public static string Build = "";
        public static realvirtualController realvirtualcontroller; // Game4Automation Controller of last Scene playing
        public static bool g4acontrollernotnull = false;
        public static bool QuickEditDisplay = true;

        #region CheckTools
        
        // check for a given list of drives if all drives have no limits set and that they are the min number and that they are not null
        public static bool CheckDrives(Drive[] drives,string CheckedBy, int minnumber)
        {
            foreach (var drive in drives)
            {
                if (drive == null)
                {
                    Debug.LogError (CheckedBy + " - Drive is null");
                    return false;
                }

                if (drive.UseLimits)
                {
                    Debug.LogError (CheckedBy + " - Drive has limits set - this is not allowed");
                }
               
            }
            
            // check number of drives
            if (drives.Length < minnumber)
            {
                Debug.LogError (CheckedBy + " - Number of Drives is less than " + minnumber);
                return false;
            }

            return true;
        }
        
        #endregion
        #region Selection Tools

        public static string GetPath(GameObject go)
        {
            string path = "/" + go.name;
            var obj = go;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }

            return path;
        }

        public static List<String> GetGroups()
        {
            var groups = GetAllSceneComponents<Group>();
            var Groups = new List<string>();
#if UNITY_EDITOR
            foreach (Group group in groups)
            {
                if (EditorUtility.IsPersistent(group.transform.root.gameObject))
                    continue;
                if (!Groups.Contains(group.GroupName))
                    Groups.Add(group.GroupName);
            }

            Groups.Sort();
#endif
            return Groups;
        }

        public static List<GameObject> GetAllWithGroup(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(Group));
#if UNITY_EDITOR
            foreach (var comp in groupcomps)
            {
                var gr = (Group) comp;
                if (EditorUtility.IsPersistent(gr.transform.root.gameObject))
                    continue;
                if (gr.GroupName == group)
                    list.Add(gr.gameObject);
            }
#endif
            return list;
        }


        public static List<GameObject> GetAllWithGroupIncludingSub(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(Group));
#if UNITY_EDITOR
            foreach (var comp in groupcomps)
            {
                var gr = (Group) comp;
                if (EditorUtility.IsPersistent(gr.transform.root.gameObject))
                    continue;
                if (gr.GroupName == group)
                    list.Add(gr.gameObject);
            }
#endif
            foreach (var go in list.ToArray())
            {
                var children = go.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (!list.Contains(child.gameObject))
                        list.Add(child.gameObject);
                }
            }

            return list;
        }


        public static T GetRootComponent<T>() where T : Component
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);
            foreach (var obj in rootObjects)
            {
                var com = obj.GetComponent<T>();
                if (com != null)
                {
                    return com;
                }
            }

            return null;
        }

        public static List<GameObject> GetAllPrefabsAtPathWithComponent<T>(string Folder) where T : Component
        {
            List<GameObject> results = new List<GameObject>();
#if UNITY_EDITOR
            var res = AssetDatabase.FindAssets("t: Prefab", new[] {Folder});

            foreach (var guid in res)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset.GetComponent<T>() != null)
                    results.Add(asset);
            }

#endif
            return results;
        }


        public static List<T> GetAllSceneComponents<T>() where T : Component
        {
            object[] objs = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(T));
            var comps = new List<T>();
#if UNITY_EDITOR
            foreach (T obj in objs)
            {
                if (EditorUtility.IsPersistent(obj.transform.root.gameObject))
                    continue;

                comps.Add(obj);
            }
#endif
            return comps;
        }


        public static bool DestroyObjectsByComponent<T>(GameObject parent) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>();
            bool deleted = false;
            for (var i = 0; i < firstList.Length; i++)
            {
                try
                {
                    Object.DestroyImmediate(firstList[i].gameObject);
                    deleted = true;
                }
                catch
                {
                }
            }

            return deleted;
        }

        // Get Child GameObject by Name from a defined Parent
        public static GameObject GetGameObjectByName(string name, GameObject parent = null)
        {
            GameObject go;

            if (parent != null)
            {
                var transform = parent.transform.Find(name);
                if (transform != null)
                {
                    go = transform.gameObject;
                    return go;
                }
            }
            else
            {
                go = GameObject.Find("/" + name);
                if (go != null)
                {
                    return go;
                }
            }

            return null;
        }

        public static GameObject AddGameObjectIfNotExisting(string name, GameObject parent = null)
        {
            GameObject go;

            if (parent != null)
            {
                var transform = parent.transform.Find(name);
                if (transform != null)
                {
                    go = transform.gameObject;
                    return go;
                }
            }
            else
            {
                go = GameObject.Find("/" + name);
                if (go != null)
                {
                    return go;
                }
            }

            go = new GameObject();
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            if (go.transform.parent != null)
                go.transform.parent = parent.transform;
            return go;
        }

        public static T AddComponentIfNotExisting<T>(GameObject gameobject) where T : Component
        {
            T comp = null;
            comp = gameobject.GetComponent<T>();
            if (comp == null)
            {
                comp = gameobject.AddComponent<T>();
            }

            return comp;
        }

        public static T DestroyComponents<T>(GameObject gameobject) where T : Component
        {
            var firstList = gameobject.GetComponents<T>();

            for (var i = 0; i < firstList.Length; i++)
            {
                try
                {
                    Object.DestroyImmediate(firstList[i]);
                }
                catch
                {
                }
            }

            return null;
        }

        public static T GetComponentByName<T>(GameObject parent, string name) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);

            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i] != null)
                    if (firstList[i].gameObject.name == name)
                    {
                        return firstList[i];
                    }
            }

            return null;
        }

        public static List<T> GetComponentsByName<T>(GameObject parent, string name) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);
            List<T> reslist = new List<T>();
            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].gameObject.name == name)
                {
                    reslist.Add(firstList[i]);
                }
            }

            return reslist;
        }

        public static T GetComponentAlsoInactive<T>(GameObject parent) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);
            if (firstList.Length > 0)
                return firstList[0];

            return null;
        }

        public static List<T> GetComponentsAlsoInactive<T>(GameObject parent) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);
            List<T> reslist = new List<T>();
            for (var i = 0; i < firstList.Length; i++)
                reslist.Add(firstList[i]);
            return reslist;
        }

        public static void SetActiveIncludingSubObjects(GameObject parent, bool active)
        {
            var allchildren = GetComponentsAlsoInactive<Transform>(parent);
            foreach (var children in allchildren)
            {
                children.transform.gameObject.SetActive(active);
            }
        }


        public static void SetActiveSubObjects(GameObject parent, bool active)
        {
            var allchildren = GetComponentsAlsoInactive<Transform>(parent);
            foreach (var children in allchildren)
            {
                if (children != parent.transform)
                    children.transform.gameObject.SetActive(active);
            }
        }


        public static void SetG4AController(realvirtualController controller)
        {
            if (controller != null)
            {
                realvirtualcontroller = controller;
                g4acontrollernotnull = true;
            }
            else
            {
                realvirtualcontroller = null;
                g4acontrollernotnull = false;
            }
        }

        public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
        {
            var result = new List<System.Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
                }
            }

            return result.ToArray();
        }


#if UNITY_EDITOR
        public static void CenterOnMainWin(this UnityEditor.EditorWindow aWin)
        {
            var center = EditorGUIUtility.GetMainWindowPosition().center;

            var main = EditorGUIUtility.GetMainWindowPosition().width;
            var pos = aWin.position;
            float w = (EditorGUIUtility.GetMainWindowPosition().width - pos.width) * 0.5f;
            float h = (EditorGUIUtility.GetMainWindowPosition().height - pos.height) * 0.5f;
            pos.x = center.x + w;
            pos.y = center.y + h;
            aWin.position = pos;
        }
#endif
        public static Bounds GetTotalBounds(GameObject root)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            ;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }


        public static void MovePositionKeepChildren(GameObject root, Vector3 deltalocalposition)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.localPosition = root.transform.localPosition + deltalocalposition;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }

        public static void MoveRotationKeepChildren(GameObject root, Quaternion deltarotation)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.localRotation = root.transform.localRotation * deltarotation;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }

        public static void SetPositionKeepChildren(GameObject root, Vector3 globalposition)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.position = globalposition;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }

        public static void SetRotationKeepChildren(GameObject root, Quaternion rotation)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.rotation = rotation;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }

        public static Vector3 GetTotalCenter(GameObject root)
        {
            var bounds = GetTotalBounds(root);
            return bounds.center;
        }

        public static Object[] GatherObjects(GameObject root)
        {
            List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
            Stack<GameObject> recurseStack = new Stack<GameObject>(new GameObject[] {root});

            while (recurseStack.Count > 0)
            {
                GameObject obj = recurseStack.Pop();
                objects.Add(obj);
                if (obj != null)
                    foreach (Transform childT in obj.transform)
                        recurseStack.Push(childT.gameObject);
            }

            return objects.ToArray();
        }

        public static bool IsGame4AutomationTypeIncluded(GameObject target)
        {
            realvirtualBehavior[] behavior = target.GetComponentsInChildren<realvirtualBehavior>();
            var length = behavior.Length;
            if (length == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsGame4AutomationViewTypeIncluded(GameObject target)
        {
            try
            {
                realvirtualBehavior[] behavior = target.GetComponentsInChildren<realvirtualBehavior>();

                var found = 0;
                foreach (var behav in behavior)
                {
#if REALVIRTUAL_PROFESSIONAL
                    if ((behav.GetType() == typeof(Group))
                        || (behav.GetType() == typeof(CAD)))
#else
                if ((behav.GetType() == typeof(Group)))
#endif
                    {
                        found++;
                    }
                }

                var length = behavior.Length - found;
                if (length == 0)
                {
                    return false;
                }
            }
            catch
            {
            }

            return true;
        }


#if UNITY_EDITOR
        public static void SetDefine(string mydefine)
        {
            var currtarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(currtarget);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            if (!symbols.Contains(mydefine))
            {
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols + ";" + mydefine);
            }
        }       
       
        public static void DeleteDefine(string mydefine)
        {
            var currtarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(currtarget);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            if (symbols.Contains(";" + mydefine))
            {
                symbols = symbols.Replace(";" + mydefine, "");
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols);
            }
       
            if (symbols.Contains(mydefine))
            {
                symbols = symbols.Replace(mydefine, "");
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols);
            }
        }

        public static void SetAssemblyDefReference(string assemblydef, string reference, bool set)
        {
            var path = Path.Combine(Application.dataPath, assemblydef);
            string assydef = File.ReadAllText(path);
            if (set)
            {
                // already there
                if (assydef.Contains(reference))
                    return;
                var search = "\"references\": [";
                var pos = assydef.IndexOf(search) + search.Length;

                var insertvalue = "\n        \"" + reference + "\",";
                assydef = assydef.Insert(pos, insertvalue);
            }

            if (!set)
            {
                if (!assydef.Contains(reference))
                    return;
                var start = assydef.IndexOf(reference);
                var posend = assydef.IndexOf(",", start);
                var posstart = assydef.LastIndexOf("\n", posend);
                assydef = assydef.Remove(posstart, posend - posstart + 1);
            }

            File.WriteAllText(path, assydef);
        }

        public static void AddComponent(string assetpath)
        {
            GameObject component = Selection.activeGameObject;
            Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = new Vector3(0, 0, 0);
            if (component != null)
            {
                go.transform.parent = component.transform;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        }

        public static GameObject AddComponentTo(Transform transform, string assetpath)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = new Vector3(0, 0, 0);
            if (transform != null)
            {
                go.transform.parent = transform;
            }

            return go;
        }


        public static void SetVisible(GameObject target, bool isActive)
        {
            if (target.activeSelf == isActive) return;

            target.SetActive(isActive);
            EditorUtility.SetDirty(target);

            Object[] objects = GatherObjects(target);
            foreach (Object obj in objects)
            {
                GameObject go = (GameObject) obj;
                go.SetActive(isActive);
                EditorUtility.SetDirty(go);
            }

            if (Selection.objects.Length > 1)
                foreach (var obj in Selection.objects)
                {
                    if (obj.GetType() == typeof(GameObject))
                    {
                        if (obj != target)
                            SetVisible((GameObject) obj, isActive);
                    }
                }
        }

        public static void HideSubObjects(GameObject target, bool hide)
        {
            if (ReferenceEquals(realvirtualcontroller, null))
                return;

            EditorUtility.SetDirty(realvirtualcontroller);
            if (!hide)
            {
                Object[] objects = GatherObjects(target);

                foreach (Object obj in objects)
                {
                    obj.hideFlags = HideFlags.None;
                }

                SetExpandedRecursive(target, true);
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            else
            {
                SetExpandedRecursive(target, true);
                Object[] objects = GatherObjects(target);
                foreach (Object obj in objects)
                {
                    if (IsGame4AutomationViewTypeIncluded((GameObject) obj) == false && obj != target)
                    {
                        obj.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                EditorApplication.DirtyHierarchyWindowSorting();
            }
        }

        public static void SetLockObject(GameObject target, bool isLocked)
        {
            try
            {
                bool objectLockState = (target.hideFlags & HideFlags.NotEditable) > 0;
                if (objectLockState == isLocked)
                    return;

                Object[] objects = GatherObjects(target);

                if (isLocked && realvirtualcontroller != null)
                {
                    if (realvirtualcontroller.LockedObjects != null)
                        if (!realvirtualcontroller.LockedObjects.Contains(target))
                            realvirtualcontroller.LockedObjects.Add(target);
                }
                else if (realvirtualcontroller.LockedObjects != null)
                    realvirtualcontroller.LockedObjects.Remove(target);

                foreach (Object obj in objects)
                {
                    GameObject go = (GameObject) obj;
                    string undoString = string.Format("{0} {1}", isLocked ? "Lock" : "Unlock", go.name);
                    Undo.RecordObject(go, undoString);

                    // Set state according to isLocked
                    if (isLocked)
                    {
                        go.hideFlags |= HideFlags.NotEditable;
                    }
                    else
                    {
                        if (Global.g4acontrollernotnull)
                            if (realvirtualcontroller.LockedObjects != null)
                                realvirtualcontroller.LockedObjects.Remove(go);
                        go.hideFlags &= ~HideFlags.NotEditable;
                    }

                    // Set hideflags of components
                    foreach (Component comp in go.GetComponents<Component>())
                    {
                        if (comp is Transform)
                            continue;

                        Undo.RecordObject(comp, undoString);

                        if (isLocked)
                        {
                            comp.hideFlags |= HideFlags.NotEditable;
                            comp.hideFlags |= HideFlags.HideInHierarchy;
                        }
                        else
                        {
                            comp.hideFlags &= ~HideFlags.NotEditable;
                            comp.hideFlags &= ~HideFlags.HideInHierarchy;
                        }

                        EditorUtility.SetDirty(comp);
                    }

                    EditorUtility.SetDirty(go);
                    if (realvirtualcontroller != null)
                        EditorUtility.SetDirty(realvirtualcontroller);
                }
            }
            catch
            {
            }
        }

        public static void SetExpandedRecursive(GameObject go, bool expand)
        {
            System.Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            System.Reflection.MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorWindow editorWindow = EditorWindow.focusedWindow;
            methodInfo.Invoke(editorWindow, new object[] {go.GetInstanceID(), expand});
        }

#endif

        #endregion
        
        #region DebugTools

        public static void DebugDrawArrow(Vector3 position, Vector3 direction, Color color, float duration = 0,
            bool depthTest = true)
        {
            Debug.DrawRay(position, direction, color, duration, depthTest);
            DebugCone(position + direction, -direction * 0.333f, color, 15, duration, depthTest);
        }

        private static void DebugCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f,
            float duration = 0, bool depthTest = true)
        {
            Vector3 _up = up.normalized * radius;
            Vector3 _forward = Vector3.Slerp(_up, -_up, 0.5f);
            Vector3 _right = Vector3.Cross(_up, _forward).normalized * radius;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = _right.x;
            matrix[1] = _right.y;
            matrix[2] = _right.z;

            matrix[4] = _up.x;
            matrix[5] = _up.y;
            matrix[6] = _up.z;

            matrix[8] = _forward.x;
            matrix[9] = _forward.y;
            matrix[10] = _forward.z;

            Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 _nextPoint = Vector3.zero;

            color = (color == default(Color)) ? Color.white : color;

            for (var i = 0; i < 91; i++)
            {
                _nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
                _nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
                _nextPoint.y = 0;

                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

                Debug.DrawLine(_lastPoint, _nextPoint, color, duration, depthTest);
                _lastPoint = _nextPoint;
            }
        }

        public static void DebugArrow(Vector3 position, Vector3 direction, float duration = 0)
        {
            DebugDrawArrow(position, direction, Color.yellow, duration);
        }

        public static void DebugArrow(Vector3 position, Vector3 direction, Color color, float duration = 0)
        {
            DebugDrawArrow(position, direction, color, duration);
        }

        private static void DebugCone(Vector3 position, Vector3 direction, Color color, float angle = 45,
            float duration = 0, bool depthTest = true)
        {
            float length = direction.magnitude;

            Vector3 _forward = direction;
            Vector3 _up = Vector3.Slerp(_forward, -_forward, 0.5f);
            Vector3 _right = Vector3.Cross(_forward, _up).normalized * length;

            direction = direction.normalized;

            Vector3 slerpedVector = Vector3.Slerp(_forward, _up, angle / 90.0f);

            float dist;
            var farPlane = new Plane(-direction, position + _forward);
            var distRay = new Ray(position, slerpedVector);

            farPlane.Raycast(distRay, out dist);

            Debug.DrawRay(position, slerpedVector.normalized * dist, color);
            Debug.DrawRay(position, Vector3.Slerp(_forward, -_up, angle / 90.0f).normalized * dist, color, duration,
                depthTest);
            Debug.DrawRay(position, Vector3.Slerp(_forward, _right, angle / 90.0f).normalized * dist, color, duration,
                depthTest);
            Debug.DrawRay(position, Vector3.Slerp(_forward, -_right, angle / 90.0f).normalized * dist, color, duration,
                depthTest);

            DebugCircle(position + _forward, direction, color, (_forward - (slerpedVector.normalized * dist)).magnitude,
                duration, depthTest);
            DebugCircle(position + (_forward * 0.5f), direction, color,
                ((_forward * 0.5f) - (slerpedVector.normalized * (dist * 0.5f))).magnitude, duration, depthTest);
        }

        public static void DebugGlobalAxis(Vector3 position, float duration = 0, Color color = default(Color))
        {
            if (color == default(Color))
            {
                Debug.DrawRay(position, new Vector3(0.5f, 0, 0), Color.red, duration, false);
                Debug.DrawRay(position, new Vector3(0, 0.5f, 0), Color.green, duration, false);
                Debug.DrawRay(position, new Vector3(0, 0, 0.5f), Color.blue, duration, false);
            }
            else
            {
                Debug.DrawRay(position, new Vector3(0.5f, 0, 0), color, duration, false);
                Debug.DrawRay(position, new Vector3(0, 0.5f, 0), color, duration, false);
                Debug.DrawRay(position, new Vector3(0, 0, 0.5f), color, duration, false);
            }
        }

        public static void DebugGlobalPoint(Vector3 position, Color color, float duration = 0)
        {
            Debug.DrawRay(position, new Vector3(0.001f, 0, 0), color, duration, false);
        }


        public static void DebugLocalAxis(Vector3 position, float duration = 0, GameObject gameObject = null)
        {
            if (gameObject == null)
            {
                DebugGlobalAxis(position, duration);
                return;
            }

            var xaxis = gameObject.transform.right;
            var yaxis = gameObject.transform.up;
            var zaxis = gameObject.transform.forward;
            var globalpos = gameObject.transform.TransformPoint(position);
            Debug.DrawRay(globalpos, xaxis * 0.5f, Color.red, duration, false);
            Debug.DrawRay(globalpos, yaxis * 0.5f, Color.green, duration, false);
            Debug.DrawRay(globalpos, zaxis * 0.5f, Color.blue, duration, false);
        }

        #endregion

        #region VERSION

        // Get Version
        public static void IncrementVersion()
        {
#if UNITY_EDITOR
            realvirtualVersion scriptableversion =
                UnityEngine.Resources.Load<realvirtualVersion>("realvirtualVersion");
            scriptableversion.Build = scriptableversion.Build + 1;
#endif
        }

        public static void SetVersion()
        {
            realvirtualVersion scriptableversion =
                UnityEngine.Resources.Load<realvirtualVersion>("realvirtualVersion");
            if (scriptableversion != null)
            {
                Build = scriptableversion.Build.ToString();
                Build = Build.Replace("\n", "");


                Release = scriptableversion.Release;
                scriptableversion.Build = int.Parse(Build);
                Version = Release + "." + Build + " (Unity " + Application.unityVersion + ")";
            }
        }

        static Global()
        {
            Initialize();
        }

        #endregion

        #region EVENTS
        

#if UNITY_EDITOR
        // Global Events
        public static void OnSceneLoaded(Scene scene, OpenSceneMode mode)
        {
            if (realvirtualcontroller != null && realvirtualcontroller.DebugMode)
                Logger.Log($"Scene Loaded: {scene.name}");
            try
            {
                var rootobjs = scene.GetRootGameObjects();
                foreach (var rootobj in rootobjs)
                {
                    if (rootobj.GetComponent<SceneInfo>() != null)
                        rootobj.GetComponent<SceneInfo>().OnSceneLoad();
                }
            }
            catch
            {
            }
            
            // get all realvirtualbeahviors in the scene
            var behaviors = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).OfType<ISceneLoaded>();
            foreach (var behavior in behaviors)
            {
                behavior.OnSceneLoaded();
            }
        }

        // Global Events
        public static void OnSceneClosing(Scene scene, bool removing)
        {
            QuickToggle.SetGame4Automation(null);
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (g4acontrollernotnull)
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    realvirtualcontroller.OnPlayModeFinished();
                }

                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    realvirtualcontroller.OnEditModeFinished();
                    // get all realvirtualbeahviors in the scene
                    var behaviors = Object
                        .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .OfType<IEditModeFinished>();
                    foreach (var behavior in behaviors)
                    {
                        behavior.OnEditModeFinished();
                    }
                }
            }
        }

#endif

        // When Unity Is Loaded
#if !UNITY_EDITOR
      [RuntimeInitializeOnLoadMethod]
#endif
        public static void Initialize()
        {
            SetVersion();

#if UNITY_EDITOR
            EditorSceneManager.sceneOpened += OnSceneLoaded;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        #endregion

        #region EditorGUIWindowVisuals

        public static Vector3 RoundVector(Vector3 vector, int decimalPlaces)
        {
            float multiplier = Mathf.Pow(10, decimalPlaces);

            float x = Mathf.Round(vector.x * multiplier) / multiplier;
            float y = Mathf.Round(vector.y * multiplier) / multiplier;
            float z = Mathf.Round(vector.z * multiplier) / multiplier;

            return new Vector3(x, y, z);
        }
#if UNITY_EDITOR
        public static float CalculateHandleSize(Vector3 vertex, float handleScaleFactor)
        {
            Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
            float zoomFactor;

            if (sceneViewCamera.orthographic)
            {
                // For orthographic cameras, zoom factor is the orthographic size
                zoomFactor = sceneViewCamera.orthographicSize;
            }
            else
            {
                // For perspective cameras, calculate zoom factor based on field of view
                float fovRadians = sceneViewCamera.fieldOfView * Mathf.Deg2Rad;
                float distance = Vector3.Distance(sceneViewCamera.transform.position, vertex);
                zoomFactor = distance * Mathf.Tan(0.5f * fovRadians);
            }

            float handleSize = zoomFactor * handleScaleFactor;
            return handleSize;
        }
#endif

        public static Texture2D CreateTexture(Color32 color)
        {
            int width = Screen.width; // Width of the texture
            int height = Screen.height; // Height of the texture
            
            Texture2D tex = new Texture2D(width, height);
            color.a = 255;

            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = color;
            }

            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        public static Vector3 CalculateCenterpoint(Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 center = Vector3.zero;

            Vector3 u = (B - A).normalized;
            Vector3 w = Vector3.Cross(C - A, u).normalized;
            Vector3 v = Vector3.Cross(w, u).normalized;

            // Calculate the 2D coordinates of B and C in the u, v plane
            float bx = Vector3.Dot(B - A, u);
            float by = Vector3.Dot(B - A, v);

            float cx = Vector3.Dot(C - A, u);
            float cy = Vector3.Dot(C - A, v);

            // Calculate the center of the circle in 2D coordinates
            float h = ((cx - bx / 2) * (cx - bx / 2) + cy * cy - (bx / 2) * (bx / 2)) / (2 * cy);

            // Calculate the center of the circle in 3D coordinates
            center = A + (bx / 2) * u + h * v;
            
            return RoundVector(center, 3);
            
        }
        

        #endregion
        
        
    }
    
    // Started Before Build
#if UNITY_EDITOR
    class Game4AutomationBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildReport target)
        {
            Global.Initialize();
        }
    }

#endif
}



