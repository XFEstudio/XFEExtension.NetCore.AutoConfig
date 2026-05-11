# XFEExtension.NetCore.AutoConfig

[![NuGet](https://img.shields.io/nuget/v/XFEExtension.NetCore.AutoConfig?label=NuGet&logo=NuGet)](https://www.nuget.org/packages/XFEExtension.NetCore.AutoConfig/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/XFEExtension.NetCore.AutoConfig?label=Downloads&logo=NuGet)](https://www.nuget.org/packages/XFEExtension.NetCore.AutoConfig/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download)

> 📖 [English](https://github.com/XFEstudio/XFEExtension.NetCore.AutoConfig/blob/master/README.zh-CN.md) | 简体中文

## 描述

XFEExtension.NetCore.AutoConfig 是一个基于 Roslyn 增量源生成器的 .NET 库，可以自动为继承自 `XFEProfile` 的配置文件类生成属性、加载/保存方法，实现配置文件的自动持久化存储。

## 快速开始

### 安装

```shell
dotnet add package XFEExtension.NetCore.AutoConfig
```

### 基础用法

为字段添加 `[ProfileProperty]` 特性，框架将自动生成对应的静态属性，并在赋值时自动保存配置：

```csharp
// 创建配置文件类
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;
}

// 使用配置文件
class Program
{
    static void Main(string[] args)
    {
        SystemProfile.Name = "Test"; // 赋值时自动保存
        Console.WriteLine(SystemProfile.Name);
        Console.WriteLine(SystemProfile.Age); // 下次启动自动读取上次保存的值
    }
}
```

> **说明：** `[AutoLoadProfile]` 特性会让框架在静态构造函数中自动调用 `LoadProfile()`，程序启动时无需手动加载。

---

## 详细用法

### 修改存储格式

通过在实例构造函数中设置 `DefaultProfileOperationMode` 来更改存储格式，文件扩展名会自动更改：

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
        DefaultProfileOperationMode = ProfileOperationMode.Xml; // 改用 XML 格式，扩展名自动变为 .xml
        // 可选值：ProfileOperationMode.XFEDictionary（默认）、Json、Xml、Custom
    }
}
```

### 自定义存储路径和文件扩展名

通过静态属性 `ProfilePath` 和 `ProfileExtension` 自定义存储位置：

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
        ProfilePath = $"MyPath/MySubPath/{nameof(SystemProfile)}"; // 自定义路径（不含扩展名）
        ProfileExtension = ".ini";                                  // 自定义文件扩展名
    }
}
```

> `ProfilePath` 和 `ProfileExtension` 均为框架自动生成的静态属性，也可在类外部直接赋值：
> ```csharp
> SystemProfile.ProfilePath = "custom/path/SystemProfile";
> SystemProfile.ProfileExtension = ".cfg";
> ```

### 使用 `[ProfilePath]` 特性指定存储路径

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

### 自定义存储方法

将 `DefaultProfileOperationMode` 设为 `Custom`，并自行提供加载和保存方法：

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

    // 自定义加载方法
    public static XFEProfile? MyCustomLoadProfileOperation(XFEProfile profileInstance, string profileString, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, SetValueDelegate> propertySetFuncDictionary)
    {
        // 在此实现自定义加载逻辑
        return null;
    }

    // 自定义保存方法
    public static string MyCustomSaveProfileOperation(XFEProfile profileInstance, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, GetValueDelegate> propertyGetFuncDictionary)
    {
        // 在此实现自定义保存逻辑
        return string.Empty;
    }
}
```

### 使用 `ProfileList` 和 `ProfileDictionary` 存储集合

`ProfileList<T>` 和 `ProfileDictionary<TKey, TValue>` 在集合发生变更（添加、删除等操作）时会自动触发保存：

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
        SystemProfile.NameList.Add("张三");               // 添加时自动保存
        SystemProfile.NameList.AddRange(["李四", "王五"]); // 批量添加
        SystemProfile.NameList.Remove("李四");            // 删除时也自动保存
        SystemProfile.NameIdDictionary.Add("张三", 100L); // 字典同理
    }
}
```

### 在 `get`/`set` 中插入自定义代码

