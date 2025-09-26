// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace realvirtual
{
    public class UIGroupElement : realvirtualBehavior
    {
        public GroupManager GroupManager;
        public bool DisableGameObjectOnHide = true;
        public bool DisableMeshOnHide=false;
        public bool InactiveOnStart = false;
        public string GroupName;
        public new TMP_Text tag;
        public List<GameObject> Objects = new List<GameObject>();
        
        [ReadOnly]public bool IsVisible = true;
        
        public rvUIToolbarButton ButtonMesh;
        public rvUIToolbarButton ButtonGO;
        
        private rvUIToolbarButton currentButton;
        
        public void OnButtonVisibility()
        {
            if (IsVisible)
            {
                CheckHideGroup(false);
                IsVisible = false;
            }
            else
            {
                CheckHideGroup(true);
                IsVisible = true;
            }
           
        }
        public void SetParameter(string groupname)
        {
            // get components in children also inactive
           currentButton = null;
            
            if(DisableMeshOnHide)
            {
                ButtonGO.gameObject.SetActive(false);
                ButtonMesh.gameObject.SetActive(true);
                currentButton = ButtonMesh;
            }
            if (DisableGameObjectOnHide)
            {
                ButtonGO.gameObject.SetActive(true);
                ButtonMesh.gameObject.SetActive(false);
                currentButton = ButtonGO;
            }
            GroupName = groupname;
            tag.text = groupname;
            if (InactiveOnStart || !IsVisible)
            {
                CheckHideGroup(false);
                currentButton.SetStatus(true);
                IsVisible = false;
            }
            else
            {
                CheckHideGroup(true);
                currentButton.SetStatus(false);
                IsVisible = true;
            }
        }

        public void UpdateButton(bool active)
        {
            if (active)
            {
                currentButton.SetStatus(false);
                IsVisible = true;
            }
            else
            {
                currentButton.SetStatus(true);
                IsVisible = false;
            }
        }

        private void CheckHideGroup(bool show)
        {
            if (!show)
            {
                GroupManager.UpdateGroup(GroupName,Objects,false,true);
            }
            else
            {
                GroupManager.UpdateGroup(GroupName,Objects,true,true);
            }
        }
    }
}
