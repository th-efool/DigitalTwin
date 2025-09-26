using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using ExcelDataReader;
using System.IO;


namespace realvirtual
{
    public enum CamFileType
    {
        Excel,
        CSV
    }

    #region doc
    //! CAM component implements electronic cam functionality for synchronized master-slave drive control in automation systems.
    
    //! The CAM component provides electronic cam control, allowing a slave drive to follow complex motion profiles
    //! based on the position of a master drive. This simulates the behavior of mechanical cam systems digitally,
    //! enabling precise, repeatable motion patterns without physical cam mechanisms. The component uses animation
    //! curves or imported data tables to define the relationship between master and slave positions.
    //!
    //! Key features:
    //! - Position-based synchronization between master and slave drives
    //! - Import cam profiles from Excel or CSV files for easy integration with engineering tools
    //! - Visual animation curve editor for custom motion profiles
    //! - Scaling and offset adjustments for both master and slave axes
    //! - Continuous mode for endless motion patterns (e.g., chain drives)
    //! - Support for complex non-linear relationships and motion interpolation
    //! - Column-based or header-based data import with flexible mapping
    //! - Real-time profile switching and modification capabilities
    //!
    //! Common applications in industrial automation:
    //! - Packaging machines with synchronized sealing and cutting operations
    //! - Bottle filling systems with coordinated nozzle and conveyor movement
    //! - Press machines with complex ram motion profiles
    //! - Pick and place robots with optimized acceleration profiles
    //! - Rotary indexing tables with variable speed segments
    //! - Web handling systems with dancer roll compensation
    //! - Flying shear applications with speed matching
    //! - Labeling machines with product tracking
    //!
    //! The CAM profile defines how the slave axis position changes relative to the master axis position.
    //! This enables complex motion patterns such as:
    //! - Dwell periods where slave remains stationary while master moves
    //! - Quick return strokes with different forward and return speeds
    //! - Smooth acceleration and deceleration profiles
    //! - Multiple motion segments with different characteristics
    //! - Cyclic patterns for continuous operation
    //!
    //! Integration with other components:
    //! - Requires connection to master Drive component for position input
    //! - Acts as a behavior on slave Drive component
    //! - Can be combined with other drive behaviors for complex control
    //! - Works with PLCInputFloat/PLCOutputFloat for dynamic parameter adjustment
    //! - Compatible with drive limits and safety functions
    //!
    //! Data import capabilities:
    //! - Excel files with configurable sheet and column selection
    //! - CSV files with header or column number mapping
    //! - Automatic curve generation from imported data points
    //! - Support for engineering units and scaling factors
    //! - Preview and validation of imported profiles
    //!
    //! Performance considerations:
    //! - CAM evaluation happens in FixedUpdate for deterministic behavior
    //! - Interpolation between curve points ensures smooth motion
    //! - Continuous mode handles wraparound for endless applications
    //! - Efficient curve evaluation using Unity's AnimationCurve system
    //!
    //! The CAM component is essential for applications requiring precise coordination between
    //! multiple axes, replacing traditional mechanical cams with flexible electronic control.
    //!
    //! For detailed documentation and examples, see:
    //! https://doc.realvirtual.io/components-and-scripts/motion/cam
    #endregion
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/motion/cam")]
    public class CAM : BaseCAM, IDriveBehavior
    {
        public class campoint
        {
            public float master;
            public float slave;
        }
        

        [Tooltip("The master drive that controls this slave drive's position through the CAM profile")]
        public Drive MasterDrive; //!< The master drive this slave drive is attached to

        [Header("Scaling and Offset")]
        [Tooltip("Scale factor applied to master drive position before CAM curve evaluation")]
        public float
            MasterDriveAxisScale =
                1; //!< A scale factor. The master drive position is multiplied with this factor to get the position which is used in the CAM curve

        [Tooltip("Offset added to master drive position before CAM curve evaluation")]
        public float
            MasterDriveAxisOffset =
                0; //!< An offset to the master drive. The offset is added to the master drive position to get the position which is used in the CAM curve

