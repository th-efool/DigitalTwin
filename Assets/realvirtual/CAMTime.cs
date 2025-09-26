using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UIElements;


namespace realvirtual
{
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/cam")]
    //! CAM for moving drives based on CAM profiles 
    public class CAMTime : BaseCAM
    {
        public class campoint
        {
            public float master;
            public float slave;
        }
        [Header("Scaling and Offset")]
        [Tooltip("Time scale factor to make CAM animation run faster or slower")]
        public float CAMTimeScale=1; //!< An scale of the Time makes CAM faster or slowér
        [Tooltip("Scale factor applied to CAM curve output values")]
        public float CAMAxisScale=1; //!< The scale of the CAM axis. It will scale the values of the CAM curve.
        [Tooltip("Offset added to CAM curve output values")]
        public float CAMAxisOffset=0; //!< The offset of the CAM axis. It will be a offset to the design position
        
        [Header("Start by Master Drive")]
        [Tooltip("Master drive that triggers this CAM when reaching start position")]
        public Drive MasterDrive;  //!< The master drive this slave drive is attached to
        [Tooltip("Start CAM when master drive position exceeds this value")]
        public float StartOnMasterPosGreaterThan;
        
        [Header("CAM Curve")]
        [Tooltip("Animation curve defining position over time")]
        public AnimationCurve CamCurve; //!< The Animation Curve which is defining the slave drive position in relation to the master drive position
        
        private char lineSeperater = '\n'; //!< It defines line seperate character
        private char fieldSeperator = ','; //!< It defines field seperate chracter

        [Tooltip("CSV text asset containing CAM profile data with time and position columns")]
        public TextAsset CamDefintion;  //!< A text assed containing the CAM definition. This asset is a table with optional headers and columns describing the master axis position and the slave axis position.
        
        [Tooltip("Use column header names to identify time and position data")]
        public bool UseColumnNames; //!< If true the Column Names are used to define the data to import
        [ShowIf("UseColumnNames")]
        [Tooltip("Column name containing time values")]
        public string MasterTime; //!< The master axis column name
        [ShowIf("UseColumnNames")]
        [Tooltip("Column name containing axis position values")]
        public string AxisColumn; //!< The slave axis column name
        
        [Tooltip("Use column numbers (starting from 1) to identify data columns")]
        public bool UseColumnNumbers; //!< If true the Column Numbers (starting with 1 for the 1st column) are used to define the data to import
        [ShowIf("UseColumnNumbers")]
        [Tooltip("Column number for time data (1-based index)")]
        public int TimeColumnNum=1;  //!< The master axis column number
        [ShowIf("UseColumnNumbers")]
        [Tooltip("Column number for axis position data (1-based index)")]
        public int AxisColumnNum=2; //!< The slave axis column number

        [Tooltip("First line of CSV contains column headers")]
        public bool CamDefinitionWithHeader = true; //!< if true during import a column header is expected and first line of the imported data should be a header
        [Tooltip("Automatically import CAM data when simulation starts")]
        public bool ImportOnStart = false; //! if true text asset is always imported on simulation start
        private List<campoint> camdata;

        [Header("Start next CAM")]
        [Tooltip("CAM to start automatically when this CAM finishes")]
        public CAMTime StartCamWhenFinished;
        
        [Header("CAM IO's")]
        [Tooltip("Start or stop the CAM animation")]
        public bool StartCAM;
        [ReadOnly] public bool IsActive;
        [ReadOnly] public bool IsFinished;
        [ReadOnly] public float CurrentCAMTime;

     
        [Header("PLC IO's")]
        [Tooltip("PLC signal to start the CAM")]
        public PLCOutputBool PLCStartCAM;
        [Tooltip("PLC signal to scale CAM time dynamically")]
        public PLCOutputFloat PLCScaleCAM;
        [Tooltip("PLC input signal indicating CAM is running")]
        public PLCInputBool PLCCAMIsRunning;
        [Tooltip("PLC input signal indicating CAM has finished")]
        public PLCInputBool PLCCAMIsFinished;
        [Tooltip("PLC input signal showing current CAM time in seconds")]
        public PLCInputFloat PLCCurrentCAMTime;
        
        private Drive _slave;
        private Drive _master;
        private bool _mastercontrolled;
        private float starttime = 0;
        private float laststoppedmaster = 0;
        private bool _startbefore;
        private bool _isPLCStartCamNotNull;
        private bool _isPLCCAMIsRunningNotNull;
        private bool _isPLCCurrentCAMTimeNotNull;
        private bool _isPLCCAMIsFinishedNotNull;

        [Button("Import CAM")]
        public void ImportCam()
        {
            CamCurve = new AnimationCurve();
            ImportCAMFile();
            
            foreach (var campoint in camdata)
            {
                CamCurve.AddKey(campoint.master, campoint.slave);
            }
            
            // Set the tangents
            for (int i = 1; i < CamCurve.keys.Length-1; i++)
            {
                Keyframe key = CamCurve[i];
                key.inTangent = 1;
                key.outTangent = 0;
                CamCurve.MoveKey(i,key);
            }
        
        }
        
