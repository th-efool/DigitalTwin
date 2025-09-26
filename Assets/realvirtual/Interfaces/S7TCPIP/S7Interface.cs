// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

// This product bundles Sharp7,  Copyright (C) 2016 Davide Nardella. It was
// individually licensed from Davide Nardella under the Apache 2.0 license.
// For details on Snap7 see http://snap7.sourceforge.net/ 

using System;
using UnityEngine;
using Sharp7;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;


namespace realvirtual
{
    //! S7 TCP/IP interface for direct communication with Siemens SIMATIC S7 PLCs (S7-300, S7-400, S7-1200, S7-1500).
    //! Establishes native S7 protocol connections over Ethernet without requiring additional OPC servers or middleware.
    //! Supports reading and writing data blocks (DB), memory areas (M), inputs (I), and outputs (Q) directly from the PLC.
    //! This interface is ideal for virtual commissioning with Siemens TIA Portal projects and PLCSIM Advanced simulations.
    //! Configure the PLC's IP address, rack and slot numbers to match your hardware configuration or PLCSIM setup.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/interfaces/s7-tcp")]
    public class S7Interface : InterfaceThreadedBaseClass
    {
        [rvPlanner]public string Adress; //!< The ip adress of the PLC
        [rvPlanner]public int Rack; //!< the rack number of the plc
        [rvPlanner]public int Slot; //!< the slot number of the plc
        [rvPlanner]public bool ReverseHighLowBytes = true;
        private int connectionstatus;
        [rvPlanner]public string SymbolTable; //!< The path to the symbol table of the PLC (*.seq text file)
        [rvPlanner]public int ReduceMaxPduLenth = 60;
        [rvPlanner]public float ReconnectingTime = 10; //!< Time in seconds for reconnecting if connection fails
        [HideIf("AreaReadWriteMode")] public bool OnlyWriteToPlcChangedSignals = true;
        [rvPlanner]public bool AreaReadWriteMode = false;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MinAreaMOutput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MaxAreaMOutput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MinAreaMInput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MaxAreaMInput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int DBOutputs;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MinAreaDBOutput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MaxAreaDBOutput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int DBInputs;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MinAreaDBInput;
        [rvPlanner][ShowIf("AreaReadWriteMode")] public int MaxAreaDBInput;

        [ReadOnly]
        public string ConnectionStatus; //!< The connection Status to the PLC - OK if everything is ok (ReadOnly)

        [ReadOnly] public string PLCStatus; //!< The Status of the PLC - running if PLC is running (ReadOnly)
        [ReadOnly] public int RequestedPduLength; //!< The requested PDU length (ReadOnly)
        [ReadOnly] public int NegotiatedPduLenght; //!< The negotiated PDU length (ReadOnly)
        [ReadOnly] public int NumberInputs; //!< The number of Inputs in the interface (ReadOnly)
        [ReadOnly] public int NumberOutputs; //!< The number of Outputs in the interface (ReadOnly)
        [ReadOnly] public ulong ThreadCycleNum;
        private S7Client Client;
        private S7MultiVar outputreader;
        private S7MultiVar inputwriter;
        private bool _isconnectedwrite;
        private bool _isconnectedread;
        private bool _isdisconnected;
        private float _lastreconnect;
        private int[] areamin;
        private int[] areamax;
        private byte[] inputarea;
        private byte[] outputarea;
        private byte[] moutputarea;
        private byte[] minputarea;
        private byte[] dboutputarea;
        private byte[] dbinputarea;
        private string commerror = "";
        private ulong lastthreadcyclenum;
        private bool firstwrite = false;

        private int GetArea(string name)
        {
            var toupper = name.ToUpper();
            int area = 0;
            switch (toupper[0])
            {
                case 'A':
                    area = S7Consts.S7AreaPA;
                    break;
                case 'Q':
                    area = S7Consts.S7AreaPA;
                    break;
                case 'E':
                    area = S7Consts.S7AreaPE;
                    break;
                case 'I':
                    area = S7Consts.S7AreaPE;
                    break;
                case 'M':
                    area = S7Consts.S7AreaMK;
                    break;
                case 'D':
                    area = S7Consts.S7AreaDB;
                    break;
                default:
                    Debug.LogError("Area for this variable [" + name + "] is not allowed!");
                    break;
            }

            return area;
        }


        //! Gets the S7 Signal type with the absolute signal name
        private S7InterfaceSignal.S7TYPE GetS7Type(string s7type)
        {
            var res = S7InterfaceSignal.S7TYPE.UNDEFINED;
            S7InterfaceSignal.S7TYPE type;

            if (Enum.TryParse(s7type.ToUpper(), out type))
            {
                res = type;
            }
            else
            {
                Debug.LogError("Type  [" + s7type + "] is not allowed!");
            }

            return res;
        }

