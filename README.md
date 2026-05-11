# XFEExtension.NetCore.AutoConfig

[![NuGet](https://img.shields.io/nuget/v/XFEExtension.NetCore.AutoConfig?label=NuGet&logo=NuGet)](https://www.nuget.org/packages/XFEExtension.NetCore.AutoConfig/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/XFEExtension.NetCore.AutoConfig?label=Downloads&logo=NuGet)](https://www.nuget.org/packages/XFEExtension.NetCore.AutoConfig/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download)

> 📖 English | [简体中文](https://github.com/XFEstudio/XFEExtension.NetCore.AutoConfig/blob/master/README.zh-CN.md)

## Description

XFEExtension.NetCore.AutoConfig is a .NET library powered by Roslyn incremental source generators. It automatically generates static properties, load/save methods, and persistence logic for any `partial` class that inherits from `XFEProfile`, eliminating the need to write boilerplate configuration code.

## Getting Started

### Installation

```shell
dotnet add package XFEExtension.NetCore.AutoConfig
```

### Basic Usage

Annotate fields with `[ProfileProperty]`. The source generator will create a corresponding static property that automatically saves whenever it is assigned:

```csharp
// Define a profile class
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;
}

// Use the profile
class Program
{
    static void Main(string[] args)
    {
        SystemProfile.Name = "Test"; // Automatically saved on assignment
        Console.WriteLine(SystemProfile.Name);
        Console.WriteLine(SystemProfile.Age); // Restored from disk on next run
    }
}
```

> **Note:** The `[AutoLoadProfile]` attribute instructs the framework to call `LoadProfile()` inside the static constructor, so the configuration is restored automatically when the program starts.

---

## Detailed Usage

### Changing the Storage Format

Set `DefaultProfileOperationMode` inside the instance constructor to switch the storage format. The file extension is updated automatically:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;

    public SystemProfile()
    {
        DefaultProfileOperationMode = ProfileOperationMode.Xml; // Switch to XML; extension becomes .xml
        // Available modes: XFEDictionary (default), Json, Xml, Custom
    }
}
```

### Custom Storage Path and File Extension

Use the generated static properties `ProfilePath` and `ProfileExtension` to control where the file is stored:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;

    public SystemProfile()
    {
        ProfilePath = $"MyPath/MySubPath/{nameof(SystemProfile)}"; // Path without extension
        ProfileExtension = ".ini";                                  // Custom file extension
    }
}
```

> `ProfilePath` and `ProfileExtension` are generated static properties and can also be set from outside the class:
> ```csharp
> SystemProfile.ProfilePath = "custom/path/SystemProfile";
> SystemProfile.ProfileExtension = ".cfg";
> ```

### Using the `[ProfilePath]` Attribute

```csharp
[AutoLoadProfile]
[ProfilePath("MyPath/MySubPath/SystemProfile")]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;
}
```

### Custom Load and Save Operations

Set `DefaultProfileOperationMode` to `Custom` and provide your own load/save delegates:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;

    public SystemProfile()
    {
        DefaultProfileOperationMode = ProfileOperationMode.Custom;
        ProfilePath = $"MyPath/MySubPath/{nameof(SystemProfile)}";
        ProfileExtension = ".ini";
        LoadOperation = MyCustomLoadProfileOperation;
        SaveOperation = MyCustomSaveProfileOperation;
    }

    // Custom load method
    public static XFEProfile? MyCustomLoadProfileOperation(
        XFEProfile profileInstance,
        string profileString,
        Dictionary<string, Type> propertyInfoDictionary,
        Dictionary<string, SetValueDelegate> propertySetFuncDictionary)
    {
        // Implement custom load logic here
        return null;
    }

    // Custom save method
    public static string MyCustomSaveProfileOperation(
        XFEProfile profileInstance,
        Dictionary<string, Type> propertyInfoDictionary,
        Dictionary<string, GetValueDelegate> propertyGetFuncDictionary)
    {
        // Implement custom save logic here
        return string.Empty;
    }
}
```

### Storing Collections with `ProfileList` and `ProfileDictionary`

`ProfileList<T>` and `ProfileDictionary<TKey, TValue>` automatically trigger a save whenever the collection is modified (add, remove, clear, etc.):

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    [ProfilePropertyAddGet("Current.nameList.CurrentProfile = Current")]
    [ProfilePropertyAddGet("return Current.nameList")]
    ProfileList<string> nameList = [];

    [ProfileProperty]
    [ProfilePropertyAddGet("Current.nameIdDictionary.CurrentProfile = Current")]
    [ProfilePropertyAddGet("return Current.nameIdDictionary")]
    ProfileDictionary<string, long> nameIdDictionary = [];
}

class Program
{
    static void Main(string[] args)
    {
        SystemProfile.NameList.Add("Alice");              // Auto-saved on add
        SystemProfile.NameList.AddRange(["Bob", "Carol"]); // Batch add
        SystemProfile.NameList.Remove("Bob");              // Auto-saved on remove
        SystemProfile.NameIdDictionary.Add("Alice", 100L); // Dictionary works the same way
    }
}
```

### Injecting Code into `get`/`set` Accessors

Use `[ProfilePropertyAddGet]` and `[ProfilePropertyAddSet]` to insert code snippets directly into the generated property accessors:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""Getting Name"")")]
    [ProfilePropertyAddGet("return Current.name")]
    [ProfilePropertyAddSet(@"Console.WriteLine(""Setting Name"")")]
    [ProfilePropertyAddSet("Current.name = value")]
    string name = string.Empty;

    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""Getting Age"")")]
    [ProfilePropertyAddGet("return Current._age")]
    [ProfilePropertyAddSet(@"Console.WriteLine(""Setting Age"")")]
    [ProfilePropertyAddSet("Current._age = value")]
    int _age;
}
```

> **Note:** When using `[ProfilePropertyAddGet]`, you must handle the full `return` statement yourself in the last `get` snippet.

### Partial Method Hooks

The generator creates `static partial void GetXxxProperty()` and `static partial void SetXxxProperty(ref T value)` for each property. Implement them in your own partial class to intercept reads and writes:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;

    static partial void GetNameProperty()
    {
        Console.WriteLine("Name was read");
    }

    static partial void SetNameProperty(ref string value)
    {
        Console.WriteLine($"Name changing: {Name} -> {value}");
    }

    static partial void GetAgeProperty()
    {
        Console.WriteLine("Age was read");
    }

    static partial void SetAgeProperty(ref int value)
    {
        value = 1999; // Modify the value before it is stored
        Console.WriteLine($"Age forced to 1999");
    }
}
```