        public static float GetFloat(string value, float defaultValue)
        {
            float result;

            if (!float.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }
            
            
            return result;
        }
      
        private void ImportCAMFile()
        {
            string[] header = new[] {""};
            camdata = new List<campoint>();
            
            var csvFile = CamDefintion.text;
            
            if (csvFile=="")
                csvFile = System.Text.Encoding.UTF8.GetString(CamDefintion.bytes);
            
            string[] lines = csvFile.Split (lineSeperater);
            var isheader = true;
            foreach (string line in lines)
            {
                if (isheader && CamDefinitionWithHeader)
                {
                    header = line.Split(fieldSeperator);
                    // Clean Header
                    for (int i = 0; i < header.Length; i++)
                    {
                        header[i] = header[i].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    }

                  
                    isheader = false;
                }
                else
                {
                    var campoint = new campoint();
                    
                    string[] fields = line.Split(fieldSeperator);
                    var fieldcol = 0;
                    foreach (var field in fields)
                    {
                        try
                        {
                            if (UseColumnNames && CamDefinitionWithHeader)
                            {
                                if (header[fieldcol] == MasterTime)
                                {
                                    campoint.master = GetFloat(field,0);
                                }

                                if (header[fieldcol] == AxisColumn)
                                {
                                    campoint.slave = GetFloat(field,0);
                                }
                            }

                            if (UseColumnNumbers)
                            {
                                if (fieldcol+1 == TimeColumnNum)
                                {
                                    campoint.master = GetFloat(field,0);
                                }

                                if (fieldcol+1 == AxisColumnNum)
                                {
                                    campoint.slave = GetFloat(field,0);
                                }
                            }

                            fieldcol++;
                        }
                        catch (Exception e)
                        {
                            Error(e.Message);
                        }
                    }
                    camdata.Add(campoint);
                }
                
            }
        }

        public void StartTimeCAM()
        {
            IsActive = true;
            starttime = Time.fixedTime;
            IsFinished = false;
         
        }
        
        void StopTimeCAM()
        {
            IsActive = false;
            starttime = 0;
            IsFinished = true;
            if (_mastercontrolled)
                 laststoppedmaster = MasterDrive.CurrentPosition;
        }

        new void Awake()
        {
            _slave = GetComponent<Drive>();
            _isPLCStartCamNotNull =  PLCStartCAM != null;
            _isPLCCAMIsRunningNotNull =  PLCCAMIsRunning != null;
            _isPLCCurrentCAMTimeNotNull =  PLCCurrentCAMTime != null;
            _isPLCCAMIsFinishedNotNull =  PLCCAMIsFinished != null;
                
            if (MasterDrive != null)
                _mastercontrolled = true;
            else
                _mastercontrolled = false;
            if (ImportOnStart)
                ImportCam();
            IsActive = false;
            laststoppedmaster = 9999999f;
            base.Awake();
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            // Set PLCOutputs if available
            if (_isPLCStartCamNotNull)
                StartCAM = PLCStartCAM.Value;
            
            if (_mastercontrolled && !IsActive)
            {
                if (MasterDrive.CurrentPosition < laststoppedmaster)
                {
                    if ((MasterDrive.CurrentPosition >= StartOnMasterPosGreaterThan)) 
                    {
                        StartTimeCAM();
                    }
                }
            }

            if (StartCAM && !_startbefore)
            {
                StartTimeCAM();
            }

            if (IsActive)
            {
                CurrentCAMTime = Time.fixedTime - starttime;
                CurrentCAMTime = CurrentCAMTime * CAMTimeScale;
                var Length = CamCurve.length;
                var lastkey = CamCurve.keys[Length-1];
                var maxtime = lastkey.time;

                if (CurrentCAMTime >= maxtime)
                {
                    _slave.SetPosition(lastkey.value);
                   StopTimeCAM();
                   if (StartCamWhenFinished!=null)
                       StartCamWhenFinished.StartTimeCAM();
                }
                else
                {
                    var posi = CamCurve.Evaluate(CurrentCAMTime)*CAMAxisScale+CAMAxisOffset;
                    _slave.SetPosition(posi);
                }
            }

            _startbefore = StartCAM;
            
            // Set PLCInputs if a vailable
            if (_isPLCCAMIsRunningNotNull)
                PLCCAMIsRunning.Value = IsActive;
            if (_isPLCCurrentCAMTimeNotNull)
                PLCCurrentCAMTime.Value = CurrentCAMTime;
            if (_isPLCCAMIsFinishedNotNull)
                PLCCAMIsFinished.Value = IsFinished;

        }
    }
}
