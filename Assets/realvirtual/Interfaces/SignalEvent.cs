namespace realvirtual
{
    //! Unity event for signal-based communication in the realvirtual framework.
    //! This event type allows components to subscribe to and respond to signal changes,
    //! providing a flexible way to connect different parts of the automation system.
    [System.Serializable]
    public class SignalEvent : UnityEngine.Events.UnityEvent<Signal> {} 
}
