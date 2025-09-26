using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    //! The drive is moving components including all sub components along the local axis of the game object.
    //! Rotational and linear movements are possible. A drive can be enhanced by DriveBehaviours which are adding special
    //! behaviours as well as Input and Output signals to drives.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/transportsurface")]
    public class TransportsurfaceCollider : realvirtualBehavior
    {

        private TransportSurface currentSurface;

        private void Start()
        {
            currentSurface = gameObject.GetComponent<TransportSurface>();
           
        }
        private void OnCollisionEnter(Collision other)
        {
            if (currentSurface.ChangeConstraintsOnEnter)
            {
                other.rigidbody.constraints = currentSurface.ConstraintsEnter;
            }
            var mu = other.gameObject.GetComponentInParent<MU>();
            if (!mu.TransportSurfaces.Contains(currentSurface))
                mu.TransportSurfaces.Add(currentSurface);
            currentSurface.LoadedPart.Add(mu.Rigidbody);
            currentSurface.OnEnterSurface(other);
        }
        private void OnCollisionExit(Collision other)
        {
            var mu = other.gameObject.GetComponentInParent<MU>();
            if (currentSurface == null)
                return;
            if (currentSurface.LoadedPart == null)
                return;
            if (mu.TransportSurfaces == null)
                return;
            
            currentSurface.LoadedPart.Remove(mu.Rigidbody);
            
            mu.TransportSurfaces.Remove(currentSurface);
            if (mu.TransportSurfaces.Count == 0)
            {
               
                if(currentSurface.ChangeConstraintsOnExit)
                    other.rigidbody.constraints = currentSurface.ConstraintsExit;
            }

            currentSurface.OnExitSurface(other);
        }
        
    }
}
