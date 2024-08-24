using System;

public class CPHInline
{
    public bool Execute()
    {
        string brbScene = CPH.GetGlobalVar<string>("BRBScene", true);
        CPH.LogInfo($"Switching to OBS scene: {brbScene}");
        CPH.ObsSetScene(brbScene);

        return true;
    }
}
