# StaticTools Documentation

This document provides comprehensive documentation for all static utility classes in the realvirtual StaticTools directory. These classes provide essential framework functionality accessible throughout the codebase.

## Overview

The StaticTools directory contains static utility classes that form the backbone of realvirtual's utility systems. These classes are designed to be easily accessible while maintaining clear separation of concerns.

## Classes

### ConnectionState

**Purpose**: Determines if components will be active based on their ActiveOnly settings by checking the realvirtualController connection state and component enabled states.

**Location**: `Assets/realvirtual/private/StaticTools/ConnectionState.cs`

#### Methods

##### IsActive
```csharp
public static bool IsActive(realvirtualBehavior behavior, bool assumeConnected = true)
```
Checks if a realvirtualBehavior component will be active based on its ActiveOnly setting.

**Parameters:**
- `behavior`: The realvirtualBehavior component to check
- `assumeConnected`: Default connection state to assume if controller is not available (default: true)

**Returns:** True if the component will be active, false otherwise

**Logic:**
- `Always`: Always returns true
- `Never`: Always returns false
- `DontChange`: Returns the component's current enabled state
- `Connected`: Returns true if controller is connected
- `Disconnected`: Returns true if controller is disconnected

### Groups

**Purpose**: Provides centralized functionality for working with Group components in realvirtual. Offers methods for querying groups, calculating bounds, managing colliders, and performing mesh operations on grouped objects.

**Location**: `Assets/realvirtual/private/StaticTools/Groups.cs`

#### Query Methods

##### GetAllGroupNames
```csharp
public static List<string> GetAllGroupNames()
```
Gets all unique group names in the scene.

**Returns:** List of all unique group names found in the scene

##### GetGameObjectsWithGroup
```csharp
public static List<GameObject> GetGameObjectsWithGroup(string groupName)
```
Gets all GameObjects that have a Group component with the specified group name.

**Parameters:**
- `groupName`: The name of the group to search for

**Returns:** List of GameObjects with the specified group name

##### GetGameObjectsWithGroupIncludingChildren
```csharp
public static List<GameObject> GetGameObjectsWithGroupIncludingChildren(string groupName)
```
Gets all GameObjects with the specified group name including all their child objects.

**Parameters:**
- `groupName`: The name of the group to search for

**Returns:** List of GameObjects and their children with the specified group name

##### GetAllGroupComponents
```csharp
public static List<Group> GetAllGroupComponents(string groupName)
```
Gets all Group components with the specified group name.

#### Bounds Calculation Methods

##### GetGroupBounds
```csharp
public static Bounds GetGroupBounds(string groupName)
```
Calculates the combined bounds of all renderers in objects with the specified group.

##### GetGroupBoundsIncludingChildren
```csharp
public static Bounds GetGroupBoundsIncludingChildren(string groupName)
```
Calculates bounds including all child renderers of grouped objects.

##### GetGroupBoundsWithPadding
```csharp
public static Bounds GetGroupBoundsWithPadding(string groupName, float padding)
```
Calculates bounds with specified padding added to all sides.

#### Collider Management Methods

##### AddCollidersToGroup
```csharp
public static int AddCollidersToGroup(string groupName, bool includeChildren = true)
```
Adds MeshColliders to all MeshRenderers in the group that don't already have colliders.

##### RemoveCollidersFromGroup
```csharp
public static int RemoveCollidersFromGroup(string groupName, bool includeChildren = true)
```
Removes all colliders from objects in the specified group.

##### SetGroupCollidersConvex
```csharp
public static int SetGroupCollidersConvex(string groupName, bool convex, bool includeChildren = true)
```
Sets the convex property of all MeshColliders in the group.

#### Mesh Operations

##### CombineGroupMeshes
```csharp
public static GameObject CombineGroupMeshes(string groupName, string combinedObjectName = null)
```
Combines all meshes in a group into a single mesh for performance optimization.

##### GetGroupMeshVertexCount
```csharp
public static int GetGroupMeshVertexCount(string groupName, bool includeChildren = true)
```
Gets the total vertex count of all meshes in the group.

#### Visibility and State Methods

##### SetGroupActive
```csharp
public static void SetGroupActive(string groupName, bool active, bool includeChildren = true)
```
Sets the active state of all GameObjects in the group.

##### SetGroupLayer
```csharp
public static void SetGroupLayer(string groupName, int layer, bool includeChildren = true)
```
Sets the layer of all GameObjects in the group.

##### SetGroupTag
```csharp
public static void SetGroupTag(string groupName, string tag, bool includeChildren = true)
```
Sets the tag of all GameObjects in the group.

### Logger

**Purpose**: Static logging class for realvirtual framework with branded output formatting. Provides consistent logging with pink icon indicators, white text, and automatic hierarchy path inclusion.

**Location**: `Assets/realvirtual/private/StaticTools/Logger.cs`

#### Features

