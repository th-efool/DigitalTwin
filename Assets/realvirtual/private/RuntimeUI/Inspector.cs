
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    public class Inspector : MonoBehaviour
    {
        public bool ShowInInspector = true;
        public bool ShowOnlyMarkedAttributes = false;
        public bool ShowOnlyDefinedComponents=true;
        public bool HideDefinedElements=false;
    
        public string HierarchyName;
        public string ComponentName;

        public List<Component> Elements;
        private realvirtualController realvirtualController;
    
        // Start is called before the first frame update
        void Start()
        {
            var g4a = GameObject.Find("realvirtual");
            if (g4a!=null)
                realvirtualController = g4a.GetComponent<realvirtualController>();

            if (ShowInInspector)
                InitInspector();
        }

        void InitInspector()
        {

            realvirtualController.InspectorController.Add(this);
        }
  
    }


}

