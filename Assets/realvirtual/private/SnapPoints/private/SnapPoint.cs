// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using NaughtyAttributes;

namespace realvirtual
{
    [ExecuteInEditMode]
    //! Snap point object. Actual used for conveyor and simulation path.
    public class SnapPoint : realvirtualBehavior
    {
        public enum SNAPTYPE
        {
            NONE = 0,
            IN = 1,
            OUT = 2,
            ALL = 3
        }
        
        [OnValueChanged("ValueChanged")] public bool SnapIsVisible = true;//! < Boolean whether a snap point is visible or not
        [OnValueChanged("ValueChanged")] public bool SnapEnabled = true;//! < Boolean is true when the snap point is snap to another one
        [OnValueChanged("ValueChanged")] public bool MultiSnapActive = false;//!< Boolean is true when more then 1 snap point is allowed to connect. For simulation path "true" is the default parameter.
        public bool SnapInGameMode = false;
        public SNAPTYPE SnapType;
        public bool DebugMode = false;
        [ReorderableList] public List<string> DontSnapTo;
        [ReadOnly] public bool snapped; //!< Read only: snapping active 
        [ReadOnly] public SnapPoint mate; //!< Read only: current mate snap point
        [ReadOnly] public List<SnapPoint> mates = new List<SnapPoint>();//!< Read only: list of current mate snap points
        [ReadOnly] public bool deactivatesnapping=false; 
        
        
        protected override void OnStartSim()
        {
            Hide(true);
        }
        
        protected override void OnStopSim()
        {
            if (!snapped)
                Hide(false);
        }

