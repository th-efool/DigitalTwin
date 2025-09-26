// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using UnityEngine;

namespace realvirtual
{
#pragma warning disable CS3003, CS3009
    public class SceneButtonMoveable : MonoBehaviour
    {
        public Vector3 axis;
        public float moveSpeed = 30;
        public float hoverOffset = 0.1f;
        public float activeOffset = 0.05f;
        public bool mirrorHoverOffset = false;
        public bool angularMovement = false;

        public Material baseMaterial;
        public Material activeMaterial;
        public new MeshRenderer renderer;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        public float currentOffset;

        private float currentOffsetTarget;

        private bool active;
        private bool hovered;

        private void Awake()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            LightOff();
        }


        public void Hover()
        {
            if (active)
            {
                currentOffsetTarget = activeOffset;
            }
            else
            {
                currentOffsetTarget = 0;
            }

            currentOffsetTarget += hoverOffset * GetMirrorMultiplier();
        }

        float GetMirrorMultiplier()
        {
            if (mirrorHoverOffset)
            {
                if (active)
                {
                    return -1;
                }
            }

            return 1;
        }

        public void Unhover()
        {
            Release();
        }

        public void LightOn()
        {
            renderer.sharedMaterial = activeMaterial;
        }

        public void LightOff()
        {
            renderer.sharedMaterial = baseMaterial;
        }

        public void Click()
        {
            active = !active;
        }


        public void Release()
        {
            if (active)
            {
                currentOffsetTarget = activeOffset;
            }
            else
            {
                currentOffsetTarget = 0;
            }
        }

        private void Update()
        {
            float t = Mathf.Min(1, Time.deltaTime * moveSpeed);
            currentOffset = Mathf.Lerp(currentOffset, currentOffsetTarget, t);
            if (angularMovement)
            {
                transform.localRotation = initialRotation * Quaternion.AngleAxis(currentOffset, axis);
            }
            else
            {
                transform.localPosition = initialPosition + axis * currentOffset;
            }
        }
    }
}
#pragma warning restore CS3003, CS3009