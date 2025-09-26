// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if UNITY_EDITOR
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

#if REALVIRTUAL_TESTS
namespace realvirtual
{
    public class TestWindow : EditorWindow
    {
        TestCallbacks testCallbacks = new TestCallbacks();
        private bool isrunning;
        private static string regexTests = "";
        private static string regexBuilds = "";

        [MenuItem("realvirtual/Testing", false, 504)]
        public static void ShowWindow()
        {
            // get the regex from editor prefs
            regexTests = EditorPrefs.GetString("TestWindowRegexFilter", "");
            regexBuilds = EditorPrefs.GetString("TestWindowRegexBuilds", "");
            GetWindow<TestWindow>("Testing");
        }


        public static void StartTests(string mode)
        {
            // get all the scenes from the scriptable objects of type TestScenes
            Debug.Log("[rv TESTING] Starting Tests");
            // get all scriptable objects of type TestScenes
            var testScenes = UnityEngine.Resources.LoadAll<SceneDataList>("Testing");

            // debug all testscenes
            foreach (var testScene in testScenes)
            {
                testScene.Reset();
                foreach (var scenedata in testScene.scenes)
                {

                    Debug.Log("[rv TESTING] Preparing Test for Scene " + scenedata.targetSceneAsset.name);
                    // Retrieve the path of the scene to add
                    var scene = scenedata.targetSceneAsset;
                    var scenePath = AssetDatabase.GetAssetPath(scene);
                    var name = testScene.name + " : " + scenedata.targetSceneAsset.name;

                    if (regexTests != "")
                        if (!Regex.IsMatch(name, regexTests))
                        {
                            scenedata.regexskipped = true;
                            Debug.Log("[rv TESTING] Skipping Test for Scene " + scenedata.targetSceneAsset.name);
                            continue;
                        }

                    scenedata.regexskipped = false;
                    // Get the current list of scenes in the build settings
                    var scenes = EditorBuildSettings.scenes;

                    // Check if the scene is already in the build settings
                    var sceneAlreadyExists = scenes.Any(s => s.path == scenePath);

                    if (!sceneAlreadyExists)
                    {
                        // If the scene is not already in the build settings, add it
                        var sceneToAdd = new EditorBuildSettingsScene(scenePath, true);
                        ArrayUtility.Add(ref scenes, sceneToAdd);
                        EditorBuildSettings.scenes = scenes;
                    }
                }
            }

            // Save the regex in editor settings
            EditorPrefs.SetString("TestWindowRegexFilter", regexTests);
            if (mode == "editor")
                TestRunner.RunEditorTests();
        }

        private void DisplayTestResults()
        {
            // get all testScenes
            var testScenes = UnityEngine.Resources.LoadAll<SceneDataList>("Testing");
            foreach (var testScene in testScenes)
            {

                foreach (var scenedata in testScene.scenes)
                {
                    var name = testScene.name + " : " + scenedata.targetSceneAsset.name;
                    if (regexTests != "")
                        if (!Regex.IsMatch(name, regexTests))
                            continue;

                    // Determine button color based on test status
                    if (scenedata.testToBeDone)
                    {
                        GUI.backgroundColor = Color.white; // Standard color for tests to be done
                    }
                    else if (scenedata.testRunning)
                    {
                        GUI.backgroundColor = Color.yellow; // Yellow color for running tests
                    }
                    else
                    {
                        // Existing logic for passed/failed tests
                        if (scenedata.testPassed)
                            GUI.backgroundColor = Color.green;
                        else
                            GUI.backgroundColor = Color.red;
                    }

                    if (GUILayout.Button(name))
                    {
                        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenedata.targetSceneAsset));
                    }

                    // Reset background color for other GUI elements
                    GUI.backgroundColor = Color.white;

                    // Display test results
                    foreach (var result in scenedata.testResults)
                    {
                        GUILayout.Label(result);
                    }

                    // Display test errors in red text
                    var errorStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };

