namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 将属性添加至储存列表
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ProfilePropertyAttribute : Attribute
{
    /// <summary>
    /// 自动生成的属性名称
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    /// <summary>
    /// 将属性添加至存储列表
    /// </summary>
    public ProfilePropertyAttribute() { }
    /// <summary>
    /// 将属性添加至存储列表，给定属性名
    /// </summary>
    /// <param name="propertyName">属性名</param>
    public ProfilePropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
