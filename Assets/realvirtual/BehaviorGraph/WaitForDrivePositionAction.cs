// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if REALVIRTUAL_BEHAVIOR
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
#pragma warning disable CS3003, CS3002
namespace realvirtual
{
    
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "WaitForDrivePosition", story: "Wait for [Drive] at [Position]", category: "realvirtual",
        id: "10cf3abd4efc6ad695c6565d70f4a80c")]
    public partial class WaitForDrivePositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<Drive> Drive;
        [SerializeReference] public BlackboardVariable<float> Position;

        protected override Status OnStart()
        {
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!Drive.Value.IsRunning && Drive.Value.CurrentPosition == Position.Value)
                return Status.Success;
            else
            {
                return Status.Running;
            }
        }
    }
}
#endif
