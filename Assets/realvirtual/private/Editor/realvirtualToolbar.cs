// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license


using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if REALVIRTUAL_PLAYMAKER
using PlayMaker;
#endif

namespace realvirtual
{
    [InitializeOnLoad]
    public class realvirtualToolbar : EditorWindow
    {
        private bool groupEnabled;
        
        //! Helper method to show Unity 2022 compatibility warnings for Unity 6-only interfaces
        private static bool CheckUnity6Compatibility(string interfaceName)
        {
#if !UNITY_6000_0_OR_NEWER
            EditorUtility.DisplayDialog("Unity 2022 Compatibility Notice",
                $"The {interfaceName} interface requires Unity 6 or newer due to advanced API dependencies " +
                "(Awaitable API, WebSocketSharp, or RenderGraphModule).\n\n" +
                "In Unity 2022, this interface is not available. Please upgrade to Unity 6 to use this interface, " +
                "or use alternative interfaces compatible with Unity 2022 such as S7, OPCUA, Modbus, TwinCAT ADS, or EthernetIP.",
                "OK");
            return false;
#else
            return true;
#endif
        }

        [MenuItem("realvirtual/Create new realvirtual Scene", false, 1)]
        static void CreateNewScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            AddComponent("Assets/realvirtual/realvirtual.prefab");
        }


        [MenuItem("realvirtual/Export/Full project as package", false, 51)]
        static void ExportWholeProjet()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];

            var path = EditorUtility.SaveFilePanel(
                "Export full project as package",
                "",
                projectName,
                "unitypackage");

            if (path.Length != 0)
            {
                AssetDatabase.ExportPackage("Assets", path,
                    ExportPackageOptions.Interactive | ExportPackageOptions.Recurse |
                    ExportPackageOptions.IncludeLibraryAssets | ExportPackageOptions.IncludeDependencies);
            }
        }

        [MenuItem("realvirtual/Export/Current scene as package", false, 52)]
        static void ExportScene()
        {
            string projectName = SceneManager.GetActiveScene().name;
            string assetpath = SceneManager.GetActiveScene().path;
            var path = EditorUtility.SaveFilePanel(
                "Export current scene including dependencies as package",
                "",
                projectName,
                "unitypackage");

            if (path.Length != 0)
            {
                AssetDatabase.ExportPackage(assetpath, path,
                    ExportPackageOptions.Interactive | ExportPackageOptions.Recurse |
                    ExportPackageOptions.IncludeDependencies);
            }
        }

        [MenuItem("realvirtual/Export/Selected as package", false, 53)]
        static void ExportSelected()
        {
            var selected = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            if (selected.Length == 0)
            {
               Debug.LogError("Please select an object within the Project to export. The current selection is not valid in the given context.");
               return;
            }
            var obj1 = selected[0];
            var projectName = obj1.name;
            string assetpath = AssetDatabase.GetAssetPath(obj1);
            var path = EditorUtility.SaveFilePanel(
                "Export selected including dependencies as package",
                "",
                projectName,
                "unitypackage");

            if (path.Length != 0)
            {
                AssetDatabase.ExportPackage(assetpath, path,
                    ExportPackageOptions.Interactive | ExportPackageOptions.Recurse |
                    ExportPackageOptions.IncludeDependencies);
            }
        }
        
        [MenuItem("realvirtual/Export/Full project as ZIP", false, 53)]
        static void ExportProjectAsZip()
        {
#if UNITY_EDITOR
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];

            string filename = projectName + "-" + Global.Version;
            filename = filename.Replace(" ", "");
            filename = filename.Replace("(", "-");
            filename = filename.Replace(")", "");
            var exportfile = EditorUtility.SaveFilePanel("Save full Project path", "", filename, "zip");
            if (exportfile.Length != 0)
            {
                ZipFile zip = new ZipFile();
                string p = Application.dataPath;
                p = p.Replace("/Assets", "");
                zip.AddDirectory(p);
                var removes = new List<string>();
                zip.RemoveSelectedEntries("Library/*");
                zip.RemoveSelectedEntries("Temp/*");
                zip.Save(exportfile);
#endif
            }
        }



        [MenuItem("realvirtual/Add CAD Link (Pro)", false, 150)]
        static void AddCADLink()
        {
            var find = AssetDatabase.FindAssets(
                "CADLink t:prefab");
            if (find.Length > 0)
                AddComponent(AssetDatabase.GUIDToAssetPath(find[0]));
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "CADLink is only included in realvirtual Professional", "OK");
            }
        }

        [MenuItem("realvirtual/Add Component/Source", false, 160)]
        static void AddSource()
        {
            AddScript(typeof(Source));
        }

        [MenuItem("realvirtual/Add Component/MU", false, 160)]
        static void AddMU()
        {
            AddScript(typeof(MU));
        }

        [MenuItem("realvirtual/Add Component/Drive", false, 160)]
        static void AddDrive()
        {
            AddScript(typeof(Drive));
        }

        [MenuItem("realvirtual/Add Component/Transport Surface", false, 160)]
        static void AddTransportSurface()
        {
            AddScript(typeof(TransportSurface));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/Simple Drive", false, 160)]
        static void AddDriveBehaviourSimple()
        {
            AddScript(typeof(Drive_Simple));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/Destination Drive", false, 160)]
        static void AddDriveBehaviourDestination()
        {
            AddScript(typeof(Drive_DestinationMotor));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/Cylinder", false, 160)]
        static void AddDriveBehaviourCylinder()
        {
            AddScript(typeof(Drive_Cylinder));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/Speed", false, 160)]
        static void AddDriveBehaviourSpeed()
        {
            AddScript(typeof(Drive_Speed));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/ContinousDestination", false, 160)]
        static void AddDriveBehaviourContinousDestination()
        {
            AddScript(typeof(Drive_ContinousDestination));
        }


        [MenuItem("realvirtual/Add Component/Drive Behaviour/Gear", false, 160)]
        static void AddDriveBehaviourGear()
        {
            AddScript(typeof(Drive_Gear));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/CAM", false, 160)]
        static void AddDriveBehaviourCAM()
        {
            AddScript(typeof(CAM));
        }

        [MenuItem("realvirtual/Add Component/Drive Behaviour/CAMTime", false, 160)]
        static void AddDriveBehaviourCAMTime()
        {
            AddScript(typeof(CAMTime));
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Delay", false, 160)]
        static void AddLogicDelay()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_Delay));
#else
    EditorUtility.DisplayDialog("Info",
                    "LogicSteps are only included in Game4Automation Professional.",
                    "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Drive to", false, 160)]
        static void AddLogicDriveTo()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_DriveTo));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Jump", false, 160)]
        static void AddLogicJump()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_JumpOnSignal));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Set Signal Bool", false, 160)]
        static void AddLogicSetSignal()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_SetSignalBool));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Start Drive", false, 160)]
        static void AddLogicStepStartDrive()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_StartDriveSpeed));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Wait for Drives", false, 160)]
        static void AddLogicStepWaitForDrives()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_WaitForDrivesAtTarget));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Wait for Sensor", false, 160)]
        static void AddLogicWaitForSensor()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_WaitForSensor));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/LogicStep (Pro)/Wait for Signal Bool", false, 160)]
        static void AddLogicWaitForSignal()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(LogicStep_WaitForSignalBool));
