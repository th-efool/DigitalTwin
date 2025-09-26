// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#if REALVIRTUAL && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
#if REALVIRTUAL_TESTS
using NUnit.Framework;
using UnityEngine.TestTools;
#endif
using UnityEngine;
using UnityEditor;
using realvirtual;
namespace Tests
{
    public class UnityTestAllScenes: TestHelper
    {

        private List<string> scenestotest;
        private List<SceneDataList> scenetestlists;
        private SceneDataList currentscenetestlist;
        private string currentscene;
        
        #if REALVIRTUAL_TESTS
        [SetUp]
        public void Setup()
        { 
            Application.logMessageReceived += HandleLog;
           Prepare("[[rv TESTING]]  Setting up all Scenes in TestModels");
           
           scenestotest = new List<string>();
           scenetestlists = new List<SceneDataList>();
           var testScenes = Resources.LoadAll<SceneDataList>("Testing");
           // debug all testscenes
           foreach (var testScene in testScenes)
           {
               foreach (var scenedata in testScene.scenes)
               {
                   // skip scenes that are regexskipped or that are not set to be tested
                     if (scenedata.regexskipped || !scenedata.runScene)
                     {
                          continue;
                     }
                   scenestotest.Add(scenedata.targetSceneAsset.name);
                   scenetestlists.Add(testScene);
               }
           }
        }
 
        [TearDown]
        public void TearDown()
        {
            Finish();
            Application.logMessageReceived -= HandleLog;
           
        }
        
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                Debug.Log($"[rv TESTING] Caught an error: {logString}\nStack Trace: {stackTrace}");
                // add the error to the current scene
                currentscenetestlist.scenes.Find(x => x.targetSceneAsset.name == currentscene).testErrors.Add(logString);
            }
        }
        
        
        [UnityTest]
        [Timeout(80000)]
        public IEnumerator TestAllScenes()
        {
            var scenetestlistnumber = 0;
            LogAssert.ignoreFailingMessages = true;
            bool alltestspassed = true;
            foreach (var scene in scenestotest)
            {
                scenetestlists[scenetestlistnumber].SceneStarting(scene);
                currentscene = scene;
                currentscenetestlist = scenetestlists[scenetestlistnumber];
           
                yield return LoadScene(scene);
                LogAssert.Expect(LogType.Exception, "test");
                // get the TestConfig
                var testconfig = GameObject.FindObjectOfType<TestModelController>();
      
                if (testconfig == null)
                {
                    Debug.Log("[rv TESTING] ERROR - TestConfig not found in Scene " + scene +  " - Skipping Test");
                    continue;
                }
                
                var timetotest = testconfig.TestTime;
                
                testconfig.PrepareTest();
                yield return null;
                int expectedcans = 1;
                float time = 0;
                while (time < timetotest)
                {
                    time += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
                
                // check the scene tests
                var tests = testconfig.AreTestsPassed();
                scenetestlists[scenetestlistnumber].SceneTestsPassed(scene,tests);
                if (tests.Count == 1)
                {
                    Debug.Log("[rv TESTING] PASSED - Test Passed for Scene " + scene);   
                }
                else
                {
                    Debug.Log("[rv TESTING] FAILED - Test Failed for Scene " + scene);
                    alltestspassed = false;
                }

                scenetestlistnumber++;


            }
        

            if (!alltestspassed)
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
            
        }
        
#endif
    }

}
#endif
