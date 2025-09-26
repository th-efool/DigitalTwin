#if REALVIRTUAL_CUTTER
using System.Collections;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.EventHandlers;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace realvirtual
{
    

    public class CutterObjectSlicedEvent : MonoBehaviour, IBzObjectSlicedEvent
    {

        public Cutter cutter; //!< Ev
        
        public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
        {
            return true;
        }

        public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
        {
            cutter.OnMuSliced(original,resultObjects);

        }
    }
}
#endif
