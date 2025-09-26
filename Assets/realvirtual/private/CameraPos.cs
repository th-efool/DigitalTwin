// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEditor;
using UnityEngine;

namespace realvirtual
{
    [CreateAssetMenu(fileName = "CameraPos", menuName = "realvirtual/Add Camera Position", order = 1)]
    //! Scriptable object for saving camera positions (user views)
    public class CameraPos : ScriptableObject
    {
        public string Description;
        public Vector3 CameraRot;
        public Vector3 TargetPos;
        public Vector3 CameraTransformPos;
        public Quaternion CameraTransformRot;
        public float CameraDistance;
        public bool OrthographicMainView = false;
        public bool OrthoViews;
        public Vector3 OrthoTransformPos;
        public Quaternion OrthoTransformRot;
        public float OrthoSize;
        public float OrthoAngle;
        public float OrthoDistance;
        public bool IsLocked = false;
        public void SaveCameraPosition(SceneMouseNavigation mousenav)
        {
            if (!IsLocked)
            {
                CameraRot = mousenav.currentRotation.eulerAngles;
                CameraDistance = mousenav.currentDistance; 
                TargetPos = mousenav.target.position;
                CameraTransformPos = mousenav.transform.position;
                CameraTransformRot = mousenav.transform.rotation;
                OrthoTransformPos = mousenav.orthoviewcontroller.transform.position;
                OrthoTransformRot = mousenav.orthoviewcontroller.transform.rotation;
                OrthoViews = mousenav.orthoviewcontroller.OrthoEnabled;
                OrthoSize = mousenav.orthoviewcontroller.Size;
                OrthoAngle = mousenav.orthoviewcontroller.Angle;
                OrthoDistance = mousenav.orthoviewcontroller.Distance;
                OrthographicMainView = mousenav.orthograhicview;
            }
            else
            {
                Debug.LogWarning("Saving camera position to " + this.name + " not possible because it is locked");
            }
            #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
            #endif
 
        }
        public void SetCameraPositionPlaymode(SceneMouseNavigation mousenav)
        {
            mousenav.currentRotation.eulerAngles = CameraRot;
            mousenav.currentDistance = CameraDistance;
            mousenav.target.position = TargetPos;
            if (mousenav.orthoviewcontroller != null)
            {
                mousenav.orthoviewcontroller.Size = OrthoSize;
                mousenav.orthoviewcontroller.Distance = OrthoDistance;
                mousenav.orthoviewcontroller.Angle = OrthoAngle;
                mousenav.SetOrthographicView(OrthographicMainView);
                mousenav.orthoviewcontroller.OrthoEnabled = OrthoViews;
                mousenav.orthoviewcontroller.transform.position = OrthoTransformPos;
                mousenav.orthoviewcontroller.UpdateViews();
            }
        }
        public void SetCameraPositionEditor(Camera camera)
        {
            camera.transform.position = CameraTransformPos;
            camera.transform.rotation = CameraTransformRot;
            var nav = camera.GetComponent<SceneMouseNavigation>();
            if (nav!=null)
                nav.SetOrthographicView(OrthographicMainView);
            var orthoviewcontroller = Global.realvirtualcontroller.gameObject.GetComponentInChildren<OrthoViewController>();
            if (orthoviewcontroller != null)
            {
                orthoviewcontroller.transform.position = OrthoTransformPos;
                orthoviewcontroller.Size = OrthoSize;
                orthoviewcontroller.Angle = OrthoAngle;
                orthoviewcontroller.Distance = OrthoDistance;
                orthoviewcontroller.OrthoEnabled = OrthoViews;
                orthoviewcontroller.UpdateViews();
            
            }
            
        }
    }

}

