using UnityEngine;

namespace realvirtual
{
    public interface IInspector
    {
        public void OnInspectValueChanged();
        public bool OnObjectDrop(Object reference);

        public void OnInspectedToggleChanged(bool arg0);
    }
}