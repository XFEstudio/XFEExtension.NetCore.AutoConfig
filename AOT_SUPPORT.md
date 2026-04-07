# AOT (Ahead-of-Time) 编译支持

## 概述

从版本 2.0.8 开始，XFEExtension.NetCore.AutoConfig 支持 .NET Native AOT 编译。

## 问题背景

在使用 Native AOT 编译时，默认情况下 .NET 会禁用基于反射的 JSON 序列化，这会导致以下错误：

```
System.InvalidOperationException: Reflection-based serialization has been disabled for this application.
Either use the source generator APIs or explicitly configure the 'JsonSerializerOptions.TypeInfoResolver' property.
```

## 解决方案

### 1. 创建 JsonSerializerContext

为您的应用程序创建一个 `JsonSerializerContext`，包含所有需要序列化的类型：

```csharp
using System.Text.Json.Serialization;
using XFEExtension.NetCore.AutoConfig;

namespace YourApp;

[JsonSerializable(typeof(YourDataType))]
[JsonSerializable(typeof(ProfileList<YourDataType>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
// 添加其他您需要序列化的类型
public partial class AppJsonContext : JsonSerializerContext
{
}
```

### 2. 配置 JsonSerializerOptions

在程序启动时配置 `XFEProfile.JsonOptions`：

```csharp
using System.Text.Json;
using XFEExtension.NetCore.AutoConfig;

// 在 Main 方法或启动代码中
XFEProfile.JsonOptions = new JsonSerializerOptions
{
    TypeInfoResolver = AppJsonContext.Default
};
```

### 3. 项目配置

在您的 `.csproj` 文件中启用 AOT：

```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
</PropertyGroup>
```

**注意**：不再需要 `<JsonSerializerIsReflectionEnabledByDefault>true</JsonSerializerIsReflectionEnabledByDefault>` 设置。

## 完整示例

参考 `AutoConfig.Analyzer.Test` 项目中的示例：

1. `ProfileJsonContext.cs` - JsonSerializerContext 定义
2. `Program.cs` - JsonOptions 配置示例

## 常见问题

### Q: 我需要为每个类型都添加 JsonSerializable 属性吗？

A: 是的，所有在配置文件中使用的类型都需要添加。包括：
- 配置文件中的数据类型
- 泛型类型（如 `ProfileList<T>`）
- 基础类型（string, int, bool, DateTime 等）

### Q: 如果我不使用 AOT 编译，还需要配置 JsonOptions 吗？

A: 不需要。如果不使用 AOT，库会使用默认的反射序列化，无需额外配置。

### Q: 我可以在运行时更改 JsonOptions 吗？

A: 可以，但建议在应用程序启动时配置一次，避免在使用过程中修改。

## 技术细节

- `XFEProfile.JsonOptions` 是一个静态属性，所有配置文件实例共享
- 所有 JSON 序列化操作（`JsonSerializer.Serialize` 和 `JsonSerializer.Deserialize`）都使用此选项
- 支持的序列化模式：XFEDictionary（默认）、JSON、XML
