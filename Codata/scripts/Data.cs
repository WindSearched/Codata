using System.Diagnostics;

namespace Codata.scripts;
using System.IO;

public static class Data
{
    public static string filePath;
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
}