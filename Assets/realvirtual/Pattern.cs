// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
namespace realvirtual
{
    //! Pattern component for generating arrays of GameObjects in circular or matrix configurations.
    //! Creates multiple instances of a template object arranged in circular patterns around an axis or rectangular grids.
    //! Useful for modeling repetitive industrial structures like palletizers, storage racks, or circular conveyors.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/pattern")]
    public class Pattern : MonoBehaviour
    {
        [Header ("Circular")]
        [Tooltip("Enable circular pattern generation")]
        public bool CircularPattern;
        [Tooltip("Angle between pattern components")] [ShowIf("CircularPattern")] public float DeltaAngle;
        [Tooltip("Direction Vector of rotation from pivot point on")] [ShowIf("CircularPattern")] public Vector3 RotationVector;
        [Tooltip("Start Angle for pattern")] [ShowIf("CircularPattern")] public float StartAngle;
        [Tooltip("Number of Components on Pattern (including Template")] [ShowIf("CircularPattern")] public float Number;

        [Header("Matrix")]
        [Tooltip("Enable matrix/grid pattern generation")]
        public bool MatrixPattern;
        [Tooltip("Number in X Direction")] [ShowIf("MatrixPattern")] public float NumberX;
        [Tooltip("Number in Y Direction")] [ShowIf("MatrixPattern")] public float NumberY;
        [Tooltip("Number in Z Direction")] [ShowIf("MatrixPattern")] public float NumberZ;
        [Tooltip("Distance in X Direction")] [ShowIf("MatrixPattern")] public float DistanceX;
        [Tooltip("Distance in Y Direction")] [ShowIf("MatrixPattern")] public float DistanceY;
        [Tooltip("Distance in Z Direction")] [ShowIf("MatrixPattern")] public float DistanceZ;
        
        [HideInInspector] public List<GameObject> PatternObjects;
        [Header ("Settings")]
        [Tooltip("If null, template is this Gameobject")] public GameObject Template;
        [Tooltip("If null, Parent is this Gameobject")] public GameObject Parent;
        [Tooltip("Automatically generate pattern when scene starts")]
        public bool GenerateOnStart;
        
        [Button("Delete Pattern")]
        void Delete()
        {
            foreach (var po in PatternObjects)
            {
                if (po != null)
                    DestroyImmediate(po);
            }
            PatternObjects.Clear();
        }

        private GameObject NewGo(GameObject template, string index)
        {
            GameObject newgo;

            GameObject tem = null;
            if (Template == null)
            {
                newgo = Instantiate(template);
                tem = this.gameObject;
            }

            else
            {
                newgo = Instantiate(Template);
                tem = Template;
            }
               
            newgo.transform.parent = this.transform;
            var templatepos = tem.transform.position;
            var templaterot = tem.transform.rotation;
            newgo.transform.position = templatepos;
            newgo.transform.rotation = templaterot;
            newgo.transform.localScale = tem.transform.localScale;
            newgo.name = this.name + "_" + index;
            if (Parent != null)
                newgo.transform.parent = Parent.transform;
            if (newgo.GetComponent<Pattern>())
                DestroyImmediate(newgo.GetComponent<Pattern>());
            PatternObjects.Add(newgo);
            newgo.SetActive(true);
            return newgo;
        }
        
        [Button("Generate Pattern")]
        void Generate()
        {
            Delete();
            var template = NewGo(this.gameObject,"template");
            if (CircularPattern)
            {
                for (int i = 0; i < Number; i++)
                {
                    var go = NewGo(template,i.ToString());
                    var rotpoint = this.transform.position;
                    var axis = this.transform.TransformDirection(RotationVector);
                    var angle = StartAngle + i * DeltaAngle;
                    go.transform.RotateAround(rotpoint,axis,angle);
                }
            }

            if (MatrixPattern)
            {
                for (int x = 0; x <  NumberX; x++)
                {
                    for (int y = 0; y <  NumberY; y++)
                    {
                        for (int z= 0; z <  NumberZ; z++)
                        {
                            var go = NewGo(template,x + "_" + y + "_" + z);
                                var pos = new Vector3(x * DistanceX, y * DistanceY, z * DistanceZ);
                                go.transform.localPosition = pos;
                        }
                    }
                }
            }
            DestroyImmediate(template);
        }
        
        // Start is called before the first frame update
        void Start()
        {
            if (GenerateOnStart)
                Generate();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

