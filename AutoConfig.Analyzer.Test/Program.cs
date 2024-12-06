using PDDShopManagementSystem.ServerConsole;
using XFEExtension.NetCore.StringExtension.Json;

namespace AutoConfig.Analyzer.Test;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"上一次读取的值是：{UserProfile.UserInfoList.ToJson()}");
        Console.WriteLine("添加值：");
        Console.WriteLine("请输入店铺名称：");
        var shopName = Console.ReadLine();
        Console.WriteLine("请输入店铺ID：");
        var shopID = Console.ReadLine() ?? "暂无ID";
        UserProfile.UserInfoList.Add(new()
        {
            SessionID = Guid.NewGuid().ToString(),
            ShopID = shopID,
            RecentShopName = shopName,
            CurrentIpAddress = "198.131.114.41",
            Banned = false,
            EndDateTime = DateTime.Now
        });
        Console.ReadLine();
    }
}
