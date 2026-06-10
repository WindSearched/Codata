namespace Codata.scripts;

public class CommandBranch
{
    public string name;
    public Func<CommandArg, bool> execute;
    public Func<CommandBranch,List<string>> suggestion;
    public List<CommandBranch> branches = new List<CommandBranch>();
    public List<Argument> arguments = new List<Argument>();

    public CommandBranch AddBranch(CommandBranch branch)
    {
        branches.Add(branch);
        return this;
    }

    public CommandBranch AddBranches(params CommandBranch[] branches)
    {
        foreach (var branch in branches)
            AddBranch(branch);
        return this;
    }

    public CommandBranch AddArgument(Argument argument)
    {
        arguments.Add(argument);
        return this;
    }

    public CommandBranch AddArguments(params Argument[] arguments)
    {
        foreach (var argument in arguments)
            AddArgument(argument);
        return this;
    }

    /// <summary>
    /// Add the execute method of this command branch
    /// </summary>
    /// <param name="execute"></param>
    /// <returns></returns>
    public CommandBranch Execute(Func<CommandArg, bool> execute)
    {
        this.execute = execute;
        return this;
    }

    public CommandBranch SetSuggestion(Func<CommandBranch,List<string>> suggestion)
    {
        this.suggestion = suggestion;
        return this;
    }

    public CommandBranch Parse(List<string> splited, out CommandArg args)
    {
        if (splited.Count == 0)
        {
            args = null;
            return this;
        }

        var s = splited[0];//get the first value
        foreach (var branch in branches.Where(branch => branch.name == s))
        {   // parse if is existed branch named s
            splited.RemoveAt(0);
            return branch.Parse(splited, out args);
        }

        if (splited.Count != arguments.Count)
        {
            args = null;
            return this;
        }

        args = new CommandArg();
        for (int i = 0; i < arguments.Count; i++)
        {
            var arg = arguments[i];
            args.SetArg(arg.argument, splited[i]);
        }
        return this;
    }

    public CommandBranch Parse(List<string> splited, out int remainsCount)
    {
        if (splited.Count == 0)
        {
            remainsCount = 0;
            return this;
        }

        var s = splited[0];//get the first value
        foreach (var branch in branches.Where(branch => branch.name == s))
        {   // parse if is existed branch named s
            splited.RemoveAt(0);
            return branch.Parse(splited, out remainsCount);
        }

        remainsCount = splited.Count;
        return this;
    }

    public bool Command(string arg)
    {
        var list = arg.Split(' ').ToList();
        var branch = Parse(list, out CommandArg args);
        return branch.execute != null && branch.execute.Invoke(args);
    }

    public delegate List<string> CMDSuggestion();

    public CommandBranch(string name)
    {
        this.name = name;
    }

    public override string ToString() => name;

    public List<string> GetSuggestions(List<string> args)
    {
        var last = args.Last();
        args.RemoveAt(args.Count - 1);

        var b = Parse(args, out int i);

        List<string> list = new();
        if(i == 0 && b.suggestion != null)
            list.AddRange(b.suggestion.Invoke(this));
        if(b.arguments.Count > i)
            list.AddRange(b.arguments[i].suggestion.Invoke());
        //
        // if (i == 0)//branches suggestions
        // {
        //     if(b.suggestion != null)
        //         list = b.suggestion.Invoke(this);
        // }
        // else if (b.arguments[i].suggestion != null)
        // {
        //     list = b.arguments[i].suggestion.Invoke();
        // }

        return list.Where(v => v.StartsWith(last)).ToList();
    }

    public class Argument
    {
        public string argument;
        public Func<List<string>> suggestion;

        public Argument(string argument)
        {
            this.argument = argument;
        }

        public Argument SetSuggestion(Func<List<string>> suggestion)
        {
            this.suggestion = suggestion;
            return this;
        }
    }
    public class CommandArg
    {
        public Dictionary<string, string> args = new();

        public string Get(string key) => args.ContainsKey(key) ? args[key] : null;

        public string Get(int index)
        {
            var a = args.Values.ToList();
            return a.Count > index ? a[index] : null;
        }

        public void SetArg(string key, string value, bool overwrite = true)
        {
            if (args.TryAdd(key, value)) return;
            if(overwrite)
                args[key] = value;
        }
    }
}