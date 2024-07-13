namespace AutoConfig.Analyzer.Test;

public class Program
{
    [SMTest]
    public static void MainTest()
    {
        SystemProfile.TestNum = 3;
        Console.WriteLine(SystemProfile.TestNum);
        Console.WriteLine(SystemProfile.ProfilePath);
    }
}
