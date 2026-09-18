using CdPlugin;

namespace Codata.scripts;

public static class Commands
{
    private static CommandBranch branch;

    public static void SetPointerCommand(string pointer, string pointedCommands, string branchPath = "")
    {
        var s = branch.Parse(branchPath, out  _);
        string[] pcs = pointedCommands.Split('\n');

        s.AddBranch(new CommandBranch(pointer)
            .Execute(arg =>
            {
                Result res = new Result("",true);
                foreach (var p in pcs)
                {
                    var r = branch.Command(p);
                    res &= r;
                }

                return res;
            })
        );
    }
    public static void AddBranch(CommandBranch branch) => Program.command.AddBranch(branch);


    public static void Init()
    {
        branch = Program.command;
    }



    public static string GetParamIndex(int index) => "param" + index;
}