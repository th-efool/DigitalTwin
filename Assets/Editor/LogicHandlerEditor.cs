
using UnityEngine;
using UnityEditor;
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
using UnityEditor;

[CustomEditor(typeof(LogicHandler))]
public class LogicHandlerEditor : Editor
{
    private string inputString = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LogicHandler handler = (LogicHandler)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn LogicExecutioner", EditorStyles.boldLabel);

        // Input field as comma-separated floats
        inputString = EditorGUILayout.TextField("Custom Positions (comma)", inputString);

        if (GUILayout.Button("Spawn with Custom Positions"))
        {
            float[] positions = ParseInput(inputString);
            if (positions != null && positions.Length == handler.Axis.Length)
            {
                handler.SpawnLogicExecutioner(positions);
            }
            else
            {
                Debug.LogError("Invalid input! Make sure you enter exactly " + handler.Axis.Length + " floats separated by commas.");
            }
        }

        if (GUILayout.Button("Spawn with Default Positions"))
        {
            handler.SpawnLogicExecutioner(handler.DestinationContainer);
        }
    }

    private float[] ParseInput(string input)
    {
        string[] parts = input.Split(',');
        float[] result = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!float.TryParse(parts[i], out result[i]))
                return null; // parsing failed
        }
        return result;
    }
}
