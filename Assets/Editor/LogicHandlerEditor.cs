
using Codice.CM.Common.Serialization.Replication;
using UnityEditor;
using UnityEngine;
/*
[CustomEditor(typeof(LogicHandler))]
public class LogicHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // draws all serialized fields

        LogicHandler handler = (LogicHandler)target;

        if (GUILayout.Button("Spawn LogicExecutioner")) // <-- runs on click
        {
            handler.SpawnLogicExecutioner(handler.DestinationContainer);
        }
    }
}
*/

using UnityEngine;

[CustomEditor(typeof(LogicHandler))]
public class LogicHandlerEditor : Editor
{
    private int inputInt = 0;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LogicHandler handler = (LogicHandler)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn LogicExecutioner", EditorStyles.boldLabel);

        // Input field as comma-separated floats
        inputInt = EditorGUILayout.IntField("ObjectIndex", inputInt);


        
        if (GUILayout.Button("Drop Object In Container"))
        {
            handler.SpawnLogicExecutioner(handler.DestinationContainer, false);
        }

   
        if (GUILayout.Button("Pick Object"))
        {
            handler.SpawnLogicExecutioner(handler.AllPositions[inputInt].values, true);
        }
    }


}
