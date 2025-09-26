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
    [NodeDescription(name: "WaitForSensor", story: "Wait for [Sensor] occupied [IsOccupied]", category: "realvirtual",
        id: "766955d826bccb56ed45a1a72b1b8b19")]
    public partial class WaitForSensorAction : Action
    {
        [SerializeReference] public BlackboardVariable<Sensor> Sensor;
        [SerializeReference] public BlackboardVariable<bool> IsOccupied;

        protected override Status OnStart()
        {
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Sensor.Value.Occupied == IsOccupied.Value)
                return Status.Success;
            else
                return Status.Running;
        }
    }
}

#endif