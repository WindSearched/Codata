using CdPlugin;

namespace TestPlugin;

public class Plugin : CdInterface
{
    public CommandBranch[] GetCommands() => [];

    public void OnStart()
    {
        Center.Print("TestPlugin Loaded");
    }

    public void OnClose()
    {
        Center.Print("TestPlugin Closed");

    }
}