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

        //script.Globals["data"] = UserData.Create(new DataLua());
        LuaAutoWrapper.RegisterStaticClass(script, typeof(Data), "data");
        LuaAutoWrapper.RegisterStaticClass(script, typeof(New), "new");
        LuaAutoWrapper.RegisterStaticClass(script, typeof(Commands), "cmd");
        LuaAutoWrapper.RegisterStaticClass(script, typeof(Tools.ReflectionHelper), "tools.refHelp");


        UserData.RegisterType<CommandBranch>();
        UserData.RegisterType<CommandBranch.CommandArg>();
        UserData.RegisterType<CommandBranch.Argument>();

    }

}

public static class LuaAutoWrapper
{
    public static void RegisterStaticClass(Script script, Type type, string luaName)
    {
        var table = new Table(script);

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (var method in methods)
        {
            table[method.Name] = WrapMethod(script, method);
        }

        // fields / properties
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            table[field.Name] = DynValue.FromObject(script, field.GetValue(null));
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
                    var paramType = parameters[i].ParameterType;

                    if (i < args.Count)
                    {
                        var arg = args[i];

                        // Lua function → delegate（基础支持）
                        if (arg.Type == DataType.Function &&
                            typeof(Delegate).IsAssignableFrom(paramType))
                        {
                            invokeArgs[i] = WrapLuaFunction(script, arg.Function, paramType);
                        }
                        else
                        {
                            invokeArgs[i] = arg.ToObject(paramType);
                        }
                    }
                    else
                    {
                        invokeArgs[i] = parameters[i].HasDefaultValue
                            ? parameters[i].DefaultValue
                            : GetDefault(paramType);
                    }
                }

                var result = method.Invoke(null, invokeArgs);

                return DynValue.FromObject(script, result);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(ex.ToString(), ex);
            }
        });
    }
    private static object WrapLuaFunction(Script script, Closure func, Type delegateType)
    {
        var invokeMethod = delegateType.GetMethod("Invoke");
        var ps = invokeMethod.GetParameters();

        if (delegateType == typeof(Func<CommandBranch.CommandArg, bool>))
        {
            return new Func<CommandBranch.CommandArg, bool>(arg =>
            {
                var result = script.Call(func, DynValue.FromObject(script, arg));
                return result.CastToBool();
            });
        }

        throw new Exception($"Unsupported delegate: {delegateType}");
    }
    private static object GetDefault(Type t)
    {
        return t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}

public class New
{
    public static CommandBranch CommandBranch(string name) => new CommandBranch(name);
    public static CommandBranch.Argument Argument(string name) => new(name);
}
