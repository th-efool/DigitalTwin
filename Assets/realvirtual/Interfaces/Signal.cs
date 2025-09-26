// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace realvirtual
{
    [Serializable]
    //! Struct for Settings of Signals
    public struct SettingsSignal
    {
        [Tooltip("Only implemented for some special interfaces - please check the doc")]
        public bool Active;
        public bool DetectBoolFlanks;
        public bool Override;
    }

    [Serializable]
    //! Struct for current status of a bool signal
    public struct StatusBool
    {
        public bool Connected;
        public bool ValueOverride;
        public bool Value;
        [HideInInspector] public bool OldValue;
    }

    [Serializable]
    //! Struct for current status of a float signal
    public struct StatusFloat
    {
        public bool Connected;
        public float ValueOverride;
        public float Value;
        [HideInInspector] public float OldValue;
    }

    [Serializable]
    //! Struct for current status of a omt signal
    public struct StatusInt
    {
        public bool Connected;
        public int ValueOverride;
        public int Value;
        [HideInInspector] public int OldValue;
    }

    [Serializable]
    //! Struct for current status of a text signal
    public struct StatusText
    {
        public bool Connected;
        public string ValueOverride;
        public string Value;
        [HideInInspector] public string OldValue;
    }

    [Serializable]
    //! Struct for current status of a bool signal
    public struct StatusTransform
    {
        public bool Connected;

        [InfoBox("Value is taken from Tanform postion if not overwritten!")]
        public Pose Value;

        public Pose ValueOverride;

        [HideInInspector] public Pose OldValue;
    }


    [Serializable]
    //! Class for saving connection information for signal - Behavior where signal is connected tp and property  where signal is connected to
    public class Connection
    {
        public GameObject Behavior;
        public string ConnectionName;
    }

    //! The base class for all Signals
    public class Signal : realvirtualBehavior, IInspector
    {
        public delegate void OnSignalChangedDelegate(Signal obj);

        [rvPlanner] public string Comment;
        [rvPlanner] public string OriginDataType;
        public SettingsSignal Settings;
        public SignalEvent EventSignalChanged;
        [HideInInspector] public bool Autoconnected;

        [HideInInspector]
        public bool UpdateEnable; // Turns on the Update function - for some signals (Transforms) needed

        [HideInInspector] public List<Connection> ConnectionInfo = new();

        [HideInInspector]
        public InterfaceSignal interfacesignal;
        protected string Visutext;

        private void Start()
        {
            SignalChangedEvent(this);
            if (EventSignalChanged != null)
            {
                if (EventSignalChanged.GetPersistentEventCount() == 0)
                    enabled = false;
                else
                    enabled = true;
            }
            else
            {
                enabled = false;
            }
            if (Settings.DetectBoolFlanks)
                enabled = true;

            if (UpdateEnable)
                enabled = true;
        }
        #if REALVIRTUAL_PLANNER
                public void ChangeSignalType(Signal newComponent)
                {
                    newComponent.Name = Name;
                    newComponent.Comment = Comment;
                    newComponent.OriginDataType = OriginDataType;
                    newComponent.StartDelayedInspectorRoutine();
                    DestroyImmediate(this);
            
                }

                public void StartDelayedInspectorRoutine()
                {
                    StartCoroutine(OpenDelayedInspector());
                }

                IEnumerator OpenDelayedInspector()
                {
                    yield return null;
                    rvUIInspectorWindow.Open(gameObject);
                    PlannerSignalBrowser.Open();
                    yield return null;
                }

      
          [rvInspectorButton(ButtonLabel = "To Bool")] public void ToBool()
                {
                    if (IsInput())
                    {
                        if (GetType() == typeof(PLCInputBool))
                            return;
                
                        ChangeSignalType(gameObject.AddComponent<PLCInputBool>());
                    }
                    else
                    {
                        if (GetType() == typeof(PLCOutputBool))
                            return;
                
                        ChangeSignalType(gameObject.AddComponent<PLCOutputBool>());
                    }
            
                }
        
                [rvInspectorButton(ButtonLabel = "To Int")] public void ToInt()
                {
                    if (IsInput())
                    {
                        if (GetType() == typeof(PLCInputInt))
                            return;
                        ChangeSignalType(gameObject.AddComponent<PLCInputInt>());
                    }
                    else
                    {
                        if (GetType() == typeof(PLCOutputInt))
                            return;
                        ChangeSignalType(gameObject.AddComponent<PLCOutputInt>());
                    }
                }
        
                [rvInspectorButton(ButtonLabel = "To Float")] public void ToFloat()
                {
                    if (IsInput())
                    {
                        if (GetType() == typeof(PLCInputFloat))
                            return;
                        ChangeSignalType(gameObject.AddComponent<PLCInputFloat>());
                    }
                    else
                    {
                        if (GetType() == typeof(PLCOutputFloat))
                            return;
                        ChangeSignalType(gameObject.AddComponent<PLCOutputFloat>());
                    }
                }

                [rvInspectorButton(ButtonLabel = "Change Direction")]
                public void ChangeDirection()
                { 
                    var currenttype = GetType();
            
                    switch (currenttype.ToString())
                    {
                        case "realvirtual.PLCInputBool":
                            ChangeSignalType(gameObject.AddComponent<PLCOutputBool>());
                            break;
                        case "realvirtual.PLCOutputBool":
                            ChangeSignalType(gameObject.AddComponent<PLCInputBool>());
                            break;
                        case "realvirtual.PLCInputFloat":
                            ChangeSignalType(gameObject.AddComponent<PLCOutputFloat>());
                            break;
                        case "realvirtual.PLCOutputFloat":
                            ChangeSignalType(gameObject.AddComponent<PLCInputFloat>());
                            break;
                        case "realvirtual.PLCInputInt":
                            ChangeSignalType(gameObject.AddComponent<PLCOutputInt>());
                            break;
                        case "realvirtual.PLCOutputInt":
                            ChangeSignalType(gameObject.AddComponent<PLCInputInt>());
                            break;
                    }
                }
        
                [rvInspectorButton(ButtonLabel = "Delete")]
                public void DeleteSignal()
                {
                    DestroyImmediate(gameObject);
                    rvUIInspectorWindow.Close();
                    PlannerSignalBrowser.Open();
                }
        #endif
                public void OnInspectValueChanged()
                {
        #if REALVIRTUAL_PLANNER
                    gameObject.name = _name;
                    base.Name = _name;
                    PlannerSignalBrowser.Open();
                    rvUIInspectorWindow.Open(gameObject);
        #endif
                }

        public bool OnObjectDrop(Object reference)
        {
            return true;
        }

        public void OnInspectedToggleChanged(bool arg0)
        {
        }

        public event OnSignalChangedDelegate SignalChanged;
        protected void SignalChangedEvent(Signal signal)
        {
            if (SignalChanged != null)
                SignalChanged(signal);
        }
        
        protected new bool hidename()
        {
            return false;
        }

        public string GetSignalName()
        {
            if (Name == "") return name;
            return Name;
        }

        //!  Virtual for getting the text for the Hierarchy View
        public virtual string GetVisuText()
        {
            return "not implemented";
        }

        public virtual byte[] GetByteValue()
        {
            return null;
        }

        public virtual int GetByteSize()
        {
            return -1;
        }


        //! Virtual for getting information if the signal is an Input
        public virtual bool IsInput()
        {
            return false;
        }

        //! Virtual for setting the value
        public virtual void SetValue(string value)
        {
        }

        public virtual void SetValue(byte[] value)
        {
        }


        //! Virtual for toogle in hierarhy view
        public virtual void OnToggleHierarchy()
        {
        }

        //! Virtual for setting the Status to connected
        public virtual void SetStatusConnected(bool status)
        {
        }

        //! Sets the value of the signal
        public virtual void SetValue(object value)
        {
        }
        //! Unforces the signal
        public void Unforce()
        {
            Settings.Override = false;
            EventSignalChanged.Invoke(this);
            SignalChangedEvent(this);
        }

        //! Gets the value of the signal
        public virtual object GetValue()
        {
            return null;
        }

        //! Virtual for getting the connected Status
        public virtual bool GetStatusConnected()
        {
            return false;
        }

        public void DeleteSignalConnectionInfos()
        {
            ConnectionInfo.Clear();
        }

        public void AddSignalConnectionInfo(GameObject behavior, string connectionname)
        {
            var element = new Connection();
            element.Behavior = behavior;
            element.ConnectionName = connectionname;

            var item = ConnectionInfo.FirstOrDefault(o => o.Behavior == behavior);
            if (item == null)
                ConnectionInfo.Add(element);
            if (IsInput())
                if (ConnectionInfo.Count > 1)
                {
                    //   Error("PLCInput Signal is connected to more than one behavior model, this is not allowed", this);
                }
        }

        //! Returns true if InterfaceSignal is connected to any Behavior Script
        public bool IsConnectedToBehavior()
        {
            if (ConnectionInfo.Count > 0)
                return true;
            return false;
        }

        //! Returns the type of the Signal as a String
        public string GetTypeString()
        {
            // returns a string with the type of the signal
            var type = GetType().ToString();
            switch (type)
            {
                case "realvirtual.PLCInputText":
                    return "TEXT";
                case "realvirtual.PLCInputBool":
                    return "BOOL";
                case "realvirtual.PLCOutputBool":
                    return "BOOL";
                case "realvirtual.PLCInputFloat":
                    return "FLOAT";
                case "realvirtual.PLCOutputFloat":
                    return "FLOAT";
                case "realvirtual.PLCInputInt":
                    return "INT";
                case "realvirtual.PLCOutputInt":
                    return "INT";
                case "realvirtual.PLCOutputText":
                    return "TEXT";
                case "realvirtual.PLCInputTransform":
                    return "TRANSFORM";
                case "realvirtual.PLCOutputTransform":
                    return "TRANSFORM";
            }

            return "";
        }

        //! Returns an InterfaceSignal Object based on the Signal Component
        public InterfaceSignal GetInterfaceSignal()
        {
            var newsignal = new InterfaceSignal();
            newsignal.OriginDataType = OriginDataType;
            newsignal.Name = name;
            newsignal.SymbolName = Name;
            newsignal.Signal = this;
            var type = GetType().ToString();
            switch (type)
            {
                case "realvirtual.PLCInputText":
                    newsignal.Type = InterfaceSignal.TYPE.TEXT;
                    newsignal.Direction = InterfaceSignal.DIRECTION.INPUT;
                    break;
                case "realvirtual.PLCInputBool":
                    newsignal.Type = InterfaceSignal.TYPE.BOOL;
                    newsignal.Direction = InterfaceSignal.DIRECTION.INPUT;
                    break;
                case "realvirtual.PLCOutputBool":
                    newsignal.Type = InterfaceSignal.TYPE.BOOL;
                    newsignal.Direction = InterfaceSignal.DIRECTION.OUTPUT;
                    break;
                case "realvirtual.PLCInputFloat":
                    newsignal.Type = InterfaceSignal.TYPE.REAL;
                    newsignal.Direction = InterfaceSignal.DIRECTION.INPUT;
                    break;
                case "realvirtual.PLCOutputFloat":
                    newsignal.Type = InterfaceSignal.TYPE.REAL;
                    newsignal.Direction = InterfaceSignal.DIRECTION.OUTPUT;
                    break;
                case "realvirtual.PLCInputInt":
                    newsignal.Type = InterfaceSignal.TYPE.INT;
                    newsignal.Direction = InterfaceSignal.DIRECTION.INPUT;
                    break;
                case "realvirtual.PLCOutputInt":
                    newsignal.Type = InterfaceSignal.TYPE.INT;
                    newsignal.Direction = InterfaceSignal.DIRECTION.OUTPUT;
                    break;
                case "realvirtual.PLCOutputText":
                    newsignal.Type = InterfaceSignal.TYPE.TEXT;
                    newsignal.Direction = InterfaceSignal.DIRECTION.OUTPUT;
                    break;
                case "realvirtual.PLCInputTransform":
                    newsignal.Type = InterfaceSignal.TYPE.TRANSFORM;
                    newsignal.Direction = InterfaceSignal.DIRECTION.INPUT;
                    break;
                case "realvirtual.PLCOutputTransform":
                    newsignal.Type = InterfaceSignal.TYPE.TRANSFORM;
                    newsignal.Direction = InterfaceSignal.DIRECTION.OUTPUT;
                    break;
            }

            return newsignal;
        }
    }
}