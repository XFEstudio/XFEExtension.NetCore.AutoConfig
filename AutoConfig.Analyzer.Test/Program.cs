using System.Text.Json;
using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.StringExtension.Json;

namespace AutoConfig.Analyzer.Test;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"上一次读取的值是{SystemProfile.MyText}");
        Console.Write("添加值：");
        SystemProfile.MyText = Console.ReadLine();
        Console.WriteLine(SystemProfile.MyText);
        Console.ReadLine();
        //Console.WriteLine($"上一次读取的值是：{UserProfile.UserInfoList.ToJson()}");
        //Console.WriteLine("添加值：");
        //Console.WriteLine("请输入店铺名称：");
        //var shopName = Console.ReadLine();
        //Console.WriteLine("请输入店铺ID：");
        //var shopID = Console.ReadLine() ?? "暂无ID";
        //Console.WriteLine("创建对象...");
        //var target = new UserInfo()
        //{
        //    SessionID = Guid.NewGuid().ToString(),
        //    ShopID = shopID,
        //    RecentShopName = shopName,
        //    CurrentIpAddress = "198.131.114.41",
        //    Banned = false,
        //    EndDateTime = DateTime.Now
        //};
        //Console.WriteLine(target.ShopID);
        //Console.WriteLine(JsonSerializer.Serialize(target));
        //Console.WriteLine("对象创建完成！准备进行分析...");
        //target.X();
        //Console.WriteLine(target.ToJson());
        //Console.WriteLine("分析完成！");
        //UserProfile.UserInfoList.Add(target);
        //Console.WriteLine(UserProfile.UserInfoList.ToJson());
        //Console.ReadLine();
    }
}
