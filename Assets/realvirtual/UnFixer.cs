// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;


namespace realvirtual
{
    [SelectionBase]
    //! UnFixer component releases fixed MUs back to physics simulation for free movement.
    //! Reverses the fixing operation performed by Fixer components, re-enabling gravity and physics interactions.
    //! Supports raycast detection and PLC control for automated release operations in material handling systems.
    [ExecuteAlways]
    public class UnFixer : BehaviorInterface
    {
        [FormerlySerializedAs("LimitFixToTags")] 
        [Tooltip("Limit unfixing operations to objects with these tags (empty = all objects)")]
        public List<string> LimitToTags; //< Limits all Fixing functions to objects with defined Tags
        [Tooltip("Direction vector for raycast detection")]
        [ShowIf("UseRayCast")] public Vector3 RaycastDirection = new Vector3(1, 0, 0); //!< Raycast direction
        [Tooltip("Length of raycast in millimeters (scale is considered)")]
        [ShowIf("UseRayCast")] public float RayCastLength = 100; //!<  Length of Raycast in mm
        [Tooltip("Layers to check for MU detection during raycast")]
        [ShowIf("UseRayCast")] public List<string> RayCastLayers = new List<string>(new string[] {"g4a MU","g4A SensorMU",}); //!< Raycast Layers
        [Tooltip("Enable unfixing of detected MUs")]
        public bool UnFixMU = true;
        [Tooltip("Tag to apply to MUs after unfixing (optional)")]
        public string SetTagAfterUnfix;
        [Tooltip("Display visual debug information for raycast status")]
        public bool ShowStatus = true; //! true if Status of Collider or Raycast should be displayed
        [Tooltip("PLC signal to trigger unfixing of MUs")]
        public PLCOutputBool SignalUnfix; 
     
     
        private int layermask;
        private  RaycastHit[] hits;
        private bool signalunfixnotnull;
        
        public void Unfix(MU mu)
        {
            mu.Unfix();
        }

        private new void Awake()
        {
     
            base.Awake(); 
            layermask = LayerMask.GetMask(RayCastLayers.ToArray());
        }
        
        private void Start()
        {
        
            if (!Application.isPlaying)
                return;
            signalunfixnotnull = SignalUnfix != null;
        }
        
        private void Raycast()
        {
            float scale = 1000;
            if (!Application.isPlaying)
            {
                if (Global.realvirtualcontroller != null) scale = Global.realvirtualcontroller.Scale;
            }
            else
            {
                scale = realvirtualController.Scale;
            }

            var globaldir = transform.TransformDirection(RaycastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;
            hits = Physics.RaycastAll(transform.position, globaldir, RayCastLength/scale, layermask,
                QueryTriggerInteraction.UseGlobal);
            if (hits.Length>0)
            {
             
                if (ShowStatus) Debug.DrawRay(transform.position, display ,Color.red,0,true);
            }
            else
            {
                if (ShowStatus) Debug.DrawRay(transform.position, display, Color.yellow,0,true);
            
            }
    
        }
        
        void Update()
        {
            if (!Application.isPlaying && ShowStatus )
            {
                Raycast();
            }
        }
        
        void FixedUpdate()
        {
            if (signalunfixnotnull)
                UnFixMU = SignalUnfix.Value;
                Raycast();
                if (hits.Length > 0)
                {
                 
                    foreach (var hit in hits)
                    {
                        var mu = hit.collider.GetComponentInParent<MU>();
                        if (mu != null)
                        {
                            if (UnFixMU)
                            {
                                if (SetTagAfterUnfix != "")
                                    mu.tag = SetTagAfterUnfix;
                                Unfix(mu);
                            }
                        }
                    }
                }
        }
        
    }
}