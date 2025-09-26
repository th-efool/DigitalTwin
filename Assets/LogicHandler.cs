using UnityEngine;
using realvirtual;

public class LogicHandler : MonoBehaviour
{
    [Header("PLC Outputs")]
    public PLCOutputBool Restart;
    public PLCOutputBool PickDrop;
    public PLCOutputBool ExecutingInstruction;
    [Header("Drives")]
    public Drive[] Axis;

    public GameObject PrefabLogicExecutioner;

    public float[] DestinationContainer = { -90, 32, 0, 0, -3, 0 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnLogicExecutioner(DestinationContainer);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnLogicExecutioner(float[] positions)
    {

        if (PrefabLogicExecutioner == null)
        {
            Debug.LogError("PrefabLogicExecutioner not assigned!");
            return;
        }

        var LogicExecutioner = Instantiate(PrefabLogicExecutioner);
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
            componentsLogicStep_DriveTo[i].Destination = positions[i];
        }

        // Set signals safely
        var componentsLogicStep_SetSignalBool = LogicExecutioner.GetComponents<LogicStep_SetSignalBool>();
        if (componentsLogicStep_SetSignalBool.Length >= 2)
        {
            componentsLogicStep_SetSignalBool[0].Signal?.SetValue(PickDrop.GetValue());
            componentsLogicStep_SetSignalBool[1].Signal?.SetValue(ExecutingInstruction.GetValue());
        }

        // DestroyObject component
        var componentLogicStep_DestroyObject = LogicExecutioner.GetComponent<LogicStep_DestroyObject>();
        if (componentLogicStep_DestroyObject != null)
            componentLogicStep_DestroyObject.ObjectToDestroy = LogicExecutioner;

        LogicExecutioner.SetActive(true);


    }
}
