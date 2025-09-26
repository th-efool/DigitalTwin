using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace realvirtual
{
    public class MuRecordingHandle
    {
        public MuRecorder MuRecorder;
        public int Frame = 0;
        public int Currentindex;
        public Hashtable idMU;
        public Hashtable MUid;
    }

    [CreateAssetMenu(fileName = "MuRecording", menuName = "realvirtual/Add MURecording", order = 1)]
    public class MuRecording : ScriptableObject
    { 
        [Serializable]
        public class Snapshot
        {
           public int Frame;
           public int MUlocalID;
           public Vector3 Position;
           public Vector3 Rotation;
        }
      
        public List<Snapshot> Takes;
        [ReadOnly] public List<MU> newMus = new List<MU>();
        [ReadOnly] public int NumberFrames;
        [ReadOnly] public int NumberMus;
        [ReadOnly] public int ReplayCounter = 0;
        
       [HideInInspector]public int _currentFrame = 0;
        private Hashtable _playIDMU = new Hashtable();
        private int _lastID = 0;
         public MuRecordingHandle NewRecording(MuRecorder muRecorder)
        {
            var MURechandle= new MuRecordingHandle();
            MURechandle.idMU= new Hashtable();
            MURechandle.MUid= new Hashtable();
            Takes= new List<Snapshot>();
            MURechandle.Frame = 0;
            return MURechandle;
        }
         
         public void OnMUCreated(MuRecordingHandle recordingHandle,MU mu)
         {
             if (mu.ID > NumberMus)
             {
                 recordingHandle.idMU.Add(mu.ID,mu);
                 recordingHandle.MUid.Add(mu, mu.ID);
                 NumberMus = mu.ID;
             }
         }

         public void OnMuDeleted(MuRecordingHandle recordingHandle, MU mu)
         {
             recordingHandle.idMU.Remove(mu.ID);
             recordingHandle.MUid.Remove(mu);
         }
         public void RecordMUs(MuRecordingHandle recordingHandle)
         {
             recordingHandle.Frame++;
             NumberFrames=recordingHandle.Frame;
             // copy content from hashtable to list
                foreach (DictionaryEntry entry in recordingHandle.idMU)
                {
                    var mu = (MU) entry.Value;
                    var snapshot = new Snapshot();
                    snapshot.Frame = recordingHandle.Frame;
                    snapshot.MUlocalID = mu.ID;
                    snapshot.Position = mu.transform.position;
                    snapshot.Rotation = mu.transform.eulerAngles;
                    Takes.Add(snapshot);
                }
         }

         public bool PrepareReplay()
         {
             var checkIO = true;
             _lastID = 0;
             ReplayCounter = 0;
             _currentFrame = 0;
            if (Takes.Count==0)
                checkIO= false;
             return checkIO;
         }
        
        public void PlayNextFrame(Source muSource)
        {
            _currentFrame++;
            if (ReplayCounter > Takes.Count)
                return;
            
            var loop = true;
            MU mu;
            while (loop && Takes[ReplayCounter].Frame <= _currentFrame)
            {
                var snap = Takes[ReplayCounter];
                if (Takes[ReplayCounter].MUlocalID > _lastID)
                {
                    mu = muSource.Generate();
                    _playIDMU.Add(mu.ID,mu);
                    _lastID = mu.ID;
                }
                else
                {
                    mu = (MU)_playIDMU[Takes[ReplayCounter].MUlocalID];
                }
                if (mu)
                {
                    mu.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    mu.transform.position = new Vector3(snap.Position.x, snap.Position.y, snap.Position.z);
                    mu.transform.eulerAngles = new Vector3(snap.Rotation.x, snap.Rotation.y, snap.Rotation.z);
                }
                
                ReplayCounter++;
                if(ReplayCounter>= Takes.Count)
                    loop = false;
            }
            
            
        }
        
    }
}
