using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// create a struct with the following fields: boolen for hit, gameobject, if it is touched and position

namespace realvirtual
{
    public class MouseAndTouchRaycaster : MonoBehaviour, IRaycaster
    {
        public string UILayer = "UI"; //!<the layer for the UI - selection will be disabled if UI is touched
        public bool EnableToch = true; //!<enable touch input

        [ReadOnly] public bool IsTouched = false;
        private int uilayer; //!<the layer for the UI - selection will be disabled if UI is touched 
        private RaycastHit GObject;

        // IRaycaster implementation
        public bool IsOnUIElement()
        {
            return IsPointerOverUIElement();
        }


        public RaycasterResult SceneRaycast(LayerMask layermask)
        {

            var ray = GetRay();
            if (Physics.Raycast(ray, out GObject, Mathf.Infinity, layermask))
            {
                return new RaycasterResult()
                {
                    Hit = true,
                    HitObject = GObject.transform.gameObject,
                    IsTouched = IsTouched,
                    HitPoint = GObject.point
                };
            }
            else
            {
                return new RaycasterResult()
                {
                    Hit = false,
                    HitObject = null,
                    IsTouched = IsTouched,
                    HitPoint = Vector3.zero
                };
            }
        }

        public List<RaycastResult> UIRaycast()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }

        public Vector3 RaycastToHorizontalPlane(float planeheight)
        {
            var ray = GetRay();
            // Define a plane with the given normal and point
            float distance;
            var plane = new Plane(Vector3.up, new Vector3(0, planeheight, 0));

            // Perform a raycast to the defined plane
            if (plane.Raycast(ray, out distance))
            {
                // If the ray hits the plane, set nomousebottomhit to false and return the hit point
                return ray.GetPoint(distance);
            }

            // If the ray does not hit the plane, set nomousebottomhit to true and return Vector3.zero
            return Vector3.zero;
        }

        public Vector3 RaycastToParallelViewPlane(Vector3 position)
        {
            var ray = GetRay();

            float distance;
            var plane = new Plane(Camera.main.transform.forward, position);
            // Perform a raycast to the defined plane
            if (plane.Raycast(ray, out distance))
            {
                // If the ray hits the plane, set nomousebottomhit to false and return the hit point
                return ray.GetPoint(distance);
            }

            // If the ray does not hit the plane, set nomousebottomhit to true and return Vector3.zero
            return Vector3.zero;
        }

        public bool PointerOutsideGame()
        {
            var view = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            return view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
        }



        // private methods

        // GetRay for mouse and touch - change needed for VR,AR
        private Ray GetRay()
        {
            Vector2 rayposition = Vector2.zero;

            // First do touch if enabled and touched
            if (EnableToch && Input.touchCount == 1)
            {
                Touch touch = new Touch();
                touch = Input.GetTouch(0);
                rayposition = touch.position;
                IsTouched = true;
            }
            else // otherwise
            {
                rayposition = Input.mousePosition;
                IsTouched = false;
            }

            return Camera.main.ScreenPointToRay(rayposition);
        }

        private void Awake()
        {
            uilayer = LayerMask.NameToLayer(UILayer);
        }

        private bool IsPointerOverUIElement()
        {
            var results = UIRaycast();
            return IsPointerOverUIElement(results);
        }

        private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            var isoverui = EventSystem.current.IsPointerOverGameObject();

            return isoverui;

        }

    }

}
