// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if REALVIRTUAL_BEHAVIOR
#pragma warning disable CS3003

using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace realvirtual
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "SetSignalBool", story: "Set [Signal] to [To]", category: "realvirtual",
        id: "6f23fb3ff95bf53583499f7d4b8a9ec3")]
    public partial class SetSignalBoolAction : Action
    {
        [SerializeReference] public BlackboardVariable<Signal> Signal;
        [SerializeReference] public BlackboardVariable<bool> To;

        protected override Status OnStart()
        {

            Signal.Value.SetValue(To.Value);
            return Status.Success;
        }
        
    }
}
#endif