### Default Field Values

Assign values directly at the field declaration site:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = "John Wick";

    [ProfileProperty]
    int _age = 59;
}
```

### XML Documentation Comments

XML doc comments placed on a field are automatically propagated to the generated static property:

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    /// <summary>
    /// The user's name. This comment is copied to the generated Name property.
    /// </summary>
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;
}
```

### Manual Load / Save / Delete / Export / Import

The following static methods are generated for every profile class:

```csharp
SystemProfile.LoadProfile();                         // Load from file
SystemProfile.SaveProfile();                         // Save to file
SystemProfile.DeleteProfile();                       // Delete the config file
string exported = SystemProfile.ExportProfile();     // Export config as a string
SystemProfile.ImportProfile(exported);               // Import config from a string
```

---

## API Reference

### Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[ProfileProperty]` | Field | Marks the field for code generation. Optionally specify a property name: `[ProfileProperty("CustomName")]` |
| `[ProfilePropertyAddGet(code)]` | Field | Appends a code line to the generated `get` accessor. Supports multiple attributes. |
| `[ProfilePropertyAddSet(code)]` | Field | Appends a code line to the generated `set` accessor. Supports multiple attributes. |
| `[AutoLoadProfile]` | Class | Calls `LoadProfile()` automatically in the static constructor. |
| `[ProfilePath(path)]` | Class | Sets the storage path for the config file. |

### Storage Modes (`ProfileOperationMode`)

| Value | Extension | Description |
|-------|-----------|-------------|
| `XFEDictionary` (default) | `.xpf` | XFE dictionary format |
| `Json` | `.json` | JSON serialization |
| `Xml` | `.xml` | XML serialization |
| `Custom` | custom | User-provided load/save delegates |

### Auto-Generated Static Members

For every `partial` class that inherits `XFEProfile` and uses `[ProfileProperty]`, the source generator produces:

| Member | Kind | Description |
|--------|------|-------------|
| `Current` | `static T` | The singleton profile instance |
| `ProfilePath` | `static string` | Storage path (without extension) |
| `ProfileExtension` | `static string` | File extension (auto-detected when empty) |
| `LoadProfile()` | `static void` | Loads config from file |
| `SaveProfile()` | `static void` | Saves config to file |
| `DeleteProfile()` | `static void` | Deletes the config file |
| `ExportProfile()` | `static string` | Exports config as a string |
| `ImportProfile(string)` | `static void` | Imports config from a string |
| `XxxProperty` (per field) | `static T` | Auto-generated static property; saves on set |
| `InstanceXxx` (per field) | `T` (instance) | Corresponding instance property |
| `GetXxxProperty()` | `static partial void` | Invoked when the property is read |
| `SetXxxProperty(ref T)` | `static partial void` | Invoked when the property is written |

### `XFEProfile` Base Class Members

| Member | Kind | Description |
|--------|------|-------------|
| `DefaultProfileOperationMode` | `ProfileOperationMode` | Load/save mode |
| `LoadOperation` | `ProfileLoadOperation` | Custom load delegate |
| `SaveOperation` | `ProfileSaveOperation` | Custom save delegate |
| `ProfilesDefaultPath` | `static string` | Default root directory for all profile files |

---

## License

This project is licensed under the [MIT License](LICENSE.txt).
