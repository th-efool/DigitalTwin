using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    public class OrthoViewController : realvirtualBehavior
    {
        [OnValueChanged("UpdateViews")] public Color BackgroundColor;
        [OnValueChanged("UpdateViews")] public float Distance = 1.0f;
        [OnValueChanged("UpdateViews")] public bool OrthoEnabled = true;
        [OnValueChanged("UpdateViews")] public float Size = 0.4f;
        [OnValueChanged("UpdateViews")] public float Angle;
        // Start is called before the first frame update
        public void UpdateViews()
        {
            if (Distance == 0)
                Distance = 1;
            if (Size == 0)
                Size = 0.3f;
            
            var x1 = 0.01f;
            var x3 = x1 + Size + 0.01f;
            this.transform.rotation = Quaternion.Euler(0, Angle, 0);

            var cameras = GetComponentsInChildren<Camera>();
            foreach (var camera in cameras)
            {
                camera.backgroundColor = BackgroundColor;
                camera.enabled = OrthoEnabled;
                camera.orthographicSize = Distance;
                camera.rect = new Rect(x1, x1, Size, Size);
                if (camera.name == "Top")
                    camera.rect = new Rect(x1, x3, Size, Size);
                if (camera.name == "Side")
                    camera.rect = new Rect(x3, x1, Size, Size);

            }
        }

        // Update is called once per frame
        void Start()
        {
           UpdateViews();
        }
}
}

