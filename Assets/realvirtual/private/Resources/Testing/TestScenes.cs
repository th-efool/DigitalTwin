// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
#if UNITY_EDITOR
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.VersionControl;
    using UnityEngine.SceneManagement;

    [CreateAssetMenu(fileName = "Add TestScenes", menuName = "realvirtual/Test Scenes", order = 2)]
    public class SceneDataList : ScriptableObject
    {
        public List<SceneData> scenes;

        public void SceneTestsPassed(string scene, List<string> tests)
        {
            foreach (var scenetest in scenes)
            {
                if (scenetest.targetSceneAsset.name == scene)
                {
                    scenetest.testResults = tests;
                    scenetest.testPassed = tests.Count == 1 && scenetest.testErrors.Count == 0;
                    scenetest.testRunning = false;
                }
            }

            // set dirty
            EditorUtility.SetDirty(this);
        }

        public void SceneStarting(string scene)
        {
            foreach (var scenetest in scenes)
            {
                if (scenetest.targetSceneAsset.name == scene)
                {
                    scenetest.testRunning = true;
                    scenetest.testToBeDone = false;
                    scenetest.testErrors.Clear();
                }
            }

            EditorUtility.SetDirty(this);
        }

        public void Reset()
        {
            foreach (var scenetest in scenes)
            {

                scenetest.testRunning = false;
                scenetest.testToBeDone = true;
                scenetest.testErrors.Clear();
                scenetest.testPassed = false;
                scenetest.regexskipped = false;
                scenetest.regexskipped = false;
                scenetest.testResults.Clear();

            }

            EditorUtility.SetDirty(this);
        }
    }



    [System.Serializable]
    public class SceneData
    {
        public UnityEditor.SceneAsset targetSceneAsset; // For runtime, use the scene name as the reference
        public bool runScene;
        [HideInInspector] public bool testToBeDone = false;
        [HideInInspector] public bool testRunning = false;
        [HideInInspector] public bool testPassed = false;
        [HideInInspector] public bool regexskipped = false; // if all tests passed
        public List<string> testResults = new List<string>(); // list of test results if not passed (string)
        public List<string> testErrors = new List<string>(); // list of test errors if not passed (string)


    }
#endif
}
