using System;
using UnityEngine;

namespace realvirtual
{
    public class rvPlanner : Attribute
    {
        public string Name = "";
    }

    public class rvInspectorButton : Attribute
    {
        public string ButtonLabel = "";
    }

    public class PlannerAttributes : MonoBehaviour
    {
    }
}