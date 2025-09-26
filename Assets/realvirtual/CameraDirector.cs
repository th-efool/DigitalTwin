// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System.Collections.Generic;
#if CINEMACHINE
using Cinemachine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
#endif
#endif

using UnityEngine;
#pragma warning disable CS3001, CS3002, CS3003, CS3009
namespace realvirtual
{

        //! Small tool for controlling cameras if Cinemachine is used
        public class CameraDirector : MonoBehaviour
        {
                
#if CINEMACHINE
                [InfoBox(
                        "This only works with Cinemachine Package installed and Cinemachine in Scripting Define Symbols",EInfoBoxType.Warning)]
                public bool HideInfo=false;
                [ReorderableList] public List<CinemachineVirtualCamera> cameras;

                public CinemachineVirtualCamera FollowCamera;
                private SceneMouseNavigation scenemousenavigation;
                
                
                #if UNITY_EDITOR
                [Button("Start Recording")]
                public void StartRecording()
                {
                        RecorderWindow recorderWindow = GetRecorderWindow();
                        if(!recorderWindow.IsRecording())
                                recorderWindow.StartRecording();
                }

                [Button("Stop Recording")]
                public void StopRecording()
                {
                        RecorderWindow recorderWindow = GetRecorderWindow();
                        if(recorderWindow.IsRecording())
                                recorderWindow.StopRecording();
                }
                #endif
                [Button("Camera0")]
                public void Camera1()
                {
                        SwitchTo(0);
                }

                [Button("Camera1")]
                public void Camera2()
                {
                        SwitchTo(1);
                }

                [Button("Camera2")]
                public void Camera3()
                {
                        SwitchTo(2);
                }

                [Button("Camera3")]
                public void Camera4()
                {
                        SwitchTo(3);
                }

                [Button("Camera4")]
                public void Camera5()
                {
                        SwitchTo(4);
                }
                
                [Button("Follow Selected")]
                public void FollowSelected()
                {
                        #if UNITY_EDITOR
                        if (FollowCamera != null)
                        {
                                var go = (GameObject) Selection.activeObject;
                                FollowCamera.LookAt = go.transform;
                                FollowCamera.Follow = go.transform;
                                SetStandardPrios();
                                FollowCamera.Priority = 100;
                        }
                       #endif
                }

           
                public void SetStandardPrios()
                {
                        foreach (var camera in cameras)
                        {
                                if (camera != null)
                                        camera.Priority = 10;
                        }
                }
                #if UNITY_EDITOR
                private RecorderWindow GetRecorderWindow()
                {
                        return (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
                }
                #endif
                public void SwitchTo(int cam)
                {
                        if (cam >= cameras.Count)
                                return;
                        if (scenemousenavigation)
                                scenemousenavigation.ActivateCinemachine(true);
                        var camera = cameras[cam];
                        SetStandardPrios();
                        if (camera != null)
                        {
                                camera.Priority = 100;
                                var dolly = camera.gameObject.GetComponentInParent<CinemachineDollyCart>();
                                if (dolly != null)
                                        dolly.m_Position = 0;
                                var camonmu = camera.gameObject.GetComponentInParent<OnSensorCameraOnMu>();
                                if (camonmu!=null)
                                        camonmu.SetCameraOnMU();
                        }

                }

                public void Start()
                {
                        scenemousenavigation = UnityEngine.Object.FindAnyObjectByType<SceneMouseNavigation>();
                }
#endif
        }
}
#pragma warning restore CS3001, CS3002, CS3003, CS3009