#else
            EditorUtility.DisplayDialog("Info",
                "LogicSteps are only included in Game4Automation Professional.",
                "OK");
#endif
        }


        [MenuItem("realvirtual/Add Component/Sensor", false, 160)]
        static void AddSensor()
        {
            AddScript(typeof(Sensor));
        }

        [MenuItem("realvirtual/Add Component/Measure", false, 160)]
        static void AddMeasureComponent()
        {
            AddScript(typeof(Measure));
        }

        [MenuItem("realvirtual/Add Component/MeasureRaycast", false, 160)]
        static void AddMeasureRaycastComponent()
        {
            AddScript(typeof(MeasureRaycast));
        }


        [MenuItem("realvirtual/Add Component/Grip", false, 160)]
        static void AddGrip()
        {
            AddScript(typeof(Grip));
        }

        [MenuItem("realvirtual/Add Component/Sink", false, 160)]
        static void AddSink()
        {
            AddScript(typeof(Sink));
        }


        [MenuItem("realvirtual/Add Component/Group", false, 160)]
        static void AddGroup()
        {
            AddScript(typeof(Group));
        }

        [MenuItem("realvirtual/Add Component/Kinematic", false, 160)]
        static void AddKinematicScript()
        {
            AddScript(typeof(Kinematic));
        }

        [MenuItem("realvirtual/Add Component/Chain", false, 160)]
        static void AddChainScript()
        {
            AddScript(typeof(Chain));
        }

        [MenuItem("realvirtual/Add Component/Chain element", false, 160)]
        static void AddChainElementScript()
        {
            AddScript(typeof(ChainElement));
        }
        
        [MenuItem("realvirtual/Add Component/Guide Line", false, 160)]
        static void AddGuideLineComp()
        {
            AddScript(typeof(GuideLine));
        }
        
        [MenuItem("realvirtual/Add Component/Guide Circle", false, 160)]
        static void AddGuideCircleComp()
        {
            AddScript(typeof(GuideCircle));
        }

        [MenuItem("realvirtual/Add Component/Playmaker FSM (Pro)", false, 160)]
        static void AddPlaymakerFSM()
        {
#if REALVIRTUAL_PLAYMAKER
            AddScript(typeof(PlayMakerFSM));
#else
            
            string sym = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (sym.Contains("REALVIRTUAL_PROFESSIONAL"))
            {
                EditorUtility.DisplayDialog("Info",
                    "You need to purchase and download Playmaker on the Unity Asset Store before using it. REALVIRTUAL_PLAYMAKER needs to be set in Scripting Define Symbols.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info",
                    "The Playmaker Actions are only included in Game4Automation Professional.",
                    "OK");
            }
#endif
        }

        [MenuItem("realvirtual/Add Component/PerformanceOptimizer (Pro)", false, 160)]
        static void AddPerformanceOptimizer()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(PerformanceOptimizer));
