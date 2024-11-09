using System.Reflection;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件信息
/// </summary>
public class ProfileInfo
{
    /// <summary>
    /// 配置文件类型
    /// </summary>
    public Type Profile { get; init; }
    /// <summary>
    /// 配置文件储存位置
    /// </summary>
    public string Path { get; init; }
    /// <summary>
    /// 配置文件描述
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// 实例成员信息
    /// </summary>
    public PropertyInfo? InstancePropertyInfo { get; private set; }
    /// <summary>
    /// 配置文件属性列表
    /// </summary>
    public List<ProfileEntryInfo> MemberInfo { get; private set; }
    /// <summary>
    /// 配置文件信息
    /// </summary>
    /// <param name="profileType">配置文件类型</param>
    /// <param name="path">配置文件储存位置</param>
    /// <param name="description">配置文件描述</param>
    public ProfileInfo(Type profileType, string path = "", string description = "")
    {
        Profile = profileType;
        var pathAttribute = profileType.GetCustomAttribute<ProfilePathAttribute>();
        if (path == "")
        {
            if(pathAttribute is not null)
            {
                Path = pathAttribute.Path;
            }
            else
            {
                if (XFEProfile.ProfilesDefaultPath[^1] == '/' || XFEProfile.ProfilesDefaultPath[^1] == '\\')
                    Path = $"{XFEProfile.ProfilesDefaultPath}{profileType.Name}.xpf";
                else
                    Path = $"{XFEProfile.ProfilesDefaultPath}/{profileType.Name}.xpf";
            }
        }
        else
        {
            Path = path;
        }
        Description = description;
        var profileEntryList = new List<ProfileEntryInfo>();
        foreach (var memberInfo in profileType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            if (memberInfo is not MethodInfo)
            {
                if (memberInfo.GetCustomAttribute<ProfilePropertyAttribute>() is not null)
                    profileEntryList.Add(new ProfileEntryInfo(memberInfo.Name, memberInfo));
                if (memberInfo.GetCustomAttribute<ProfileInstanceAttribute>() is not null)
                    InstancePropertyInfo = memberInfo as PropertyInfo;
            }
        MemberInfo = profileEntryList;
    }
    /// <summary>
    /// 获取当前配置文件的数据实例
    /// </summary>
    /// <returns></returns>
    public object? GetProfileInstance() => InstancePropertyInfo?.GetValue(null);
    /// <summary>
    /// 配置文件类型生成
    /// </summary>
    /// <param name="profileType"></param>
    public static implicit operator ProfileInfo(Type profileType) => new(profileType);
}