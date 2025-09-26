// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{ public class ButtonShowGroup: MonoBehaviour
    {
        public GroupManager GroupManager;
        [Dropdown("GetGroupNames")] public string GroupName;
        [ReadOnly]public bool IsVisible = true;
        
        private bool _isManagedByGroupManager;
        private GroupSettings _groupSettings;
        private List<GameObject> Objects = new List<GameObject>();
        private rvUIToolbarButton _button;

        public void Start()
        {
            _button = GetComponent<rvUIToolbarButton>();
            if(GroupManager.CheckManagerActive(GroupName,this))
            {
                _isManagedByGroupManager = true;
                _groupSettings = GroupManager.GetGroupSetting(GroupName);
                Objects = GroupManager.GetGOlist(GroupName);
                if (_groupSettings.Active)
                {
                    IsVisible = true;
                    _button.SetStatus(false);
                }
                else
                {
                    IsVisible = false;
                    _button.SetStatus(true);
                }
            }
            else
            {
                Debug.LogWarning("Group is not managed by GroupManager");
            }
        }

        public void CheckGroupStatus()
        {
            if(_groupSettings.Active != IsVisible)
            {
                if (_groupSettings.Active)
                {
                    IsVisible = true;
                    _button.SetStatus(false);
                }
                else
                {
                    IsVisible = false;
                    _button.SetStatus(true);
                }
            }
        }

        public void OnButtonVisibility()
        {
            if (_isManagedByGroupManager)
            {
                if (IsVisible)
                {
                    CheckHideGroup(false);
                    _groupSettings.Active = false;
                    IsVisible = false;
                }
                else
                {
                    CheckHideGroup(true);
                    _groupSettings.Active = true;
                    IsVisible = true;
                }
            }
        }
        private void CheckHideGroup(bool show)
        {
            if (!show)
            {
                GroupManager.UpdateGroup(GroupName,Objects,false,false);
            }
            else
            {
                GroupManager.UpdateGroup(GroupName,Objects,true,false);
            }
        }
#if UNITY_EDITOR
        private List<string> GetGroupNames()
        {
            var Groups = GroupManager.GetAllVisualGroups();
            if (Groups == null)
                Groups = new List<string>();
            if (!Groups.Contains(GroupName))
            {
                Groups.Insert(0,GroupName);
            }
            return Groups;
        }
#endif 
    }
}
