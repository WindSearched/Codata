namespace CdPlugin;

public interface CodataPlugin
{
    public void OnStart()
    {

    }
    public void OnClose()
    {

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