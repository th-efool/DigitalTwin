#if UNITY_EDITOR
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace realvirtual
{
    
    public class TestCallbacks : ICallbacks
    {
        
        // make a delegate for getting informed when all tests are finished
        public delegate void OnTestsFinished();
        public OnTestsFinished onTestsFinished;
        
        public void RunStarted(ITestAdaptor testsToRun)
        {
       
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            onTestsFinished?.Invoke();
        }

        public void TestStarted(ITestAdaptor test)
        {
       
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!result.HasChildren && result.ResultState != "Passed")
            {
                Debug.Log(string.Format("[rv TESTING] TEST {0} {1}", result.Test.Name, result.ResultState));
            }
       
        }
    }
}
#endif