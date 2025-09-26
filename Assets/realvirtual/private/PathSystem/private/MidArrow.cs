
// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.UI;


namespace realvirtual
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    //! Draw the mid arrow within the path system
    public class MidArrow : MonoBehaviour
    {
        public bool CondensedView = true;
        public float Size;
        public Material Material;
        
        [HideInInspector] public LineRenderer Linerenderer;
       
        private SimulationPath path;

        void Init()
        {
            Linerenderer = GetComponent<LineRenderer>();
            path = GetComponentInParent<SimulationPath>();
            // Set Material if not there
            Linerenderer.alignment = LineAlignment.TransformZ;
            
            Linerenderer.alignment = LineAlignment.TransformZ;
            if (CondensedView)
            {
                if (Linerenderer.sharedMaterial != null)
                    Linerenderer.sharedMaterial.hideFlags = HideFlags.HideInInspector;
                Linerenderer.hideFlags = HideFlags.HideInInspector;
            }
            else
            {
                if (Linerenderer.sharedMaterial != null)
                    Linerenderer.sharedMaterial.hideFlags = HideFlags.None;
                Linerenderer.hideFlags = HideFlags.None;
            }
            transform.localScale = new Vector3(1,1,1);
            Linerenderer.enabled = path. ShowPathOnSimulation;

        }

        void Reset()
        {
            Init();
        }

        void Awake()
        {
            Init();
        }

        public void Hide()
        {
            transform.GetComponent<LineRenderer>().enabled = false;
        }
        public void Draw()
        {
            Vector3 dir;
            var line = this.GetComponentInParent<SimulationPath>();
            var rend = line;
           if (line.gameObject.GetComponent<Curve>() != null )
            {
                dir = path.GetDirection(0.5f);
            }
            else
            {
                if (name == "Start")
                {
                     dir = path.GetDirection(0);
                }
                else
                {
                    dir = path.GetDirection(1);
                }
            }
            Linerenderer.useWorldSpace = true;
            Linerenderer.SetPosition(0, (transform.position)- (dir * Size)/2) ;
            Linerenderer.SetPosition(1, (transform.position )+ (dir* Size)/2);
            Linerenderer.startWidth = Size;
            Linerenderer.endWidth = 0;
            Linerenderer.material = Material;
            
            
            
        }

        void Update()
        {
            if (transform.hasChanged)
                Draw();
        }
    }
}
