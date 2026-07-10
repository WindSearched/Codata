using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using Codata.scripts.classes;

namespace Codata.scripts
{
    public class CommandBranch
    {
        public string name;

        // =========================
        // C# / Lua 双执行体系
        // =========================
        private CdFunc<CommandArg, Result> execute;

        private CdFunc<CommandBranch, List<string>> suggestion;

        public List<CommandBranch> branches = new();
        public List<Argument> arguments = new();

        // =========================
        // 构造
        // =========================
        public CommandBranch(string name)
        {
            execute = new(Lua.script);
            suggestion = new(Lua.script);
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
            execute.Set(func);
            return this;
        }

        // =========================
        // Lua 执行
        // =========================
        public CommandBranch Execute(Closure func)
        {
            execute.Set(func);
            return this;
        }

        // =========================
        // 统一执行入口（关键）
        // =========================
        public Result Run(CommandArg arg) => execute.Invoke(arg);

        // =========================
        // 命令入口
        // =========================
        public Result Command(string input)
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
            args = new CommandArg();

            if (split.Count == 0)
            {
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
                return this;
            }


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
            suggestion.func = func;
            return this;
        }
        public CommandBranch SetSuggestion(Closure func)
        {
            suggestion.closure = func;
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
                list.AddRange(suggestion.Invoke(node));
            }

            if (node.arguments.Count > i &&
                node.arguments[i].suggestion != null)
                list.AddRange(node.arguments[i].suggestion.Invoke());

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
            public CdFunc<List<string>> suggestion;

            public Argument(string argument)
            {
                suggestion = new(Lua.script);
                this.argument = argument;
            }

            public Argument SetSuggestion(Func<List<string>> func)
            {
                suggestion.func = func;
                return this;
            }
            public Argument SetSuggestion(Closure func)
            {
                suggestion.closure = func;
                return this;
            }

        }

        public class CommandArg
        {
            public Dictionary<string, string> args = new();

            public string Get(string key)
                => args.TryGetValue(key, out var v) ? v : "";
            public bool TryGet(string key, out string value) => args.TryGetValue(key, out value);

            public int GetInt(string key)
            {
                string v = Get(key);
                return int.Parse(v);
            }

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