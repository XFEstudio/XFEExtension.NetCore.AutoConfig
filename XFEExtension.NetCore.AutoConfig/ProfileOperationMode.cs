namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件存储和读取操作模式
/// </summary>
public enum ProfileOperationMode
{
    /// <summary>
    /// 使用XFE字典加载和存储配置文件（默认）
    /// </summary>
    XFEDictionary,
    /// <summary>
    /// 使用Json序列化加载和存储配置文件
    /// </summary>
    Json,
    /// <summary>
    /// 使用Xml序列化加载和存储配置文件
    /// </summary>
    Xml,
    /// <summary>
    /// 使用自定义方法加载和存储配置文件
    /// </summary>
    Custom
}
