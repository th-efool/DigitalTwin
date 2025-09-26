// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

namespace realvirtual
{
    //! Unity event triggered when an MU is destroyed by a sink, passing the destroyed MU as parameter
    [System.Serializable]
    public class SinkEventOnDestroy : UnityEngine.Events.UnityEvent<MU> {}
}
