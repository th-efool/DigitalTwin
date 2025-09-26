// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
#if REALVIRTUAL_AGX
using Mesh = AGXUnity.Collide.Mesh;
using AGXUnity;
#endif

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/sink")]
    [SelectionBase]
    #if !REALVIRTUAL_AGX
    [RequireComponent(typeof(BoxCollider))]
    #endif
    #region doc
    //! Sink component for removing MUs from the simulation, representing endpoints in material flow systems.
    
    //! The Sink is a fundamental component in realvirtual that acts as the termination point for MUs in the automation system.
    //! It simulates various endpoints such as shipping areas, quality control rejection points, recycling stations, or any location
    //! where products exit the simulated system. Sinks automatically detect and remove MUs that enter their trigger area.
    //! 
    //! Key Features:
    //! - Automatic detection and removal of MUs entering the sink area
    //! - Optional visual dissolve effect for smooth MU disappearance
    //! - Selective deletion based on MU tags for filtering specific products
    //! - Production statistics tracking (total destroyed, rate per hour)
    //! - PLC signal integration for controlled deletion
    //! - Collision-based detection using Unity trigger colliders
    //! - Support for both immediate and delayed destruction
    //! - Event system for tracking destroyed MUs
    //! 
    //! Common Applications:
    //! - End of production lines where finished products exit
    //! - Quality control stations removing defective items
    //! - Shipping and dispatch areas
    //! - Recycling and waste collection points
    //! - Buffer overflow handling
    //! - Testing and validation endpoints
    //! - Order fulfillment completion points
    //! - Rework station exits
    //! 
    //! Operation Modes:
    //! - Automatic Mode: MUs are destroyed immediately upon entering
    //! - PLC Controlled: Deletion triggered by PLC signals
    //! - Selective Mode: Only specific tagged MUs are destroyed
    //! - Statistical Mode: Tracks throughput without deletion
    //! 
    //! Visual Effects:
    //! - Dissolve Effect: Smooth fade-out using shader-based dissolution
    //! - Configurable fade duration for visual feedback
    //! - Compatible with standard and custom shaders
    //! - Optional immediate destruction without effects
    //! 
    //! Statistics and Monitoring:
    //! - SumDestroyed: Total count of destroyed MUs
    //! - DestroyedPerHour: Current throughput rate calculation
    //! - CollidingObjects: Real-time list of MUs in sink area
    //! - Event notifications for external tracking systems
    //! 
    //! PLC Integration:
    //! - Delete Signal: Triggers deletion of all MUs currently in sink
    //! - Can be integrated with production control systems
    //! - Supports batch processing scenarios
    //! 
    //! Performance Considerations:
    //! - Efficient trigger-based detection
    //! - Automatic cleanup of destroyed objects
    //! - Optimized for high-throughput scenarios
    //! - Minimal impact on simulation performance
    //! 
    //! Events:
    //! - OnMUDelete: Fired when an MU is destroyed
    //! - Provides destroyed MU reference for logging or statistics
    //! 
    //! Setup Requirements:
    //! - Requires BoxCollider component set as trigger
    //! - Automatically configured on component addition
    //! - Size determines the detection area
    //! 
    //! For detailed documentation see: https://doc.realvirtual.io/components-and-scripts/sink
    #endregion
    public class Sink : realvirtualBehavior
    {
#if REALVIRTUAL_AGX
        public bool UseAGXPhysics;
#else
        [HideInInspector] public bool UseAGXPhysics = false;
#endif
        // Public - UI Variables 
        [Header("Settings")]
        [Tooltip("Enable deletion of MUs that enter this sink")]
        public bool DeleteMus = true; //!< Delete MUs
        [ShowIf("DeleteMus")]
        [Tooltip("Use dissolve effect when deleting MUs")]
        public bool Dissolve = true; //!< Dissolve MUs
        [ShowIf("DeleteMus")]
        [Tooltip("Only delete MUs with this tag (leave empty to delete all)")]
        public string DeleteOnlyTag; //!< Delete only MUs with defined Tag
        [ShowIf("DeleteMus")]
        [Tooltip("Duration of the dissolve effect in seconds")]
        public float DestroyFadeTime=0.5f; //!< Time in seconds to fade out MU
        [Header("Sink IO's")]
        [Tooltip("PLC signal to trigger deletion of all MUs in sink")]
        public PLCOutputBool Delete; //!< PLC output for deleting MUs
        private bool _lastdeletemus = false;
    
        [Header("Status")] 
        [ReadOnly] public float SumDestroyed; //!< Sum of destroyed objects
        [ReadOnly] public float DestroyedPerHour; //!< Sum of destroyed objects per Hour
        [ReadOnly] public List<GameObject> CollidingObjects; //!< Currently colliding objects

        public SinkEventOnDestroy OnMUDelete; //!< Event triggered when an MU is deleted by the sink
        
        private bool _isDeleteNotNull;

        // Use this when Script is inserted or Reset is pressed
        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }    
    
        // Use this for initialization
        private void Start()
        {
            _isDeleteNotNull = Delete != null;
#if REALVIRTUAL_AGX
            if (UseAGXPhysics)
            {
                var body = this.GetComponent<AGXUnity.Collide.Box>();
                if (body == null)
                {
                    Error ("Sink using AGX: Expecting an AGX Box Shape Collider component with Collissions Enabled and IsSensor", this);
                    return;
                }
                body.CollisionsEnabled = true;
                body.IsSensor = true;
                Simulation.Instance.ContactCallbacks.OnContact(OnContact,body);
            }
#endif
        }

        //! Deletes all MUs currently in the sink area
        public void DeleteMUs()
        {
            
            var tmpcolliding = CollidingObjects;
            foreach (var obj in tmpcolliding.ToArray())
            {
                var mu = GetTopOfMu(obj);
                if (mu != null)
                {
                    if (DeleteOnlyTag == "" || (mu.gameObject.tag == DeleteOnlyTag))
                    {

                        OnMUDelete.Invoke(mu);
                        if (!Dissolve)
                             Destroy(mu.gameObject);
                        else
                            mu.Dissolve(DestroyFadeTime);
                        SumDestroyed++;
                    }
                }

                CollidingObjects.Remove(obj);
            }
        }
    
        // ON Collission Enter
        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            SensorEnter(obj);
        }

        private void SensorEnter(GameObject obj)
        {
            CollidingObjects.Add(obj);
            if (DeleteMus==true)
            {
                // Act as Sink
                DeleteMUs();
            }
        }
    
        // ON Collission Exit
        private void OnTriggerExit(Collider other)
        {
            GameObject obj = other.gameObject;
            CollidingObjects.Remove(obj);
        }
        
        #if REALVIRTUAL_AGX
        private bool OnContact(ref ContactData data)
        {
            var obj = data.Component1.gameObject;
            SensorEnter(obj);
            return false;
        }
        #endif
        private void Update()
        {
            DestroyedPerHour = SumDestroyed / (Time.time / 3600);
            if (_isDeleteNotNull)
            {
                DeleteMus = Delete.Value;
            }
        
            if (DeleteMus && !_lastdeletemus)
            {
                DeleteMUs();
            }
            _lastdeletemus = DeleteMus;

        }
    }
}