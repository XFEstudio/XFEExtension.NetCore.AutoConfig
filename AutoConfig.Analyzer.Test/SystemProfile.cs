using XFEExtension.NetCore.AutoConfig;

namespace AutoConfig.Analyzer.Test;

public partial class SystemProfile : XFEProfile
{

    [ProfileProperty]
    private string myText = "";
    [ProfileProperty]
    private int myInt = 1;

    public SystemProfile()
    {
        DefaultProfileOperationMode = ProfileOperationMode.Json;
    }
}