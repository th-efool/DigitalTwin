// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;
using System.IO.MemoryMappedFiles;
using System.Text;
    
namespace realvirtual
{
    
    [System.Serializable] 
    //! Struct for an SHM Signal
    public struct SHMSignal
    {
        [ReadOnly] public Signal signal; //!< Connected Signal to the SHM signal
        [ReadOnly] public string name; //!< Name of the SHM signal
        [ReadOnly] public SIGNALTYPE type; //!< Type of the SHM signal
        [ReadOnly] public SIGNALDIRECTION direction; //!< Direction of the SHM signal
        [ReadOnly] public int mempos; //!< Memory position (byte position) in the Shared memory of the signal
        [ReadOnly] public byte bit;  //!< Bit position (0 if no bit) of the Signal in the shared memory
    }

    [HelpURL("https://doc.realvirtual.io/components-and-scripts/custom-interfaces")]
    //! Base class for all types of shared memory interfaces (even with different structure as simit like the plcsimadvanced interface
    public class InterfaceSHMBaseClass : InterfaceBaseClass
    {
        protected void WriteString(MemoryMappedViewAccessor accessor, string text, long pos)
        {
        
            byte[] buffer = Encoding.Default.GetBytes(text);
            accessor.WriteArray(pos,buffer,0,buffer.Length);
          
        }
        
        protected string ReadString(MemoryMappedViewAccessor accessor, long pos, int size)
        {
            string res = "";

            byte[] buffer = new byte[size];
            int count = accessor.ReadArray<byte>(pos, buffer, 0, (byte) size);
            if (count == size)
            {
                res = Encoding.Default.GetString(buffer);
            }

            return res;
        }
        
    }
}