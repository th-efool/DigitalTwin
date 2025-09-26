using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! MaterialChanger dynamically switches between two materials based on various trigger conditions.
    //! It can change materials based on collisions, sensor states, or PLC output signals, enabling visual feedback for status changes.
    [HelpURL("https://doc.realvirtual.io/components-and-scripts/materialchanger")]
    public class MaterialChanger : realvirtualBehavior,ISignalInterface
    {
        
        [Tooltip("Material to display when status is off")]
        public Material MaterialOff; //!< Material to display when the status is off
        [Tooltip("Material to display when status is on")]
        public Material MaterialOn; //!< Material to display when the status is on

        [OnValueChanged("ChangeDisplay")]
        [Tooltip("Current status that determines which material is displayed")]
        public bool StatusOn; //!< Current status state that determines which material is displayed
        [Tooltip("Enable material change on collision enter/exit events")]
        public bool ChangeOnCollission; //!< If set to true, material changes on collision events
        [Tooltip("Sensor that triggers material change when objects are detected")]
        public Sensor ChangeOnSensor; //!< Sensor that triggers material changes when objects are detected
        [Tooltip("PLC output signal that triggers material change")]
        public PLCOutputBool ChangeOnPLCOutput; //!< PLC output signal that triggers material changes

        private MeshRenderer meshrenderer;
        
        void Start()
        {
            meshrenderer = GetComponent<MeshRenderer>();
            if (MaterialOff == null)
                MaterialOff = meshrenderer.material;
            
            if (ChangeOnPLCOutput != null)
                    ChangeOnPLCOutput.EventSignalChanged.AddListener(OnPLCOutputOnSignalChanged);
            
            if (ChangeOnSensor != null)
                ChangeOnSensor.EventNonMUGameObjectSensor.AddListener(OnSensor);
        }

        private void OnCollisionEnter(Collision other)
        {
            StatusOn = true;
            ChangeDisplay();
        }

        private void OnCollisionExit(Collision other)
        {
            StatusOn = false;
            ChangeDisplay();
        }

        private void OnSensor(GameObject obj, bool occupied)
        {
            StatusOn = occupied;
            ChangeDisplay();
        }

        private void OnPLCOutputOnSignalChanged(Signal obj)
        {
            StatusOn = ((PLCOutputBool) obj).Value;
            ChangeDisplay();
        }

        // Update is called once per frame
        void ChangeDisplay()
        {
            if (StatusOn)
                meshrenderer.material = MaterialOn;
            else
                meshrenderer.material = MaterialOff;
        }
    }
}
