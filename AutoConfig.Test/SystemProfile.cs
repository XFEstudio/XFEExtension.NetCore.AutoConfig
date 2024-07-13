using XFEExtension.NetCore.AutoConfig;

namespace AutoConfig.Test;

[ProfilePath(@$"C:\Users\XFEstudio\Downloads\{nameof(SystemProfile)}.xfe")]
public partial class SystemProfile
{
    [ProfileProperty]
    private int myNum;
}
