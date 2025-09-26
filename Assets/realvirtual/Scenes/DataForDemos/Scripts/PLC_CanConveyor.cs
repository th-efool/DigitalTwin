
namespace realvirtual
{

    using UnityEngine;

    [SelectionBase]
    //! PLC Script for the realvirtual demo model
    public class PLC_CanConveyor : ControlLogic
    {
        public bool On = true;

        [Header("References")] public PLCOutputBool StartConveyor;
        public PLCInputBool SensorOccupied;
        public PLCInputBool ButtonConveyorOn;
        public PLCOutputBool LampCanAtPosition;

        // Call this when all Updates are done
        void FixedUpdate()
        {
            if (ForceStop)
                return;
            
            var converyoron = true;


            if (ButtonConveyorOn != null) 
            {
                converyoron = ButtonConveyorOn.Value;
            }
            
            
            if (SensorOccupied.Value == false && On && converyoron)
            {
                StartConveyor.Value = true;
            }
            else
            {
                StartConveyor.Value = false;
            }

            if(LampCanAtPosition!=null)
                LampCanAtPosition.Value = SensorOccupied.Value;
        }
    }
}
