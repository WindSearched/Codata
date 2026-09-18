namespace CdPlugin;

public interface CdInterface
{
    public void OnStart()
    {

    }
    public void OnClose()
    {

    }

    public CommandBranch[] GetCommands()
    {
        return [];
    }

    public string GetAuthor()
    {
        return "WS";
    }

    public string GetName()
    {
        return "Codata";
    }
}