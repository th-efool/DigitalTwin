// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

//! Interface for all Classes which are Fixing components (currently Grip and Fixer)
namespace realvirtual
{
    public interface IFix
    {
        void DeActivate(bool activate);  
        void Fix(MU mu);
        void Unfix(MU mu);
    }
}