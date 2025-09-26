
using UnityEngine;
using UnityEngine.EventSystems;
using Plane = UnityEngine.Plane;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace realvirtual
{
    //! Controls touch interaction during game4automation simulation / play mode
    public class TouchInteraction : MonoBehaviour
    {
        [Header("Settings")] 
        public bool SimulateMouseWithTouch = false;
       
        [Header("Status")] [ReadOnly] public bool IsTouching;
        [ReadOnly] public bool IsBeginPhase;
        [ReadOnly] public bool IsEndPhase;
        [ReadOnly] public bool IsStationary;
        [ReadOnly] public bool IsTwoFinger;
        [ReadOnly] public bool IsDoubleTap;
        [ReadOnly] public bool IsOverUI;
        [ReadOnly] public Vector2 StartTouchPos;
        [ReadOnly] public Vector2 TouchPos;
        [ReadOnly] public Vector2 TouchDeltaPos; //! Delta position of touch between Frames
        [ReadOnly] public Vector2 TouchTotalDeltaPos; //! Total delta position of touch since start
        [ReadOnly] public Vector2 TwoFingerMiddlePos;
        [ReadOnly] public Vector2 TwoFingerMiddleDeltaPos;
        [ReadOnly] public float TwoFingerDeltaDistance;
        [ReadOnly] public Vector2 FirstTouch;
        [ReadOnly] public Vector2 SecondTouch;
        [ReadOnly] public Vector2 ThirdTouch;
        [ReadOnly] public float DPIScale = 1;

        private Vector2 _firstbefore;
        private Vector2 _secondbefore;
        private Vector2 _firstdeltapos;
        private Vector2 _seconddeltapos;
        private int _tapcount;
        private float _doubleTapTimer;
        private Vector3 mousebottomraycastpos;
        private Vector3 mousebottomraycastposbefore;
        private Vector2 lasttouchpos;
        private Vector2 lasttwofingermiddlepos;
        private float lasttwofingerdistance;
        private bool touchstarted;
        public delegate void OneTouchRotDelegate(Vector2 pos, Vector3 pan);

        public OneTouchRotDelegate oneTouchRotEvent;
        public delegate void TwoTouchPanZoomDelegate(Vector2 pos, Vector3 pan, float zoom, float rot);

        public TwoTouchPanZoomDelegate twoTouchPanZoomDelegate;
        public delegate void DoubleTouchDelegate(Vector2 pos);

        public DoubleTouchDelegate doubleTouchDelegate;

        private Camera mycamera;

        private int touchcount = 0;

        void Awake()
        {
            mycamera = GetComponent<Camera>();
            DPIScale = 144/Screen.dpi;
            Input.simulateMouseWithTouches = SimulateMouseWithTouch;
        }

        Vector3 RayCastToBottom()
        {
            var pos = FirstTouch;
            if (Input.touchCount > 1)
            {
                // Middle betwen first and second touch
                pos = (Vector3.Lerp(FirstTouch, SecondTouch, 0.5f));
            }

            Ray ray = mycamera.ScreenPointToRay(pos);
            Plane plane = new Plane(mycamera.transform.forward, Vector3.zero);
            // raycast from mouseposition to this plane
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        public void ResetTouchDeltaPos()
        {
            TouchTotalDeltaPos = Vector2.zero;
            StartTouchPos = TouchPos;
        }


        // Update is called once per frame
        void Update()
        {
          
            
            if (Input.touchCount > 0)
                IsTouching = true;
            else
                IsTouching = false;
            IsBeginPhase = false;

            if (!IsTouching && touchstarted)
            {
                IsEndPhase = true;
                touchstarted = false;
            }
            else
            {
                if (IsTouching && !touchstarted)
                {
                    touchstarted = true;
                    IsBeginPhase = true;
                }
                else
                {
                    IsBeginPhase = false;
                }

                if (IsEndPhase && !touchstarted)
                {
                    IsEndPhase = false;
                }

                if (IsTouching)
                {
                    Touch touch = Input.GetTouch(0);
                    // check if touch is over ui
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        IsOverUI = true;
                    }
                }
                else
                {
                    IsOverUI = false;
                }
            }


            if (Input.touchCount > 0)
            {
                if (IsBeginPhase)
                {
                    lasttouchpos = Vector2.zero;
                    StartTouchPos = Input.GetTouch(0).position;
                }
            }
            else
            {
                StartTouchPos = Vector2.zero;
                TouchTotalDeltaPos = Vector2.zero;
            }

            if (IsTouching)
            {
                TouchTotalDeltaPos = Input.GetTouch(0).position - StartTouchPos;
            }
            

            // Two Finger?
            if (Input.touchCount > 1)
            {
               // Debug.Log(Input.GetTouch(0).position + " " + Input.GetTouch(1).position);
               var delta = Input.GetTouch(0).position - Input.GetTouch(1).position;
               // Debug.Log(delta.magnitude);
               // if (delta.magnitude > 20.0f )
               // {
                if (!IsTwoFinger)
                    lasttwofingermiddlepos = Vector2.zero;
                IsTwoFinger = true;
               // }
               // else
               // {
               //     IsTwoFinger = false;
               // }
                
            }
            else
            {
                IsTwoFinger = false;
            }

            IsStationary = false;
            if (Input.touchCount > 0)
                if (Input.GetTouch(0).phase == TouchPhase.Stationary)
                    IsStationary = true;
            

            // Double touch
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _tapcount++;
            }

            if (_tapcount > 0)
            {
                _doubleTapTimer += Time.deltaTime;
            }

            //Double Tap Detected
            if (_tapcount >= 2)
            {
                _doubleTapTimer = 0.0f;
                _tapcount = 0;
                IsDoubleTap = true;
                if (doubleTouchDelegate != null)
                {
                    doubleTouchDelegate(Input.GetTouch(0).position);
                }
            }
            else
            {
                IsDoubleTap = false;
            }

            if (_doubleTapTimer > 0.5f)
            {
                _doubleTapTimer = 0f;
                _tapcount = 0;
            }
            
            touchcount = Input.touchCount;
           
            if (touchcount == 1)
            {
                Touch First = Input.GetTouch(0);
                FirstTouch = First.position;
                _firstdeltapos = First.deltaPosition;
            }

            if (IsTwoFinger)
            {
                Touch First = Input.GetTouch(0);
                Touch Second = Input.GetTouch(1);
                FirstTouch = First.position;
                _firstdeltapos = First.deltaPosition;
                SecondTouch = Second.position;
                _seconddeltapos = Second.deltaPosition;
                TwoFingerMiddlePos = (FirstTouch + SecondTouch) / 2.0f;
                if (lasttwofingermiddlepos == Vector2.zero)
                {
                    lasttwofingermiddlepos = TwoFingerMiddlePos;
                    lasttwofingerdistance = (FirstTouch - SecondTouch).magnitude;
                }
                var distance = (FirstTouch - SecondTouch).magnitude;
                   
                TwoFingerMiddleDeltaPos = TwoFingerMiddlePos - lasttwofingermiddlepos;
                TwoFingerDeltaDistance = distance - lasttwofingerdistance;
                lasttwofingermiddlepos = TwoFingerMiddlePos;
                lasttwofingerdistance = distance;
            }
            else
            {
                TwoFingerMiddlePos = Vector2.zero;
                TwoFingerMiddleDeltaPos = Vector2.zero;
                lasttwofingermiddlepos = Vector2.zero;
                TwoFingerDeltaDistance = 0;
                lasttwofingerdistance = 0;
            }

            if (touchcount == 3)
            {
                Touch First = Input.GetTouch(0);
                Touch Second = Input.GetTouch(1);
                Touch Third = Input.GetTouch(2);
                FirstTouch = First.position;
                _firstdeltapos = First.deltaPosition;
                SecondTouch = Second.position;
                _seconddeltapos = Second.deltaPosition;
                ThirdTouch = Third.position;
            }

            TouchPos = FirstTouch;
            if (lasttouchpos == Vector2.zero)
                lasttouchpos = TouchPos;
            TouchDeltaPos = TouchPos - lasttouchpos;
            
            if (IsEndPhase)
            {
                lasttouchpos = Vector2.zero;
                TouchDeltaPos = Vector2.zero;
            }
               
            lasttouchpos = TouchPos;

        }
    }
}