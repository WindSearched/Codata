namespace Codata.scripts;


public class Info
{
    public static readonly string version = "test1.1";

	public string user = "Codata";
    public bool debug = false;

    public static Info ReadJson(string path)
    {
        var i = Data.ReadJson<Info>(path);
        return i == null ? new Info() : i;
    }
    public static void WriteJson(string path, Info info) => Data.WriteJson(path, info);
}

