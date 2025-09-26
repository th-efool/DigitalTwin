// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
    [SelectionBase]
    public class HandleSwitch3D : BehaviorInterface
    {
        public PLCInputBool stateSignal;
        public bool activeOnStart;

        SceneButtonBase sceneButtonBase;

        void Start()
        {
            sceneButtonBase = GetComponentInChildren<SceneButtonBase>();
            sceneButtonBase.SetInputSignal(stateSignal);

            if (activeOnStart)
            {
                sceneButtonBase.Click();
            }
        }
    }
}