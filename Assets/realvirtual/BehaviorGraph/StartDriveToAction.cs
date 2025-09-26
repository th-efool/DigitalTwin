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
    [NodeDescription(name: "StartDriveTo", story: "Start [Drive] to [destination]", category: "realvirtual", id: "889d8c727e0243483a08038422ed4a8d")]
    public partial class StartDriveToAction : Action
    {
        [SerializeReference] public BlackboardVariable<Drive> Drive;
        [SerializeReference] public BlackboardVariable<float> Destination;

        protected override Status OnStart()
        {
            Drive.Value.DriveTo(Destination.Value);
            return Status.Success;
        }
        
        
    }
}

#endif
