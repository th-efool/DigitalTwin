// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Linq;
using UnityEngine;


namespace realvirtual
{
    //! Group component for organizing GameObjects into logical collections within the automation system.
    //! Enables filtering in hierarchy views and dynamic kinematic restructuring through the Kinematic component.
    //! Supports prefix-based naming for reusable prefabs and multiple group assignments per GameObject.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/group")]
    public class Group : realvirtualBehavior
    {
        // Start is called before the first frame update
        [Tooltip("Name of the group for filtering and kinematic structure")]
        public string GroupName; //!< The Group name
        [Tooltip("GameObject whose name is used as prefix for the group name (useful for reusable prefabs)")]
        public GameObject GroupNamePrefix; //!< A prefix for the Groupname (used for using Groups in reusable Prefabs)

        // Gets the Groupname
        public string GetGroupName()
        {
            if (GroupNamePrefix!=null)
                return (GroupNamePrefix.name + GroupName);
            else
                return GroupName;
        }
        
        // Gets the text for the hierarchy view
        public string GetVisuText()
        {
            string text = "";
            // Collect all groups
            var groups = GetComponents<Group>().ToArray();

            for (int i = 0; i < groups.Length; i++)
            {
                if (i != 0)
                    text = text + "/";
                text = text + groups[i].GetGroupName();
            }

            return text;
        }

        
        private new void Awake()
        {
            
        }


        public void ChangeConnectionMode()
        {
            
        }
        
    }
}