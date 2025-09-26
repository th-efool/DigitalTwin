// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;
using System.Threading;
using System;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/custom-interfaces")]
    public class InterfaceThreadedBaseClass : InterfaceBaseClass
    {
        [FormerlySerializedAs("MinCommCycleMs")]
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        public int MinUpdateCycle = 8;
        
        // First thread
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        public int CommCycleMeasures= 1000;
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        [ReadOnly] public int CommCycleNr;
        [HideIf("NoThreading")]
        [Foldout("Thread Status")]
        [ReadOnly] public int CommTimeMs;
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        [ReadOnly] public int CommTimeMin;
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        [ReadOnly] public float CommTimeMed;
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        [ReadOnly] public int CommTimeMax;
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        [ReadOnly] public int CommCycleMeasureNum;
        [HideIf("NoThreading")]
        [Foldout("Thread Status")]
        [ReadOnly] public int UpdateCycleMs;
        [HideIf("NoThreading")] 
        [Foldout("Thread Status")]
        [ReadOnly] public string ThreadStatus; 
       
        
        // Second thread
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        public int MinUpdateCycle2 = 8;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public int CommCycleNr2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public int CommTimeMs2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public int CommTimeMin2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public float CommTimeMed2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public int CommTimeMax2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public int CommCycleMeasureNum2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public int UpdateCycleMs2;
        [ShowIf("TwoThreads")] 
        [Foldout("Second Thread Status")]
        [ReadOnly] public string ThreadStatus2; 
        
        private Thread CommThread;
        private Thread SecondThread;
        private DateTime ThreadTime;
        private DateTime ThreadTime2;
        private bool run;
        private float commtimesum = 0;
        private float commtimesum2 = 0;
        private DateTime last;
        private DateTime end;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource cancellationTokenSource2;
        
        [HideInInspector] public bool NoThreading = false;
        [HideInInspector] public bool TwoThreads = false;
        protected virtual void CommunicationThreadUpdate()
        {
        }
        
        
        protected virtual void CommunicationThreadClose()
        {
        }
        
        protected virtual void SecondCommunicationThreadUpdate()
        {
        }
        
        
        protected virtual void SecondCommunicationThreadClose()
        {
        }

        public override void OpenInterface()
        {
            if (realvirtualController!=null)
                if (realvirtualController.DebugMode)
                    Debug.Log ("Opening Interface " + this.name);
            if (NoThreading)
            {
                ThreadStatus = "Threading turned off";
                return;
            }

            ThreadStatus = "running";
            cancellationTokenSource = new CancellationTokenSource();
            CommThread = new Thread(() => CommunicationThread(cancellationTokenSource.Token));
            if (realvirtualController!=null)
                if (realvirtualController.DebugMode)
                    Debug.Log ("Started Interface Thread " + this.name);
            if (TwoThreads)
            {
                cancellationTokenSource2 = new CancellationTokenSource();
                SecondThread = new Thread(() => SecondCommunicationThread(cancellationTokenSource2.Token));
                ThreadStatus2 = "running";
            }
            else
            {
                ThreadStatus2 = "no second thread";
                SecondThread = null;
            }
                
            CommCycleNr = 0;
            run = true;
            ResetMeasures();
            CommThread.Start();
            if (TwoThreads)
                SecondThread.Start();
        }

        public override void CloseInterface()
        {
            if (realvirtualController!=null)
                if (realvirtualController.DebugMode)
                    Debug.Log ("Stopping Interface Thread " + this.name);
            run = false;
            if (CommThread!=null)
                   CommThread.Abort();
            if (SecondThread!=null)
                SecondThread.Abort();
        }

        private void ResetMeasures()
        {
            CommCycleMeasureNum = 0;
            CommTimeMin = 99999;
            CommTimeMax = 0;
            commtimesum = 0;
            
            CommCycleMeasureNum2 = 0;
            CommTimeMin2 = 99999;
            CommTimeMax2 = 0;
            commtimesum2 = 0;
        }
        
        void SecondCommunicationThread(CancellationToken token)
        {
            DateTime end, start;
            bool first = true;
            do
            {
                start = DateTime.Now;
                CommCycleMeasureNum2++;
                SecondCommunicationThreadUpdate();
                ThreadTime2 = last;
                CommCycleNr2++;
                end = DateTime.Now;
                TimeSpan span = end - start;
                last = DateTime.Now;
                if (!first)
                {
                    CommTimeMs2 = (int) span.TotalMilliseconds;

                    // Calculate Communication Statistics
                    commtimesum2 = commtimesum + CommTimeMs2;
                    if (CommTimeMs2 > CommTimeMax2)
                        CommTimeMax2 = CommTimeMs2;
                    if (CommTimeMs2 < CommTimeMin2)
                        CommTimeMin2 = CommTimeMs2;
                    CommTimeMed2 = commtimesum2 / CommCycleMeasureNum2;
                    if (CommCycleMeasureNum2 > CommCycleMeasures)
                        ResetMeasures();
                }

                first = false;

                if (MinUpdateCycle2-CommTimeMs2>0)
                    Thread.Sleep(MinUpdateCycle2-CommTimeMs2);
                TimeSpan updatecycle = DateTime.Now - start;
                UpdateCycleMs2 = (int) updatecycle.TotalMilliseconds;
                
                if (token.IsCancellationRequested)
                {
                    break;
                }
                
            } while (run == true);
            SecondCommunicationThreadClose();
        }
        
        void CommunicationThread(CancellationToken token)
        {
            DateTime end, start;
            bool first = true;
            do
            {
                start = DateTime.Now;
                CommCycleMeasureNum++;
                CommunicationThreadUpdate();
                ThreadTime = last;
                CommCycleNr++;
                end = DateTime.Now;
                TimeSpan span = end - start;
                last = DateTime.Now;
                if (!first)
                {
                    CommTimeMs = (int) span.TotalMilliseconds;

                    // Calculate Communication Statistics
                    commtimesum = commtimesum + CommTimeMs;
                    if (CommTimeMs > CommTimeMax)
                        CommTimeMax = CommTimeMs;
                    if (CommTimeMs < CommTimeMin)
                        CommTimeMin = CommTimeMs;
                    CommTimeMed = commtimesum / CommCycleMeasureNum;
                    if (CommCycleMeasureNum > CommCycleMeasures)
                        ResetMeasures();
                }

                first = false;

                if (MinUpdateCycle-CommTimeMs>0)
                    Thread.Sleep(MinUpdateCycle-CommTimeMs);
                TimeSpan updatecycle = DateTime.Now - start;
                UpdateCycleMs = (int) updatecycle.TotalMilliseconds;
                
                if (token.IsCancellationRequested)
                {
                    break;
                }
                
            } while (run == true);
            CommunicationThreadClose();
            
        }

    }
}