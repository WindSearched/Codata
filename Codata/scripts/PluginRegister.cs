using System.Reflection;
using CdPlugin;

namespace Codata.scripts;

public class PluginRegister
{
    private string path = "";

    public static void Registering(string path)
    {
        if (!Data.DirectoryExists(path))
        {
            return;
        }

        foreach (var dll in Directory.GetFiles(path, "*.dll"))
        {
            Assembly a =  Assembly.LoadFrom(dll);
            var v = a.GetTypes().First(x =>
                typeof(CdInterface).IsAssignableFrom(x) &&
                !x.IsInterface &&
                !x.IsAbstract
            );
            var i = (CdInterface)Activator.CreateInstance(v)!;
            i.OnStart();

            var bs = i.GetCommands();
            Program.command.AddBranches(bs);

            Program.OnProgramClose += i.OnClose;

        }

    }
}