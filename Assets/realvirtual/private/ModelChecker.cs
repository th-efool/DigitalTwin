// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace realvirtual
{
#pragma warning disable 0414
    [InitializeOnLoad]
    //! Class to handle the creation of the realvirtual menu
    public class ModelChecker : EditorWindow
    {
        private Vector2 scrollPos;

        // create a list class with a hint string field and a link string field
        public class hint
        {
            public string header;
            public string text;
            public string link;
            public List<GameObject> objects;
        }

        // List for the hints
        public static List<hint> hints = new();

        // method for adding hints
        public static void AddHint(string header, string text, string link = "", List<GameObject> objects = null)
        {
            hints.Add(new hint { header = header, text = text, link = link, objects = objects });
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }


        // unsubscribe from the event
        private static void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Handle the transition from Play mode back to Edit mode
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Check if we have a pending ModelChecker state change from Play mode
                var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                var prefKey = "ModelCheckerEnabled_" + scenePath;
                
                if (EditorPrefs.HasKey(prefKey))
                {
                    var enabled = EditorPrefs.GetBool(prefKey);
                    var controller = FindAnyObjectByType<realvirtualController>();
                    if (controller != null)
                    {
                        controller.ModelCheckerEnabled = enabled;
                        EditorUtility.SetDirty(controller);
                    }
                    // Clean up the temporary preference
                    EditorPrefs.DeleteKey(prefKey);
                }
            }
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            // Scene opened - ModelCheckerEnabled will be loaded from the scene's serialized data
            // No action needed as the realvirtualController's field will be restored automatically
        }

        private static void OnNewSceneCreated(UnityEngine.SceneManagement.Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            // New scene created - ensure ModelChecker is enabled by default
            EditorApplication.delayCall += () => 
            {
                var controller = FindAnyObjectByType<realvirtualController>();
                if (controller != null)
                {
                    controller.ModelCheckerEnabled = true;
                    EditorUtility.SetDirty(controller);
                }
            };
        }


        [MenuItem("realvirtual/Model Checker (Alt+T) &T", false, 400)]
        public static void Init()
        {
            Check();
        }

        private static void CheckNonStaticMeshes()
        {
            var nonStaticMeshes = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.GetComponent<MeshRenderer>() != null && !go.isStatic);

            var finalmeshes = nonStaticMeshes.Where(go => go.GetComponentInParent<Drive>() == null);

            finalmeshes =
                finalmeshes.Where(go => go.GetComponentInParent<realvirtualController>() == null);

            // deleta all meshes which are children of a MU
            finalmeshes =
                finalmeshes.Where(go => go.GetComponentInParent<MU>() == null);

            // get all meshes with groups into a new list
            var nonStaticMeshesWithGroups = finalmeshes.Where(go => go.GetComponent<Group>() != null);

            // get all objects with kinematic component in scene into new list
            var allkinematics = finalmeshes.Where(go => go.GetComponent<Kinematic>() != null);
            // get all different group names
            var groupNames = allkinematics.Select(go => go.GetComponent<Group>().GetGroupName()).Distinct();

            // now go throh all nonStaticMeshesWithGroups and check if they are in a group of groupnames
            var nonStaticMeshesWithGroupsNotInGroup =
                nonStaticMeshesWithGroups.Where(go => !groupNames.Contains(go.GetComponent<Group>().GetGroupName()));


            // remove all meshes with chainalement in a parent
            finalmeshes = finalmeshes.Where(go => go.GetComponentInParent<ChainElement>() == null);


            // now delete all of nonStaticMeshesWithGroupsNotInGroup from nonStaticMeshesWithoutDrive
            finalmeshes = finalmeshes.Except(nonStaticMeshesWithGroupsNotInGroup);

            if (finalmeshes.Any())
            {
                AddHint("Non static meshes",
                    $"There are {finalmeshes.Count()} non static meshes which don't seem to move (there is no drive in a parent)." +
                    "\nSetting all non moving Gameobjects to static will increase performance.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#gameobject-static-settings",
                    finalmeshes.ToList());
            }
        }

        private static void CheckStaticMeshes()
        {
            var staticMeshes = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.GetComponent<MeshRenderer>() != null && go.isStatic);

            var staticMeshesWithDrive = staticMeshes.Where(go => go.GetComponentInParent<Drive>() != null);

            // remove all drives which are connected to a transport surface
            staticMeshesWithDrive =
               staticMeshesWithDrive.Where(go => go.GetComponentInParent<Drive>().GetTransportSurfaces().Count == 0);

            if (staticMeshesWithDrive.Any())
            {
                AddHint("Static Meshes at Drives",
                    $"There are {staticMeshesWithDrive.Count()} static meshes as children of Drives." +
                    "\nThis will prevent the meshes from moving.",
                    "https://doc.realvirtual.io/components-and-scripts/motion/drive", staticMeshesWithDrive.ToList());
            }
        }

        private static void HugeMeshes()
        {
            // get all objects excluding those under realvirtualController and children
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.GetComponentInParent<realvirtualController>() == null);

            // get the total number of vertices of all objects in the scene (excluding realvirtualController children)
            var totalVertices = allGameObjects
                .Where(go => go.GetComponent<MeshFilter>() != null)
                .Sum(go => go.GetComponent<MeshFilter>().sharedMesh.vertexCount);

            // loop through all objects and detect if they are bigger than 5% of the total vertices
            List<GameObject> bigobjects = new List<GameObject>();
            foreach (var go in allGameObjects)
            {
                if (go.GetComponent<MeshFilter>() != null)
                {
                    var mesh = go.GetComponent<MeshFilter>().sharedMesh;
                    if (mesh.vertexCount > totalVertices * 0.05f)
                    {
                        bigobjects.Add(go);
                    }
                }
            }

            if (bigobjects.Count > 0)
            {
                AddHint("Massive Meshes",
                    $"The scene contains {bigobjects.Count} object(s) with a significantly larger number of vertices (>5% of total) compared to others." +
                    "\nFor large-scale models, consider optimization strategies or reducing the vertex count to enhance performance."+
                    "\nFor more information you can use CADChecker (Pro) for getting more insights into the model mesh data.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#simplifying-meshes", bigobjects);
            }
        }

        private static void NumberOfMeshes()
        {
            // get all meshes in the scene into a list
            var meshes = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.GetComponent<MeshRenderer>() != null);
            
            // now delete all where the meshfilter is not enabled
            meshes = meshes.Where(go => go.GetComponent<MeshRenderer>().enabled);
            
            // get the total number of meshes in the scene
            var totalMeshes = meshes.Count();
         
            // remove from totalmeshes all Meshrenderers which are disabled
            
            
            // if total number is bigger than 1000 then write a message
            if (totalMeshes > 1000)
            {
                AddHint("Number of Meshes",
                    $"The scene contains {totalMeshes} meshes.\nConsider using the Performance Optimizer (Pro) to combine meshes and improve performance." +
                    "\nYou can also use the complexity of meshes or delete unnecessary meshes.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#performance-optimizer-only-included-in-professional-version");
            }
        }

        private static void NonSharedMaterials()
        {
            // get all meshes with non shared / instantiated materials
            var nonSharedMaterials = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go =>
                    go.GetComponent<MeshRenderer>() != null);

            // get the material assets of the materials
            var differentmaterials = nonSharedMaterials.Select(go => go.GetComponent<MeshRenderer>().sharedMaterial)
                .Distinct();


            // write the hint
            if (differentmaterials.Count() > 20)
            {
                AddHint("Many materials",
                    $"There are {differentmaterials.Count()} distinct materials in your scene." +
                    "\nTo enhance performance, consider minimizing the number of materials for example by assigning  Material Assets to the GameObjects.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance",
                    null);
            }
        }

        private static void MUsWithoutRigidbody()
        {
            // get all MUs without a rigidbody
            var musWithoutRigidbody = FindObjectsByType<MU>(FindObjectsSortMode.None)
                .Where(mu => mu.GetComponent<Rigidbody>() == null);

            if (musWithoutRigidbody.Any())
            {
                AddHint("MUs without Rigidbody",
                    $"There are {musWithoutRigidbody.Count()} MUs without a Rigidbody." +
                    "\nTo enable physics interactions, consider adding a Rigidbody component to the MUs.",
                    "https://doc.realvirtual.io/components-and-scripts/motion/mu",
                    musWithoutRigidbody.Select(mu => mu.gameObject).ToList());
            }
        }

        private static void GuidedTransport()
        {
            // Check if there are any guided transports in the scene
            var guidedTransport = FindObjectsByType<TransportGuided>(FindObjectsSortMode.None);
            
            if (!guidedTransport.Any())
                return;
            
            var mus = FindObjectsByType<MU>(FindObjectsSortMode.None);
            
            var musObjects = mus.Select(mu => mu.gameObject).ToList();
            
            var musObjectsWithoutGuidedMU = musObjects.Where(mu => mu.GetComponent<GuidedMU>() == null).ToList();
            
            if (musObjectsWithoutGuidedMU.Any())
            {
                AddHint("Guided Transport",
                    $"There are {musObjectsWithoutGuidedMU.Count} MUs without a GuidedMU component." +
                    "\nThere are Guided Transportsurfaces in the scene. \nTo enable guided transport, consider adding a GuidedMU component to the MUs.",
                    "https://doc.realvirtual.io/components-and-scripts/motion/guided-transport#prerequisites-guidedmu",
                    musObjectsWithoutGuidedMU);
            }
        }

        private static void SensorRaycastLayer()
        {
           
            var sensors = FindObjectsByType<Sensor>(FindObjectsSortMode.None);
            
            // get all raycast sensors with UseRaycast = true
            var raycastSensors = sensors.Where(sensor => sensor.UseRaycast);
            
            // check in raycast sensors if layer rvMU or rvMUSernsor are in Additionalraycastlayers
            var raycastSensorsWithoutLayer = raycastSensors.Where(sensor =>
                !sensor.AdditionalRayCastLayers.Contains("rvMU") &&
                !sensor.AdditionalRayCastLayers.Contains("rvMUSensor"));
            
            // check if the gameobject is not on a standard layer
            var raycastSensorsWithoutLayerOnStandardLayer = raycastSensorsWithoutLayer.Where(sensor =>
                sensor.gameObject.layer != LayerMask.NameToLayer("rvMU") &&
                sensor.gameObject.layer != LayerMask.NameToLayer("rvMUSensor"));
            
            // add the hint that no standard layer is defined
            if (raycastSensorsWithoutLayerOnStandardLayer.Any())
            {
                AddHint("Sensor Layer",
                    $"There are {raycastSensorsWithoutLayer.Count()} Raycast Sensors without the standard layers rvMU or rvMUSensor." +
                    "\nThis might be a custom implementation and it could be ok.\nTo enable collision Sensor detection of MUs, consider adding the layers rvMU and rvMUSensor to the AdditionalRaycastLayers.",
                    "https://doc.realvirtual.io/components-and-scripts/sensor#sensor-using-raycasts",
                    raycastSensorsWithoutLayer.Select(sensor => sensor.gameObject).ToList());
            }
        }

        private static void CheckSinkLayer()
        {
           var sinks = FindObjectsByType<Sink>(FindObjectsSortMode.None);
            
            // check if the gameobject is not on a standard layer
            var sinksWithoutLayer = sinks.Where(sink =>
                (sink.gameObject.layer != LayerMask.NameToLayer("rvSensor"))); 
            
            // add the hint
            if (sinksWithoutLayer.Any())
            {
                AddHint("Sink Layer",
                    $"There are {sinksWithoutLayer.Count()} Sinks without the standard layer rvSensir." +
                    "\nTo enable collision detection of MUs, consider using the layers rvSensor.",
                    "https://doc.realvirtual.io/components-and-scripts/sink",
                    sinksWithoutLayer.Select(sink => sink.gameObject).ToList());
            } 
        }

        private static void SensorColliderLayer()
        {
            
            // get all raycast sensors with UseRaycast = false
            var colliderSensors = FindObjectsByType<Sensor>(FindObjectsSortMode.None).Where(sensor => !sensor.UseRaycast);
            
            // get all with gameobjects not on a standard layer
            var colliderSensorsWithoutLayer = colliderSensors.Where(sensor =>
                sensor.gameObject.layer != LayerMask.NameToLayer("rvSensor"));
            
            // add the hint
            if (colliderSensorsWithoutLayer.Any())
            {
                AddHint("Sensor Layer",
                    $"There are {colliderSensorsWithoutLayer.Count()} Collider Sensors without the standard layer rvSensor." +
                    "\nTo enable collision Sensor detection of MUs, consider using the layer rvSensor.",
                    "https://doc.realvirtual.io/components-and-scripts/sensor#sensor-using-colliders",
                    colliderSensorsWithoutLayer.Select(sensor => sensor.gameObject).ToList());
            }
        }

        private static void ComplexColliders()
        {
            // get all mesh colliders in the scene
            var meshColliders = FindObjectsByType<MeshCollider>(FindObjectsSortMode.None);

            if (meshColliders.Count() > 20)
            {
                // add the hint
                AddHint("Complex Colliders",
                    $"There are {meshColliders.Count()} Mesh Colliders in the scene." +
                    "\nTo enhance performance, consider using less or simpler colliders like Box Colliders.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#colliders",
                    meshColliders.Select(mc => mc.gameObject).ToList());
            }
        }
        
        private static void ManyColliders()
        {
            // get all mesh colliders in the scene
            var meshColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);

            if (meshColliders.Count() > 300)
            {
                // add the hint
                AddHint("Many Colliders",
                    $"There are {meshColliders.Count()}  Colliders in the scene." +
                    "\nTo enhance performance, consider using less colliders if possible.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#colliders",
                    meshColliders.Select(mc => mc.gameObject).ToList());
            }
        }

        private static void CollidersOnNonStandardLayers()
        {
            // get all mesh colliders in the scene which are not on the layers rvMU, rvMUSensor, rvTransport and rvSensor
            var meshColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(collider =>
                collider.gameObject.layer != LayerMask.NameToLayer("rvMU") &&
                collider.gameObject.layer != LayerMask.NameToLayer("rvMUSensor") &&
                collider.gameObject.layer != LayerMask.NameToLayer("rvTransport") &&
                collider.gameObject.layer != LayerMask.NameToLayer("rvSelection") &&
                collider.gameObject.layer != LayerMask.NameToLayer("rvSensor"));
            
            // add the hint
            if (meshColliders.Any())
            {
                AddHint("Collider Layer",
                    $"There are {meshColliders.Count()} Colliders in the scene which are not on the standard layers." +
                    "\nFor good performance collission detections it is recommended to use standard layers",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#colliders",
                    meshColliders.Select(mc => mc.gameObject).ToList());
            }
        }

        private static void NumberOfLights()
        {
            // implement a method to check the number of lights in the scene and to warn if there are more than 3
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            
            // add the hint
            if (lights.Count() > 2)
            {
                AddHint("Number of Lights",
                    $"There are {lights.Count()} Lights in the scene." +
                    "\nTo enhance performance, consider using less lights if possible.\nTurning off shadows also improves performance.",
                    "https://doc.realvirtual.io/advanced-topics/improving-performance#lights-and-shadows",
                    lights.Select(l => l.gameObject).ToList());
            }
        }

        private static void PerformChecks()
        {
            CheckNonStaticMeshes();
            CheckStaticMeshes();
            HugeMeshes();
            NumberOfMeshes();
            NonSharedMaterials();
            MUsWithoutRigidbody();
            GuidedTransport();
            SensorRaycastLayer();
            SensorColliderLayer();
            CheckSinkLayer();
            ComplexColliders();
            ManyColliders();
            NumberOfLights();
            CollidersOnNonStandardLayers();
        }

        public static void Check()
        {
            hints.Clear();

            AddHint("Model Checker",
                "The Modelchecker performs a pre-scene check to identify common issues and suggest performance optimizations.\nYou can enable or disable it in the realvirtualController.",
                "https://doc.realvirtual.io");

            PerformChecks();

            if (hints.Count == 1)
                AddHint("Check Finished", "Congratulation, there is no issue in your current scene.", "");
            else
            {
                AddHint("Check Finished", $"There are {hints.Count - 1} issues in your current scene.", "");
            }

            if (hints.Count > 1)
            {
                var window =
                    (ModelChecker)GetWindow(typeof(ModelChecker));

                window.Show();
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            EditorGUILayout.Separator();
            foreach (var hint in hints)
            {
                EditorGUILayout.LabelField(hint.header, EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                var textContent = new GUIContent(hint.text);
                GUI.skin.label.wordWrap = true;
                var size = GUI.skin.label.CalcHeight(textContent, position.width * 0.8f);
                EditorGUILayout.LabelField(textContent, GUILayout.MaxWidth(position.width * 0.8f),
                    GUILayout.Height(size), GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (hint.objects != null && hint.objects.Count > 0 && GUILayout.Button("Show ", GUILayout.Width(50)))
                {
                    EditorApplication.delayCall += () => Selection.objects = hint.objects.ToArray();
                }

                if (!string.IsNullOrEmpty(hint.link) && GUILayout.Button("Info", GUILayout.Width(50)))
                    Application.OpenURL(hint.link);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }

            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Update Check"))
            {
                Check();
            }

            // Check if enabled if not show button for enable

            // get the realvirtualController
            var controller = FindAnyObjectByType<realvirtualController>();
            if (controller != null)
            {
                if (controller.ModelCheckerEnabled)
                {
                    if (GUILayout.Button("Disable checks for current scene"))
                    {
                        controller.ModelCheckerEnabled = false;
                        
                        if (Application.isPlaying)
                        {
                            // In Play mode, save to EditorPrefs for later restoration in Edit mode
                            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                            var prefKey = "ModelCheckerEnabled_" + scenePath;
                            EditorPrefs.SetBool(prefKey, false);
                        }
                        else
                        {
                            // In Edit mode, mark the controller as dirty to save to scene
                            EditorUtility.SetDirty(controller);
                        }
                        Close();
                    }
                }
                else
                {
                    if (GUILayout.Button("Enable checks for current scene"))
                    {
                        controller.ModelCheckerEnabled = true;
                        
                        if (Application.isPlaying)
                        {
                            // In Play mode, save to EditorPrefs for later restoration in Edit mode
                            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                            var prefKey = "ModelCheckerEnabled_" + scenePath;
                            EditorPrefs.SetBool(prefKey, true);
                        }
                        else
                        {
                            // In Edit mode, mark the controller as dirty to save to scene
                            EditorUtility.SetDirty(controller);
                        }
                    }
                }
            }


            if (GUILayout.Button("Close"))
            {
                Close();
            }
            
            // make a distance
            EditorGUILayout.Separator();


        }
    }
}
#endif