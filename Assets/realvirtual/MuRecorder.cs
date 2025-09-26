using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace realvirtual
{


    public class MuRecorder : realvirtualBehavior
    {
        public MuRecording MuRecording;
        public bool Record = true;
        public bool Replay = false;
        
        [ReadOnly] public bool IsRecording = false;
        [ReadOnly] public bool IsReplaying = false;
        
        
        private Source MuSource;
        private int CurrentFrame;
        private MuRecordingHandle MuRecordingHandle;

        public new void Awake()
        {
            MuSource = this.gameObject.GetComponent<Source>();
            if (MuSource == null)
            {
                Debug.LogError("No Source found on " + this.gameObject.name);
            }
            // register this object in source event
            MuRecording.NumberMus = 0;
            MuSource.EventMUCreated.RemoveListener(OnMUCreated);
            MuSource.EventMUCreated.AddListener(OnMUCreated);
            if (!MuSource.OnCreateDestroyComponents.Contains("realvirtual.MuRecorder"))
            {
                MuSource.OnCreateDestroyComponents.Add("realvirtual.MuRecorder");
            }
            base.Awake();

        }

        public void Start()
        {
            if (Replay && Record)
            {
                Debug.LogError("Replay and Recording can not be active at the same time");
                return;
            }
            if (Record)
            {
                MuSource.PositionOverwrite = false;
                MuRecordingHandle = MuRecording.NewRecording(this);
                IsRecording = true;
            }

            if (Replay)
            {
                if(!MuRecording.PrepareReplay())
                {
                    Debug.LogError("No Recording found");
                    return;
                }
                else
                {
                    MuSource.PositionOverwrite = true;
                    IsReplaying = true;
                }
                
            }
        }

        public void OnMUCreated(MU mu)
        {
            if (Record)
            {
                MuRecording.OnMUCreated(MuRecordingHandle,mu);
                mu.EventMUDeleted.AddListener(OnMuDeleted);
            }
        }
        public void OnMuDeleted(MU mu)
        {
            if (Record)
            {
                MuRecording.OnMuDeleted(MuRecordingHandle,mu);
            }
        }

        private void FixedUpdate()
        {
            if (Replay && Record)
            {
                Debug.LogError("Replay and Recording can not be active at the same time");
                return;
            }
            if (Record)
            {
                MuRecording.RecordMUs(MuRecordingHandle);
            }

            if (Replay)
            {
                if(MuRecording.Takes.Count== 0)
                {
                    Debug.LogError("No Recording found");
                    return;
                }
                if( MuRecording.ReplayCounter < MuRecording.Takes.Count )
                {
                    MuRecording.PlayNextFrame(MuSource);
                }
                else
                {
                    IsReplaying = false;
                }
               
            }
        }
    }
}