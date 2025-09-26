using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace realvirtual
{
    //! Controls a 3-axis pick and place automation sequence using three drives.
    //! This script coordinates synchronized movement between three axes to perform pick and place operations
    //! between two positions, with automatic return to a standard position after each cycle.
    public class LogicMove3AxisPickAndPlace : MonoBehaviour
    {
        [InfoBox("This script stops the drive when the sensor is getting to occupied. It is starting on a positive flank on StartOnPosivieFlank.")]
        public Drive Axis1; //!< First axis drive (typically X-axis) for horizontal movement
        public Drive Axis2; //!< Second axis drive (typically Y-axis) for horizontal movement
        public Drive Axis3; //!< Third axis drive (typically Z-axis) for vertical movement
        public PLCInputBool StartPickAndPlace; //!< Input signal to start the pick and place sequence
        public PLCOutputBool Pick; //!< Output signal to activate the gripper or pick mechanism
        public PLCInputBool PlacePositionReached; //!< Input signal confirming that the place position has been reached
        public PLCOutputBool ContinueAfterPlace; //!< Output signal to continue after placing the object
        public Vector3 Position1; //!< Pick position coordinates in millimeters for all three axes
        public Vector3 Position2; //!< Place position coordinates in millimeters for all three axes
        public Vector3 StandardPos; //!< Standard home position coordinates in millimeters for all three axes
     
        private bool startbefore = false;
        [ReadOnly] public string status = "waiting"; //!< Current status of the pick and place sequence for debugging

        void Start()
        {
            status = "moving to standard pos";
            Axis1.DriveTo(StandardPos.x);
            Axis2.DriveTo(StandardPos.y);
            Axis3.DriveTo(StandardPos.z); 
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            if (status == "waiting")
            {
                if (StartPickAndPlace != null)
                {
                    if (StartPickAndPlace.Value)
                    { status = "moving to pos1";
                        Axis1.DriveTo(Position1.x);
                        Axis2.DriveTo(Position1.y);
                    }
                }
            }
            if (status == "moving to pos1")
            {
                if (Axis1.IsAtTarget && Axis2.IsAtTarget )
                {
                    status = "moving down on pos1";
                    Axis3.DriveTo(Position1.z);
                }
            }
            
            
            if (status == "moving down on pos1")
            {
                if (Axis3.IsAtTarget)
                {
                    status = "moving up on pos1";
                    Pick.Value = true;
                    Axis3.DriveTo(StandardPos.z);
                }
            }
            
            if (status == "moving up on pos1")
            {
                if (Axis3.IsAtTarget)
                {
                    status = "moving to pos2";
                    Axis1.DriveTo(Position2.x);
                    Axis2.DriveTo(Position2.y);
                }
            }
            
            if (status == "moving to pos2")
            {
                if (Axis1.IsAtTarget && Axis2.IsAtTarget)
                {
                    status = "moving down on pos2";
                    Axis3.DriveTo(Position2.z);
                }
            }
            
            if (status == "moving down on pos2")
            {
                if (Axis3.IsAtTarget)
                {
                    if (PlacePositionReached!=null) PlacePositionReached.Value = true;
                    status = "waitingformovingupsignal";
     
                
                }
            }
            
            if (status == "waitingformovingupsignal")
            {
                var cont = false;
                if (ContinueAfterPlace == null)
                    cont = true;
                else
                    cont = ContinueAfterPlace.Value;
                if (cont)
                {
                    Pick.Value = false;
                    Axis3.DriveTo(StandardPos.z);
                    status = "moving up on pos2";
                }
            }
            
            if (status == "moving up on pos2")
            {
                if (Axis3.IsAtTarget)
                {
                    status = "moving to standard pos";
                    Axis1.DriveTo(StandardPos.x);
                    Axis2.DriveTo(StandardPos.y);
                    if (PlacePositionReached!=null) PlacePositionReached.Value = false;
                }
            }
            if (status == "moving to standard pos")
            {
                if (Axis1.IsAtTarget && Axis2.IsAtTarget && Axis3.IsAtTarget)
                {
                    status = "waiting";
                }
            }
          
            
            startbefore = StartPickAndPlace.Value;
          
        }
    }
}

