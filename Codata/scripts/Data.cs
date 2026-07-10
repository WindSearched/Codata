using System.Diagnostics;

namespace Codata.scripts;
using System.IO;
using System.Text.Json;

public static class Data
{
    public static string filePath;
    public static string infoPath;
    public static string defaultFilePath = AppDomain.CurrentDomain.BaseDirectory;


    public static void Init()
    {
        Program.Log(defaultFilePath);

        if (FileExists(defaultFilePath + "path.txt"))
        {
            filePath = ReadFile(defaultFilePath + "path.txt");
        }
        else
        {
            filePath = defaultFilePath;
            CreateFile(defaultFilePath + "path.txt", defaultFilePath);
        }

        infoPath = Path.Combine(filePath, "info.json");
    }

    public static bool FileExists(string path) => File.Exists(path);
    public static bool DirectoryExists(string path) => Directory.Exists(path);

    public static void CreateFile(string path, string content = "", bool rewrite = true)
    {
        if (FileExists(path) && !rewrite)
            return;
        using StreamWriter sw = new(path);
        sw.Write(content);
        sw.Close();
    }

    public static void CreateDirectory(string path, bool rewrite = true)
    {
        if(DirectoryExists(path) && !rewrite) return;
        Directory.CreateDirectory(path);
    }

    public static string ReadFile(string path)
    {
        if (!FileExists(path))
            return string.Empty;
        using StreamReader sr = new(path);
        string result = sr.ReadToEnd();
        sr.Close();
        return result;
    }
    public static string PathCombine(string path1, string path2) => Path.Combine(path1, path2);

    public static FileInfo[] GetFilesInfo(string directoryPath)
    {
        var d = new DirectoryInfo(directoryPath);
        return d.GetFiles();
    }

    public static void OpenForm(string path)
    {
        Process.Start("explorer.exe", path);
    }

    public static void OpenForm(ProcessStartInfo startInfo)
    {
        Process.Start(startInfo);
    }

    public static void OpenForm(string fileName, string arguments, bool useShell)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = useShell
        });
    }
    public static void OpenForm(string fileName, bool useShell) => OpenForm(fileName, "", useShell);

    public static bool WriteJson<T>(string path, T content, bool rewrite = true)
    {
        if (!FileExists(path) || rewrite)
        {
            string json =  JsonSerializer.Serialize(content, new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true
            });
            CreateFile(path, json, rewrite);
            return true;
        }
        return false;
    }

    public static T ReadJson<T>(string path)
    {
        if (FileExists(path))
        {
            string json = ReadFile(path);
            return JsonSerializer.Deserialize<T>(json) ;
        }
        return default;
    }

    public static bool TryReadJson<T>(string path, out T content)
    {
        if (FileExists(path))
        {
            string json = ReadFile(path);
            content = JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException();
            return true;
        }
        content = default;
        return false;
    }
}
public class DataLua
{
    public bool FileExists(string path) => Data.FileExists(path);

    public bool DirectoryExists(string path) => Data.DirectoryExists(path);

    public void CreateFile(string path, string content = "", bool rewrite = true)
        => Data.CreateFile(path, content, rewrite);

    public void CreateDirectory(string path, bool rewrite = true)
        => Data.CreateDirectory(path, rewrite);

    public string ReadFile(string path)
        => Data.ReadFile(path);

    public string PathCombine(string a, string b)
        => Data.PathCombine(a, b);

    public FileInfo[] GetFilesInfo(string path)
        => Data.GetFilesInfo(path);

    public void OpenForm(string path)
        => Data.OpenForm(path);

    public void OpenForm(string fileName, string args, bool useShell)
        => Data.OpenForm(fileName, args, useShell);

    public void Init()
        => Data.Init();
}