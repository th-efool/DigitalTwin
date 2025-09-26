// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;

namespace realvirtual
{
    public enum SIGNALTYPE
    {
        UNDEDIFINED,
        BOOL,
        BYTE,
        WORD,
        INT,
        DWORD,
        DINT,
        REAL,
        TEXT,
        TRANSFORM
    };

    public enum SIGNALDIRECTION
    {
        NOTDEFDINED,
        INPUT,
        OUTPUT,
        INPUTOUTPUT
    };
    
    [Serializable]
    // list for all signals that are exported to the realvirtual interface
    public class SignalExportList
    {
        public SignalExport[] Signals;
    }

    [Serializable]
    public class SignalExport
    {
        public string Name; // optional name of the signal - if no symbolname is defined this is also the symbolname ("ID") of the signal, if a symbolname is defined the name is just for readabilty in realvirtual
        public string Symbolname; // symbolname of the signal - can be empty if it is the same as the name
        public string Comment; // optional description of the signal - can be empty 
        public string Type; // type "INT", "BOOL", "FLOAT", "TEXT"
        public string Direction; // direction "INPUT", "OUTPUT" - Input is Input to the PLC/Partner, Output is Output from the PLC/Partner
        public string Folder; // optional defines a "Folder" container for the signal , the can be organized in realvirtual by empty gameobjects
    }
}