#else
                EditorUtility.DisplayDialog("Info",
                    "The PerformanceOptimizer is only included in Game4Automation Professional.",
                    "OK");
#endif
        }

        [MenuItem("realvirtual/Add Component/SignalManager (Pro)", false, 160)]
        static void AddSignalManager()
        {
#if REALVIRTUAL_PROFESSIONAL
            AddScript(typeof(SignalManager));
#else
                EditorUtility.DisplayDialog("Info",
                    "SignalManager is only included in Game4Automation Professional.",
                    "OK");
#endif
        }
        
        [MenuItem("realvirtual/Add Component/RobotIK (Pro)", false, 160)]
        static void AddRobotIK()
        {
#if REALVIRTUAL_PROFESSIONAL && !UNITY_WEBGL
            AddScript(typeof(RobotIK));
#else
                EditorUtility.DisplayDialog("Info",
                    "SignalManager is only included in Game4Automation Professional.",
                    "OK");
#endif
        }
        
        [MenuItem("realvirtual/Add Component/Robot Path (Pro)", false, 160)]
        static void AddIKPath ()
        {
#if REALVIRTUAL_PROFESSIONAL && !UNITY_WEBGL
            AddScript(typeof(IKPath));
#else
                EditorUtility.DisplayDialog("Info",
                    "SignalManager is only included in Game4Automation Professional.",
                    "OK");
#endif
        }
        
        [MenuItem("realvirtual/Add Object/Sensor Beam", false, 151)]
        static void AddSensorBeamn()
        {
            AddComponent("Assets/realvirtual/SensorBeam.prefab");
        }

        [MenuItem("realvirtual/Add Object/Measure", false, 151)]
        static void AddMeasure()
        {
            AddComponent("Assets/realvirtual/Measure.prefab");
        }

        [MenuItem("realvirtual/Add Object/MeasureRaycast", false, 151)]
        static void AddMeasureRaycast()
        {
            AddComponent("Assets/realvirtual/MeasureRaycast.prefab");
        }

        [MenuItem("realvirtual/Add Object/Lamp", false, 170)]
        static void AddLamp()
        {
            AddComponent("Assets/realvirtual/Lamp.prefab");
        }

        [MenuItem("realvirtual/Add Object/UI/Button", false, 170)]
        static void AddPushButton()
        {
            AddComponent("Assets/realvirtual/UIButton.prefab");
        }

        [MenuItem("realvirtual/Add Object/UI/Lamp", false, 170)]
        static void AddUILamp()
        {
            AddComponent("Assets/realvirtual/UILamp.prefab");
        }


        [MenuItem("realvirtual/Add Object/Signal/PLC Input Bool", false, 155)]
        static void AddPLCInputBool()
        {
            AddComponent("Assets/realvirtual/PLCInputBool.prefab");
        }

        [MenuItem("realvirtual/Add Object/Signal/PLC Input Float", false, 155)]
        static void AddPLCInputFloat()
        {
            AddComponent("Assets/realvirtual/PLCInputFloat.prefab");
        }

        [MenuItem("realvirtual/Add Object/Signal/PLC Input Int", false, 155)]
        static void AddPLCInpuInt()
        {
            AddComponent("Assets/realvirtual/PLCInputInt.prefab");
        }

        [MenuItem("realvirtual/Add Object/Signal/PLC Output Bool", false, 155)]
        static void AddPLCOutputBool()
        {
            AddComponent("Assets/realvirtual/PLCOutputBool.prefab");
        }

        [MenuItem("realvirtual/Add Object/Signal/PLC Output Float", false, 155)]
        static void AddPLCOutputFloat()
        {
            AddComponent("Assets/realvirtual/PLCOutputFloat.prefab");
        }

        [MenuItem("realvirtual/Add Object/Signal/PLC Output Int", false, 155)]
        static void AddPLCOutputInt()
        {
            AddComponent("Assets/realvirtual/PLCOutputInt.prefab");
        }


        [MenuItem("realvirtual/Add Interface/S7", false, 155)]
        static void AddS7Interface()
        {
            AddComponent("Assets/realvirtual/S7Interface.prefab");
            var find = AssetDatabase.FindAssets(
                "S7Interface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
        }

        [MenuItem("realvirtual/Add Object/Realvirtual", false, 160)]
        static void AddGame4Automatoin()
        {
            AddComponent("Assets/realvirtual/realvirtual.prefab");
        }


#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/TwinCAT ADS (Pro)", false, 156)]
        static void AddTwinCATInterface()
        {
            var find = AssetDatabase.FindAssets(
                "TwinCATInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
#endif

        [MenuItem("realvirtual/Add Interface/TwinCAT HMI (Pro)", false, 156)]
        static void AddTwinCATHMIInterface()
        {
            var find = AssetDatabase.FindAssets(
                "TwinCATHMIInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/SEW MQTT (Pro)", false, 156)]
        static void AddSEWMQTTINterface()
        {
            var find = AssetDatabase.FindAssets(
                "SEWSimInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/IgusRebel", false, 156)]
        static void AddIgusRebelInterface()
        {
            var find = AssetDatabase.FindAssets(
                "igusRebelInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                
            }
        }
        
        [MenuItem("realvirtual/Add Interface/Fanuc (Pro)", false, 156)]
        static void AddFanucInterface()
        {
            var find = AssetDatabase.FindAssets(
                "FanucInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/Denso Robotics (Pro)", false, 156)]
        static void AddDensoInterface()
        {
            var find = AssetDatabase.FindAssets(
                "DensoInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/UniversalRobots (Pro)", false, 156)]
        static void AddUniversalRobots()
        {
            var find = AssetDatabase.FindAssets(
                "UniversalRobotsInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/ABB RobotStudio (Pro)", false, 156)]
        static void AddRobotStudioInterface()
        {
            var find = AssetDatabase.FindAssets(
                "ABBRobotStudioInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
#endif


#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/SIMIT Shared Memory (Pro)", false, 156)]
        static void AddSharedMemoryInterface()
        {
            var find = AssetDatabase.FindAssets(
                "SharedMemoryInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
                {
                    EditorUtility.DisplayDialog("Warning",
                        "This interface is only included in realvirtual.io Professional", "OK");
                }
        }
#endif

#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/PLCSIMAdvanced (Pro)", false, 157)]
        static void AddPLCSimAdvancedInterface()
        {
            var find = AssetDatabase.FindAssets(
                "PLCSIMAdvancedInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
#endif

#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/Simulink (Pro)", false, 156)]
        static void AddSimulinkInterface()
        {
            var find = AssetDatabase.FindAssets(
                "SimulinkInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
#endif
        
        
#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/Siemens Simit (Pro)", false, 156)]
        static void AddSiemensSimitInterface()
        {
            var find = AssetDatabase.FindAssets(
                "SiemensSimitInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
#endif


        [MenuItem("realvirtual/Add Interface/OPCUA (Pro)", false, 155)]
        static void AddOPCUAInterface()
        {
            var find = AssetDatabase.FindAssets(
                "OPCUAInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }

#if UNITY_STANDALONE_WIN
        [MenuItem("realvirtual/Add Interface/RoboDK (Pro)", false, 157)]
        static void AddRoboDKInterface()
        {
            var find = AssetDatabase.FindAssets(
                "RoboDKInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
#endif


        [MenuItem("realvirtual/Add Interface/Modbus (Pro)", false, 157)]
        static void AddPLCConnectInterface()
        {
            var find = AssetDatabase.FindAssets(
                "ModbusInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }

        [MenuItem("realvirtual/Add Interface/EthernetIP (Pro)", false, 157)]
        static void AddEthernetIPInterface()
        {
            var find = AssetDatabase.FindAssets(
                "EthernetIPInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/MQTT (Pro)", false, 157)]
        static void AddMQTTInterface()
        {
            var find = AssetDatabase.FindAssets(
                "MQTTInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/Websocket Realtime (Pro)", false, 158)]
        static void AddWebsocketInterface()
        {
            if (!CheckUnity6Compatibility("Websocket Realtime"))
                return;
                
            var find = AssetDatabase.FindAssets(
                "WebsocketRealtimeInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }

        
        [MenuItem("realvirtual/Add Interface/UDP (Pro)", false, 158)]
        static void AddUDPInterface()
        {
            var find = AssetDatabase.FindAssets(
                "UDPInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/RFSuite (Pro)", false, 158)]
        static void AddRFSuiteInterface()
        {
            var find = AssetDatabase.FindAssets(
                "RFSuiteInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/Winmod Y200 (Pro)", false, 158)]
        static void AddWinmodInterface()
        {
            var find = AssetDatabase.FindAssets(
                "WinmodY200Interface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Interface/Wandelbots NOVA (Pro)", false, 159)]
        static void AddWandelbotsInterface()
        {
            if (!CheckUnity6Compatibility("Wandelbots NOVA"))
                return;
                
            var find = AssetDatabase.FindAssets(
                "WandelbotsNOVAInterface t:prefab");
            if (find.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(find[0]);
                AddComponent(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "This interface is only included in realvirtual.io Professional", "OK");
            }
        }
        
        [MenuItem("realvirtual/Add Object/Kinematic", false, 156)]
        static void AddKinematic()
        {
            AddComponent("Assets/realvirtual/Kinematic.prefab");
        }
        
        [MenuItem("realvirtual/Add Object/TransportGuided", false, 157)]
        static void AddTransportGuided()
        {
            AddComponent("Assets/realvirtual/TransportGuided.prefab");
        }
        
        [MenuItem("realvirtual/Add Object/GuideLine", false, 158)]
        static void AddGuideLine()
        {
            AddComponent("Assets/realvirtual/GuideLine.prefab");
        }
        
        [MenuItem("realvirtual/Add Object/GuideCircle", false, 159)]
        static void AddGuideCircle()
        {
            AddComponent("Assets/realvirtual/GuideCircle.prefab");
        }


        [MenuItem("realvirtual/Settings/Apply standard settings", false, 501)]
        private static void SetStandardSettingsMenu()
        {
            ProjectSettingsTools.SetStandardSettings(true);
            if (Global.g4acontrollernotnull)
                Global.realvirtualcontroller.ResetView();
        }
        
        // Note: Removed Unity 2022/6 renderer switching menu items as they are no longer needed

        [MenuItem("realvirtual/Open demo scene", false, 700)]
        static void OpenDemoScene()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoRealvirtual.unity");
        }


        [MenuItem("realvirtual/Additional demos/Radial conveyor", false, 700)]
        static void OpenDemoRadial()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/RadialConveyordemo.unity");
        }
        
        [MenuItem("realvirtual/Additional demos/Guided Transport", false, 700)]
        static void OpenDemoGuidedTransport()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoGuidedTransport.unity");
        }

        [MenuItem("realvirtual/Additional demos/Moving drives with CAM profiles", false, 700)]
        static void OpenDemoCAM()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/CAMDemo.unity");
        }

        [MenuItem("realvirtual/Additional demos/Changing MU appearance", false, 700)]
        static void OpenDemoChangeMU()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoChangeMU.unity");
        }

        [MenuItem("realvirtual/Additional demos/Modelling chain systems", false, 700)]
        static void OpenDemoChain()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoChain.unity");
        }

        [MenuItem("realvirtual/Additional demos/Simulate industrial robots with RoboDK (Pro)", false, 700)]
        static void OpenDemoRoboDK()
        {
            #if REALVIRTUAL_PROFESSIONAL
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoRoboDK.unity");
            #else
                   EditorUtility.DisplayDialog("Warning",
                    "The RobotDK interface is only included in Realvirtual Professional", "OK");
            #endif
        }

        [MenuItem("realvirtual/Additional demos/Starting Drives with Conditions", false, 700)]
        static void OpenDemoStartingDrives()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoStartDriveOnCondition.unity");
        }

        [MenuItem("realvirtual/Additional demos/Gripping MUs", false, 700)]
        static void OpenDemoGripping()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoGripping.unity");
        }
        
        [MenuItem("realvirtual/Additional demos/Moving Transport Surfaces", false, 700)]
        static void OpenDemoMovingTransportSurface()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/MovingTransportSurface.unity");
        }

        [MenuItem("realvirtual/Additional demos/ForceDrive", false, 700)]
        static void OpenDemoForceDrive()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoForceDrive.unity");
        }

        [MenuItem("realvirtual/Additional demos/Drive with Raycast Limit", false, 700)]
        static void OpenDemoRaycastDrive()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoDriveRaycastLimit.unity");
        }

        [MenuItem("realvirtual/Additional demos/Robot Inverse Kinematic (Pro)", false, 700)]
        static void OpenDemoRobotIK()
        {
            #if REALVIRTUAL_PROFESSIONAL
            EditorSceneManager.OpenScene("Assets/realvirtual/Professional/IK/DemoRobotIK.unity");
            #else
                EditorUtility.DisplayDialog("Warning",
                    "The Inverse Kinematics for Robots is only included in Realvirtual Professional", "OK");
            #endif
        }
        
        [MenuItem("realvirtual/Additional demos/Robot Performance Test (Pro)", false, 700)]
        static void OpenDemoRobotPerformance()
        {
#if REALVIRTUAL_PROFESSIONAL
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoPerformanceRobots.unity");
#else
                EditorUtility.DisplayDialog("Warning",
                    "The Inverse Kinematics for Robots is only included in Realvirtual Professional", "OK");
#endif
        }
        
        [MenuItem("realvirtual/Additional demos/Guided Transport Loading and Unloading", false, 700)]
        static void OpenDemoGuidedLoading()
        {
            EditorSceneManager.OpenScene("Assets/realvirtual/Scenes/DemoLoadingUnloadingGuidedTransport.unity");
        }

        [MenuItem("realvirtual/Documentation ", false, 701)]
        static void OpenDocumentation()
        {
            Application.OpenURL("https://doc.realvirtual.io");
        }
        
        [MenuItem("realvirtual/Unity Version Compatibility Info", false, 702)]
        static void ShowUnityCompatibilityInfo()
        {
#if UNITY_6000_0_OR_NEWER
            EditorUtility.DisplayDialog("Unity 6 Compatibility", 
                "You are running Unity 6 - all realvirtual interfaces and features are available.\n\n" +
                "This includes advanced interfaces like Websocket Realtime, Wandelbots NOVA, and all rendering features.",
                "OK");
#else
            EditorUtility.DisplayDialog("Unity 2022 Compatibility", 
                "You are running Unity 2022 - most realvirtual features are available with some limitations.\n\n" +
                "Available interfaces: S7, OPCUA, Modbus, TwinCAT ADS, PLCSIMAdvanced, MQTT, EthernetIP, and more.\n\n" +
                "Unity 6-only interfaces: Websocket Realtime, Wandelbots NOVA (require Unity 6 advanced APIs).\n\n" +
                "Recommendation: Upgrade to Unity 6 for full feature compatibility.",
                "OK");
#endif
        }
        
        static void Info()
        {
            Application.OpenURL("https://realvirtual.io");
        }


        static void AddScript(System.Type type)
        {
            GameObject component = Selection.activeGameObject;

            if (component != null)
            {
                Undo.AddComponent(component, type);
            }
            else
            {
                EditorUtility.DisplayDialog("Please select an Object",
                    "Please select first an Object where the script should be added to!",
                    "OK");
            }
        }


        static GameObject AddComponent(string assetpath)
        {
            GameObject component = Selection.activeGameObject;
            Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (go != null)
            {
                go.transform.position = new Vector3(0, 0, 0);
                if (component != null)
                {
                    go.transform.parent = component.transform;
                }

                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            }

            return go;
        }
    }
}