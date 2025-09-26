// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#pragma warning disable 0414    
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

namespace realvirtual
{
    public class TestModelController : MonoBehaviour
    {
        [InfoBox("This is just for internal realvirtual.io Development and Test automation. For using REALVIRTUAL_TEST Scripting Define Symbol needs to be added", EInfoBoxType.Warning)]
        public bool RunTest = true;
        public int TimeScale = 7;
        public float TestTime = 33;
        public float FixedTimeStep = 0.02f;
        private bool manualtest = false;
        
        [ReadOnly] public bool AllSceneTestsPassed = false;
    
        #if REALVIRTUAL_TESTS
        // Button to run the test
        [Button("Run Test")]
        #if UNITY_EDITOR
        public void RunTestButton()
        {
            if (RunTest)
            {
                // start this scene in playmode
                if (!Application.isPlaying)
                {
                    // Save it in the eidtorprefs that this scene is playing
                    EditorPrefs.SetBool("rvManualTest", true);
                    EditorApplication.isPlaying = true;
                }
               
            }
        }
        #endif
        
        
        // Start is called before the first frame update
        public void PrepareTest()
        {
            // get the realvirtualcontroller 
            var controller = GameObject.FindObjectOfType<realvirtualController>();
            controller.TimeScale = TimeScale;
            controller.ChangedTimeScale();
            // change the fixed timestep
            Time.fixedDeltaTime = FixedTimeStep;
            // now call all itestprepare
            ITestPrepare[] testprepares = FindObjectsOfType<MonoBehaviour>(true).OfType<ITestPrepare>().ToArray();
            foreach (var test in testprepares)
            {
                test.Prepare();
            }
        }

        public List<string> AreTestsPassed()
        {
            var results = new List<string>();
            var passed = 0;
            var total = 0;
            AllSceneTestsPassed = true;
            // now call all itestcheck
            ITestCheck[] testchecks = FindObjectsOfType<MonoBehaviour>(true).OfType<ITestCheck>().ToArray();
            foreach (var test in testchecks)
            {
                total++;
                var testresult = test.Check();
                if (testresult != "")
                         results.Add(testresult);
                else
                    passed++;
            }
            
            // no test defined
            if (testchecks.Length == 0)
            {
                results.Add("No tests defined, please check");
            }
            
            // Add a message about the number of the tests and the result
            if (results.Count == 0)
            {
                results.Add($"PASSED {passed} of {total} ");
            }
            else
            {
                results.Add($"FAILED {passed} of {total} passed - {total-passed} failed");
                AllSceneTestsPassed = false;
            }
            
            return results;
        }

        public void Start()
        {
            #if UNITY_EDITOR
            // check if started manually
            manualtest = EditorPrefs.GetBool("rvManualTest", false);
            if (manualtest)
            {
                Invoke("PrepareTest",0);
            }
            // set rvmanualtest to false
            EditorPrefs.SetBool("rvManualTest", false);
            #endif
        }
#if UNITY_EDITOR
         void FixedUpdate()
         {
            // if manual test check if stopping is needed
            
            if (manualtest)
            {
                if (Time.fixedTime>TestTime)
                {
                    EditorApplication.isPlaying = false;
                    var result = AreTestsPassed();
                    Debug.Log("Test Results: ");
                    foreach (var res in result)
                    {
                        Debug.Log(res);
                    }
                    manualtest = false;
                }
            }
        }
         
#endif
#endif
    }
}

