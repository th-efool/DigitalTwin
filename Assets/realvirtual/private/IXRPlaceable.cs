// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

//! Interface for Placeable Objects in XR / AR Space
namespace realvirtual
{
    public interface IXRPlaceable 
    {
        //! Called when the object is initialized in XR / AR Space
        public void OnXRInit(GameObject placedobj);
    
        //! Called when the object is starting to be moved in XR / AR Space
        public void OnXRStartPlace(GameObject placedobj);

        //! Called when the object is ending to be moved in XR / AR Space
        public void OnXREndPlace(GameObject placedobj);
    }
}