使用 `[ProfilePropertyAddGet]` 和 `[ProfilePropertyAddSet]` 在生成的属性访问器中插入代码片段：

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""获取了 Name"")")]
    [ProfilePropertyAddGet("return Current.name")]
    [ProfilePropertyAddSet(@"Console.WriteLine(""设置了 Name"")")]
    [ProfilePropertyAddSet("Current.name = value")]
    string name = string.Empty;

    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""获取了 Age"")")]
    [ProfilePropertyAddGet("return Current._age")]
    [ProfilePropertyAddSet(@"Console.WriteLine(""设置了 Age"")")]
    [ProfilePropertyAddSet("Current._age = value")]
    int _age;
}
```

> **注意：** 使用 `[ProfilePropertyAddGet]` / `[ProfilePropertyAddSet]` 时，需要自行完整处理返回值/赋值逻辑，最后一条 get 语句需包含 `return`。

### 使用部分方法钩子

框架为每个属性自动生成 `static partial void GetXxxProperty()` 和 `static partial void SetXxxProperty(ref T value)` 分部方法，可在用户代码中实现：

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
        Console.WriteLine("获取了 Name");
    }

    static partial void SetNameProperty(ref string value)
    {
        Console.WriteLine($"设置了 Name：从 {Name} 变为 {value}");
    }

    static partial void GetAgeProperty()
    {
        Console.WriteLine("获取了 Age");
    }

    static partial void SetAgeProperty(ref int value)
    {
        value = 1999; // 可直接修改即将写入的值
        Console.WriteLine($"设置了 Age：从 {Age} 变为 1999");
    }
}
```

### 设置属性初始值

在字段声明处直接赋值即可：

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

### 为字段添加 XML 文档注释

字段上的 `<summary>` 注释会被自动复制到生成的静态属性上：

```csharp
[AutoLoadProfile]
partial class SystemProfile : XFEProfile
{
    /// <summary>
    /// 用户名称（此注释会自动同步至生成的 Name 属性）
    /// </summary>
    [ProfileProperty]
    string name = string.Empty;

    [ProfileProperty]
    int _age;
}
```

### 手动调用加载/保存/删除/导入/导出

框架为每个配置文件类自动生成以下静态方法：

```csharp
SystemProfile.LoadProfile();                         // 从文件加载配置
SystemProfile.SaveProfile();                         // 将配置保存到文件
SystemProfile.DeleteProfile();                       // 删除配置文件
string exported = SystemProfile.ExportProfile();     // 将当前配置导出为字符串
SystemProfile.ImportProfile(exported);               // 从字符串导入配置
```

---

## API 参考

### 特性（Attributes）

| 特性 | 应用目标 | 说明 |
|------|----------|------|
| `[ProfileProperty]` | 字段 | 标记该字段参与自动生成，可指定属性名 `[ProfileProperty("CustomName")]` |
| `[ProfilePropertyAddGet(code)]` | 字段 | 在生成的 `get` 访问器中追加代码行，支持多个 |
| `[ProfilePropertyAddSet(code)]` | 字段 | 在生成的 `set` 访问器中追加代码行，支持多个 |
| `[AutoLoadProfile]` | 类 | 在静态构造函数中自动调用 `LoadProfile()` |
| `[ProfilePath(path)]` | 类 | 指定配置文件存储路径 |

### 存储模式（ProfileOperationMode）

| 值 | 文件扩展名 | 说明 |
|----|------------|------|
| `XFEDictionary`（默认）| `.xpf` | 使用 XFE 字典格式 |
| `Json` | `.json` | 使用 JSON 序列化 |
| `Xml` | `.xml` | 使用 XML 序列化 |
| `Custom` | 自定义 | 使用自定义的加载/保存委托 |

### 自动生成的静态成员

对于每个继承 `XFEProfile` 并使用 `[ProfileProperty]` 的 `partial` 类，框架将自动生成：

| 成员 | 类型 | 说明 |
|------|------|------|
| `Current` | `static T` | 当前配置文件实例 |
| `ProfilePath` | `static string` | 配置文件路径（不含扩展名） |
| `ProfileExtension` | `static string` | 配置文件扩展名（空则自动推断） |
| `LoadProfile()` | `static void` | 从文件加载配置 |
| `SaveProfile()` | `static void` | 将配置保存到文件 |
| `DeleteProfile()` | `static void` | 删除配置文件 |
| `ExportProfile()` | `static string` | 导出配置为字符串 |
| `ImportProfile(string)` | `static void` | 从字符串导入配置 |
| `XxxProperty`（每个字段）| `static T` | 自动生成的静态属性，读写时自动持久化 |
| `InstanceXxx`（每个字段）| `T`（实例）| 对应的实例属性 |
| `GetXxxProperty()` | `static partial void` | get 钩子分部方法 |
| `SetXxxProperty(ref T)` | `static partial void` | set 钩子分部方法 |

### XFEProfile 基类成员

| 成员 | 类型 | 说明 |
|------|------|------|
| `DefaultProfileOperationMode` | `ProfileOperationMode` | 存储/加载模式 |
| `LoadOperation` | `ProfileLoadOperation` | 自定义加载委托 |
| `SaveOperation` | `ProfileSaveOperation` | 自定义保存委托 |
| `ProfilesDefaultPath` | `static string` | 所有配置文件的默认根目录 |

---

## 许可证

本项目基于 [MIT 许可证](LICENSE.txt) 开源。
