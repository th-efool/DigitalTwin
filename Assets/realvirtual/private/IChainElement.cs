using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace realvirtual
{
    public interface IChainElement 
    {
      float StartPosition { get; set; }
      float Position { get; set; }
      Drive ConnectedDrive { get; set; }
      Chain Chain { get; set; }
      
      bool UsePath { get; set; }

       bool UseUnitySpline  { get; set; }
      public void InitPos(float pos);
    }
}
