// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

namespace realvirtual
   
{
#pragma warning disable 0108
    //! Adds buttons or Lights which can be connected to Signals tp a 3D scene
    public class Interact3D : BehaviorInterface
    {
        [Header("Status")] [OnValueChanged("UpdateVisual")]
        [Tooltip("Current on/off status of the button or switch")]
        public bool On;  //!< Status On

        [OnValueChanged("UpdateVisual")]
        [Tooltip("Indicates if mouse button is currently pressed")]
        public bool MouseDown; //!< Mouse is down
        [OnValueChanged("UpdateVisual")]
        [Tooltip("Indicates if PLC signal is on")]
        public bool PLCOn;  //!< PLC Signal is ON
        [OnValueChanged("UpdateVisual")]
        [Tooltip("Blocks interaction when true (e.g., for security doors)")]
        public bool Blocked; //< true if interaction is blocked by a PLCSignal (e.g. for security doors)
        [Header("Settings")]
        [Tooltip("Switch mode (toggle) when true, button mode (momentary) when false")]
        public bool Switch; //!< true if interaction should work like a switch, if false it works like a button
        
        [Tooltip("Material to display when button/switch is on")]
        public Material MaterialOn;  //!< Material which should be used for the ON status of the Switch or Button
        [Tooltip("Material to display when mouse button is pressed")]
        public Material MaterialOnMouseDown; //!< Material which should be used on mouse down
        [Tooltip("Material to display when interaction is blocked")]
        public Material MaterialOnBlocked; //!< Material which should be used if interaction is blocked by a PLCSignal
        [Tooltip("Duration in seconds to show blocked material feedback")]
        public float DurationMaterialOnBlocked;  //!< Duration of visual feedback with MaterialOnBlocked. If user is pressing button and it is blocked by PLC it will be visualized
        [Tooltip("Material to display when PLC signal is on")]
        public Material MaterialPLCOn;  //!< Material which should be used if PLC Output signal SignalOn is true
        [Tooltip("Optional light to turn on when button/switch is on")]
        public Light LightOn; //!< Optional light which is turned on on ON status
        [Tooltip("Optional light to turn on when PLC signal is on")]
        public Light LightPLCOn; //!< Optional light which is turned on if PLC Output signal SignalOn is true


        [Header("PLC IOs")]
        [Tooltip("PLC input signal reflecting button/switch state")]
        public PLCInputBool SignalIsOn; //!< PLCInput if button or switch status is on
        [Tooltip("PLC output signal to control PLCOn status")]
        public PLCOutputBool SignalOn; //!< PLCOutput to turn PLCOn Status on
        [Tooltip("PLC output signal to block button interaction")]
        public PLCOutputBool SignalBlocked; //!< PLCOutput to block interaction with button


        private  Collider collider;
        private Material standardmaterial;

        private  MeshRenderer renderer;

        // Start is called before the first frame update
        private bool signalisonnotnull;
        private bool signalblockendnotnull;
        private bool signalonnutnull;
        private bool lastsginalon;
        
#if UNITY_EDITOR
        void OnScene(SceneView scene)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 2)
            {

                Vector3 mousePos = e.mousePosition;

                Ray ray = scene.camera.ScreenPointToRay(mousePos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider == collider)
                    {
                      
                        OnMouseDown();
                        e.Use();
                    }
                }
            }

            if (e.type == EventType.MouseUp && e.button == 2)
            {
            
                OnMouseUp();
            }

        }
#endif
        
        //! Public method for activating the button or switch for custom developments 
        public void OnActivated()
        {
            if (Switch)
                On = !On;
            if (!Switch)
                On = true;
            UpdateVisual();
        }
        
        //! Public method for deactivating the button or switch for custom developments 
        public void OnDeActivated()
        {
            if (Switch)
                On = !On;
            if (!Switch)
                On = false;
            UpdateVisual();
        }
        
        new void  Awake()
        {
            #if UNITY_EDITOR
            SceneView.duringSceneGui += OnScene;
            #endif
            collider = GetComponent<Collider>();
            if (collider == null)
                collider = gameObject.AddComponent<MeshCollider>();
            renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
                standardmaterial = renderer.material;
            signalisonnotnull = SignalIsOn != null;
            signalblockendnotnull = SignalBlocked != null;
            signalonnutnull = SignalOn != null;
            base.Awake();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnScene;
#endif
        }

        private void ChangeMaterial(Material newmaterial)
        {
            if (renderer != null)
                if (standardmaterial != null)
                    if (newmaterial != null)
                        renderer.material = newmaterial;
        }

        private void UpdateVisual()
        {
            if (!On)
                ChangeMaterial(standardmaterial);
            if (On)
                ChangeMaterial(MaterialOn);
            if (MouseDown)
                ChangeMaterial(MaterialOnMouseDown);
            if (PLCOn)
                ChangeMaterial(MaterialPLCOn);

            if (LightOn != null)
                LightOn.enabled = On;
            if (LightPLCOn != null)
                LightPLCOn.enabled = PLCOn;
            var halo = GetComponent("Halo");
            if (halo != null)
            {
                if (On || PLCOn)
                    halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
                else
                    halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
            }

        }

        private void Start()
        {
            UpdateVisual();
        }

        private void OnMouseDown()
        {
            if (!On && Blocked)
            {
                ChangeMaterial(MaterialOnBlocked);
                Invoke("UpdateVisual", DurationMaterialOnBlocked);
                return;
            }

            if (Switch)
                On = !On;
            if (!Switch)
                On = true;
            MouseDown = true;
            UpdateVisual();
        }

        private void OnMouseUp()
        {
            if (!On && Blocked)
                return;

            if (!Switch)
                On = false;
            MouseDown = false;
            UpdateVisual();
        }

        private void FixedUpdate()
        {
            if (signalisonnotnull)
            {
                SignalIsOn.Value = On;
            }

            if (signalonnutnull)
            {
                PLCOn = SignalOn.Value;
                if (SignalOn.Value != lastsginalon)
                {
                    UpdateVisual();
                }

                lastsginalon = SignalOn.Value;
            }

            if (signalblockendnotnull)
            {
                if (SignalBlocked.Value != Blocked)
                {
                    Blocked = SignalBlocked.Value;
                    UpdateVisual();
                }
            }
            
        }
    }
}
