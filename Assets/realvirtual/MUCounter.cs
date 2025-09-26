
using UnityEngine;
using UnityEngine.UI;

namespace realvirtual
{
    //! MUCounter displays the current count of MUs in the scene as UI text.
    //! Provides real-time monitoring of the total number of MUs for production tracking and debugging.
    //! Updates periodically for performance optimization while maintaining accurate counts.
    public class MUCounter : MonoBehaviour
    {
        // Start is called before the fi    TextMesh textMesh;e update
        [Tooltip("Text to display before the MU count")]
        public string CounterText;
        [Tooltip("Update the counter every X frames for performance")]
        public int CountAllxUpdates = 20;
        
        private Text text;
        void Start()
        {
            text = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            // only count all x updates
            if (Time.frameCount % CountAllxUpdates != 0) return;
            // get number of MUs in scene
            var mus = FindObjectsByType<MU>(FindObjectsSortMode.None);
            text.text = CounterText+mus.Length.ToString();
        }
    }
}