        //! Gets the standard signal type based on the S7 type
        private InterfaceSignal.TYPE GetType(S7InterfaceSignal.S7TYPE s7type)
        {
            switch (s7type)
            {
                case S7InterfaceSignal.S7TYPE.BOOL:
                    return InterfaceSignal.TYPE.BOOL;

                case S7InterfaceSignal.S7TYPE.BYTE:
                case S7InterfaceSignal.S7TYPE.WORD:
                case S7InterfaceSignal.S7TYPE.DWORD:
                case S7InterfaceSignal.S7TYPE.SINT:
                case S7InterfaceSignal.S7TYPE.INT:
                case S7InterfaceSignal.S7TYPE.DINT:
                case S7InterfaceSignal.S7TYPE.USINT:
                case S7InterfaceSignal.S7TYPE.UINT:
                case S7InterfaceSignal.S7TYPE.UDINT:
                case S7InterfaceSignal.S7TYPE.TIME:
                    return InterfaceSignal.TYPE.INT;

                case S7InterfaceSignal.S7TYPE.REAL:
                    return InterfaceSignal.TYPE.REAL;

                default:
                    return InterfaceSignal.TYPE.UNDEFINED;
            }
        }

        //! Gets the direction (Input or Output) based on the absolute S7 signal name
        private InterfaceSignal.DIRECTION GetDirection(string name)
        {
            InterfaceSignal.DIRECTION direction = InterfaceSignal.DIRECTION.NOTDEFINED;
            var toupper = name.ToUpper();
            var firsttwo = toupper.Substring(0, 2);
            if (firsttwo == "DB")
            {
                direction = InterfaceSignal.DIRECTION.INPUTOUTPUT;
            }
            else
            {
                switch (toupper[0])
                {
                    case 'A':
                        direction = InterfaceSignal.DIRECTION.OUTPUT;
                        break;
                    case 'Q':
                        direction = InterfaceSignal.DIRECTION.OUTPUT;
                        break;
                    case 'E':
                        direction = InterfaceSignal.DIRECTION.INPUT;
                        break;
                    case 'I':
                        direction = InterfaceSignal.DIRECTION.INPUT;
                        break;
                    case 'M':
                        direction = InterfaceSignal.DIRECTION.OUTPUT;
                        if (AreaReadWriteMode)
                        {
                            var mempos = GetFirstNum(name, 0);
                            if (mempos >= MinAreaMInput && mempos <= MaxAreaMInput)
                                direction = InterfaceSignal.DIRECTION.INPUT;
                            else
                                direction = InterfaceSignal.DIRECTION.OUTPUT;
                        }
                        else
                        {
                            direction = InterfaceSignal.DIRECTION.OUTPUT;
                        }

                        break;
                    case 'D':
                        direction = InterfaceSignal.DIRECTION.OUTPUT;
                        break;
                    default:
                        Debug.LogError("Type for this variable [" + name + "] is not allowed!");
                        break;
                }
            }


            return direction;
        }

        private static S7InterfaceSignal.S7TYPE GetDBType(string name)
        {
            // get pos of ".DB"
            S7InterfaceSignal.S7TYPE type = S7InterfaceSignal.S7TYPE.UNDEFINED;
            var pos = name.IndexOf(".DB");
            switch (name[pos + 3])
            {
                case 'W':
                    type = S7InterfaceSignal.S7TYPE.WORD;
                    break;
                case 'B':
                    type = S7InterfaceSignal.S7TYPE.BYTE;
                    break;
                case 'D':
                    type = S7InterfaceSignal.S7TYPE.DWORD;
                    break;
                default: // if no char
                    type = S7InterfaceSignal.S7TYPE.BOOL;
                    break;
            }

            return type;
        }

        private static int GetDBMemPos(string name)
        {
            var pos = name.IndexOf(".DB");
            return GetFirstNum(name, pos + 3);
        }

        //! Gets the S7 datatype from the absolute S7 signal name
        private static S7InterfaceSignal.S7TYPE GetTypeFromName(string name)
        {
            S7InterfaceSignal.S7TYPE type = S7InterfaceSignal.S7TYPE.UNDEFINED;
            var toupper = name.ToUpper();
            var firsttwo = toupper.Substring(0, 2);
            if (firsttwo == "DB")
            {
                type = GetDBType(toupper);
            }
            else
            {
                switch (toupper[1])
                {
                    case 'W':
                        type = S7InterfaceSignal.S7TYPE.WORD;
                        break;
                    case 'B':
                        type = S7InterfaceSignal.S7TYPE.BYTE;
                        break;
                    case 'D':
                        type = S7InterfaceSignal.S7TYPE.DWORD;
                        break;
                    default: // if no char
                        type = S7InterfaceSignal.S7TYPE.BOOL;
                        break;
                }
            }

            return type;
        }

        private static int GetFirstNum(string str, int start)
        {
            string res = "";
            for (int i = start; i < str.Length; i++) // loop over the complete input
            {
                if (Char.IsDigit(str[i])) //check if the current char is digit
                    res += str[i];
                else
                {
                    if (res.Length > 0)
                    {
                        break;
                    }
                }
            }

            // If no digits found, return 0
            if (string.IsNullOrEmpty(res))
            {
                return 0;
            }

            try
            {
                return Convert.ToInt32(res);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in FormatDrivePosition " + e.ToString());
                throw;
            }
        }

        private int GetNumberAfterPoint(string str)
        {
            // find first point
            var pospoint = str.IndexOf('.');
            if (pospoint != -1)
            {
                return GetFirstNum(str, pospoint);
            }
            else
            {
                return 0;
            }
        }

