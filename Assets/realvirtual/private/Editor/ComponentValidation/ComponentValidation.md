# Simplified Component Validation System

A lightweight validation system that checks components when they're added to GameObjects and before entering play mode.

## Features

- Generic type-safe validation rules
- Automatic context detection (component added vs pre-play)
- No interfaces, just simple base classes
- Skip validation with "NoValidation" tag

## Built-in Rules

1. **Single Drive** (`ComponentAddedRule<Drive>`) - Prevents multiple Drive components
2. **Component Compatibility** (`ValidationRule<Component>`) - Removes incompatible colliders
3. **Behavior Jogging** (`PrePlayRule<BehaviorInterface>`) - Disables jogging when BehaviorInterface controls drive
4. **Multiple BehaviorInterfaces** (`PrePlayRule<BehaviorInterface>`) - Ensures only one active BehaviorInterface per GameObject
5. **TransportSurface Drive Hierarchy** (`PrePlayRule<TransportSurface>`) - Ensures TransportSurface has exactly one Drive component in hierarchy (warns if none or more than one)

## Creating Custom Rules

### Rule Types

1. **ValidationRule<T>** - Runs in all contexts
2. **ComponentAddedRule<T>** - Only when components are added
3. **PrePlayRule<T>** - Only before entering play mode

### Examples

```csharp
// Rule that runs in all contexts
public class MyRule : ValidationRule<MyComponent>
{
    public override string RuleName => "My Rule";
    
    public override bool Validate(MyComponent component)
    {
        if (/* invalid condition */)
        {
            LogWarning("Description of issue", component);
            return false;
        }
        return true;
    }
}

// Rule that only runs when component is added
public class MyAddedRule : ComponentAddedRule<Drive>
{
    public override string RuleName => "Drive Setup";
    
    public override bool Validate(Drive drive)
    {
        // Can modify/remove components
        if (drive.Speed > 1000)
        {
            RemoveComponent(drive);
            return false;
        }
        return true;
    }
}

// Rule that only runs before play mode
public class MyPlayRule : PrePlayRule<Sensor>
{
    public override string RuleName => "Sensor Check";
    
    public override bool Validate(Sensor sensor)
    {
        // Check configuration before play
        if (sensor.SignalFloat == null)
        {
            LogWarning("No signal assigned", sensor);
            return false;
        }
        return true;
    }
}
```

### Registration

**No registration needed!** All rules are automatically discovered and registered when Unity starts.

Simply create your rule class and it will be active immediately:
- Inherit from `ValidationRule<T>`, `ComponentAddedRule<T>`, or `PrePlayRule<T>`
- The system automatically finds and registers all rules
- To disable a rule, comment it out or delete it

## Usage

### Skip Validation
Add "NoValidation" tag to any GameObject to skip all validation.

### Pre-Play Validation
Pre-play validation runs automatically before entering play mode. To disable it programmatically:
```csharp
PlayModeValidation.ValidationEnabled = false;
```

### Helper Methods

- `LogWarning(message, component)` - Log with consistent format
- `RemoveComponent(component)` - Remove with undo support
- `SetValue(component, propertyName, value)` - Set property with undo support

## Example

See `ExampleCustomRule.cs` for complete examples of all three rule types.