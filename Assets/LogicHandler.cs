using realvirtual;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LogicHandler : MonoBehaviour
{
    [Header("PLC Outputs")]
    public PLCOutputBool Restart;
    public PLCOutputBool PickDrop;
    public PLCOutputBool ExecutingInstruction;
    [Header("Drives")]
    public Drive[] Axis;
    public Sensor PickDropSensor;
    public GameObject PrefabPickupLogicExecutioner;
    public GameObject PrefabDropLogicExecutioner;


    public float[] DestinationContainer = { -90, 32, 0, 0, -3, 0 };



    [Serializable] public class FloatArrayWrapper { public float[] values; }
    [Header("Multiple Positions")]
    public List<FloatArrayWrapper> AllPositions = new List<FloatArrayWrapper>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnLogicExecutioner(float[] positions, bool grip)
    {
        float[] npositions = positions;
        if (!grip)
        {
            npositions = new float[6] { positions[1], positions[0], positions[2], positions[3], positions[4], positions[5] };
        }

        if (PrefabPickupLogicExecutioner == null)
        {
            Debug.LogError("PrefabLogicExecutioner not assigned!");
            return;
        }

        GameObject LogicExecutioner;
        if (grip)
        {
            LogicExecutioner = Instantiate(PrefabPickupLogicExecutioner);
        }
        else
        {
            LogicExecutioner = Instantiate(PrefabDropLogicExecutioner);
        }

            LogicExecutioner.SetActive(false);

      

        var componentsLogicStep_DriveTo = LogicExecutioner.GetComponents<LogicStep_DriveTo>();
        if (componentsLogicStep_DriveTo.Length != Axis.Length)
        {
            Debug.LogError("Mismatch in LogicStep_DriveTo count!");
            return;
        }

        // Configure DriveTo components
        for (int i = 0; i < Axis.Length; i++)
        {
            if (componentsLogicStep_DriveTo[i] == null) continue;
            componentsLogicStep_DriveTo[i].drive = Axis[i];
            componentsLogicStep_DriveTo[i].Destination = npositions[i];
        }
        if (!grip)
        {
            componentsLogicStep_DriveTo[0].drive = Axis[1];
            componentsLogicStep_DriveTo[1].drive = Axis[0];
        }
        


        var componentLogicStep_WaitForSensor = LogicExecutioner.GetComponent<LogicStep_WaitForSensor>();

        if(grip)
            {componentLogicStep_WaitForSensor.Sensor = PickDropSensor;}

        // Set signals safely
        var componentsLogicStep_SetSignalBool = LogicExecutioner.GetComponents<LogicStep_SetSignalBool>();
        if (componentsLogicStep_SetSignalBool.Length >= 2)
        {
            componentsLogicStep_SetSignalBool[0].Signal = PickDrop;
            componentsLogicStep_SetSignalBool[0].SetToTrue = grip; // grip true/false
/*            PickDrop.SetValue(grip); // set immediately
*/            componentsLogicStep_SetSignalBool[1].Signal = ExecutingInstruction;
        }

     

        // DestroyObject component
        var componentLogicStep_DestroyObject = LogicExecutioner.GetComponent<LogicStep_DestroyObject>();
        if (componentLogicStep_DestroyObject != null)
            componentLogicStep_DestroyObject.ObjectToDestroy = LogicExecutioner;

        LogicExecutioner.SetActive(true);


    }
}
