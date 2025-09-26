// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace realvirtual
{
    public class TestHelper : MonoBehaviour
    {
        private string initialScenePath;
        private string sceneloaded;


        protected void Prepare(string Testname)
        {
            LogAssert.ignoreFailingMessages = true;
            initialScenePath = SceneManager.GetActiveScene().path;
#if REALVIRTUAL
            Global.RuntimeInspectorEnabled = false;
#endif
        }

        protected void Finish()
        {
            //SceneManager.LoadScene(initialScenePath);
#if REALVIRTUAL
            Global.RuntimeInspectorEnabled = true;
#endif

        }



        protected IEnumerator LoadScene(string scene)
        {
            sceneloaded = scene;

            var loadSceneOperation = SceneManager.LoadSceneAsync(scene);
            loadSceneOperation.allowSceneActivation = true;

            while (!loadSceneOperation.isDone)
                yield return null;
        }
    }
}
#endif
