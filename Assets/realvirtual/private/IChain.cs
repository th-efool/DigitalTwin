using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    public interface IChain
    {
        public Vector3 GetClosestDirection(Vector3 position);


        public Vector3 GetClosestPoint(Vector3 position);
       
        
        public Vector3 GetPosition(float normalizedposition , bool normalized = true);
        public Vector3 GetDirection(float normalizedposition , bool normalized = true);
        
        public Vector3 GetUpDirection(float normalizedposition , bool normalized = true);

        public float CalculateLength();

        public bool UseSimulationPath();
    }

}