        private int GetNumberAfterLastPoint(string str)
        {
            // find first point
            var pospoint = str.LastIndexOf('.');
            if (pospoint != -1)
            {
                return GetFirstNum(str, pospoint);
            }
            else
            {
                return 0;
            }
        }


        private static int GetLenght(string name)
        {
            int len;
            var toupper = name.ToUpper();
            var mychar = toupper[1];
            var firsttwo = toupper.Substring(0, 2);
            if (firsttwo == "DB")
            {
                var pos = name.IndexOf(".DB");
                mychar = toupper[pos + 3];
            }

            switch (mychar)
            {
                case 'W':
                    len = 4;
                    // len = S7Consts.S7WLWord;
                    break;
                case 'B':
                    //len = 1;
                    len = S7Consts.S7WLByte;
                    break;
                case 'D':
                    //len = 4;
                    len = S7Consts.S7WLDWord;
                    break;
                default: // if no char
                    //len = 1;
                    len = S7Consts.S7WLBit;
                    break;
            }

            return len;
        }

        //! Gets a signal based on its name
        public override GameObject GetSignal(string name)
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            // First check names of signals
            foreach (var child in children)
            {
                var signal = child.GetComponent<Signal>();
                if (signal != null && child != gameObject.transform)
                {
                    if (signal.Name == name)
                    {
                        return child.gameObject;
                    }
                }
            }

            // Second check names of components
            foreach (var child in children)
            {
                if (child != gameObject.transform)
                {
                    if (child.name == name)
                    {
                        return child.gameObject;
                    }
                }
            }

            return null;
        }


        //! Imports all signal objects under the interface gameobject during simulation start
        private void ImportSignals(bool simstart)
        {
            NumberInputs = 0;
            NumberOutputs = 0;
            areamin = new int[256];
            areamax = new int[256];
            for (int i = 0; i < 256; i++)
            {
                areamin[i] = -1;
                areamax[i] = -1;
            }

            areamax = new int[256];

            if (simstart)
            {
                Signal[] signals = GetComponentsInChildren<Signal>();
                DeleteSignals();
                foreach (var Signal in signals)
                {
                    var type = InterfaceSignal.TYPE.UNDEFINED;
                    var s7type = S7InterfaceSignal.S7TYPE.UNDEFINED;
                    if (Signal.OriginDataType == "")
                    {
                        s7type = GetTypeFromName(Signal.Name);
                        type = GetType(s7type);
                    }
                    else
                    {
                        s7type = GetS7Type(Signal.OriginDataType);
                        type = GetType(s7type);
                    }

                    var direction = GetDirection(Signal.Name);
                    S7InterfaceSignal s7signal = new S7InterfaceSignal(Signal.Name, direction, type);
                    s7signal.Area = GetArea(Signal.Name);

                    if (s7signal.Area == S7Consts.S7AreaMK)
                    {
                        // If M is already there and definded as Input - leave it as an Input (Standard is Output)
                        if (Signal.IsInput())
                            direction = InterfaceSignal.DIRECTION.INPUT;
                    }

                    if (direction == InterfaceSignal.DIRECTION.INPUT)
                    {
                        NumberInputs++;
                    }

                    if (direction == InterfaceSignal.DIRECTION.OUTPUT)
                    {
                        NumberOutputs++;
                    }

                    if (direction == InterfaceSignal.DIRECTION.INPUTOUTPUT)
                    {
                        if (Signal.IsInput())
                        {
                            direction = InterfaceSignal.DIRECTION.INPUT;
                            NumberInputs++;
                        }
                        else
                        {
                            direction = InterfaceSignal.DIRECTION.OUTPUT;
                            NumberOutputs++;
                        }
                    }


                    s7signal.Signal = Signal;
                    s7signal.S7Type = s7type;

                    s7signal.Mempos = GetFirstNum(Signal.Name, 0);
                    s7signal.Bit = (byte) GetNumberAfterPoint(Signal.Name);
                    s7signal.DBNumber = 0;
                    if (s7signal.Area == S7Consts.S7AreaDB)
                    {
                        s7signal.IsDB = true;
                        s7signal.DBNumber = GetFirstNum(Signal.Name, 0);
                        s7signal.Mempos = GetDBMemPos(Signal.Name);
                        s7signal.Bit = (byte) GetNumberAfterLastPoint(Signal.Name);
                    }

                    s7signal.Size = GetLenght(Signal.Name);
                    s7signal.Comment = Signal.Comment;
                    s7signal.OriginDataType = Signal.OriginDataType;
                    s7signal.Direction = direction;

                    // Check Area Mins and Max
                    if (AreaReadWriteMode)
                    {
                        if (areamin[s7signal.Area] == -1)
                            areamin[s7signal.Area] = s7signal.Mempos;
                        if (s7signal.Mempos < areamin[s7signal.Area])
                            areamin[s7signal.Area] = s7signal.Mempos;
                        if (s7signal.Mempos > areamax[s7signal.Area])
                            areamax[s7signal.Area] = s7signal.Mempos;
                        if (s7signal.Area == S7Consts.S7AreaDB)
                        {
                            if (s7signal.Mempos > (MaxAreaDBOutput) &&
                                s7signal.Direction == InterfaceSignal.DIRECTION.OUTPUT)
                            {
                                Error(
                                    $"Signal [{Signal.name}] is outside the defined Area for AreaReadWriteMode, please check your S7Interface settings");
                            }

                            if (s7signal.Mempos > (MaxAreaDBInput) &&
                                s7signal.Direction == InterfaceSignal.DIRECTION.INPUT)
                            {
                                Error(
                                    $"Signal [{Signal.name}] is outside the defined Area for AreaReadWriteMode, please check your S7Interface settings");
                            }
                        }

                        if (s7signal.Area == S7Consts.S7AreaMK)
                        {
                            if (s7signal.Mempos > (MaxAreaMOutput) &&
                                s7signal.Direction == InterfaceSignal.DIRECTION.OUTPUT)
                            {
                                Error(
                                    $"Signal [{Signal.name}] is outside the defined Area for AreaReadWriteMode, please check your S7Interface settings");
                            }

                            if (s7signal.Mempos > (MaxAreaMInput) &&
                                s7signal.Direction == InterfaceSignal.DIRECTION.INPUT)
                            {
                                Error(
                                    $"Signal [{Signal.name}] is outside the defined Area for AreaReadWriteMode, please check your S7Interface settings");
                            }
                        }
                    }


                    AddSignal(s7signal);
                }
            }

            /// Create Areas
            if (AreaReadWriteMode)
            {
                if (areamin[S7Consts.S7AreaPE] != -1)
                {
                    var diff = areamax[S7Consts.S7AreaPE] - areamin[S7Consts.S7AreaPE];
                    inputarea = new byte[diff + 4];
                }

                if (areamin[S7Consts.S7AreaPA] != -1)
                {
                    var diff = areamax[S7Consts.S7AreaPA] - areamin[S7Consts.S7AreaPA];
                    outputarea = new byte[diff + 4];
                }

                if (MinAreaDBInput > MaxAreaDBInput && MinAreaDBOutput > MaxAreaDBOutput)
                {
                    Error("Max DB Area needs to be greater than Min DB Area");
                }
                else
                {
                    dboutputarea = new byte[MaxAreaDBOutput - MinAreaDBOutput + 4];
                    dbinputarea = new byte[MaxAreaDBInput - MinAreaDBInput + 4];
                }

                if (MinAreaMInput > MaxAreaMInput && MinAreaMOutput > MaxAreaMOutput)
                {
                    Error("Max M Area needs to be greater than Min M Area");
                }
                else
                {
                    moutputarea = new byte[MaxAreaMOutput - MinAreaMOutput + 4];
                    minputarea = new byte[MaxAreaMInput - MinAreaMInput + 4];
                }
            }
        }