- **Branded Output**: Pink diamond/warning/error icons with white text and "realvirtual:" prefix
- **Automatic Path Inclusion**: When a context object is provided, the full hierarchy path is automatically appended
- **Stack Trace Control**: Each method allows optional stack trace display
- **Rich Text Support**: Messages support Unity's rich text color tags
- **Clean Formatting**: Pink icons for visual identification, all text in white for readability

#### Methods

##### Message
```csharp
public static void Message(string message, Object context = null)
```
Logs a message without stack trace.

**Parameters:**
- `message`: The message to log (supports Unity rich text color tags)
- `context`: Optional context object to link in console and include hierarchy path

##### Log
```csharp
public static void Log(string message, Object context = null, bool showStackTrace = true)
```
Logs a message with optional stack trace (excluding Logger from stack).

**Parameters:**
- `message`: The message to log
- `context`: Optional context object
- `showStackTrace`: Whether to show stack trace (default: true)

##### Warning
```csharp
public static void Warning(string message, Object context = null, bool showStackTrace = true)
```
Logs a warning with orange warning icon and optional stack trace.

**Parameters:**
- `message`: The warning message
- `context`: Optional context object
- `showStackTrace`: Whether to show stack trace (default: true)

##### Error
```csharp
public static void Error(string message, Object context = null, bool showStackTrace = true)
```
Logs an error with red error icon and optional stack trace.

**Parameters:**
- `message`: The error message
- `context`: Optional context object
- `showStackTrace`: Whether to show stack trace (default: true)

#### Usage Examples

```csharp
// Simple message without stack trace
Logger.Message("Component initialized");

// Message with color formatting
Logger.Message("<color=green>Simulation started successfully</color>");

// Warning with context object - automatically includes hierarchy path
Logger.Warning("Connection timeout", gameObject);

// Error without stack trace
Logger.Error("Critical failure in system", null, false);

// With context object, output includes path: "[Parent/Child/Object]"
Logger.Log("Processing component", myComponent);
```

### SceneTools

**Purpose**: Static utility class for scene cleanup and management operations. Provides functionality to unlock hidden GameObjects and clean up scene artifacts.

**Location**: `Assets/realvirtual/private/StaticTools/SceneTools.cs`

#### Methods

##### CleanupScene (Editor Only)
```csharp
#if UNITY_EDITOR
public static void CleanupScene(bool showDialog = false)
#endif
```
Cleans up the scene by unlocking hidden GameObjects and removing highlight system artifacts. This method is only available in the Unity Editor.

**Parameters:**
- `showDialog`: Whether to show a summary dialog after cleanup (default: false)

**Features:**
- Removes HideFlags that prevent object selection
- Cleans up highlight system artifacts if available
- Logs detailed information about unlocked objects
- Marks scene as dirty for proper saving

**Menu Item:** Available via "realvirtual/Settings/Cleanup Scene"

##### GetObjectPath
```csharp
public static string GetObjectPath(GameObject obj)
```
Gets the full hierarchy path of a GameObject.

**Parameters:**
- `obj`: The GameObject to get the path for

**Returns:** Full hierarchy path as string (e.g., "Parent/Child/Object")

**Note:** This method is available at runtime and is used by the Logger for automatic path inclusion.

#### Usage Examples

```csharp
// Get full path of a GameObject
string path = SceneTools.GetObjectPath(myGameObject);
// Returns: "Canvas/Panel/Button"

#if UNITY_EDITOR
// Clean up scene without dialog
SceneTools.CleanupScene(false);

// Clean up scene with summary dialog
SceneTools.CleanupScene(true);
#endif
```

## Design Principles

1. **Static Access**: All utilities are static for easy access throughout the codebase
2. **No State**: Static classes maintain no state, ensuring thread safety
3. **Clear Purpose**: Each class has a single, well-defined responsibility
4. **Performance**: Methods are optimized for performance, especially for operations on groups
5. **Null Safety**: All methods handle null inputs gracefully
6. **Unity Integration**: Designed to work seamlessly with Unity's component system

## Best Practices

1. **Groups**: Use Groups utility for batch operations on grouped objects to improve performance
2. **ConnectionState**: Always use ConnectionState.IsActive() to check component activation status
3. **Logger**: Use Logger instead of Debug.Log for consistent framework messaging
4. **Error Handling**: All methods handle edge cases and invalid inputs gracefully

## Performance Considerations

- **Groups.CombineGroupMeshes**: Reduces draw calls but increases memory usage
- **Groups query methods**: Use FindObjectsByType with FindObjectsSortMode.None for better performance
- **Logger**: Message() method has no stack trace overhead compared to Log()

## Version History

- Initial implementation: realvirtual 6.0
- Groups utility added: realvirtual 6.1
- Logger enhanced with color support: realvirtual 6.2
- SceneTools moved from Editor to StaticTools: realvirtual 6.3
- Logger enhanced with automatic hierarchy path inclusion: realvirtual 6.3