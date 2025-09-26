// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
#if CINEMACHINE
using Cinemachine;
#endif
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/defining-logic/logicsteps")]
    public class LogicStep_CinemachineCamera: LogicStep
    {
#if CINEMACHINE
        public CinemachineVirtualCamera Camera;
#endif
#if !CINEMACHINE
        [InfoBox("CINEMACHINE needs to be installed for this LogicStep")]
#endif
        public bool UseCustomBlend = false;
        [ShowIf("UseCustomBlend")]public float CustomBlendTime = 10;
#if CINEMACHINE    
        [ShowIf("UseCustomBlend")]public CinemachineBlendDefinition.Style CustomBlendStyle;
#endif
        private realvirtualController controller;
        public void Awake()
        {
            controller = UnityEngine.Object.FindAnyObjectByType<realvirtualController>();
        }
        
        protected new bool NonBlocking()
        {
            return true;
        }

        protected override void OnStarted()
        {
            State = 50;
#if CINEMACHINE
            if (UseCustomBlend)
            {
                var brain = UnityEngine.Object.FindAnyObjectByType<CinemachineBrain>();
                brain.m_DefaultBlend.m_Time = CustomBlendTime;
                brain.m_DefaultBlend.m_Style = CustomBlendStyle;
            }
            controller.SetCinemachineCamera(Camera);
#endif
            NextStep();
        }

    }

}

