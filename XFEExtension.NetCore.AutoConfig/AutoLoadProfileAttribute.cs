namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 自动加载配置文件
/// </summary>
/// <param name="autoLoad">是否自动加载</param>
[AttributeUsage(AttributeTargets.Class)]
public class AutoLoadProfileAttribute(bool autoLoad = true) : Attribute
{
    /// <summary>
    /// 是否自动加载
    /// </summary>
    internal bool AutoLoad { get; set; } = autoLoad;
}
