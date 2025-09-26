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
    [NodeDescription(name: "StopDrive", story: "Stop [Drive]", category: "Action",
        id: "b2263ecb436045ee8c39df95d68a482a")]
    public partial class StopDriveAction : Action
    {
        [SerializeReference] public BlackboardVariable<Drive> Drive;

        protected override Status OnStart()
        {
            Drive.Value.JogForward = false;
            Drive.Value.JogBackward = false;
            Drive.Value.Stop();
            return Status.Success;
         
        }
        
    }
}
#endif
