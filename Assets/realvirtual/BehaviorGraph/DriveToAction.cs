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
#pragma warning disable CS3009, CS3003, CS3002
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "DriveTo", story: "[Drive] to position [Position]", category: "realvirtual",
        id: "355ae1ec6b435f917a25334c2ae84d0a")]
    public partial class DriveToAction : Action
    {
        [SerializeReference] public BlackboardVariable<Drive> Drive;
        [SerializeReference] public BlackboardVariable<float> Position;

        private float Destination;

        protected override Status OnStart()
        {
            Destination = Position.Value;
            Drive.Value.DriveTo(Destination);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!Drive.Value.IsRunning && Drive.Value.CurrentPosition == Destination)
                return Status.Success;
            else
            {
                return Status.Running;
            }
        }

        protected override void OnEnd()
        {
        }
    }
#pragma warning restore CS3009, CS3003, CS3002
}
#endif
