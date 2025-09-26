using System.Collections;
using System.Collections.Generic;

using NaughtyAttributes;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

#pragma warning disable 0219
namespace realvirtual
{
    //! PathChain implements continuous chain-driven transport systems following complex path networks.
    //! This component enables simulation of chain conveyors, power-and-free systems, and circular transport loops
    //! where the chain follows a closed path through multiple connected SimulationPath segments.
    //! Automatically calculates total chain length from connected path elements and manages continuous movement
    //! around the circuit, making it ideal for overhead conveyors, assembly line carriers, and continuous
    //! material flow systems in automotive, electronics, and heavy industry applications.
    [RequireComponent(typeof(Chain))]
    public class PathChain : realvirtualBehavior, IChain
    {
        public SimulationPath Startelement;
        [InfoBox("The path for the chain is automatically detected by all connected Path Elements. You need to define the first element (StartElement). The length of the chain is calculated by the sum of the lengths of the SimulationPath elements.")]
        [ReadOnly][FormerlySerializedAs("pathforChain")] public List<SimulationPath> PathForChain = new List<SimulationPath>();

        private SimulationPath currPath;

        [ReadOnly] public float Length;
        private float currlength;

        private float indexHash;
        private List<float> indexList = new List<float>();

        new void Awake()
        {
            currPath = Startelement;
            if (Length == 0)
            {
                Length = Startelement.Predecessors[0].GetLength();
                 do
                 {
                 Length = Length + (currPath.GetLength()*1000);
                 currPath = currPath.Successors[0];
                 } while (!currPath.Successors.Contains(Startelement));
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            if (indexList.Count == 0)
            {
                currPath = Startelement;
                do
                {
                    currlength = currlength + currPath.GetLength();
                    indexHash = currlength / Length;
                    PathForChain.Add(currPath);
                    indexList.Add(indexHash);
                    currPath = currPath.Successors[0];
                } while (!currPath.Successors.Contains(Startelement));

                currPath = Startelement.Predecessors[0];
                currlength = currlength + currPath.GetLength();
                indexHash = currlength / Length;
                PathForChain.Add(currPath);
                indexList.Add(indexHash);
            }
        }
        
        private SimulationPath GetCurrentPath(ref float normalizedposition)
        {
            int low = 0;
            int high = indexList.Count - 1;
            SimulationPath actPath = Startelement;
            float newpercent = 0f;
            float actlength = 0f;
            if (normalizedposition > 0.35f)
            {
               //actPath = Startelement;
                
            }
            while (low < high)
            {
                int mid = (low + high) / 2;
                if ( mid !=0 && normalizedposition <= indexList[mid] && normalizedposition >= indexList[mid-1])
                {
                    actPath = PathForChain[mid];
                    if (mid > 0)
                    {
                        newpercent = normalizedposition- indexList[mid - 1];
                    }
                    else
                    {
                        newpercent = normalizedposition- indexList[0];
                    }
                    actlength = (Length * newpercent)/1000;
                    normalizedposition = actlength/ actPath.Length;
                    low = high+1;
                }
                else
                {
                    if (normalizedposition > indexList[mid] )
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        if (mid == 0)
                            high = mid;
                        else
                            high = mid - 1;
                    }
                }
                if (low == high)
                {
                    actPath = PathForChain[low];
                    if (low == 0)
                    {
                        actlength = (Length * normalizedposition)/1000;
                    }
                    else
                    {
                        newpercent = normalizedposition- indexList[low - 1];
                        actlength = (Length * newpercent)/1000;
                    }
                    normalizedposition = actlength/ actPath.Length;
                }
            }
            return actPath;
        }

        public Vector3 GetClosestDirection(Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetClosestPoint(Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetPosition(float normalizedposition, bool normalized = true )
        {
            SimulationPath currpath = GetCurrentPath(ref normalizedposition);
           
            BasePath path = null;
            Vector3 Pos = currpath.GetPosition(normalizedposition, ref path);
            
            return Pos;
        }
        public Vector3 GetDirection(float normalizedposition, bool normalized = true)
        {
            SimulationPath currpath = GetCurrentPath(ref normalizedposition);
            
            BasePath path = null;
            Vector3 Dir = currpath.GetDirection(normalizedposition);
            
            return Dir;
        }

        public Vector3 GetUpDirection(float normalizedposition, bool normalized = true)
        {
            return currPath.GetDirection(normalizedposition);
        }

        public float CalculateLength()
        {
            Length = 0;
            currPath = Startelement;
            Length = Startelement.Predecessors[0].GetLength()*1000;
            do
            {
                Length = Length + (currPath.GetLength()*1000);
                currPath = currPath.Successors[0];
            } while (!currPath.Successors.Contains(Startelement));
            
            currPath = Startelement;
            currlength = 0;
            PathForChain.Clear();
            indexList.Clear();
            do
            {
                    currlength = currlength + (currPath.GetLength()*1000);
                    indexHash = currlength / Length;
                    PathForChain.Add(currPath);
                    indexList.Add(indexHash);
                    currPath = currPath.Successors[0]; 
            } while (!currPath.Successors.Contains(Startelement));
            
            currPath = Startelement.Predecessors[0];
            currlength = currlength + currPath.GetLength();
            indexHash = currlength / Length;
            PathForChain.Add(currPath);
            indexList.Add(indexHash);
            
            return Length;
        }
        public bool UseSimulationPath()
        {
            return true;
        }
    }
}
