// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if REALVIRTUAL_BEHAVIOR

using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace realvirtual
{

    using System;
    using Unity.Behavior;
    using UnityEngine;
    using Action = Unity.Behavior.Action;
    using Unity.Properties;

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "DriveJog", story: "Start [Drive] Jog Forward [Forward]", category: "realvirtual",
        id: "71b3dab870ee2823a13edde58d28ede3")]
    public partial class DriveJogAction : Action
    {
        [SerializeReference] public BlackboardVariable<Drive> Drive;
        [SerializeReference] public BlackboardVariable<bool> Forward;

        protected override Status OnStart()
        {
            if (Forward.Value)
            {
                Drive.Value.JogForward = true;
                Drive.Value.JogBackward = false;
            }
            else
            {
                Drive.Value.JogBackward = true;
                Drive.Value.JogForward = false;
            }

            return Status.Success;
        }
        
    }
}
#endif
