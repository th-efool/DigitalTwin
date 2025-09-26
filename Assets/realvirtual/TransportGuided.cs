using UnityEngine;
using NaughtyAttributes;

namespace realvirtual
{
    //! TransportGuided creates guided transport paths for MUs with automatic path following.
    //! Supports both linear and circular (curved) transport configurations with automatic collider generation.
    //! Integrates with Drive components to create conveyor systems where MUs follow predefined paths without physics simulation.
    [RequireComponent(typeof(Drive))]
    public class TransportGuided : realvirtualBehavior, IGuidedSurface
    {
        [Tooltip("Enables circular (curved) transport path instead of linear transport")]
        [OnValueChanged("Init")] public bool Circular = false;

        [Tooltip("Length of the linear transport surface in meters")]
        [HideIf("Circular")] [OnValueChanged("Init")]
        public float Length = 1.0f;
        
        [Tooltip("Radius of the circular transport path in meters")]
        [ShowIf("Circular")] [OnValueChanged("Init")]
        public float Radius = 1.0f;

        [Tooltip("Arc angle of the circular transport path in degrees")]
        [ShowIf("Circular")] [OnValueChanged("Init")]
        public float Angle = 90;

        [Tooltip("Additional collider border in Z direction in meters")]
        [ShowIf("Circular")] [OnValueChanged("Init")]
        public float ColliderBorderZ = 0.2f;
        [Tooltip("Additional collider border in X direction in meters")]
        [ShowIf("Circular")] [OnValueChanged("Init")]
        public float ColliderBorderX = 0.2f;
        
        [Tooltip("Height of the transport surface collider in meters")]
        [OnValueChanged("Init")] public float Height = 0.5f;
        [Tooltip("Width of the linear transport surface collider in meters")]
        [OnValueChanged("Init")] [HideIf("Circular")]public float Width = 0.5f;

        [Tooltip("Automatically sets the layer to 'rvSimStatic' for proper collision detection")]
        [OnValueChanged("Init")] public bool UseStandardLayer = true;
        [Tooltip("Distance offset of the collider below the surface in meters")]
        [OnValueChanged("Init")] public float DistanceCollider = 0.005f;
        [Tooltip("Shows visual representation of the transport path in Scene view")]
        [OnValueChanged("Init")] public bool ShowGizmos = true;
        private Vector3[] pathpoints;
        private int pathpointsnumber;
        [HideInInspector] [SerializeField] private Drive drive;
        [HideInInspector] [SerializeField] BoxCollider boxcollider;
        private IGuide guide;

        public void Init()
        {
            if (UseStandardLayer)
                this.gameObject.layer = LayerMask.NameToLayer("rvSimStatic");
            drive = GetComponent<Drive>();
            if (drive == null)
            {
                Logger.Error("TransportGuided requires a Drive component on the same GameObject. Please add a Drive component.", this);
                return;
            }
            drive.Direction = DIRECTION.Virtual;
            if (!Circular)
            {
                DestroyImmediate(GetComponent<GuideCircle>());
                guide = (Global.AddComponentIfNotExisting<GuideLine>(this.gameObject));
                // set length in GuideLine
                var guideLine = (GuideLine) guide;
                guideLine.Length = Length;
               
                boxcollider = Global.AddComponentIfNotExisting<BoxCollider>(this.gameObject);
                boxcollider.size = new Vector3(Length, Height, Width);
                boxcollider.center = new Vector3(Length / 2, -Height / 2 - DistanceCollider, 0);
                guideLine.ShowGizmos = ShowGizmos;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(guideLine);
#endif
                
            }
            else
            {
                DestroyImmediate(GetComponent<GuideLine>());
                guide = (Global.AddComponentIfNotExisting<GuideCircle>(this.gameObject));
                // set length in GuideLine
                var guidecircle = (GuideCircle) guide;
                guidecircle.Radius = Radius;
                guidecircle.Angle = Angle;
                guidecircle.CreatePath();
                guidecircle.ShowGizmos = ShowGizmos;
                boxcollider = Global.AddComponentIfNotExisting<BoxCollider>(this.gameObject);
                SetColliderSizeArc();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(guidecircle);
#endif
            }
        }

        private void SetColliderSizeArc()
        {
            var guidecircle = (GuideCircle) guide;
            if (Mathf.Approximately(Angle, 0)) return ;
            var sizex = guidecircle.maxx +ColliderBorderX;
            var sizez = guidecircle.maxy + ColliderBorderZ;
            var borz = ColliderBorderZ;
            if (Angle< 0)
            {
                sizez = -sizez;
                borz = -borz;
                
            }
            boxcollider.size = new Vector3(sizex, Height, sizez);
            boxcollider.center = new Vector3(sizex*0.5f, -Height * 0.5f, -sizez*0.5f+borz);
        }

        public void OnDrawGizmos()
        {
            if (!ShowGizmos) return;
            Gizmos.color = Color.green;
            // Draw a Gimzo for BoxCollider
            if (boxcollider != null)
            {
                var center = boxcollider.center;
                // to global coordinates
                center = this.transform.TransformPoint(center);
                var size = boxcollider.size;
                // to global size
                size = this.transform.TransformVector(size);
                Gizmos.DrawWireCube(center, size);
            }
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot;
            dir = Quaternion.Euler(angles) * dir;
            point = dir + pivot;
            return point;
        }


        private void Reset()
        {
            Init();
        }

        private new void Awake()
        {
            base.Awake();
            Init();
        }


        public bool IsSurfaceGuided()
        {
            return this.enabled;
        }

        public Vector3 GetClosestDirection(Vector3 position)
        {
            return guide.GetClosestDirection(position);
        }

        public Vector3 GetClosestPoint(Vector3 position)
        {
            return guide.GetClosestPoint(position);
        }

        public Drive GetDrive()
        {
            return drive;
        }
        
    }
}