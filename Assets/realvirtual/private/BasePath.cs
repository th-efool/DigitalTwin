// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using UnityEngine;

namespace realvirtual
{
    //! realvirtual Simulation - the base class of all Pathes
    public abstract class BasePath : realvirtualBehavior
    {
        public abstract float GetLength();  //!< Gets the length of the path
        public abstract Vector3 GetPosition(float normalizedposition, ref BasePath currentpath);  //!< Gets the position in World coordinates of the path at the defined  normalized position
        public abstract Vector3 GetDirection(float normalizedposition);  //!< Gets the tangent of the pat at the defined normalized position.

        //! Gets the direction tangent in local coordinate system
        public Vector3 GetLocalDirection(float normalizedposition) 
        {
            var global = GetDirection(normalizedposition);
            return Vector3.Normalize(transform.InverseTransformDirection(global));
        }
    
        //! Gets the position in global coordinate system at absolute position (in meters) at the path
        public Vector3 GetAbsPosition(float abspositon, ref BasePath currentpath)
        {
            return GetPosition(abspositon / GetLength(), ref currentpath);
        }
        
        //!  Gets the direction tangent in global coordinate system at absolute position (in meters) at the path
        public  Vector3 GetAbsDirection(float abspositon)
        {
            return GetDirection(abspositon / GetLength());
        }
    }
}

