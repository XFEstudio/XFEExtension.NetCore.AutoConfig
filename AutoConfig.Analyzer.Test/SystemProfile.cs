using XFEExtension.NetCore.AutoConfig;

namespace AutoConfig.Analyzer.Test;

public partial class SystemProfile
{
    [ProfileProperty]
    private int testNum;
    public SystemProfile() => ProfilePath = @"C:\Users\XFEstudio\Downloads\SystemProfile.xfe";
}
