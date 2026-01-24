namespace XFEExtension.NetCore.XUnit.Test;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 店铺最近登录使用的店铺名称
    /// </summary>
    public string? RecentShopName { get; set; }
    /// <summary>
    /// 店铺ID
    /// </summary>
    public required string ShopID { get; set; }
    /// <summary>
    /// 当前活动期ID
    /// </summary>
    public required string SessionID { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// 最近一次登录的IP地址
    /// </summary>
    public required string CurrentIpAddress { get; set; }
    /// <summary>
    /// 剩余天数
    /// </summary>
    public DateTime EndDateTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 该商家店铺是否被封禁
    /// </summary>
    public bool Banned { get; set; }
}
