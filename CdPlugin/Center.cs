using Codata.scripts;
using Codata.scripts.classes;
using MoonSharp.Interpreter;

namespace CdPlugin;

public static class Center
{
    public static Script luascript;
    public static Langue langue;
    public static Info info;

    public static Action<string> logAction;
    public static void Log(string message) => logAction?.Invoke(message);
}