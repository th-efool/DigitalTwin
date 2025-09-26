// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
namespace realvirtual
{
    //! Interface to define the needed methods for snap points
    public delegate void OnSnappedEvent(SnapPoint own, SnapPoint other);
    
    public delegate void OnUnsnappedEvent (SnapPoint own, SnapPoint other);
    
    public interface ISnapable
    {
        OnSnappedEvent OnSnapped { get; set; }
        
        public void CheckSnap();
        
        public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject, bool ismoved);

        public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable Mateobj, bool ismoved);

        public void Modify();

        public void AttachTo(SnapPoint attachto);
    }
}