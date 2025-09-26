// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{


#if UNITY_EDITOR
    using realvirtual;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.TestTools.TestRunner.Api;

    [InitializeOnLoad]

    public static class TestRunner

    {

        private static TestRunnerApi _runner = null;
        private static TestCallbacks _callbacks;

        // delegate for test finished
        public delegate void TestsFinished();

        public static TestsFinished EventTestsFinished;

        static TestRunner()
        {
            _runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callbacks = new TestCallbacks();
            _callbacks.onTestsFinished += OnTestsFinished;
            _runner.RegisterCallbacks(_callbacks);
        }

        private static void OnTestsFinished()
        {
#if REALVIRTUAL_DEV
            ProjectBuilder.OnTestsFinished();
#endif
        }


        // add a pointer to a method when test is finished

        public static void RunEditorTests()
        {
            Filter filter = new Filter()
            {
                testMode = TestMode.PlayMode
            };
            _runner.Execute(new ExecutionSettings(filter));
        }

        public static void RunBuildTests()
        {

        }
    }
#endif
}

