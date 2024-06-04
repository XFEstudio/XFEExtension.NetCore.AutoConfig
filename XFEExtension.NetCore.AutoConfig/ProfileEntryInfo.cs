using System.Reflection;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件属性信息
/// </summary>
/// <param name="name">属性名称</param>
/// <param name="memberInfo">属性信息</param>
public class ProfileEntryInfo(string name, MemberInfo memberInfo)
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; set; } = name;
    /// <summary>
    /// 属性信息
    /// </summary>
    public MemberInfo MemberInfo { get; init; } = memberInfo;
}
