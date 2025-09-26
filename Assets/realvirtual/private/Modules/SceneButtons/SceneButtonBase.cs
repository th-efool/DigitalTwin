// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using UnityEngine;

namespace realvirtual
{
    public class SceneButtonBase : BehaviorInterface
    {
        public SceneButtonMoveable moveable;
        public bool autoLight = true;
        public bool isToggle;
        public float simpleClickTime = 0.5f;

        public UnityEngine.Events.UnityEvent OnToggleOn;
        public UnityEngine.Events.UnityEvent OnToggleOff;

        bool active;
        bool lightOn;
        private PLCInputBool signal;
        private bool released = true;
        private bool request_release;


        public void Click()
        {
            OnMouseDown();
        }

        private void OnMouseEnter()
        {
            moveable.Hover();
        }

        private void OnMouseExit()
        {
            moveable.Unhover();
        }

        private void OnMouseDown()
        {
            if (isToggle)
            {
                ToggleClick();
            }
            else
            {
                released = false;
                request_release = false;
                SimpleClickOn();
            }
        }

        private void OnMouseUp()
        {
            released = true;
            if (request_release)
            {
                request_release = false;
                SimpleClickOff();
            }
        }

        private void SimpleClickOn()
        {
            if (active)
            {
                return;
            }

            ToggleClick();

            Invoke("SimpleClickOff", simpleClickTime);
        }

        private void SimpleClickOff()
        {
            if (!active)
            {
                return;
            }

            if (!released)
            {
                request_release = true;
                return;
            }

            ToggleClick();
        }

        private void ToggleClick()
        {
            moveable.Click();
            moveable.Hover();

            active = !active;

            if (signal != null)
            {
                signal.SetValue(active);
            }

            if (autoLight)
            {
                if (active)
                {
                    LightOn();
                }
                else
                {
                    LightOff();
                }
            }

            if (active)
            {
                OnToggleOn.Invoke();
            }
            else
            {
                OnToggleOff.Invoke();
            }
        }

        public void SetLight(bool state)
        {
            if (state == true && lightOn == false)
            {
                LightOn();
            }
            else if (state == false && lightOn == true)
            {
                LightOff();
            }
        }

        public void LightOn()
        {
            moveable.LightOn();
            lightOn = true;
        }

        public void LightOff()
        {
            moveable.LightOff();
            lightOn = false;
        }


        public void SetInputSignal(PLCInputBool inputSignal)
        {
            signal = inputSignal;
        }
    }
}