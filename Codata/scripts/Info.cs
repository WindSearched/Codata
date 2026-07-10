namespace Codata.scripts;


public class Info
{
    public static readonly string version = "test3";
    public static readonly string author = "WindSearched";
    public static readonly string bilibili = "https://space.bilibili.com/1611824177";
    public static readonly string gitHub = "https://github.com/WindSearched";
    public static readonly string codataGit = "https://github.com/WindSearched/Codata";

	public string user = "Codata";
    public bool debug = false;

    public static Info ReadJson(string path)
    {
        var i = Data.ReadJson<Info>(path);
        return i == null ? new Info() : i;
    }
    public static void WriteJson(string path, Info info) => Data.WriteJson(path, info);
}

