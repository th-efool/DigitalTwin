using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace realvirtual
{
    using NaughtyAttributes;
    using UnityEngine;

    [RequireComponent(typeof(SignalTracer))]
    public class TestDriveRecordings : MonoBehaviour,ITestCheck,ITestPrepare
    {
  
        // An serializable class for sensors and counters on each sensor
        public float ToleranceMM = 1;
        public bool DoTestTrace = false;
        
        // a list of drive positions
        [InfoBox("This is just for internal realvirtual.io Development and Test automation", EInfoBoxType.Warning)]
        [ReorderableList] public List<Drive> Drives = new List<Drive>();
        
        // a hash table for all drives with the path
        private Hashtable driveids = new Hashtable();
        
        private SignalTracer signalTracer;

        private float timestart;
        
        // a button to get all sensors in the model and create a sensorcounter for each
        [Button("Get All Drives")]
        public void GetAllDrives()
        {

            Drives = FindObjectsByType<Drive>(FindObjectsSortMode.None).ToList();
        }
        
        [Button("Save Traces as reference")]
        public void SaveTracesAsReference()
        {
            signalTracer = GetComponent<SignalTracer>();
            signalTracer.CopyTracesToReference("_Reference");
        }

        

         void FixedUpdate()
        {
            if (!DoTestTrace) return;
            // check all drive positions
            foreach (var drive in Drives)
            {
               // get id of drive
               var driveid = driveids[drive].ToString();
               var time = Time.fixedTime - timestart;
               signalTracer.TraceValue(driveid, time, drive.CurrentPosition);
            }
        }

        public string Check()
        {
            var result = "";
            
            // check all drive position in the signaltaces
            foreach (var drive in Drives)
            {
                // get id of drive
                var driveid = driveids[drive].ToString();
                var reference = driveid + "_Reference";
                var diff = signalTracer.CompareCurves(driveid, reference, ToleranceMM);
                if (diff > ToleranceMM)
                {
                    if (result != "")
                    {
                        result += "\n";
                    }
                    // add this with a linefeed
                    result += "Drive position at " + drive.name + " is different to reference by " + diff;
                }
            }
            
            return result;
        }

        public void Prepare()
        {
            DoTestTrace = true;
            signalTracer = GetComponent<SignalTracer>();
            timestart = Time.fixedTime;
            // create hash table for all drives
            foreach (var drive in Drives)
            {
                var driveid = Global.GetPath(drive.gameObject);
                driveids.Add(drive,driveid);
                signalTracer.InitValue(driveid);
            }
        }
    }

}