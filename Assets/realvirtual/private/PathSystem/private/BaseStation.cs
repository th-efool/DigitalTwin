using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
    //! BaseStation provides intelligent workstation control for path-based material handling systems.
    //! This base class implements station logic for controlling MU flow through processing points, assembly stations,
    //! and transfer locations in automated production lines. Features entry/exit control with queuing management,
    //! configurable work zones with visual feedback, and event-driven communication for MES integration.
    //! Supports multi-MU handling, selective entry conditions, and synchronized release mechanisms essential
    //! for implementing buffer stations, quality control points, assembly workstations, and sorting nodes
    //! in modern flexible manufacturing and logistics systems.
    [SelectionBase]
    [HelpURL("https://doc.realvirtual.io/extensions/page-4/realvirtual.io-path-system/workstation")]
    public class BaseStation : realvirtualBehavior
    {

        [Foldout ("Settings")][OnValueChanged("SettingChanged")] public Vector3 Dimension = new Vector3(1,1,1);
        [Foldout ("Status")][ReadOnly] public PathMover CurrentPathMover;
        protected List<PathMover> PathMoverWaitingForEntry = new List<PathMover>();
        protected List<PathMover> PathMoverMovingIn = new List<PathMover>();
        protected List<PathMover> PathMoverMovingOut = new List<PathMover>();
        
        protected bool LimitToTransportablesonPath=false;
        protected MeshRenderer areameshrenderer;
        protected PathMover transportablewaitingforentry;

        private PathMover waitingforstop;
        
        // Methods to optionally override in Stations
        protected virtual bool AllowEntry(PathMover pathMover)
        {
            return true;
        }
        
        protected virtual void OnAtPositon(PathMover pathMover)
        {
      
        }
        
        protected virtual void OnExit(PathMover pathMover)
        {
      
        }
        
        protected virtual void OnFixedUpdate()
        {
      
        }

        // Methods to be used by Stations
        public void OpenEntry(PathMover pathmover)
        {
            if (!PathMoverWaitingForEntry.Contains(pathmover))
                return;
            
            if (pathmover.IsStopped)
                pathmover.Start();
            pathmover.StationEntered(this);
            PathMoverMovingIn.Add(pathmover);
            PathMoverWaitingForEntry.Remove(pathmover);
            CheckNext();
        }
        // start release of the current MU
        public void Release()
        {
            CurrentPathMover.Start();
            PathMoverMovingOut.Add(CurrentPathMover);
            CurrentPathMover = null;
            CheckNext();
        }
        
        // Trigger Enter and Exit from Sensor
        public void OnTriggerEnter(Collider other)
        {
            var pathmover = other.gameObject.GetComponent<PathMover>();
            if (pathmover == null)
                pathmover = other.gameObject.GetComponentInParent<PathMover>();
            if (pathmover == null)
                return;
            if (LimitToTransportablesonPath && pathmover.Path == null)
                return;
            if (PathMoverWaitingForEntry.Contains(pathmover))
                return;
            PathMoverWaitingForEntry.Add(pathmover);
         
            if (AllowEntry(pathmover))
                OpenEntry(pathmover);
            else
            {
                pathmover.Stop();
            }
        }
        // Trigger when MU has left
        public void OnTriggerExit(Collider other)
        {
           
            var trans = other.gameObject.GetComponent<PathMover>();
            if (trans == null)
                trans = other.gameObject.GetComponentInParent<PathMover>();
            if (trans == null)
                return;
            if (trans == waitingforstop)
                Debug.LogError("Error");
            PathMoverMovingOut.Remove(trans);
            OnExit(trans);
            if (LimitToTransportablesonPath && trans.Path == null)
                return;
            CheckNext();
            
        }

        private void AtPosition(PathMover pathmover)
        {
            CurrentPathMover = pathmover;
            PathMoverMovingIn.Remove(pathmover);
            pathmover.EnteringStationDistance = 99999999;
            if (!CurrentPathMover.IsStopped)
            {
                CurrentPathMover.OnStopped.AddListener(OnFullyStopped);
                CurrentPathMover.Stop();
                waitingforstop = CurrentPathMover;
            }
            else
            {
                OnFullyStopped(pathmover.Path, pathmover);
            }

        }
        // Called when MU reaches main position
        public void OnFullyStopped(SimulationPath path, PathMover mover)
        {
            if (waitingforstop == mover)
                waitingforstop = null;
            mover.OnStopped.RemoveListener(OnFullyStopped);
            OnAtPositon(mover);
            CheckNext();
        }

        private void CheckNext()
        {
        }
        // Called when settings are changed
        public void SettingChanged()
        {
            var area = GetComponentInChildren<StationSensor>();
            area.transform.localScale = Dimension;
          
        }
        
        private float GetDistance(PathMover pathmover)
        {
            return Vector3.Distance(pathmover.gameObject.transform.position, this.transform.position);
        }

        void Reset()
        {
            SettingChanged();
        }

        // Start is called before the first frame update
        new void Awake()
        {
            areameshrenderer = GetComponentInChildren<StationSensor>().gameObject.GetComponent<MeshRenderer>();
            base.Awake();
        }
        
        void FixedUpdate()
        {

            // Check if transportable is in station center
            if (PathMoverMovingIn.Count == 0)
                return;
            int i = PathMoverMovingIn.Count-1;
            PathMover pathmover;
            while ( i >= 0)
            {
                pathmover = PathMoverMovingIn[i];
                var transportabledistance = GetDistance(pathmover);
                if (transportabledistance > pathmover.EnteringStationDistance)
                {
                    AtPosition(pathmover);
                }
                else
                {
                    pathmover.EnteringStationDistance = transportabledistance;
                }
                i--;
            }

            OnFixedUpdate();
        }
    }
}