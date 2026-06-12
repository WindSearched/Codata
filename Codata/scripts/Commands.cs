namespace Codata.scripts;

public static class Commands
{
    public static CommandBranch branch;

    public static void SetPointerCommand(string pointer, string pointedCommand, string branchPath = "")
    {
        var s = branch.Parse(branchPath, out int _);
        s.AddBranch(new CommandBranch(pointer)
            .Execute(arg => branch.Command(pointedCommand))
        );
    }

    public static void Init()
    {
        branch = Program.command;
    }

    public static List<string> ParseArgs(string input)
    {
        var result = new List<string>();
        var current = "";
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (current.Length > 0)
            result.Add(current);

        return result;
    }
}