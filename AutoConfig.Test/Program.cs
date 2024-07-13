namespace AutoConfig.Test;

internal class Program
{
    [SMTest]
    public static void MainTest()
    {
        SystemProfile.MyNum = 2;
        Console.WriteLine(SystemProfile.MyNum);
    }
}