using XFEExtension.NetCore.AutoConfig;

namespace PDDShopManagementSystem.ServerConsole;

/// <summary>
/// 用户配置文件
/// </summary>
internal partial class UserProfile
{
    /// <summary>
    /// 用户信息列表
    /// </summary>
    [ProfileProperty]
    private ProfileList<UserProfile, UserInfo> userInfoList = [];
}
