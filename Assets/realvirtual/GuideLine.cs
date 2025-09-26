// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz    

using UnityEngine;

namespace realvirtual
{
    //! GuideLine provides linear path guidance for movement systems.
    //! It defines a straight line path that objects can follow, extending from the transform position along the local right axis.
    //! Implements the IGuide interface to provide closest point and direction calculations for path following.
    public class GuideLine : realvirtualBehavior, IGuide
    {
        [Tooltip("Length of the guide line in meters")]
        public float Length=1.0f; //!< Length of the guide line in meters
        [Tooltip("Shows visual representation of the line in the Scene view")]
        public bool ShowGizmos = true; //!< Shows visual representation of the line in the Scene view

        public void OnDrawGizmos()
        {
            if (!ShowGizmos) return;
            var start = this.transform.position;
            var end = this.transform.position + this.transform.right * Length;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(start, 0.02f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(end, 0.02f);

        }


        //! Checks if the guide component is active and enabled.
        public bool IsActive()
        {
            return this.enabled;
        }

        //! Gets the direction of the line (always returns the transform's right vector).
        public Vector3 GetClosestDirection(Vector3 position)
        {
            return this.transform.right;
        }

        //! Calculates the closest point on the line to the given position (clamped to line segment).
        public Vector3 GetClosestPoint(Vector3 position)
        {
            var origin = this.transform.position;
            var end = origin + this.transform.right * Length;
            Vector3 heading = (end - origin);
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector3 lhs = position - origin;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + heading * dotP;
        }
    }


    //! IGuide defines the interface for path guidance components in material transport systems.
    //! Implementations provide directional guidance and closest point calculations for objects following
    //! defined paths. Used by transport surfaces, AGV systems, and guided conveyors to maintain objects
    //! on predetermined routes. Essential for creating reliable guided transport in automation systems
    //! where precise path following is required for safety and efficiency.
    public interface IGuide
    {
        public Vector3 GetClosestDirection(Vector3 position);
        public Vector3 GetClosestPoint(Vector3 position);

        public bool IsActive();
    }
}
