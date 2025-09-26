// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if REALVIRTUAL_BEHAVIOR

using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace realvirtual
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "WaitForSignalBool", story: "Wait for Bool [Signal] [value]", category: "realvirtual",
        id: "73895d9c4319645ac3353d06ed9d5aa9")]
    public partial class WaitForSignalBoolAction : Action
    {
        [SerializeReference] public BlackboardVariable<Signal> Signal;
        [SerializeReference] public BlackboardVariable<bool> Value;

        protected override Status OnStart()
        {
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
             if ((bool)Signal.Value.GetValue() == Value.Value)
                return Status.Success;
             else
                 return Status.Running;
        }
        
    }
}
#endif
