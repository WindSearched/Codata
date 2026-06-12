using System.Reflection;
using Microsoft.VisualBasic.Logging;

namespace Codata.scripts;
using MoonSharp.Interpreter;

public static class Lua
{
    public static Script script = new();

    public static void Load(string path)
    {

        script.DoFile(path);

        string name = Path.GetFileNameWithoutExtension(path);

        var table = script.Globals.Get(name).Table;
        var func = table.Get("OnLoad");

        script.Call(func);
    }

    public static void Init()
    {
        Register();

        string path = Data.PathCombine(Data.filePath, "mods");
        Data.CreateDirectory(path,false);
        Program.Log(path);
        foreach (var filepath in Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories))
        {
            Load(filepath);
        }
    }

    public static void Register()
    {
        var s = script;

        void r (string name, object? o) => s.Globals[name] = o;
        r("log", (Action<object>)Program.Log);

        RegisterStaticClass(typeof(Data), "data");
        RegisterStaticClass(typeof(Commands), "cmd");
    }

    public static void RegisterStaticClass(Type type, string luaName)
    {
        var table = new Table(script);

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (var method in methods)
        {
            table[method.Name] = WrapMethod(script, method);
        }

        script.Globals[luaName] = table;
    }
    private static DynValue WrapMethod(Script script, MethodInfo method)
    {
        return DynValue.NewCallback((ctx, args) =>
        {
            try
            {
                var parameters = method.GetParameters();

                object[] invokeArgs = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i < args.Count)
                        invokeArgs[i] = args[i].ToObject(parameters[i].ParameterType);
                    else
                        invokeArgs[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                }

                var result = method.Invoke(null, invokeArgs);

                return DynValue.FromObject(script, result);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(ex.Message);
            }
        });
    }
}