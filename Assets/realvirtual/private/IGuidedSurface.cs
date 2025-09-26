using UnityEngine;

namespace realvirtual
{
    //! IGuidedSurface defines the contract for transport surfaces that provide path guidance for material units.
    //! This interface enables transport components to guide MUs along predefined paths using directional vectors
    //! and closest point calculations. Implementations provide guided movement for conveyor systems, AGV paths,
    //! and other transport mechanisms that require objects to follow specific routes rather than free movement.
    //! Essential for creating intelligent transport systems with path-following capabilities in automated
    //! material handling and intralogistics applications.
    public interface IGuidedSurface
    {
        public bool IsSurfaceGuided();
        public Vector3 GetClosestDirection(Vector3 position);
        
        public Vector3 GetClosestPoint(Vector3 position);
        
        public Drive GetDrive();
    }
}