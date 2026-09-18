using System.Text;
using CdPlugin;
using Codata.scripts.classes;

namespace Codata.scripts.commandBranches;

public static class FileViewer
{
    public static CommandBranch branch =
        new CommandBranch("fileViewer")
            .AddArgument(new CommandBranch.Argument("path"))
            .Execute(arg =>
            {
                if (arg.TryGet("path", out var path))
                {
                    List<CommandLine> cls = new();
                    StringBuilder put = new StringBuilder();

                    int count = 1;
                    foreach (var di in GetDirectories(path))
                    {
                        var p = di.FullName;
                        var n =  di.Name;
                        var s = FormatBytes(GetDirectorySize(p, out var remind));

                        put.AppendLine($"\t{n}\t\t{s} {remind}");
                        cls.Add(new(count++, "fileViewer " + p));

                    }

                    foreach (var fi in GetFiles(path))
                    {
                        var n =  fi.Name;
                        var s = FormatBytes(fi.Length);

                        put.AppendLine($"\t{n}\t\t{s}");
                    }

                    return new (put.ToString(), true, cls);
                }
                else
                {
                    return new("The path is not exist", false);
                }
            })
        ;

    public static long GetDirectorySize(string path) => GetDirectorySize(path, out _);
    public static long GetDirectorySize(string path, out string remind)
    {
        long totalSize = 0;
        remind = "";
        try
        {
            // 当前目录下的文件
            foreach (string file in Directory.EnumerateFiles(path))
            {
                try
                {
                    totalSize += new FileInfo(file).Length;
                }
                catch (UnauthorizedAccessException)
                {
                    remind = "has inaccessible resources";
                }
                catch (IOException)
                {
                    // 文件不可用，跳过
                }
            }

            // 当前目录下的子目录
            foreach (string directory in Directory.EnumerateDirectories(path))
            {
                try
                {
                    totalSize += GetDirectorySize(directory);
                }
                catch (UnauthorizedAccessException)
                {
                    // 子目录没有权限，跳过
                    remind = "has inaccessible resources";

                }
                catch (IOException)
                {
                    // 子目录不可用，跳过
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 当前目录没有权限，跳过
            remind = "has inaccessible resources";

        }
        catch (IOException)
        {
            // 当前目录不可访问，跳过
        }

        return totalSize;
    }
    public static string FormatBytes(long bytes, int digits = 2)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };

        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size.ToString($"F{digits}")} {units[unitIndex]}";
    }

    public static FileInfo[] GetFiles(string path) => Data.GetFilesInfo(path);
    public static DirectoryInfo[] GetDirectories(string path) => Data.GetDirectoriesInfo(path);

}