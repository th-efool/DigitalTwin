// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;

namespace realvirtual
{
#pragma warning disable 0414

    
    public class PlayGroupWindow : realvirtualBehavior
    {
        public GroupManager GroupManager;
        public GameObject GroupViewport;
        public GameObject Content;
        public bool ActiveOnStart = false;
        
        private List<UIGroupElement> currentGroups = new List<UIGroupElement>();
        private GameObject groupGO;
        
        public void OnEnable()
        {
           // SetWindowactive(true);
        }
        public void OnDisable()
        {
            SetWindowactive(false);
        }
        public void SetWindowactive(bool active)
        {
            DestroyGroups();
            if (active)
            {
                CollectGroupInfos();
            }
            Content.SetActive(active);
        }
        public List<UIGroupElement> GetCurrentGroups()
        {
            return currentGroups;
        }
        public void OnToolbarButtonClicked()
        {
           if(Content.activeSelf)
               SetWindowactive(false);
           else
               SetWindowactive(true);
        }
       
        private void CollectGroupInfos()
        {
            // get all components within the scene of type signal
            var groups = GroupManager.GetAvailableGroupList();
            foreach (var gr in groups)
            {
                string groupName = gr.GetGroupName();
                var setting=GroupManager.GetGroupSetting(groupName);
                var objs = GroupManager.GetGOlist(groupName);
                groupGO = CreateGroupUI(this.transform.position, this.transform.rotation, GroupViewport);
                groupGO.transform.localScale= new Vector3(1, 1, 1);
                groupGO.transform.name = groupName;
                
                UIGroupElement groupElement = groupGO.GetComponent<UIGroupElement>();
                
                groupElement.GroupManager = GroupManager;
                groupElement.DisableMeshOnHide = setting.DisableMeshOnHide;
                groupElement.DisableGameObjectOnHide = setting.DisableGameObjectOnHide;
                groupElement.InactiveOnStart = setting.InactiveOnStart;
                groupElement.IsVisible = setting.Active;
                groupElement.Objects = objs;
                groupElement.SetParameter(groupName);
                currentGroups.Add(groupElement);
            }
        }
        private void DestroyGroups()
        {
            foreach (var signal in currentGroups)
            {
                Destroy(signal.gameObject);
            }
            currentGroups.Clear();
        }
        private GameObject CreateGroupUI(Vector3 position, Quaternion rotation, GameObject obj)
        {
            if(groupGO == null) 
                groupGO=UnityEngine.Resources.Load<GameObject>("UIGroupElement");
            
            GameObject GroupInfo=(GameObject) Object.Instantiate(groupGO, position, rotation);
            if(obj!=null)
                GroupInfo.transform.SetParent(obj.transform);
            return GroupInfo; 
        }
    }
}
