# XFEExtension.NetCore.AutoConfig

## 描述

XFEExtension.NetCore.AutoConfig是一个可以自动实现配置文件存储的工具

## 自动实现配置文件的存储

#### 基础用法

```csharp
//创建配置文件类
partial class SystemProfile
{
    [ProfileProperty]
    string name;

    [ProfileProperty]
    int _age;
}

//使用配置文件
class Program
{
    static void Main(string[] args)
    {
        SystemProfile.Name = "Test";//在设置值的时候会自动记录并储存
        //SystemProfile.Age = 1;
        Console.WriteLine(SystemProfile.Name);
        Console.WriteLine(SystemProfile.Age);//下次打开程序会自动读取上次程序退出时储存的值
    }
}
```

#### 设置get和set方法

```csharp
partial class SystemProfile
{
    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""获取了Name"")")]
    [ProfilePropertyAddGet("return Current.name")]
    [ProfilePropertyAddSet(@"Console.WriteLine(""设置了Name"")")]
    [ProfilePropertyAddSet("Current.name = value")]
    string name = string.Empty;

    [ProfileProperty]
    [ProfilePropertyAddGet(@"Console.WriteLine(""获取了Age"")")]
    [ProfilePropertyAddGet("return Current._age")]
    [ProfilePropertyAddSet(@"Console.WriteLine(""设置了Age"")")]
    [ProfilePropertyAddSet("Current._age = value")]
    int _age;
}
```

#### 设置初始值

```csharp
partial class SystemProfile
{
    [ProfileProperty]
    string name = "John Wick";

    [ProfileProperty]
    int _age = 59;
}
```

#### 为属性添加注释

```csharp
partial class SystemProfile
{
    /// <summary>
    /// 名称
    /// 这段注释会自动添加至自动生成的Name属性上
    /// </summary>
    [ProfileProperty]
    string name;

    [ProfileProperty]
    int _age;
}
```

#### 使用部分方法来设置get和set方法

```csharp
partial class SystemProfile
{
    [ProfileProperty]
    string name;

    [ProfileProperty]
    int _age;

    static partial void GetNameProperty()
    {
        Console.WriteLine("获取了Name");
    }

    static partial void SetNameProperty(string value)
    {
        Console.WriteLine($"设置了Name：从{Name}变为了{value}");
    }

    static partial void GetAgeProperty()
    {
        Console.WriteLine("获取了Age");
    }

    static partial void SetAgeProperty(int value)
    {
        Console.WriteLine($"设置了Age：从{Age}变为了{value}");
    }
}
```