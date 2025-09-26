// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

#region

using System.IO;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

#endregion

namespace realvirtual
{
    public class SignalImporterExporter : realvirtualBehavior
    {
        public string SignalImportListFilePath = "PathtoSignalImportList";
        public string SignalExportFilePath = "PathtoSignalExport";
        public string SignalExportFilename = "SignalExport.json";

        public bool DeleteUnneededSignalsOnImport = true;
        
        private InterfaceBaseClass plcinterface;
            
        // Button for importing signals
        [Button("Import Signals")]
        public void ImportSignals()
        {
            // get plc interface in parent
            plcinterface = GetComponentInParent<InterfaceBaseClass>();
            if (plcinterface == null)
            {
                Debug.LogError("No PLC Interface found here or in parent");
                return;
            }
            
            // Check if a valid file is existing
            if (!File.Exists(SignalImportListFilePath))
            {
                Debug.LogError("Signal Import File not found");
                return;
            }

            Debug.Log("Importing Signals");

            // Read the JSON file
            var json = File.ReadAllText(SignalImportListFilePath);

            // Deserialize the JSON to SignalExportList
            var signalExportList = JsonUtility.FromJson<SignalExportList>(json);

            // Create signals based on the imported data
            for (var i = 0; i < signalExportList.Signals.Length; i++)
            {
                var signalExport = signalExportList.Signals[i];

                var signalType = signalExport.Type.ToLower();
                var signalDirection = signalExport.Direction.ToLower();
                
                InterfaceSignal insignal = new InterfaceSignal();

                switch (signalType)
                {
                    case "text":
                        insignal.Type = InterfaceSignal.TYPE.TEXT;
                        break;
                    case "bool":
                        insignal.Type = InterfaceSignal.TYPE.BOOL;
                        break;  
                    case "float":
                        insignal.Type = InterfaceSignal.TYPE.REAL;
                        break;
                    case "int":
                        insignal.Type = InterfaceSignal.TYPE.INT;
                        break;
                    case "transform":
                         insignal.Type = InterfaceSignal.TYPE.TRANSFORM;
                        break;
                    default:
                        Debug.LogError("Unknown signal type: " + signalExport.Type);
                        continue;
                }

                switch (signalDirection)
                {
                    case "input":
                        insignal.Direction = InterfaceSignal.DIRECTION.INPUT;
                        break;
                    case "output":
                        insignal.Direction = InterfaceSignal.DIRECTION.OUTPUT;
                        break;
                    default:
                        Debug.LogError("Unknown signal direction: " + signalExport.Direction);
                        continue;
                }
                
                insignal.SymbolName = signalExport.Symbolname;
                insignal.Name = signalExport.Name;
                insignal.Comment = signalExport.Comment;
                
                // create the signalobject
                Signal thesignal = plcinterface.AddSignal(insignal);
                
                
                
                // Set the parent folder if available
                if (!string.IsNullOrEmpty(signalExport.Folder))
                {
                    var parentFolder = GameObject.Find(signalExport.Folder);
                    if (parentFolder == null)
                    {
                        parentFolder = new GameObject(signalExport.Folder);
                        parentFolder.transform.SetParent(transform);
                    }

                    thesignal.transform.SetParent(parentFolder.transform);
                } else
                {
                    thesignal.transform.SetParent(transform);
                }

                // Set the sibling index to maintain the order
                thesignal.transform.SetSiblingIndex(i);
            }
            
            // Delete unneeded signals
            if (DeleteUnneededSignalsOnImport)
            {
                var signals = GetComponentsInChildren<Signal>();
                foreach (var signal in signals)
                {
                    var found = false;
                    foreach (var signalExport in signalExportList.Signals)
                    {
                        var goname = signalExport.Name;
                        if (signalExport.Name == "")
                            goname = signalExport.Symbolname;
                        
                        if (signal.gameObject.name == goname)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        DestroyImmediate(signal.gameObject);
                    }
                }
            }

            Debug.Log("Signals imported successfully");
        }

        // Button for exporting signals
        [Button("Export Signals")]
        public void ExportSignals()
        {
            // Check if the signal export folder is set
            if (SignalExportFilePath == "PathtoSignalExport")
            {
                Debug.LogError("Signal Export Folder not set");
                return;
            }

            Debug.Log("Exporting Signals");

            // Get all signals from the interface
            var signals = GetComponentsInChildren<Signal>();

            // Create a SignalExportList
            var signalExportList = new SignalExportList
            {
                Signals = new SignalExport[signals.Length]
            };

            // Convert the signals to SignalExport
            for (var i = 0; i < signals.Length; i++)
            {
                var signal = signals[i];

                var folder = "";
                if (signal.transform.parent != null && signal.transform.parent != transform)
                    folder = signal.transform.parent.name;


                signalExportList.Signals[i] = new SignalExport
                {
                    Name = signal.gameObject.name,
                    Symbolname = signal.Name, // Assuming Symbolname is the same as Name if not defined
                    Comment = signal.Comment, // Add description if available
                    Type = signal.GetTypeString(),
                    Direction = signal.IsInput() ? "INPUT" : "OUTPUT",
                    Folder = folder // Add folder if available
                };
            }

            // Save the SignalExportList to a .json file
            var json = JsonUtility.ToJson(signalExportList, true);
            var filePath = Path.Combine(SignalExportFilePath, SignalExportFilename);

            // check if it is a valid path
            if (!Directory.Exists(SignalExportFilePath))
            {
                Debug.LogError("Signal Export Folder does not exist");
                return;
            }

            File.WriteAllText(filePath, json);
            Debug.Log("Signals exported to: " + filePath);
        }

        // Button for selecting the signal import list
        [Button("Select Signal Import File")]
        public void SelectSignalImportList()
        {
#if UNITY_EDITOR
            Debug.Log("Selecting Signal Import List");
            // Open file dialog to select the signal import list .json file
            var path = EditorUtility.OpenFilePanel("Select Signal Import List", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                SignalImportListFilePath = path;
                Debug.Log("Selected file: " + path);
            }
            else
            {
                Debug.Log("No file selected");
            }
#endif
        }

        // Button for selecting the signal export folder
        [Button("Select Signal Export Folder")]
        public void SelectSignalExportFolder()
        {
#if UNITY_EDITOR
            Debug.Log("Selecting Signal Export Folder");
            // Open folder dialog to select the signal export folder
            var path = EditorUtility.OpenFolderPanel("Select Signal Export Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                SignalExportFilePath = path;
                Debug.Log("Selected folder: " + path);
            }
            else
            {
                Debug.Log("No folder selected");
            }
#endif
        }
    }
}