                    foreach (var error in scenedata.testErrors)
                    {
                        GUILayout.Label(error, errorStyle);
                    }
                }
            }
        }

        private void DisplayBuilds()
        {
#if REALVIRTUAL_DEV
            // get all buildconfigs and make a button for each and display if build successfull and buildmessage
            var buildconfigs = UnityEngine.Resources.LoadAll<BuildConfig>("BuildConfigs");
            foreach (var buildconfig in buildconfigs)
            {
                if (buildconfig.DoBuild)
                {
                    buildconfig.RegexSkip = true;
                    var buildname = buildconfig.target.ToString() + ": " + buildconfig.scenes.First().name;
                    if (regexBuilds != "")
                        if (!Regex.IsMatch(buildname, regexBuilds))
                            continue;
                    buildconfig.RegexSkip = false;
                    if (buildconfig.BuildSuccessfull)
                        GUI.backgroundColor = Color.green;
                    else if (buildconfig.BuildError)
                        GUI.backgroundColor = Color.red;
                    else
                        GUI.backgroundColor = Color.white;
                    if (GUILayout.Button(buildconfig.target.ToString() + ": " + buildconfig.scenes.First().name))
                    {
                        EditorSceneManager.OpenScene(buildconfig.scenes.First().name);
                    }

                    GUI.backgroundColor = Color.white;
                    if (buildconfig.BuildMessage != "")
                        GUILayout.Label(buildconfig.BuildMessage);
                }
            }
#endif
        }

        void OnGUI()
        {
            // get regex from editor prefs
            regexTests = EditorPrefs.GetString("TestWindowRegexFilter", "");
            // Results in a scrollbar if needed
            GUILayout.BeginScrollView(Vector2.zero);
            // Start horizontal layout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Test scenes", EditorStyles.boldLabel);
            var newregexFilter = EditorGUILayout.TextField("Filter:", regexTests);

            // save filter if it Textfiled is changed
            if (newregexFilter != regexTests)
            {
                regexTests = newregexFilter;
                EditorPrefs.SetString("TestWindowRegexFilter", regexTests);
            }

            GUILayout.EndHorizontal();
            DisplayTestResults();
            // regex for buildsö
#if REALVIRTUAL_DEV
            GUILayout.BeginHorizontal();
            regexBuilds = EditorPrefs.GetString("TestWindowRegexBuilds", "");
            GUILayout.Label("Builds", EditorStyles.boldLabel);
            var newregexBuilds = EditorGUILayout.TextField("Filter:", regexBuilds);
            // save filter
            if (regexBuilds != newregexBuilds)
            {
                regexBuilds = newregexBuilds;
                EditorPrefs.SetString("TestWindowRegexBuilds", regexBuilds);
            }

            GUILayout.EndHorizontal();
            DisplayBuilds();
#endif
            GUILayout.EndScrollView();

            // separating space
            GUILayout.Space(30);

            if (GUILayout.Button("Run Tests ", GUILayout.Height(30)))
            {
                StartTests("editor");
            }
#if REALVIRTUAL_DEV
            GUILayout.Space(10);


            if (GUILayout.Button("Import realvirtual Professional into empty Unity Project"))
            {
                ProjectBuilder.CreateProject(false, false, "Professional");
            }

            if (GUILayout.Button("Import realvirtual Starter into empty Unity Project"))
            {
                ProjectBuilder.CreateProject(false, false, "Starter");
            }

            if (GUILayout.Button("Import realvirtual Professional into empty Unity Project and Import Simulation"))
            {
                ProjectBuilder.CreateProject(false, false, "Simulation");
            }

            if (GUILayout.Button("Import realvirtual Starter into empty Unity Project"))
            {
                ProjectBuilder.CreateProject(false, false, "Starter");
            }

            if (GUILayout.Button("Import realvirtual Professional into empty Unity Project and Run Tests"))
            {
                ProjectBuilder.CreateProject(true, false, "Professional");
            }

            if (GUILayout.Button("Import realvirtual Professional into empty Unity Project, Test & Build"))
            {
                ProjectBuilder.CreateProject(true, true, "Professional");
            }

            if (GUILayout.Button("Test & Build"))
            {
                ProjectBuilder.TestAndBuild();
            }



            if (GUILayout.Button("Build "))
            {
                ProjectBuilder.Build();
            }
#endif
        }
    }
}
#endif
#endif
