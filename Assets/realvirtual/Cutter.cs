// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
#if REALVIRTUAL_CUTTER
using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.EventHandlers;
#endif
using UnityEngine;

namespace realvirtual
{


    [RequireComponent(typeof(Plane))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    //! Cutter component for dynamically slicing MU meshes during simulation runtime.
    //! Simulates cutting operations like saws or laser cutting by splitting meshes along a plane.
    //! Requires BzKovSoft.ObjectSlicer asset and supports selective cutting with material application to cut faces.
    public class Cutter : MonoBehaviour
    {
#if !REALVIRTUAL_CUTTER
        [InfoBox(
            "This component requires an Asset Store Component called 'BzKovSoft.ObjectSlicer' to be installed. Please install it from the Asset Store and set after installing the Scripting Define Symbol REALVIRTUAL_CUTTER. There is a demo download available. See https://assetstore.unity.com/packages/tools/modeling/mesh-slicer-59618")]
#endif
        [Header("Settings")]
        [Tooltip("Specifies the material to use for the faces created by the cut.")]
        public Material CutMaterial;

        [Tooltip("If true, the positive side of the cut object will be deleted.")]
        public bool DeletePositiveCut;

        [Tooltip("If true, the negative side of the cut object will be deleted.")]
        public bool DeleteNegativeCut;

#if REALVIRTUAL_CUTTER
    [Tooltip("Time in seconds to block further cutting actions after a cut is performed")]
    public float BlockForSecondsAfterCut = 0;

    [Tooltip("Cut only specific submeshes instead of the entire mesh")]
    public bool OnlySubmeshes;

    [ShowIf("OnlySubmeshes")]
    [Tooltip("Tag to identify submeshes that can be cut")]
    public string SubmeshTag;

    [Header("Signals / IOs")]
    [Tooltip("Trigger cutting process when enabled")]
    public bool StartCut;

    [Tooltip("PLC signal to trigger cutting operation")]
    public PLCOutputBool SignalCut;

    [NaughtyAttributes.ReadOnly]
    [Tooltip("Indicates whether the component is currently blocked from cutting.")]
    public bool isblocked;
   
    
    private bool startslicebefore;
    private bool signalstartslicenotnull;
    private MeshRenderer planemesh;
    private List<Collider> colliders = new List<Collider>();

    public void Slice()
    {
        if (BlockForSecondsAfterCut > 0)
        {
            isblocked = true;
            Invoke("Unblock", BlockForSecondsAfterCut);
        }

        // loop through all current colliders
        planemesh.enabled = true;
        Invoke("HidePlane", 0.1f);

        // loop through colliders and remove all nulls
        colliders.RemoveAll(item => item == null);
        List<MU> mus = new List<MU>();
        List<MeshRenderer> meshestocut = new List<MeshRenderer>();
        foreach (var collider in colliders)
        {
            if (!OnlySubmeshes)
            {
                var mu = collider.gameObject.GetComponentInParent<MU>();
                // create a list with unique mus
                if (mus.Contains(mu) == false)
                    mus.Add(mu);
            }
            else
            {
                if (collider.gameObject.tag == SubmeshTag)
                {
                    meshestocut.Add(collider.gameObject.GetComponent<MeshRenderer>());
                }
            }
        }

        // Slice submeshes if not full MU should be cout
        foreach (var mesh in meshestocut)
        {
            #pragma warning disable 4014
            DoSlice(mesh.gameObject);
        }

        // Slice Full MUs
        foreach (var mu in mus)
        {
             DoSlice(mu.gameObject);
        }
    }

    async Task DoSlice(GameObject obj)
    {
        BzSliceableObject sliceable = null;
        CutterObjectSlicedEvent sliceevent = null;

        sliceable = Global.AddComponentIfNotExisting<BzSliceableObject>(obj);
        Global.AddComponentIfNotExisting<BzAvoidOversliceHandler>(obj);
        Global.AddComponentIfNotExisting<BzSmoothDepenetration>(obj);
        sliceevent = Global.AddComponentIfNotExisting<CutterObjectSlicedEvent>(obj);
        sliceevent.cutter = this;

        sliceable.defaultSliceMaterial = CutMaterial;
        var slicer = sliceable.GetComponent<IBzMeshSlicer>();
        var plane = new Plane(this.transform.up, this.transform.position);
    
        await slicer.SliceAsync(plane);
    }

    void Unblock()
    {
        isblocked = false;
    }

    //! Event Handler for MU Sliced
    public void OnMuSliced(GameObject orignal, GameObject[] cuttedobjects)
    {
        // loop through cuttedobjects and delete all before added scripts
        foreach (var cuttedobject in cuttedobjects)
        {
            var cutevent = cuttedobject.GetComponent<CutterObjectSlicedEvent>();
            Destroy(cutevent);
            var slicable = cuttedobject.GetComponent<BzSliceableObject>();
            if (slicable != null)
            {
                Destroy(slicable);
            }

            // delete BzAvoidOversliceHandler
            var avoidoverslice = cuttedobject.GetComponent<BzAvoidOversliceHandler>();
            if (avoidoverslice != null)
            {
                Destroy(avoidoverslice);
            }

            // delete BzSmoothDepenetration
            var smoothdepenetration = cuttedobject.GetComponent<BzSmoothDepenetration>();
            if (smoothdepenetration != null)
            {
                Destroy(smoothdepenetration);
            }
        }

        // delete positive object
        if (DeletePositiveCut)
        {
            foreach (var cuttedobject in cuttedobjects)
            {
                // Delete if in name is "neg"
                if (cuttedobject.name.Contains("_pos_"))
                {
                    Destroy(cuttedobject);
                }
            }
        }

        // delete negative object
        if (DeleteNegativeCut)
        {
            foreach (var cuttedobject in cuttedobjects)
            {
                // Delete if in name is "neg"
                if (cuttedobject.name.Contains("_neg_"))
                {
                    Destroy(cuttedobject);
                }
            }
        }
    }

    private void DestroyCuttedObject(GameObject obj)
    {
        // Check if it is not an 
    }

    private void OnTriggerEnter(Collider collider)
    {
        colliders.Add(collider);
    }

    private void OnTriggerExit(Collider collider)
    {
        colliders.Remove(collider);
    }

    void OnChangeVisual()
    {
    }

    void HidePlane()
    {
        planemesh.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {

        signalstartslicenotnull = SignalCut != null;
        planemesh = GetComponent<MeshRenderer>();
        planemesh.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isblocked)
            return;
        if (signalstartslicenotnull)
        {
            StartCut = SignalCut.Value;
        }


        if (StartCut && !startslicebefore)
        {
             Slice();
        }

        startslicebefore = StartCut;
    }
#endif
    }
}