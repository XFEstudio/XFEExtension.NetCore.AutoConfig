using System.Text.Json.Serialization;
using XFEExtension.NetCore.AutoConfig;

namespace XFEExtension.NetCore.XUnit.Test;

/// <summary>
/// JSON序列化上下文，用于AOT编译支持
/// </summary>
[JsonSerializable(typeof(UserInfo))]
[JsonSerializable(typeof(ProfileList<UserInfo>))]
[JsonSerializable(typeof(List<UserInfo>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Guid))]
public partial class ProfileJsonContext : JsonSerializerContext
{
}
