// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    [SelectionBase]
    public class PushButton3D : BehaviorInterface
    {
        public PLCInputBool stateSignal;

        [Tooltip("If not set, the light will reflect the state of the button")]
        public PLCOutputBool lightSignal;

        [OnValueChanged("SetLabel")] public string label;

        public bool showTimerProperty
        {
            get { return !toggle; }
        }

        [ShowIf("showTimerProperty")] [Tooltip("Time in seconds to wait before the button turns off")]
        public float timer = 0.5f;

        [Tooltip("If true, the button will toggle on/off")]
        public bool toggle;

        [Tooltip("If true, the button will be active on start")]
        public bool activeOnStart;

        SceneButtonBase sceneButtonBase;
        bool useSignalLight = false;

        void Start()
        {
            sceneButtonBase = GetComponentInChildren<SceneButtonBase>();
            sceneButtonBase.SetInputSignal(stateSignal);
            sceneButtonBase.isToggle = toggle;
            sceneButtonBase.simpleClickTime = timer;

            if (lightSignal != null)
            {
                sceneButtonBase.autoLight = false;
                useSignalLight = true;
            }
            else
            {
                sceneButtonBase.autoLight = true;
                useSignalLight = false;
            }

            if (activeOnStart)
            {
                sceneButtonBase.Click();
            }
        }


        void Update()
        {
            if (useSignalLight)
            {
                sceneButtonBase.SetLight(lightSignal.Value);
            }
        }

        void SetLabel()
        {
            TMPro.TextMeshPro text = GetComponentInChildren<TMPro.TextMeshPro>(true);
            if (text != null)
            {
                text.text = label;
            }
        }
    }
}