using CdPlugin;

namespace TestPlugin;

public class Plugin : CdInterface
{
    public CommandBranch[] GetCommands() => [];

    public void OnStart()
    {
        Center.Log("TestPlugin Loaded");
    }

    public void OnClose()
    {
        Center.Log("TestPlugin Closed");

    }
}