        [Tooltip("Scale factor applied to CAM curve output values")]
        public float CAMAxisScale = 1; //!< The scale of the CAM axis. It will scale the values of the CAM curve.

        [Tooltip("Offset added to CAM curve output values for slave axis position")]
        public float
            CAMAxisOffset =
                0; //!< The offset of the CAM axis. It will be added to the values of the CAM cure to get the position which is applied to the CAM (slave) axis. 

        [Header("CAM Curve")]
        [Tooltip("File format for importing CAM profile data")]
        public CamFileType FileType = CamFileType.Excel;

        [ShowIf("FileType", CamFileType.Excel)]
        [Tooltip("Name of the Excel sheet to import (leave empty for first sheet)")]
        public string ExcelSheet;

        [ShowIf("FileType", CamFileType.Excel)]
        [Tooltip("Path to the Excel file containing CAM data")]
        public string ExcelFile;

        [Button("Select CAM File")]
        void selectFile()
        {
            try
            {
#if UNITY_EDITOR
                ExcelFile = EditorUtility.OpenFilePanel("Select file to import", "Assets", "xlsx");
#endif
            }
            catch (Exception e)
            {
                var error = e;
            }
        }

        [Tooltip("Animation curve defining slave drive position based on master drive position")]
        public AnimationCurve
            CamCurve; //!< The Animation Curve which is defining the slave drive position in relation to the master drive position

        private char lineSeperater = '\n'; //!< It defines line seperate character
        private char fieldSeperator = ','; //!< It defines field seperate chracter

        [ShowIf("FileType", CamFileType.CSV)]
        [Tooltip("CSV text asset containing CAM profile data with master and slave axis columns")]
        public TextAsset
            CamDefintion; //!< A text assed containing the CAM definition. This asset is a table with optional headers and columns describing the master axis position and the slave axis position.

        [Tooltip("Use column header names to identify master and slave data columns")]
        public bool UseColumnNames; //!< If true the Column Names are used to define the data to import
        [ShowIf("UseColumnNames")]
        [Tooltip("Column name containing master axis position data")]
        public string MasterColumn; //!< The master axis column name
        [ShowIf("UseColumnNames")]
        [Tooltip("Column name containing slave axis position data")]
        public string SlaveColumn; //!< The slave axis column name

        [ShowIf("FileType", CamFileType.CSV)]
        [Tooltip("Use column numbers (starting from 1) to identify data columns")]
        public bool
            UseColumnNumbers =
                false; //!< If true the Column Numbers (starting with 1 for the 1st column) are used to define the data to import

        [ShowIf("UseColumnNumbers")]
        [Tooltip("Column number for master axis data (1-based index)")]
        public int MasterColumnNum = 1; //!< The master axis column number
        [ShowIf("UseColumnNumbers")]
        [Tooltip("Column number for slave axis data (1-based index)")]
        public int SlaveColumnNum = 2; //!< The slave axis column number

        [ShowIf("FileType", CamFileType.CSV)]
        [Tooltip("First line of CSV contains column headers")]
        public bool
            CamDefinitionWithHeader =
                true; //!< if true during import a column header is expected and first line of the imported data should be a header

        [Tooltip("Automatically import CAM data when simulation starts")]
        public bool ImportOnStart = false; //! if true text asset is always imported on simulation start

        [Header("Behaviour")]
        [Tooltip("Enable continuous CAM operation with offset for endless motion (e.g., transport chains)")]
        public bool
            IsContinous =
                false; //!< If set to true the CAM will continue as an offset based on the the last CAM position, for example for continous positive moving things like transport chains 

        [ReadOnly] public float ContinousOffset = 0;
        private List<campoint> camdata;
        private Drive _slave;
        private float _lastmasterpos;
        private float _lastslavepos;
        private float currentmasterpos;
       
        
        private bool masterjumpedtolowerlimit = false;
        private float slavedelta;
        private float lastmasterdrivepos;
        private float deltatobeonmastercurve;

