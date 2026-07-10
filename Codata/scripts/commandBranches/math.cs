namespace Codata.scripts.commandBranches;

public static class _math
{
    public static CommandBranch math = new CommandBranch("math")
        .AddBranches(
            new CommandBranch("add")
                .AddArguments(
                        new CommandBranch.Argument("a"),
                        new CommandBranch.Argument("b")
                    )
                .Execute(args =>
                {
                    double a = double.Parse(args.Get("a"));
                    double b = double.Parse(args.Get("b"));
                    return new Result((a + b).ToString(), true);
                })
            );
}