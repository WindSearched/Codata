namespace Codata.scripts.commandBranches;

public static class Test
{
    public static CommandBranch test = new CommandBranch("test")
        .AddBranches(
            new CommandBranch("linecmd")
                .Execute( args => new("test", true, [(1, "print test")]))
        );
}