        private void Update()
        {

            if (Application.isPlaying && !SnapInGameMode)
                return;

            if (!SnapEnabled)
                return;
            
            if (snapped)
            {
                if (MultiSnapActive == true)
                {
                    if (mates.Count == 0)
                    {
                        Unsnap();
                    }
                    else
                    {
                        for (int i = 0; i < mates.Count; i++)
                        {
                            if (mates[i] != null)
                            {
                                var dist = Vector3.Distance(transform.position, mates[i].transform.position);
                                if (dist > 0.05)
                                {
                                    Unsnap();
                                }
                            }
                            else
                            {
                                mates.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                else
                {
                    if (SnapEnabled)
                    {
                        if (!mate || Vector3.Distance(transform.position, mate.transform.position) > 0.0005)
                        {
                            Unsnap();
                        }
                    }
                }
            }
        }

        // Update changed values
        public void ValueChanged()
        {
            Enable(SnapEnabled);
            IsVisible(SnapEnabled);
        }

        // activate snap point
        public void Enable(bool active)
        {
            SnapEnabled = active;
            gameObject.SetActive(active);
        }

        // visibility status of the snap point
        public void IsVisible(bool visible)
        {
            SnapIsVisible = visible;
            var meshrenderer = GetComponent<MeshRenderer>();
            meshrenderer.enabled = visible;
        }

        // check wheter a snap point is avaiblable for snapping
        public void CheckSnap()
        {
            if (!SnapEnabled)
                return;
          
            var layername = LayerMask.LayerToName(gameObject.layer);
            var layermask = LayerMask.GetMask(layername);
            //OverlapSphere for snapping near SnapPoints
            Collider[] overlap = Physics.OverlapSphere(transform.position, transform.localScale.y, layermask);
            for (int i = 0; i < overlap.Length; i++)
            {
               

                if (overlap[i].gameObject.transform.parent != gameObject.transform.parent)
                {
                    if (DebugMode) 
                    {
                        Debug.Log("SnapPoint: " + gameObject.name + " - Overlap: " + overlap[i].gameObject.name);
                    }
                    Snap(overlap[i].gameObject);
                }
                  
                
            }
        }

        // main snap method
        public void Snap(GameObject other)
        {
            bool valid = false;

            var otheerpoint = other.GetComponent<SnapPoint>();
            var othersnap = other.GetComponent<SnapPoint>().SnapType;
            if (SnapType == SnapPoint.SNAPTYPE.IN && othersnap == SnapPoint.SNAPTYPE.OUT)
            {
                valid = true;
            }

            if (SnapType == SnapPoint.SNAPTYPE.OUT && othersnap == SnapPoint.SNAPTYPE.IN)
            {
                valid = true;
            }

            if (SnapType != SnapPoint.SNAPTYPE.NONE && othersnap == SnapPoint.SNAPTYPE.ALL)
            {
                valid = true;
            }

            if (SnapType == SnapPoint.SNAPTYPE.ALL && othersnap != SnapPoint.SNAPTYPE.NONE)
            {
                valid = true;
            }
            
            if (DontSnapTo != null)
            {
                if (DontSnapTo.Contains(other.gameObject.name))
                    valid = false;
            }

            if (valid)
            {
                var thisobj = GetComponentInParent<ISnapable>();
                var matesnap = other.transform.GetComponent<SnapPoint>();
                var tmpmate = matesnap;
                var mateobj = tmpmate.GetComponentInParent<ISnapable>();
                bool Changedsnap = true;
                
                if (mateobj != thisobj) // Don't snap to same object
                {
                    if (MultiSnapActive == true)
                    {
                        if (!mates.Contains(tmpmate))
                        {
                            mates.Add(tmpmate);
                        }
                        else
                        {
                            Changedsnap = false;
                        }
                    }
                    else
                    {
                        if (mate == tmpmate)
                        {
                            Changedsnap = false;
                        }
                        else
                        {
                            mate = tmpmate; 
                        }
                       
                    }

                    snapped = true;
                    if (Changedsnap)
                    {
                        // Events in both Snappoints 
                        matesnap.OnSnapped(this);
                        OnSnapped(matesnap);

                        // Events to Connect and Align in both Library Objects - only moved is aligned

                        thisobj.Connect(this, matesnap, mateobj, true);
                        mateobj.Connect(matesnap, this, thisobj, false);
                    }
                }
            }
        }

        public void Hide(bool hiding)
        {
            if (hiding == true)
            {
                transform.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                transform.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        public void OnSnapped(SnapPoint snappedto)
        {
            if (MultiSnapActive == true)
            {
                if (!mates.Contains(snappedto))
                {
                    mates.Add(snappedto);
                }
            }
            else
            {
                mate = snappedto;
            }

            snapped = true;
            if (!MultiSnapActive == true)
            {
                transform.GetComponent<MeshRenderer>().enabled = false;
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void OnUnSnapped()
        {
            mate = null;
            mates.Clear();
            snapped = false;
            Hide(false);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void signalUnsnap(GameObject SPout)
        {
            foreach (var snapPoint in mates)
            {
                if (snapPoint.gameObject == SPout.gameObject)
                {
                    mates.Remove(snapPoint);
                }
            }
        }

        public void Unsnap()
        {
            ISnapable pl = null;
            snapped = false;
            var libobj = gameObject.GetComponentInParent<SimulationPath>();
            if (mate != null || mates.Count > 0)
            {
                
                if (MultiSnapActive == true)
                {
                    foreach (var snapPoint in mates)
                    {
                        var mateline = snapPoint.GetComponentInParent<SimulationPath>();
                        mateline.Disconnect(this, snapPoint,pl , false);
                        snapPoint.OnUnSnapped();
                        snapPoint.signalUnsnap(this.gameObject);
                    }
                }
                
                else
                {
                    var mateline = mate.GetComponentInParent<SimulationPath>();
                    if (mateline != null)
                    {
                        mateline.Disconnect(this, mate, pl, false);
                    }

                    mate.OnUnSnapped();
                }
            }

            OnUnSnapped();
            
        }

        public void OnDestroy()
        {
           
        }
      
    }
    
}