namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件储存位置
/// </summary>
/// <param name="path">储存路径</param>
[AttributeUsage(AttributeTargets.Class)]
public class ProfilePathAttribute(string path) : Attribute
{
    /// <summary>
    /// 配置文件的存储路径
    /// </summary>
    public string Path { get; set; } = path;
}
