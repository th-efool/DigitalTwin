// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace realvirtual
{
    [Serializable]
    public class GroupSettings
    {
        [Dropdown("GetGroupNames")]
        public string GroupName;
        public bool DisableGameObjectOnHide=true;
        // make it read only if DisableGameObjectOnHide is true
        [ShowIf("DisableGameObjectOnHide",true)]
        public bool DisableMeshOnHide;
        public bool InactiveOnStart=true;
        [HideInInspector]public bool Active;
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
    public class GroupManager : MonoBehaviour
    {
        
       // public bool OnlyVisualGroups=false;
        public PlayGroupWindow PlayGroupWindow;
        public rvUIToolbarButton ToolbarButton;
        public List<GroupSettings> VisibleGroups=new List<GroupSettings>();
       
        
        private List<GameObject> _hiddenGroups=new List<GameObject>();
        private List<MeshRenderer> _hiddenMeshes=new List<MeshRenderer>();
        private Hashtable _visibleGroupsHash=new Hashtable();
        private Hashtable allGroups=new Hashtable();
        private List<GameObject> activeGroupGO = new List<GameObject>();
        private List<ButtonShowGroup> _groupConnections=new List<ButtonShowGroup>();
        private List<UIGroupElement> _groupElements=new List<UIGroupElement>();

        private void Awake()
        {
            ShowAll();
            GetAllGroups();
            _visibleGroupsHash = GetAllVisibleGroups();
            UpdateGroupSettings();
            if(VisibleGroups.Count==0)
            {
                ToolbarButton.gameObject.SetActive(false);
            }
            else
            {
                ToolbarButton.gameObject.SetActive(true);
            }
            
        }
        [Button("Show All")]
        public void ShowAll()
        {
            ResetHiddenGroups();
            foreach (var groupsetting in VisibleGroups)
            {
                groupsetting.Active = true;
            }
            UpdateGroupSettings();
        }
        // Interface method for hmi connection
        public void ShowVisualGroup(string groupname)
        {
            
            UpdateGroupSettings();
        }
        public void UpdateGroupSettings()
        {
            ResetHiddenGroups();
            Hashtable availableGroups;
            availableGroups = GetAllGroups();
            if (VisibleGroups.Count != 0)
            {
                activeGroupGO.Clear();
                Hashtable GroupGOToHide = new Hashtable();
                _visibleGroupsHash = GetAllVisibleGroups();
                foreach (DictionaryEntry group in availableGroups)
                {
                    if(_visibleGroupsHash.ContainsKey(group.Key))
                    {
                     
                            if (Application.isPlaying)
                            {
                                var setting=_visibleGroupsHash[group.Key] as GroupSettings;
                                if(setting.InactiveOnStart)
                                {
                                    
                                    CheckHideGroup(_visibleGroupsHash[group.Key] as GroupSettings,
                                        (group.Value as List<Group>)[0], false);
                                }
                                else
                                {
                                    CheckHideGroup(_visibleGroupsHash[group.Key] as GroupSettings,
                                        (group.Value as List<Group>)[0], true);
                                    
                                }
                                activeGroupGO.Add((group.Value as List<Group>)[0].gameObject);
                            }
                            else
                            {
                                foreach (var groupElement in group.Value as List<Group>)
                                {
                                    CheckHideGroup(_visibleGroupsHash[group.Key] as GroupSettings, groupElement, true);
                                    activeGroupGO.Add(groupElement.gameObject);
                                }
                            }
                    }
                }
                foreach (DictionaryEntry groupElement in GroupGOToHide)
                {
                    GameObject go=(groupElement.Key as Group).gameObject;
                    if(!activeGroupGO.Contains(go))
                        CheckHideGroup(groupElement.Value as GroupSettings, groupElement.Key as Group , false);
                }
            }
        }
        public bool CheckManagerActive(string groupname,ButtonShowGroup userButton)
        {
            if (_visibleGroupsHash.ContainsKey(groupname))
            {
                if(!_groupConnections.Contains(userButton))
                    _groupConnections.Add(userButton);
                return true;
            }
            return false;
        }
        
     
        public static List<Kinematic> GetAllGroupsConnectedtoKinematic()
        {
            var kinematicComponents = FindObjectsByType<Kinematic>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(k => k.IntegrateGroupEnable).ToList();         
            return kinematicComponents;
        }
        public static List<string> GetAllVisualGroups()
        {
            List<string> allVisualGroups = new List<string>();
            #if UNITY_EDITOR
            allVisualGroups = UnityEngine.Resources.FindObjectsOfTypeAll<Group>()
                    .Where(group => !EditorUtility.IsPersistent(group.transform.root.gameObject))
                    .Select(group => (group.GetGroupName()))
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();
           #endif

            return allVisualGroups;
        }

        public List<Group> GetAvailableGroupList()
        {
            List<Group> availableGroups = new List<Group>();
            allGroups = GetAllGroups();
            foreach (DictionaryEntry group in allGroups)
            {
                if (_visibleGroupsHash.ContainsKey(group.Key.ToString()))
                {
                    var obj=group.Value as List<Group>;
                    availableGroups.Add(obj[0]);
                }
                
            }
            return availableGroups;
        }
        public GroupSettings GetGroupSetting(string groupname)
        {
           GroupSettings setting=_visibleGroupsHash[groupname] as GroupSettings;
           return setting;
        }
        public List<GameObject> GetGOlist(string groupname)
        {
            List<GameObject> goList = new List<GameObject>();
           
            if (allGroups.ContainsKey(groupname))
            {
                foreach (var group in (List<Group>)allGroups[groupname])
                {
                    goList.Add(group.gameObject);
                }
            }
            return goList;
        }
        public void UpdateGroup(string groupname,List<GameObject>Objects, bool active,bool callFromWindow)
        {
            GroupSettings currentSetting = _visibleGroupsHash[groupname] as GroupSettings;
            if(!active)
            {
                if (currentSetting.DisableGameObjectOnHide)
                {
                    foreach (var obj in Objects)
                    {
                        if (!CheckParallelGroupActive(obj, groupname))
                            obj.SetActive(false);
                    }
                }
                if (currentSetting.DisableMeshOnHide)
                {
                    foreach (var obj in Objects)
                    {
                        if (!CheckParallelGroupActive(obj, groupname))
                        {
                            var meshRenderer = obj.GetComponent<MeshRenderer>();
                            if (meshRenderer != null && !CheckParallelGroupActive(obj, groupname))
                            {
                                meshRenderer.enabled = false;
                            }
                        }
                       
                    }
                }
            }
            else
            {
                if(currentSetting.DisableGameObjectOnHide)
                {
                    foreach (var obj in Objects)
                        obj.SetActive(true);
                }
                if(currentSetting.DisableMeshOnHide)
                {
                    foreach (var obj in Objects)
                    {
                        var meshRenderer = obj.GetComponent<MeshRenderer>();
                        if (meshRenderer != null && !CheckParallelGroupActive(obj, groupname))
                        {
                            obj.GetComponent<MeshRenderer>().enabled = true;
                        }
                    }
             
                }
            }
            currentSetting.Active = active;
            if(callFromWindow)
            {
                foreach (var groupConnection in _groupConnections)
                {
                    if (groupConnection.GroupName == groupname)
                    {
                        groupConnection.CheckGroupStatus();
                    }
                }
            }
            else
            {
                var uigroups = PlayGroupWindow.GetCurrentGroups();
                foreach (var group in uigroups)
                {
                    if(group.GroupName==groupname)
                    {
                        group.UpdateButton(active);
                    }
                  
                }
            }
        }
        public bool CheckParallelGroupActive(GameObject group, string groupname)
        {
            bool activeByAnotherGroup = false;
            foreach (DictionaryEntry GOlist in allGroups)
            {
                string key = GOlist.Key.ToString();
                if(key!=groupname)
                {
                    List<GameObject> GroupGOlist = GetGOlist(GOlist.Key.ToString());

                    if ((GroupGOlist.Contains(group)))
                    {
                        if (_visibleGroupsHash.ContainsKey(GOlist.Key))
                        {
                            if (((GroupSettings)_visibleGroupsHash[GOlist.Key]).Active)
                            {
                                activeByAnotherGroup = true;
                                break;
                            }
                        }
                    }
                }
            }
            return activeByAnotherGroup;
        }
        private void CheckHideGroup(GroupSettings groupsetting, Group groupElement,bool show)
        {
            if(!show)
            {
                if (groupsetting.DisableGameObjectOnHide)
                {
                    groupElement.gameObject.SetActive(false);
                    _hiddenGroups.Add(groupElement.gameObject);
                    groupsetting.Active = false;
                }
                else if (groupsetting.DisableMeshOnHide)
                {
                    foreach (var mesh in groupElement.gameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        mesh.enabled = false;
                        _hiddenMeshes.Add(mesh);
                        groupsetting.Active = false;
                    }
                }
            }
            else
            {
                if (groupsetting.DisableGameObjectOnHide)
                    groupElement.gameObject.SetActive(true);
                if(groupsetting.DisableMeshOnHide)
                {
                    foreach (var mesh in groupElement.gameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        mesh.enabled = true;
                    }
                }
                groupsetting.Active = true;
            }
        }
        private Hashtable GetAllGroups() // index groupname , value list of gameobjects with the same groupname
        {
            allGroups.Clear();
            var sourcedata= FindObjectsByType<Group>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<Kinematic> kinematicGroups = GetAllGroupsConnectedtoKinematic();
            List<string> kinematicGroupNames = kinematicGroups.Select(k => k.GetGroupName()).ToList();
            List<Group> VisualGroup;
           /* if(OnlyVisualGroups)
            {
                VisualGroup = sourcedata
                    .Where(group => !kinematicGroupNames.Contains(group.GetGroupName()))
                    .ToList();
            }
            else
            {*/
                VisualGroup = sourcedata.ToList();
            //}
            
            foreach (var group in VisualGroup)
            {
                var groupName = group.GetGroupName();
                if(!allGroups.ContainsKey( groupName))
                    allGroups[groupName]=new  List<Group>();
                
                ((List<Group>)allGroups[groupName]).Add(group);
            }
            return allGroups;
        }
        private Hashtable GetAllVisibleGroups()
        {
            Hashtable visibleGroups = new Hashtable();
            foreach (var groupsetting in VisibleGroups)
            {
                if(!visibleGroups.ContainsKey(groupsetting.GroupName))
                    visibleGroups.Add(groupsetting.GroupName,groupsetting);
            }
            return visibleGroups;
        }
        private void ResetHiddenGroups()
        {
            foreach (var obj in _hiddenGroups)
            {
                obj.SetActive(true);
            }
            _hiddenGroups.Clear();
            foreach (var mesh in _hiddenMeshes)
            {
                mesh.enabled = true;
            }
            _hiddenMeshes.Clear();
        }
    }
}