        //! Imports the symbol table and creates the signal objects under this S7interface object
        public void ReadSignalFile()
        {
            List<string> iosymbol = new List<string>();
            List<string> ioadress = new List<string>();
            List<string> iotype = new List<string>();
            List<string> iocomment = new List<string>();
            try
            {
                using (StreamReader sr = new System.IO.StreamReader(SymbolTable))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var tmp = "\"";
                        var newline = line.Replace(tmp, string.Empty);
                        var values = newline.Split(',');
                        iosymbol.Add(values[0]);
                        var newvalues = values[1].Replace("%", string.Empty);
                        ioadress.Add(newvalues);
                        iotype.Add(values[2]);
                        iocomment.Add(values[6]);
                    }
                }
            }
            catch (Exception e)
            {
                Error("Error in reading PLC Signal table " + e.ToString());
            }

            for (int i = 0; i < iosymbol.Count; i++)
            {
                var direction = GetDirection(ioadress[i]);

                var s7type = GetS7Type(iotype[i]);
                var type = GetType(s7type);
                if (direction == InterfaceSignal.DIRECTION.INPUTOUTPUT)
                    direction = InterfaceSignal.DIRECTION.OUTPUT;
                S7InterfaceSignal newsignal = new S7InterfaceSignal(ioadress[i], direction, type);
                newsignal.SymbolName = iosymbol[i];
                newsignal.S7Type = GetS7Type(iotype[i]);
                newsignal.Name = ioadress[i];
                newsignal.OriginDataType = s7type.ToString();
                newsignal.Comment = iocomment[i];
                AddSignal(newsignal);
            }
        }


        //!  Connects to the S7 and checks the connection
        public void CheckConnection()
        {
            ConnectS7();
            Client.Disconnect();
        }


        private void ConnectS7()
        {
            _lastreconnect = Time.time;
            Client = new S7Client();
            connectionstatus = Client.ConnectTo(Adress, Rack, Slot);
            int plcstatus = 0;
            if (connectionstatus == 0)
            {
                int ExecTime = Client.ExecTime();
                Log("S7 interface connected - time: " + ExecTime.ToString() + " ms");
            }
            else
            {
                Error(Client.ErrorText(connectionstatus), this);
                _isconnectedread = false;
                _isconnectedwrite = false;
                return;
            }

            RequestedPduLength = Client.RequestedPduLength();
            NegotiatedPduLenght = Client.NegotiatedPduLength();
            Client.PlcGetStatus(ref plcstatus);
            switch (plcstatus)
            {
                case S7Consts.S7CpuStatusRun:
                    PLCStatus = "Running";
                    break;
                case S7Consts.S7CpuStatusStop:
                    PLCStatus = "Stopped";
                    break;
                case S7Consts.S7CpuStatusUnknown:
                    PLCStatus = "Unknown";
                    break;
            }

            ConnectionStatus = Client.ErrorText(connectionstatus);
            _isconnectedread = true;
            _isconnectedwrite = true;
        }

        private void ReadS7()
        {
            var toread = NumberOutputs;
            var currsignal = 0;
            var pdufull = false;
            var pdu = 0;
            var numsignals = 0;
            var payload = 0;
            var numout = 0;


            if (AreaReadWriteMode)
            {
                var error = 0;

                if (areamin[S7Consts.S7AreaPA] != -1)
                    error = Client.ReadArea(S7Consts.S7AreaPA, 0, areamin[S7Consts.S7AreaPA],
                        areamax[S7Consts.S7AreaPA] - areamin[S7Consts.S7AreaPA] + 4, S7Consts.S7WLByte, outputarea);
                if (error != 0)
                    ThreadStatus = "S7 interface plc output area read error " + Client.ErrorText(error);
                if (areamin[S7Consts.S7AreaDB] != -1 && MaxAreaDBOutput > 0)
                {
                    var min = MinAreaDBOutput;
                    var max = MaxAreaDBOutput;
                    var amount = max - min;
                    error = Client.ReadArea(S7Consts.S7AreaDB, DBOutputs, min
                        , amount, S7Consts.S7WLByte, dboutputarea);
                }

                if (error != 0)
                    ThreadStatus = "S7 interface DB area read error " + Client.ErrorText(error);
                if (areamin[S7Consts.S7AreaMK] != -1 && MaxAreaMOutput > 0)
                {
                    var min = MinAreaMOutput;
                    var max = MaxAreaMOutput;
                    var amount = max - min;
                    error = Client.ReadArea(S7Consts.S7AreaMK, 0, min
                        , amount, S7Consts.S7WLByte, moutputarea);
                }

                if (error != 0)
                    ThreadStatus = "S7 interface M area read error " + Client.ErrorText(error);
            }
            else
            {
                /// Multiread Mode
                outputreader = new S7MultiVar(Client);
                while (toread > 0)
                {
                    var signal = (S7InterfaceSignal) InterfaceSignals[currsignal];
                    if (signal.Direction == InterfaceSignal.DIRECTION.OUTPUT)
                    {
                        if (signal.Size % 2 != 0)
                            payload++;
                        payload = payload + signal.Size;
                        pdu = 14 + 4 * (numsignals + 1) + payload;

                        if (pdu < NegotiatedPduLenght - ReduceMaxPduLenth) // Add
                        {
                            if (signal.S7Type == S7InterfaceSignal.S7TYPE.BOOL)
                                outputreader.Add(signal.Area, signal.Size, signal.DBNumber,
                                    signal.Mempos * 8 + signal.Bit,
                                    1, ref signal.TransferValue);
                            else
                                outputreader.Add(signal.Area, signal.Size, signal.DBNumber, signal.Mempos, 1,
                                    ref signal.TransferValue);
                            toread--;
                            numsignals++;
                        }
                        else
                        {
                            pdufull = true;
                        }
                    }

                    // send if pdu full or all signals are created
                    if (pdufull || toread == 0 || numsignals >= 20)
                    {
                        numsignals = 0;
                        payload = 0;
                        // Read PLC Outputs
                        numout++;
                        var connected = outputreader.Read();

                        if (connected == 0)
                        {
                            _isconnectedread = true;
                        }
                        else
                        {
                            _isconnectedread = false;
                            ThreadStatus = "S7 interface plc output read error " + Client.ErrorText(connected);
                        }

                        if (toread > 0)
                            outputreader = new S7MultiVar(Client);
                    }


                    if (pdufull)
                        pdufull = false;
                    else
                        currsignal++;
                }
            }


            // lock now signals and transfer from temporary transfervalue for better multitasking performance
            toread = NumberOutputs;
            currsignal = 0;
            lock (InterfaceSignals)
            {
                while (toread > 0)
                {
                    var mysignal = (S7InterfaceSignal) InterfaceSignals[currsignal];
                    byte[] newbyte;
                    if (mysignal.Direction == InterfaceSignal.DIRECTION.OUTPUT)
                    {
                        if (AreaReadWriteMode)
                        {
                            var min = areamin[mysignal.Area];
                            var max = areamax[mysignal.Area];
                            newbyte = new byte[4];
                            if (mysignal.Area == S7Consts.S7AreaPA)
                            {
                                Buffer.BlockCopy(outputarea, mysignal.Mempos - min, newbyte, 0, 4);
                            }

                            if (mysignal.Area == S7Consts.S7AreaMK)
                            {
                                min = MinAreaMOutput;
                                if ((mysignal.Mempos - min) > (MaxAreaMOutput - MinAreaMOutput))
                                    ThreadStatus =
                                        ($"Signal [{mysignal.Name} can not be updated because Min and Max Areas for AreaReadWrite ,mode are not set correctly");
                                else
                                    Buffer.BlockCopy(moutputarea, mysignal.Mempos - min, newbyte, 0, 4);
                            }

                            if (mysignal.Area == S7Consts.S7AreaDB)
                            {
                                min = MinAreaDBOutput;
                                if ((mysignal.Mempos - min) > (MaxAreaDBOutput - MinAreaDBInput))
                                    ThreadStatus =
                                        ($"Signal [{mysignal.Name} can not be updated because Min and Max Areas for AreaReadWrite ,mode are not set correctly");
                                else
                                    Buffer.BlockCopy(dboutputarea, mysignal.Mempos - min, newbyte, 0, 4);
                            }
                        }
                        else
                        {
                            newbyte = (byte[]) mysignal.TransferValue.Clone();
                        }

                        if (ReverseHighLowBytes)
                        {
                            if (mysignal.S7Type != S7InterfaceSignal.S7TYPE.BYTE)
                                Array.Reverse(newbyte);
                        }

                        mysignal.Value = newbyte;
                        toread--;
                    }

                    currsignal++;
                }
            }
        }

        private void WriteS7()
        {
            var towrite = NumberInputs;
            var currsignal = 0;
            var pdufull = false;
            var pdu = 0;
            var numsignals = 0;
            var payload = 0;
            var request = 0;

            lock (InterfaceSignals)
            {
                //lock now signals and transfer from temporary transfervalue for better multitasking performance
                while (towrite > 0)
                {
                    var mysignal = (S7InterfaceSignal) InterfaceSignals[currsignal];
                    if (mysignal.Direction == InterfaceSignal.DIRECTION.INPUT)
                    {
                        byte[] newbyte = (byte[]) mysignal.Value.Clone();
                        mysignal.TransferValue = newbyte;

                        /// On AreaWrite mode transfer to the correct byteposition
                        if (AreaReadWriteMode)
                        {
                            var bytes = mysignal.Value;
                            byte[] dstarea = null;
                          
                            var pos = mysignal.Mempos - areamin[S7Consts.S7AreaPE];
                            switch (mysignal.Area)
                            {
                                case S7Consts.S7AreaMK:
                                    pos = mysignal.Mempos - MinAreaMInput;
                                    dstarea = minputarea;
                                    break;
                                case S7Consts.S7AreaDB:
                                    pos = mysignal.Mempos - MinAreaDBInput;
                                    dstarea = dbinputarea;
                                    break;
                                case S7Consts.S7AreaPE:
                                    pos = mysignal.Mempos - areamin[S7Consts.S7AreaPE];
                                    dstarea = inputarea;
                                    break;
                            }

                            switch (mysignal.S7Type)
                            {
                                case S7InterfaceSignal.S7TYPE.BOOL:
                                    byte byteval = 0;
                                    bool valbool = ((PLCInputBool) mysignal.Signal).Value;
                                    var valbyte = 0;

                                    switch (mysignal.Area)
                                    {
                                        case S7Consts.S7AreaMK:
                                            valbyte = minputarea[pos];
                                            break;
                                        case S7Consts.S7AreaDB:
                                            valbyte = dbinputarea[pos];
                                            break;
                                        case S7Consts.S7AreaPE:
                                            valbyte = inputarea[pos];
                                            break;
                                    }

                                    if (valbool)
                                    {
                                        byteval = (byte) (valbyte | (byte) (0x01 << mysignal.Bit));
                                    }
                                    else
                                    {
                                        byteval = (byte) (valbyte & ~(1 << mysignal.Bit));
                                    }

                                    bytes[0] = byteval;
                                    Buffer.BlockCopy(bytes, 0, dstarea, pos, 1);
                                    break;
                                case S7InterfaceSignal.S7TYPE.BYTE:
                                    Buffer.BlockCopy(bytes, 3, dstarea, pos, 1);
                                    break;
                                case S7InterfaceSignal.S7TYPE.WORD:
                                    Buffer.BlockCopy(bytes, 2, dstarea, pos, 2);
                                    break;
                                case S7InterfaceSignal.S7TYPE.DWORD:
                                    Buffer.BlockCopy(bytes, 0, dstarea, pos, 4);
                                    break;
                                case S7InterfaceSignal.S7TYPE.SINT:
                                    Buffer.BlockCopy(bytes, 2, dstarea, pos, 2);
                                    break;
                                case S7InterfaceSignal.S7TYPE.INT:
                                    Buffer.BlockCopy(bytes, 2, dstarea, pos, 2);
                                    break;
                                case S7InterfaceSignal.S7TYPE.DINT:
                                    Buffer.BlockCopy(bytes, 0, dstarea, pos, 4);
                                    break;
                                case S7InterfaceSignal.S7TYPE.USINT:
                                    Buffer.BlockCopy(bytes, 3, dstarea, pos, 1);
                                    break;
                                case S7InterfaceSignal.S7TYPE.UINT:
                                    Buffer.BlockCopy(bytes, 2, dstarea, pos, 2);
                                    break;
                                case S7InterfaceSignal.S7TYPE.UDINT:
                                    Buffer.BlockCopy(bytes, 0, dstarea, pos, 4);
                                    break;
                                case S7InterfaceSignal.S7TYPE.REAL:
                                    Buffer.BlockCopy(bytes, 0, dstarea, pos, 4);
                                    break;
                                case S7InterfaceSignal.S7TYPE.TIME:
                                    Buffer.BlockCopy(bytes, 0, dstarea, pos, 4);
                                    break;
                            }
                        }

                        towrite--;
                    }

                    currsignal++;
                }
            }

            if (AreaReadWriteMode)
            {
                var error = 0;
                if (areamin[S7Consts.S7AreaPE] != -1)
                    error = Client.WriteArea(S7Consts.S7AreaPE, 0, areamin[S7Consts.S7AreaPE],
                        areamax[S7Consts.S7AreaPE] - areamin[S7Consts.S7AreaPE] + 4, S7Consts.S7WLByte, inputarea);
                if (areamin[S7Consts.S7AreaDB] != -1 && MaxAreaDBInput > 0)
                    error = Client.WriteArea(S7Consts.S7AreaDB, DBInputs, MinAreaDBInput,
                        MaxAreaDBInput - MinAreaDBInput, S7Consts.S7WLByte, dbinputarea);
                if (areamin[S7Consts.S7AreaMK] != -1 && MaxAreaMInput > 0)
                    error = Client.WriteArea(S7Consts.S7AreaMK, 0, MinAreaMInput,
                        MaxAreaMInput - MinAreaMInput, S7Consts.S7WLByte, minputarea);
                if (error != 0)
                    ThreadStatus = "S7 interface plc input area write error " + Client.ErrorText(error);
            }
            else
            {
                towrite = NumberInputs;
                currsignal = 0;
                inputwriter = new S7MultiVar(Client);
                while (towrite > 0)
                {
                    var signal = (S7InterfaceSignal) InterfaceSignals[currsignal];
                    if (signal.Direction == InterfaceSignal.DIRECTION.INPUT)
                    {
                        bool needtowrite = true;
                        // Check if write needs to be done
                        if (OnlyWriteToPlcChangedSignals && !firstwrite)
                        {
                            if (Enumerable.SequenceEqual(signal.LastWriteValue, signal.TransferValue))
                                needtowrite = false;

                            if (signal.FirstTransfer == false)
                            {
                                signal.FirstTransfer = true;
                                needtowrite = true;
                            }

                            if (needtowrite)
                                signal.LastWriteValue = signal.TransferValue;
                        }

                        if (needtowrite)
                        {
                            if (signal.Size % 2 != 0)
                                payload++;
                            payload = payload + signal.Size;
                            pdu = 12 + 16 * (numsignals + 1) + payload;
                            request = 7 + 7 + 12 + (numsignals + 1) * 12;
                            if (pdu < NegotiatedPduLenght - ReduceMaxPduLenth &&
                                request < NegotiatedPduLenght - ReduceMaxPduLenth) // Add
                            {
                                if (signal.S7Type == S7InterfaceSignal.S7TYPE.BOOL)

                                    inputwriter.Add(signal.Area, signal.Size, signal.DBNumber,
                                        signal.Mempos * 8 + signal.Bit,
                                        1, ref signal.TransferValue);
                                else
                                    inputwriter.Add(signal.Area, signal.Size, signal.DBNumber, signal.Mempos, 1,
                                        ref signal.TransferValue);
                                towrite--;
                                numsignals++;
                            }
                            else
                            {
                                pdufull = true;
                            }
                        }
                        else
                        {
                            towrite--;
                        }
                    }

                    // send if pdu full or all signals are created
                    if (numsignals > 0)
                        if (pdufull || towrite == 0 || numsignals >= 20)
                        {
                            numsignals = 0;
                            payload = 0;
                            // Read PLC Outputs
                            var connected = inputwriter.Write();
                            if (connected == 0)
                            {
                                _isconnectedwrite = true;
                            }
                            else
                            {
                                _isconnectedwrite = false;
                                ThreadStatus = "S7 interface plc input write error " + Client.ErrorText(connected);
                            }

                            if (towrite > 0)
                                inputwriter = new S7MultiVar(Client);
                        }

                    if (pdufull)
                        pdufull = false;
                    else
                        currsignal++;
                }
            }

            firstwrite = true;
        }


        //! Updates all signals in the parallel communication thread
        protected override void CommunicationThreadUpdate()
        {
            if (!IsConnected)
                return;

            if (!Client.Connected)
                return;

            if (!_isconnectedread || !_isconnectedwrite)
                return;

            ReadS7();
            WriteS7();
            commerror = "";

            ThreadCycleNum++;
        }

        //! Updates one signal in the communication thread
        void UpdateSignal(InterfaceSignal interfacesignal)
        {
            lock (InterfaceSignals)
            {
                PLCOutputInt plcoutputint = null;
                PLCOutputBool plcoutputbool = null;
                PLCOutputFloat plcoutputfloat = null;
                PLCInputInt plcinputint = null;
                PLCInputFloat plcinputfloat = null;
                PLCInputBool plcinputbool = null;
                if (Client == null)
                    return;
                var s7 = (S7InterfaceSignal) interfacesignal;

                var bytes = s7.Value;
                
                if (interfacesignal.Direction == InterfaceSignal.DIRECTION.OUTPUT)
                {
                    switch (s7.S7Type)
                    {
                        case S7InterfaceSignal.S7TYPE.BOOL:
                            plcoutputbool = (PLCOutputBool) s7.Signal;
                            if (!AreaReadWriteMode)
                            {
                                if (bytes[3] == 1)
                                    plcoutputbool.Value = true;
                                else
                                {
                                    plcoutputbool.Value = false;
                                }
                            }
                            else
                            {
                                var valbyte = bytes[3];
                                plcoutputbool.Value = ((valbyte >> interfacesignal.Bit) & 1) != 0;
                            }

                            plcoutputbool.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.BYTE:

                            plcoutputint = (PLCOutputInt) s7.Signal;
                            if (s7.IsDB || (s7.Area == S7Consts.S7AreaMK))
                                plcoutputint.Value = bytes[0];
                            else
                                plcoutputint.Value = bytes[3];
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.WORD:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = BitConverter.ToInt16(bytes, 2);
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.DWORD:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = (int) BitConverter.ToUInt32(bytes, 0);
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.SINT:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            unchecked
                            {
                                sbyte s = (sbyte) bytes[3];
                                plcoutputint.Value = s;
                            }

                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.INT:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = BitConverter.ToInt16(bytes, 2);
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.DINT:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = BitConverter.ToInt32(bytes, 0);
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.USINT:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = bytes[3];
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.UINT:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = BitConverter.ToUInt16(bytes, 2);
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.UDINT:
                            plcoutputint = (PLCOutputInt) s7.Signal;
                            plcoutputint.Value = (int) BitConverter.ToUInt32(bytes, 0);
                            plcoutputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.REAL:
                            plcoutputfloat = (PLCOutputFloat) s7.Signal;
                            plcoutputfloat.Value = BitConverter.ToSingle(bytes, 0);
                            plcoutputfloat.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.TIME:
                            plcoutputfloat = (PLCOutputFloat) s7.Signal;
                            plcoutputfloat.Value = BitConverter.ToInt32(bytes, 0);
                            plcoutputfloat.Status.Connected = IsConnected;
                            break;
                    }
                }

                var notreverse = false;
                if (interfacesignal.Direction == InterfaceSignal.DIRECTION.INPUT)
                {
                    switch (s7.S7Type)
                    {
                        case S7InterfaceSignal.S7TYPE.BOOL:
                            plcinputbool = (PLCInputBool) s7.Signal;
                            if (plcinputbool.Value)
                                bytes[0] = 255;
                            else
                                bytes[0] = 0;
                            plcinputbool.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.BYTE:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes[0] = (byte) plcinputint.Value;
                            notreverse = true;
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.WORD:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((UInt16) plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.DWORD:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((UInt32) plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.SINT:
                            plcinputint = (PLCInputInt) s7.Signal;
                            unchecked
                            {
                                sbyte s;
                                s = (sbyte) plcinputint.Value;
                                byte b = (byte) s;
                                bytes[0] = b;
                            }

                            notreverse = true;
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.INT:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((Int16) plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.DINT:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes(plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.USINT:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((Byte) plcinputint.Value);
                            notreverse = true;
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.UINT:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((UInt16) plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.UDINT:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((UInt32) plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.REAL:
                            plcinputfloat = (PLCInputFloat) s7.Signal;
                            bytes = BitConverter.GetBytes((Single) plcinputfloat.Value);
                            plcinputfloat.Status.Connected = IsConnected;
                            break;
                        case S7InterfaceSignal.S7TYPE.TIME:
                            plcinputint = (PLCInputInt) s7.Signal;
                            bytes = BitConverter.GetBytes((Int32) plcinputint.Value);
                            plcinputint.Status.Connected = IsConnected;
                            break;
                    }

                    if (s7.S7Type != S7InterfaceSignal.S7TYPE.BOOL && ReverseHighLowBytes)
                    {
                        if (!notreverse)
                            bytes = bytes.Reverse().ToArray();
                    }

                    s7.Value = bytes;
                }
            }
        }

#if UNITY_EDITOR


        [Button("Select symbol table")]
        private void SelectSymbolTable()
        {
            var File = "";
            File = EditorUtility.OpenFilePanel("Select file to import", File, "sdf");
            SymbolTable = File;
        }

        [Button("Import symbol table")]
        private void ButtonReadSignalFile()
        {
            ReadSignalFile();
        }

        [Button("Check Connection")]
        private void ButtonCheckConnection()
        {
            CheckConnection();
        }
#endif

        public override void OpenInterface()
        {
            ThreadCycleNum = 0;
            _isdisconnected = true;
            firstwrite = false;
            ImportSignals(true);
            ConnectS7();
            base.OpenInterface();
        }

        public override void CloseInterface()
        {
            if (Client != null)
            {
                Client.Disconnect();
            }

            base.CloseInterface();
        }


        private void FixedUpdate()
        {
            if (commerror != "")
            {
                Debug.LogError("S7 Interface " + commerror);
                ConnectionStatus = commerror;
            }

            if ((!_isconnectedread || !_isconnectedwrite) && !_isdisconnected)
            {
                OnDisconnected();
                CloseInterface();
                _isdisconnected = true;
            }

            if (_isdisconnected && (Time.time - _lastreconnect) > ReconnectingTime)
                OpenInterface();

            if (_isdisconnected && _isconnectedread && _isconnectedwrite)
            {
                OnConnected();
                _isdisconnected = false;
            }

            if (lastthreadcyclenum < ThreadCycleNum)
            {
                foreach (var interfacesignal in InterfaceSignals)
                {
                    UpdateSignal(interfacesignal);
                }

                lastthreadcyclenum = ThreadCycleNum;
            }
        }
    }
}