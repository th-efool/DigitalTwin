
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NaughtyAttributes;
namespace realvirtual
{
    public class DrivesRecorder : realvirtualBehavior
    {
        public DrivesRecording DrivesRecording;
        public bool RecordAllDrivesWithinScene = false;
        [ReadOnly] public bool Recording;
        [ReadOnly] public bool Replaying;
        public int ReplayStartFrame;
        public int ReplayEndFrame;
        public bool Loop;
        public bool RecordOnStart;
        public bool PlayOnStart;
        [ReadOnly] public int CurrentFrame;
        [ReadOnly] public int NumberFrames;
        [ReadOnly] public float CurrentSeconds;
        [ReadOnly] public float Duration;

        private RecordingHandle handle;
       
    
        [OnValueChanged("Jump")] [Range(0.0f, 100.0f)] public float JumpToPositon;

        public UnityEvent ReplayEnd;
        

        void Jump()
        {
            Replaying = false;
            Recording = false;
            handle = DrivesRecording.MoveToFrame(this,JumpToPositon);
            CurrentFrame = handle.Frame;
            NumberFrames = DrivesRecording.NumberFrames;
            CurrentSeconds = Time.fixedDeltaTime * handle.Frame;
           
        }
      
        [Button ("Start Recording")]
        public void StartRecording()
        {
            #if UNITY_EDITOR
            if (!DrivesRecording.Protected)
            {
                handle = DrivesRecording.NewRecording(this);
                Recording = true;
            }
            else
            {
                Debug.LogWarning("Recording is not possible because recording is protected!");
            }
            #endif
        }
        
        [Button ("Stop Recording")]
        public void StopRecording()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(DrivesRecording);
            Recording = false;
#endif
        }
        
        [Button ("Start Replay")]
        public void StartReplay()
        {
            handle = DrivesRecording.StartReplay(this);
            if (ReplayStartFrame > 0)
                handle = DrivesRecording.MoveToFrame(this,ReplayStartFrame);
            Replaying = true;
        }
        
        public void StartReplay(string Sequence)
        {
            ReplayStartFrame = DrivesRecording.GetSequenceStart(Sequence);
            ReplayEndFrame = DrivesRecording.GetSequenceEnd(Sequence);
            StartReplay();
        }

        public void Replay(int startframe, int endframe)
        {
            ReplayStartFrame = startframe;
            ReplayEndFrame = endframe;
            StartReplay();
        }
        
        [Button ("Stop Replay")]
        public void StopReplay()
        {
            Replaying = false;
        }
        
        
        [Button ("One Frame Forward")]
        public void OneFrameForward()
        {
            Replaying = false;
            Recording = false;
            handle = DrivesRecording.MoveToFrame(this,handle.Frame);
            CurrentFrame = handle.Frame;
            NumberFrames = DrivesRecording.NumberFrames;
            CurrentSeconds = Time.fixedDeltaTime * handle.Frame;
        }
        
        [Button ("One Frame Backward")]
        public void OneFrameBackward()
        {
            Replaying = false;
            Recording = false;
            handle = DrivesRecording.MoveToFrame(this,handle.Frame-2);
            CurrentFrame = handle.Frame;
            NumberFrames = DrivesRecording.NumberFrames;
            CurrentSeconds = Time.fixedDeltaTime * handle.Frame;
        }

        private void OnEnable()
        {
            if (DrivesRecording != null)
                Duration = Time.fixedDeltaTime * DrivesRecording.NumberFrames;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (PlayOnStart && RecordOnStart)
            {
                Debug.LogError("PlayOnStart and RecordOnStart are both true. This is not possible! Recorder is stopped.");
                return;
            }

            if (PlayOnStart)
                StartReplay();
            if(RecordOnStart)
                StartRecording();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnDestroy()
        {
            #if UNITY_EDITOR
            if (DrivesRecording != null)
                EditorUtility.SetDirty(DrivesRecording);
            #endif
        }

        private void FixedUpdate()
        {
            if (Recording)
                DrivesRecording.RecordFrame(handle);

            if (ReferenceEquals(handle, null))
                return;
            if (Replaying)
            {
                if (!DrivesRecording.PlayNextFrame(handle))
                {
                    StopReplay();
                    ReplayEnd.Invoke();
                    if (Loop) StartReplay();
                }

                if (handle.Frame > ReplayEndFrame && ReplayEndFrame > 0)
                {
                    StopReplay();
                    ReplayEnd.Invoke();
                    if (Loop) StartReplay();
                }
            }

            if (Recording || Replaying)
            {
                CurrentFrame = handle.Frame;
                NumberFrames = DrivesRecording.NumberFrames;
                Duration = Time.fixedDeltaTime * DrivesRecording.NumberFrames;
                CurrentSeconds = Time.fixedDeltaTime * handle.Frame;
            }

            if (Replaying)
                JumpToPositon = (float)CurrentFrame / (float)NumberFrames * 100;
        }
    }
}