        [Button("Import CAM")]
        public void ImportCam()
        {
            CamCurve = new AnimationCurve();

            switch (FileType)
            {
                case CamFileType.Excel:
                    ImportCamFileExcel();
                    break;
                case CamFileType.CSV:
                    ImportCAMFileCSV();
                    break;
            }

            if (camdata.Count == 0)
                Debug.LogError("No data found in the defined files.");

            else
            {
                foreach (var campoint in camdata)
                {
                    CamCurve.AddKey(campoint.master, campoint.slave);
                }

                // Set the tangents
                for (int i = 1; i < CamCurve.keys.Length - 1; i++)
                {
                    Keyframe key = CamCurve[i];
                    key.inTangent = 1;
                    key.outTangent = 0;
                    CamCurve.MoveKey(i, key);
                }
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

        void ImportCamFileExcel()
        {
            using (var stream = File.Open(ExcelFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    if (result == null)
                    {
                        Debug.LogError("The chosen excel file is not valid. Please check for the excel file.");
                    }

                    DataTable currentSheet;
                    if (ExcelSheet != "")
                    {
                        if (result.Tables.Contains(ExcelSheet))
                        {
                            currentSheet = result.Tables[ExcelSheet];
                            ImportExcelSheetData(currentSheet);
                        }
                        else
                        {
                            Debug.LogError("Defined sheet is not in chosen excel file.");
                        }
                    }
                    else
                    {
                        int sheetIndex = 0;
                        do
                        {
                            DataTable sheet = result.Tables[sheetIndex];
                            ImportExcelSheetData(sheet);
                            sheetIndex++;
                        } while (sheetIndex < result.Tables.Count && camdata.Count == 0);
                    }
                }
            }
        }

        private void ImportCAMFileCSV()
        {
            string[] header = new[] { "" };
            camdata = new List<campoint>();

            var csvFile = CamDefintion.text;

            if (csvFile == "")
                csvFile = System.Text.Encoding.UTF8.GetString(CamDefintion.bytes);

            string[] lines = csvFile.Split(lineSeperater);
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
                                if (header[fieldcol] == MasterColumn)
                                {
                                    campoint.master = GetFloat(field, 0);
                                }

                                if (header[fieldcol] == SlaveColumn)
                                {
                                    campoint.slave = GetFloat(field, 0);
                                }
                            }

                            if (UseColumnNumbers)
                            {
                                if (fieldcol + 1 == MasterColumnNum)
                                {
                                    campoint.master = GetFloat(field, 0);
                                }

                                if (fieldcol + 1 == SlaveColumnNum)
                                {
                                    campoint.slave = GetFloat(field, 0);
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


        private void ImportExcelSheetData(DataTable sheet)
        {
            int masterColumn = 0;
            int slaveColumn = 0;
            int slaveRow = 0;
            bool masterFound = false;
            bool slaveFound = false;

            int row = 0;
            do
            {
                DataRow currentRow = sheet.Rows[row];
                int col = 0;
                do
                {
                    object cellValue = currentRow[col];
                    var valueType = cellValue.ToString();

                    if (valueType == MasterColumn)
                    {
                        masterColumn = col;
                        masterFound = true;
                    }
                    else if (valueType == SlaveColumn)
                    {
                        slaveColumn = col;
                        slaveRow = row;
                        slaveFound = true;
                    }

                    col++;
                } while (col < sheet.Columns.Count && (!masterFound || !slaveFound));

                row++;
            } while ((row < sheet.Rows.Count && (!masterFound || !slaveFound)) || row == sheet.Rows.Count);

            if (masterFound && slaveFound)
            {
                camdata = new List<campoint>();
                
                for (int i = slaveRow + 1; i < sheet.Rows.Count; i++)
                {
                    var campoint = new campoint();
                    
                    if (float.TryParse(sheet.Rows[i][masterColumn].ToString(), System.Globalization.NumberStyles.Any,
                            CultureInfo.InvariantCulture, out float floatValue))
                        campoint.master = floatValue;

                    if (float.TryParse(sheet.Rows[i][slaveColumn].ToString(), System.Globalization.NumberStyles.Any,
                            CultureInfo.InvariantCulture, out float floatValue2))
                        campoint.slave = floatValue2;

                    camdata.Add(campoint);
                }
            }
            else
            {
                Debug.LogError("Definded columns not found!");
            }
        }

        private void MasterDriveOnOnJumpToLowerLimit(Drive drive)
        {
            masterjumpedtolowerlimit = true;
            if (MasterDriveAxisOffset == 0 && !IsContinous)
            {
                // determine offset
                var slavepos = CamCurve.Evaluate(drive.UpperLimit) * CAMAxisScale + CAMAxisOffset;
                ContinousOffset = ContinousOffset + slavepos;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
       
            var _thisdrive = GetComponent<Drive>();
            if (MasterDrive != null)
                MasterDrive.AddSubDrive(
                    _thisdrive); //! Add this drive as a subdrive to the master drive for guaranteed fixed update sequence
            ContinousOffset = 0;
            _slave = GetComponent<Drive>();
            if (ImportOnStart)
                ImportCam();
            if (IsContinous)
            {
                slavedelta = (MasterDrive.UpperLimit - MasterDrive.LowerLimit) * MasterDriveAxisScale;
                if (MasterDrive.UseLimits == false || MasterDrive.JumpToLowerLimitOnUpperLimit == false)
                {
                    Error("Continous CAM requires Master Drive with Limits and Jump To Lower Limit settings!");
                }
                // get the last value of the slave
                _lastslavepos= CamCurve.Evaluate(MasterDrive.UpperLimit * MasterDriveAxisScale);
                MasterDrive.OnJumpToLowerLimit += MasterDriveOnOnJumpToLowerLimit;
            }
        }

        private float Evaluate(float masterdrivepos)
        {
          // get masterdrive offset and scaled value - this gives where to loook in the cam curve on x
            var masterdriveoffsetandscaled = masterdrivepos * MasterDriveAxisScale + MasterDriveAxisOffset;
            var masterdrivescaledmin = MasterDrive.LowerLimit * MasterDriveAxisScale;
            var masterdrivescaledmax = MasterDrive.UpperLimit * MasterDriveAxisScale;
            if (IsContinous) // is this x value available in the cam curve  - if not correct it to be always in the positive range of the x axis
            {
                if (masterjumpedtolowerlimit)
                    masterjumpedtolowerlimit = true;

                float localdeltatobeonmastercurve = 0;
                if (masterdriveoffsetandscaled < masterdrivescaledmin)
                {
                    localdeltatobeonmastercurve = slavedelta;
                }

                if (masterdriveoffsetandscaled > masterdrivescaledmax)
                {
                    localdeltatobeonmastercurve = -slavedelta;
                }

                float tododelta = 0;
                bool nodeltasave = false;
                if (!masterjumpedtolowerlimit)
                {
                    // the delta changed - curve is starting again  
                    if (deltatobeonmastercurve != localdeltatobeonmastercurve)
                    {
                            ContinousOffset = ContinousOffset + _lastslavepos;
                            deltatobeonmastercurve = localdeltatobeonmastercurve;
                            tododelta = -slavedelta;
                            nodeltasave = true;
                    }
                    else
                    {
                        tododelta = localdeltatobeonmastercurve;
                    }
                }
                else
                {
                    if (MasterDriveAxisOffset != 0)
                        tododelta = localdeltatobeonmastercurve;
                    else
                    {
                        ContinousOffset = ContinousOffset + _lastslavepos;
                        tododelta = 0;
                    }
                }

                masterdriveoffsetandscaled = masterdriveoffsetandscaled + tododelta;

                if (!nodeltasave)
                    deltatobeonmastercurve = tododelta;
            }


            var slavepos = CamCurve.Evaluate(masterdriveoffsetandscaled) * CAMAxisScale + CAMAxisOffset +
                           ContinousOffset;
            return slavepos;
        }


        // Update is called once per frame
        public void CalcFixedUpdate()
        {
            var slavepos = Evaluate(MasterDrive.CurrentPosition);
            _slave.SetPosition(slavepos);
            _lastmasterpos = MasterDrive.CurrentPosition;
            masterjumpedtolowerlimit = false;
        }
    }
}