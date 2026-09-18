using CdPlugin;

namespace Codata.scripts.classes;


public class Langue
{
	public SDict<string, string> dict = new();
	public string language = "en";

	public Langue(string language, string[] paths)
	{
		this.language = language;
		dict = Read(language, paths);
	}
	public Langue(string language, string path)
	{
		this.language = language;
		dict = Read(language, path);
	}

	public string Get(string key) => dict.Get(key, language);

	public override string ToString()
	{
		var s = dict.ToString();
		return language + s;
	}

	public static SDict<string, string> Read(string language, string[] paths, SDict<string, string> dict = null)
	{
		dict ??= new();
		foreach (string path in paths)
		{
			Read(language, path, dict);
		}
		return dict;
	}
	public static SDict<string,string> Read(string language, string path, SDict<string, string> dict = null)
	{
		dict ??= new();
		string[] lines = Data.ReadFileInLines(path);

		string valprefix = "", valsuffix = "";
		string keyprefix = "", keysuffix = "";
		bool notSelectedLangue = false;

		foreach (string line in lines)
		{
			if (line.StartsWith('/'))//is special line
			{
				var args = line.TrimStart('/').Split(' ');
				switch (args[0])
				{
					case "l":
						notSelectedLangue = language != args[1];
						break;
					case "vpf":
						valprefix = args[1];
						break;
					case "vsf":
						valsuffix = args[1];
						break;
					case "kpf":
						keyprefix = args[1];
						break;
					case "ksf":
						keysuffix = args[1];
						break;
				}
			}
			else
			{
				var args = line.Split(':');
				if (args.Length != 2 || notSelectedLangue) continue;
				dict.Set(keyprefix+args[0]+keysuffix, valprefix+language+valsuffix, args[1]);
			}
		}
		return dict;
	}
	public static SDict<string,string> Read(string path, SDict<string, string> dict = null)
	{
		dict ??= new();
		string[] lines = Data.ReadFileInLines(path);

		string language = "en";
		string valprefix = "", valsuffix = "";
		string keyprefix = "", keysuffix = "";

		foreach (string line in lines)
		{
			if (line.StartsWith("/"))//is special line
			{
				var args = line.TrimStart('/').Split(' ');
				switch (args[0])
				{
					case "l":
						language = args[1];
						break;
					case "vpf":
						valprefix = args[1];
						break;
					case "vsf":
						valsuffix = args[1];
						break;
					case "kpf":
						keyprefix = args[1];
						break;
					case "ksf":
						keysuffix = args[1];
						break;
				}
			}
			else
			{
				var args = line.Split(':');
				if (args.Length != 2) continue;
				dict.Set(keyprefix+args[0]+keysuffix, valprefix+language+valsuffix, args[1]);
			}
		}
		return dict;
	}



	public static void Init(out Langue langue)
	{
		string path = Path.Combine(Data.filePath, "langue");
		string[] readPaths = [];
		if (Data.DirectoryExists(path))
		{
			DirectoryInfo di = new DirectoryInfo(path);
			readPaths = di.GetFiles("*.lgu", SearchOption.AllDirectories).Select(x => x.FullName).ToArray();
		}
		else
		{
			Data.CreateDirectory(path);
		}

		langue = new Langue(Center.info.language, readPaths);
	}
}