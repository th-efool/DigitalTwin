using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    public class BuildPerformanceTestModel : MonoBehaviour
    {
        // Start is called before the first frame update
        public bool BuildOnStart = false;
        public GameObject TestCell;
        public int NumTestCells;
        public float DeltaX = 1;
        public float DeltaZ = 1;

        [Button("Delete Builds")]
        void DeleteBuilds()
        {
            var children = transform.GetComponentsInChildren<Transform>().ToArray();
            foreach (Transform child in children)
            {
                if (child != null)
                      if (this.transform != child)
                             DestroyImmediate(child.gameObject);
            }
        }

        [Button("Build")]
        void Build()
        {
            // Create TestCell with DeltaX and DeltaZ
            DeleteBuilds();
            GameObject testCell = null;
            for (int j = 0; j < NumTestCells; j++)
            {
                testCell = Instantiate(TestCell, new Vector3((j+1) * DeltaX, 0, (j+1) * DeltaZ), Quaternion.identity);
                testCell.name = "TestCell" + j;
                testCell.transform.parent = transform;
            }
            // Activate Last Sink
            if (testCell != null)
            {
                var go = testCell.GetComponentInChildren<Sink>(true);
                go.gameObject.SetActive(true);
            }
           
        }

        void Awake()
        {
            if (BuildOnStart)
                Build();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}