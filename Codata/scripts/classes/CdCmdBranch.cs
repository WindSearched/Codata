using CdPlugin;
using MoonSharp.Interpreter;

namespace Codata.scripts.classes;

public class CdCmdBranch: CommandBranch
{
    public CdCmdBranch(string name) : base(name) => this.name = name;

    public CdCmdBranch Execute(Closure closure)
    {
        execute = new LuaFunc<CommandArg, Result>(closure);
        return this;
    }

    public CommandBranch SetSuggestion(Closure func)
    {
        suggestion = new LuaFunc<CommandBranch, List<string>>(func);
        return this;
    }
    public class LuaArgument : CommandBranch.Argument
    {
        public LuaArgument(string name) : base(name) => this.argument = name;

        public LuaArgument SetSuggestion(Closure func)
        {
            suggestion = new LuaFunc<List<string>>(func);
            return this;
        }
    }
}