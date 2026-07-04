using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codata.scripts
{
    public class CommandBranch
    {
        public string name;

        // =========================
        // C# / Lua 双执行体系
        // =========================
        private Func<CommandArg, Result> _csExecute;
        private Closure _luaExecute;
        private Script _script;

        private Func<CommandBranch, List<string>> _csSuggestion;
        private Closure _luaSuggestion;

        public List<CommandBranch> branches = new();
        public List<Argument> arguments = new();

        // =========================
        // 构造
        // =========================
        public CommandBranch(string name)
        {
            this.name = name;
        }

        // =========================
        // 添加子节点
        // =========================
        public CommandBranch AddBranch(CommandBranch branch)
        {
            branches.Add(branch);
            return this;
        }

        public CommandBranch AddBranches(params CommandBranch[] branches)
        {
            foreach (var b in branches)
                AddBranch(b);
            return this;
        }

        // =========================
        // 参数
        // =========================
        public CommandBranch AddArgument(Argument argument)
        {
            arguments.Add(argument);
            return this;
        }

        public CommandBranch AddArgument(string argument) => AddArgument(new Argument(argument));

        public CommandBranch AddArguments(params Argument[] arguments)
        {
            foreach (var a in arguments)
                AddArgument(a);
            return this;
        }

        // =========================
        // C# 执行
        // =========================
        public CommandBranch Execute(Func<CommandArg, Result> func)
        {
            _csExecute = func;
            return this;
        }

        // =========================
        // Lua 执行
        // =========================
        public CommandBranch Execute(Closure func, Script script)
        {
            _luaExecute = func;
            _script = script;
            return this;
        }

        // =========================
        // 统一执行入口（关键）
        // =========================
        public bool Run(CommandArg arg)
        {
            // Lua 优先
            if (_luaExecute != null)
            {
                var result = _script.Call(
                    _luaExecute,
                    DynValue.FromObject(_script, arg)
                );

                return result.CastToBool();
            }

            // C# fallback
            return _csExecute?.Invoke(arg).success ?? false;
        }

        // =========================
        // 命令入口
        // =========================
        public bool Command(string input)
        {
            var branch = Parse(input, out CommandArg args);
            return branch.Run(args);
        }

        // =========================
        // Parse
        // =========================
        public CommandBranch Parse(string path, out CommandArg args)
        {
            var split = Commands.ParseArgs(path.Trim());
            return Parse(split, out args);
        }

        public CommandBranch Parse(List<string> split, out CommandArg args)
        {
            if (split.Count == 0)
            {
                args = null;
                return this;
            }

            var head = split[0];

            foreach (var b in branches.Where(x => x.name == head))
            {
                split.RemoveAt(0);
                return b.Parse(split, out args);
            }

            if (split.Count != arguments.Count)
            {
                args = null;
                return this;
            }

            args = new CommandArg();

            for (int i = 0; i < arguments.Count; i++)
            {
                args.SetArg(arguments[i].argument, split[i]);
            }

            return this;
        }

        // =========================
        // Suggestion
        // =========================

        public CommandBranch SetSuggestion(Func<CommandBranch, List<string>> func)
        {
            _csSuggestion = func;
            return this;
        }
        public CommandBranch SetSuggestion(Closure func, Script script)
        {
            _luaSuggestion = func;
            _script = script;
            return this;
        }
        public List<string> GetSuggestions(List<string> args)
        {
            var last = args.Last();
            args.RemoveAt(args.Count - 1);

            var node = Parse(args, out var a);
            int i = args.Count;

            var list = new List<string>();

            if (i == 0)
            {
                if (node._luaSuggestion != null)
                {
                    var result = node._script.Call(
                        node._luaSuggestion,
                        DynValue.FromObject(node._script, node)
                    );

                    foreach (var v in result.Table.Values)
                        list.Add(v.String);
                }
                else if (node._csSuggestion != null)
                {
                    list.AddRange(node._csSuggestion(node));
                }
            }

            if (node.arguments.Count > i &&
                node.arguments[i].suggestion != null)
                list.AddRange(node.arguments[i].suggestion());

            return list
                .Where(x => x.StartsWith(last))
                .ToList();
        }

        // =========================
        // Inner types
        // =========================
        public class Argument
        {
            public string argument;
            public Func<List<string>> suggestion;

            public Argument(string argument)
            {
                this.argument = argument;
            }

            public Argument SetSuggestion(Func<List<string>> func)
            {
                suggestion = func;
                return this;
            }
        }

        public class CommandArg
        {
            public Dictionary<string, string> args = new();

            public string Get(string key)
                => args.TryGetValue(key, out var v) ? v : null;

            public string Get(int index)
                => args.Values.ElementAtOrDefault(index);

            public void SetArg(string key, string value, bool overwrite = true)
            {
                if (!args.ContainsKey(key))
                {
                    args[key] = value;
                    return;
                }

                if (overwrite)
                    args[key] = value;
            }
        }

        public override string ToString() => name;
    }

    public struct Result
    {
        public string put;
        public bool success;

        public Result(string put, bool success)
        {
            this.put = put;
            this.success = success;
        }

        public Result()
        {
            put = "default";
            success = false;
        }

        public Result(bool success)
        {
            this.success = success;
            put = success ? "success" : "fail";
        }
    }
}