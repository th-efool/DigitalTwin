using System;
using System.Collections.Generic;
using CRI_Client;
using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    public class igusREBELInterface : InterfaceThreadedBaseClass
    {
        public string Adress = "127.0.0.1";
        public int Port = 3921;
        public List<Drive> Drives = new List<Drive>();
        public int NumOutputs;
        public int NumInputs;
        
        private HardwareProtocolClient robotClient;
        private List<Signal> SignalsChanged = new List<Signal>();
        private List<Signal> OutputSignals = new List<Signal>();
        
        [Button("Connnect")]
        public override void OpenInterface()
        {
            try
            {
                // Create an instance of the robot interface
                robotClient = new HardwareProtocolClient();
                robotClient.IPAddress = Adress;
                robotClient.Port = Port;
                robotClient.Connect();
                var connected =robotClient.GetConnectionStatus();
                // check if connected
                if (connected)
                {
                    Debug.Log("igusREBEL Interface - connected to robot at IP Adress " + Adress);
                    OnConnected();
                    base.OpenInterface();
                }
                else
                {
                    Debug.LogError("igusREBEL Interface - Connection to robot at IP Adress " + Adress + " failed");
                }
                SignalsChanged.Clear();
                OutputSignals.Clear();
                // get all signals - after this call all signals under this gameobject are in the list of InterfaceSignals
                UpdateInterfaceSignals(ref NumInputs, ref NumOutputs);

                var numoutput = 20;
                var numinput = 0;
                // subscribe to all plcinputs to only send when changed
                foreach (var interfaceSignal in InterfaceSignals)
                {
                    if (interfaceSignal.Type == InterfaceSignal.TYPE.BOOL) // only bools for rebel available
                    {
                        if (interfaceSignal.Direction == InterfaceSignal.DIRECTION.OUTPUT)
                        {
                        
                            interfaceSignal.Mempos = numoutput;
                            numoutput++;
                            interfaceSignal.Signal.Comment= "DOut " + numoutput;
                            OutputSignals.Add(interfaceSignal.Signal);
                        }
                        else
                        {
                            interfaceSignal.Signal.SignalChanged += SignalOnSignalChanged;
                            interfaceSignal.Mempos = numinput;
                            interfaceSignal.Signal.Comment = "GSig " + numinput;
                            numinput++;
                        }

                        interfaceSignal.Signal.interfacesignal = interfaceSignal;
                    }
                  
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError("igusREBEL Interface - Connection to robot at IP Adress " + Adress + " failed");
                Debug.LogError(e);
            }
            
           
            // warning if not 6 drives
            if (Drives.Count != 6)
            {
                Debug.LogWarning("igusREBEL Interface - There are not 6 drives connected to the interface");
            }
        }

        private void SignalOnSignalChanged(Signal obj)
        {
            SignalsChanged.Add(obj);
        }

        public override void CloseInterface()
        {
            if (robotClient != null)
            {
                robotClient.Disconnect();
            }
            OnDisconnected();
            base.CloseInterface();
        }

        protected override void CommunicationThreadUpdate()
        {
            if (IsConnected == false)
                return;
            // get joint values
            for (int i = 0; i < 6; i++)
            {
                Drives[i].CurrentPosition = (float)robotClient.posJointsCurrent[i];
            }
            
            // get Inputs and Outputs
            var outputs = robotClient.digialOutputs;
            
            // loop through all outputsignals
            for (int i =20; i < 20+OutputSignals.Count; i++)
            {
                bool value = (outputs & (1UL << i)) != 0;
                OutputSignals[i-20].SetValue(value);
            }
            
            
            // loop through all inputs that have changed in SignalsChanged
            foreach (var signal in SignalsChanged)
            {
                var value = "false";
                if (((PLCInputBool)signal).Value)
                {
                    value = "true";
                }

                var command = "CMD GSIG " + signal.interfacesignal.Mempos + " " + value;
                Debug.Log(command);
                robotClient.SendCommand(command);
            }
            SignalsChanged.Clear();
            
            
        }

        private void FixedUpdate()
        {
       
        }
    }
}