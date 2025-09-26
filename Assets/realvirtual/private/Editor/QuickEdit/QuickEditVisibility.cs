// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

#if UNITY_2021_2_OR_NEWER
using System;

namespace realvirtual
{
    //! Visibility rule definition for QuickEdit UI elements
    internal class VisibilityRule
    {
        public Func<QuickEditContext, bool> Condition { get; set; }
        public string Description { get; set; }
        
        public bool IsVisible(QuickEditContext context) => Condition(context);
    }
    
    //! Static visibility rules for QuickEdit UI
    internal static class QuickEditVisibility
    {
        public static readonly VisibilityRule RotationButtons = new()
        {
            Condition = ctx => ctx.HasSelection && 
                              ctx.IsSingleSelection && 
                              !ctx.HasLogicStep && 
                              !ctx.IsUnderInterface,
            Description = "Rotation buttons hidden for LogicStep components or objects under interfaces"
        };
        
        public static readonly VisibilityRule ComponentCreationButtons = new()
        {
            Condition = ctx => ctx.HasSelection &&
                              !ctx.HasSignal && 
                              !ctx.HasLogicStep && 
                              !ctx.IsUnderInterface,
            Description = "Component creation disabled for signals, LogicSteps, or objects under interfaces"
        };
        
        public static readonly VisibilityRule DriveButtons = new()
        {
            Condition = ctx => ctx.HasSelection &&
                              !ctx.HasSignal && 
                              !ctx.HasLogicStep && 
                              !ctx.IsUnderInterface &&
                              !ctx.HasDrive,
            Description = "Drive button hidden when object already has a Drive component"
        };
        
        public static readonly VisibilityRule KinematicButtons = new()
        {
            Condition = ctx => ctx.HasSelection &&
                              !ctx.HasSignal && 
                              !ctx.HasLogicStep && 
                              !ctx.IsUnderInterface &&
                              !ctx.HasKinematic,
            Description = "Kinematic button hidden when object already has a Kinematic component"
        };
        
        public static readonly VisibilityRule TransportSurfaceButtons = new()
        {
            Condition = ctx => ctx.HasSelection &&
                              !ctx.HasSignal && 
                              !ctx.HasLogicStep && 
                              !ctx.IsUnderInterface &&
                              !ctx.IsUnderTransportSurface,
            Description = "Transport surface dependent buttons hidden when under transport surface"
        };
        
        public static readonly VisibilityRule DriveBehaviors = new()
        {
            Condition = ctx => ctx.HasSelection &&
                              ctx.HasDrive && 
                              !ctx.IsUnderInterface,
            Description = "Drive behaviors only shown when Drive component exists and not under interface"
        };
        
        public static readonly VisibilityRule SignalButtons = new()
        {
            Condition = ctx => ctx.HasSelection &&
                              !ctx.HasDrive && 
                              !ctx.HasKinematic && 
                              !ctx.HasLogicStep,
            Description = "Signal buttons hidden when object has Drive, Kinematic, or LogicStep components"
        };
        
        public static readonly VisibilityRule TransformButtons = new()
        {
            Condition = ctx => ctx.HasSelection && !ctx.HasSignal,
            Description = "Transform buttons hidden for signal objects"
        };
    }
}
#endif