
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace realvirtual
{

    public struct RaycasterResult
    {
        public bool Hit;
        public GameObject HitObject;
        public bool IsTouched;
        public Vector3 HitPoint;
    }

    public interface IRaycaster
    {
        public bool IsOnUIElement();
        
        public RaycasterResult SceneRaycast(LayerMask layermask);

        public Vector3 RaycastToHorizontalPlane(float planeheight);

        public Vector3 RaycastToParallelViewPlane(Vector3 position);

        public List<RaycastResult> UIRaycast();

        public bool PointerOutsideGame();


    }
}