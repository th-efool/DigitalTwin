
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    public class UpdateScene2022 : EditorWindow
    {
        // [MenuItem("realvirtual/Settings/Update Scene to 2022", false, 502)]
        private static void UpdateSceneTo2022()
        {
            if (EditorUtility.DisplayDialog("Update Scene to 2022", "Do you want to update this scene made with realvirtual 2021 to 2022 - this will change layers, raycasts and colliders to standard settings. You might need to do additional changes to make the Scene work with realvirtual 2022", "Yes", "No"))
            {
              
            }
            else
            {
                return;
            }
            
            // get all needed components in scene
            var sensors = GameObject.FindObjectsByType<Sensor>(FindObjectsSortMode.None);
            var transportsurfaces = GameObject.FindObjectsByType<TransportSurface>(FindObjectsSortMode.None);
            var mus = GameObject.FindObjectsByType<MU>(FindObjectsSortMode.None);
            var sinks = GameObject.FindObjectsByType<Sink>(FindObjectsSortMode.None);
            var sources = GameObject.FindObjectsByType<Source>(FindObjectsSortMode.None);
            var transforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            
            // change layers in colliders
            foreach (var transform in transforms)
            {
                // check if collider layernumber is 19
                
                if (transform.gameObject.layer == 19)
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvTransport");
                }
                
                // get layer number of collider
                
                // check if collider ist g4a MU
                if (transform.gameObject.layer == 20)
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvMU");
                }
                
                // check if collider ist g4a SensorMU
                if (transform.gameObject.layer == 16 )
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvMUSensor");
                }
                
                if (transform.gameObject.layer == 18)
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvSensor");
                }
                if(transform.gameObject.layer ==22)
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvSnapping");
                }
                if(transform.gameObject.layer ==23)
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvSimDynamic");
                }
                if(transform.gameObject.layer ==24)
                {
                    transform.gameObject.layer = LayerMask.NameToLayer("rvSimStatic");
                }
                
                EditorUtility.SetDirty(transform);
                
            }
            
            
          
            
            // change layers in sensors
            foreach (var sensor in sensors)
            {
                if (!sensor.UseRaycast)
                {
                    sensor.gameObject.layer = LayerMask.NameToLayer("rvSensor");
                    sensor.AdditionalRayCastLayers.Clear();
                }
                else
                {
                    sensor.gameObject.layer = LayerMask.NameToLayer("rvMU");
                    sensor.AdditionalRayCastLayers.Clear();
                    sensor.AdditionalRayCastLayers.Add("rvMUSensor");
                }
                EditorUtility.SetDirty(sensor);
            }
            
            // change layers in sinks
            foreach (var sink in sinks)
            {
                sink.gameObject.layer = LayerMask.NameToLayer("rvSensor");
                EditorUtility.SetDirty(sink);
            }
            
            // change layers in transportsurfaces
            foreach (var transportsurface in transportsurfaces)
            {
                transportsurface.gameObject.layer = LayerMask.NameToLayer("rvTransport");
                transportsurface.Layer = "rvTransport";
                EditorUtility.SetDirty(transportsurface);
            }
            
            // change layers in sources
            foreach (var source in sources)
            {
                source.gameObject.layer = LayerMask.NameToLayer("rvMU");
                if (source.GenerateOnLayer == "g4a SensorMU")
                    source.GenerateOnLayer = "rvMUSensor";
                if (source.GenerateOnLayer == "g4a MU")
                    source.GenerateOnLayer = "rvMU";
                EditorUtility.SetDirty(source);
            }
            
  
            
            
        }
    }
}

