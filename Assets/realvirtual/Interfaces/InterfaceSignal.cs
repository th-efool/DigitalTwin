

using UnityEngine;

namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/custom-interfaces")]
    public class InterfaceSignal 
    {
        public enum TYPE
        {
            BOOL,            
            INT,
            REAL,
            TRANSFORM,
            TEXT,
            UNDEFINED
        };

        public enum DIRECTION
        {
            NOTDEFINED,
            INPUT,
            OUTPUT,
            INPUTOUTPUT
        };
    
        public Signal Signal;
        public string Name;
        public TYPE Type;
        public DIRECTION Direction;
        public int Mempos;
        public byte Bit;
        public string SymbolName;
        public string Comment;
        public string OriginDataType;
        public object LastValue;  // Can be used by some interfaces for detecting signal change

        public InterfaceSignal()
        {
       
        }

        public string GetSymbolName()
        {
            if (Signal.Name != null)
            {
                return Signal.Name;
            }
            else
            {
                return Signal.name;
            }
        }

  
        public InterfaceSignal(string name, DIRECTION direction, TYPE type)
        {
            Name = name;
            Direction = direction;
            Type = type;
        }

        public void UpdateSignal(Signal signal)
        {
        
        }
    }

}

