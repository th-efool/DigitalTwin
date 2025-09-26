using System;
using NaughtyAttributes;
using UnityEngine;

namespace realvirtual
{
    //! ConveyorBelt provides advanced visual representation of industrial conveyor systems with realistic belt animation.
    //! This component renders a conveyor belt with cylindrical end caps and flat transport surfaces, automatically 
    //! synchronizing belt movement with the associated TransportSurface drive speed for accurate material flow visualization.
    //! Supports dynamic belt sizing, texture scrolling animation, and integration with realvirtual's transport physics system.
    //! Ideal for creating visually realistic conveyor systems in factory simulations and material handling applications.
    public class ConveyorBelt : MonoBehaviour
    {

        [Header("Dimensions")]
        public float length;
        public float width;
        public float height;
        public float scroll = 0;
        public float speed = 1;
        
        
        [Header("Meshes")]
        public GameObject topSurface;
        public GameObject bottomSurface;
        public GameObject leftCap;
        public GameObject rightCap;

        private MaterialPropertyBlock[] _materialPropertyBlocks;
        private TransportSurface _transportSurface;
        private bool _hasTransportSurface;
        private MeshRenderer[] _renderes;
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, length));
            Gizmos.DrawLine(new Vector3(0, 0, -length / 2 + height/2), new Vector3(0, 0, length / 2 - height/2));
        
        }

        private void OnValidate()
        {
            if(!Application.isPlaying)
                AdjustAll();
        }

        private void Awake()
        {
            FindTransportSurface();
            AdjustAll();
        }

        private void Update()
        {
            if(_hasTransportSurface)
            {
                speed = _transportSurface.Drive.IsSpeed/1000f;
            }
            
            Integrate(Time.deltaTime);
            
            AdjustScroll();
            
            UpdateMaterialPropertyBlocks();
        }

        void Integrate(float dt)
        {
            scroll += speed * dt;
        }
        
        private void FindTransportSurface()
        {
            _transportSurface = GetComponentInParent<TransportSurface>();
            _hasTransportSurface = _transportSurface != null;
        }
        
        private void UpdateMaterialPropertyBlocks()
        {
            for (int i = 0; i < 4; i++)
            {
                
                _renderes[i].SetPropertyBlock(_materialPropertyBlocks[i]);
            }
        }

        private void FindMaterialPropertyBlocks()
        {
            _renderes = new MeshRenderer[4];
            _renderes[0] = topSurface.GetComponent<MeshRenderer>();
            _renderes[1] = bottomSurface.GetComponent<MeshRenderer>();
            _renderes[2] = leftCap.GetComponent<MeshRenderer>();
            _renderes[3] = rightCap.GetComponent<MeshRenderer>();
            
            _materialPropertyBlocks = new MaterialPropertyBlock[4];
            _materialPropertyBlocks[0] = new MaterialPropertyBlock();
            _materialPropertyBlocks[1] = new MaterialPropertyBlock();
            _materialPropertyBlocks[2] = new MaterialPropertyBlock();
            _materialPropertyBlocks[3] = new MaterialPropertyBlock();
            
            _renderes[0].GetPropertyBlock(_materialPropertyBlocks[0]);
            _renderes[1].GetPropertyBlock(_materialPropertyBlocks[1]);
            _renderes[2].GetPropertyBlock(_materialPropertyBlocks[2]);
            _renderes[3].GetPropertyBlock(_materialPropertyBlocks[3]);
            
        }
        
        public void SetScroll(float x)
        {
            this.scroll = x;
            for(int i = 0; i < _materialPropertyBlocks.Length; i++)
            {
                MaterialPropertyBlock block = _materialPropertyBlocks[i];
                _materialPropertyBlocks[i].SetFloat("_Scroll", x);
            }
        }

        public void SetDimensions(float length, float width, float height)
        {
            this.length = length;
            this.width = width;
            this.height = height;
            AdjustAll();
        }
        
        void AdjustAll()
        {
            FindMaterialPropertyBlocks();
            AdjustTransforms();
            AdjustSurfaceTiling();
            AdjustCapTiling();
            AdjustScroll();
            UpdateMaterialPropertyBlocks();
        }

        void AdjustScroll()
        {
            SetScroll(scroll);
        }
    
        void AdjustTransforms()
        {
            this.height = Mathf.Max(0, height);
            this.width = width = Mathf.Max(0, width);
            this.length = Mathf.Max(0, length);
        
            if (topSurface != null)
            {
                topSurface.transform.localScale = new Vector3(width, length-height, 1);
                topSurface.transform.localPosition = new Vector3(0, height / 2, 0);
            }
            if (bottomSurface != null)
            {
                bottomSurface.transform.localScale = new Vector3(width, length-height, 1);
                bottomSurface.transform.localPosition = new Vector3(0, -height / 2, 0);
            }
            if (leftCap != null)
            {
                leftCap.transform.localScale = new Vector3(width, height, height);
                leftCap.transform.localPosition = new Vector3(0, 0, -length / 2 + height / 2);
            }
            if (rightCap != null)
            {
                rightCap.transform.localScale = new Vector3(width, height, height);
                rightCap.transform.localPosition = new Vector3(0, 0, length / 2 - height / 2);
            }
        }

        void AdjustSurfaceTiling()
        {
            float surfaceLength = GetSurfaceLength();
        
            if (topSurface != null)
            {
                MaterialPropertyBlock block = _materialPropertyBlocks[0];
                
                // Set the texture scale for the "_BaseMap" property
                Vector2 tiling = new Vector2(surfaceLength, width);
                block.SetVector("_Parameters", new Vector4(tiling.x, tiling.y, -surfaceLength, 0)); // Scale and Offset (x, y, z, w)
            
            }
        
            if (bottomSurface != null)
            {
                MaterialPropertyBlock block = _materialPropertyBlocks[1];

               

                // Set the texture scale for the "_BaseMap" property
                Vector2 tiling = new Vector2(length-height, width);
                float offset = -surfaceLength * 2 - CapLength();
            
                block.SetVector("_Parameters", new Vector4(tiling.x, tiling.y, offset, 0)); // Scale and Offset (x, y, z, w)

            }
        }
    
        float GetSurfaceLength()
        {
            return length - height;
        }
    
        void AdjustCapTiling()
        {
            float surfaceLength = GetSurfaceLength();
            float capLength = CapLength();
        
            if (leftCap != null)
            {
                MaterialPropertyBlock block = _materialPropertyBlocks[2];

                // Set the texture scale for the "_BaseMap" property
                Vector2 tiling = new Vector2(CapLength(), width);
                float offset = 0;
                block.SetVector("_Parameters", new Vector4(tiling.x, tiling.y, offset, 0)); // Scale and Offset (x, y, z, w)

            }
        
            if (rightCap != null)
            {
                MaterialPropertyBlock block = _materialPropertyBlocks[3];

                // Set the texture scale for the "_BaseMap" property
                Vector2 tiling = new Vector2(CapLength(), width);
                float offset = -surfaceLength - capLength;
                block.SetVector("_Parameters", new Vector4(tiling.x, tiling.y, offset, 0)); // Scale and Offset (x, y, z, w)

            }
        }
    
        float CircleCircumference(float radius)
        {
            return radius * 2 * Mathf.PI;
        }
    
        float CapLength()
        {
            float radius = height / 2;
            float circumference = CircleCircumference(radius);
            return circumference/2;
        }
    
     

    }
}