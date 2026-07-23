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

        private CdFunc<CommandArg, Result> execute;

        private CdFunc<CommandBranch, List<string>> suggestion;

        public List<CommandBranch> branches = new();
        public List<Argument> arguments = new();

        /// <summary>
        /// if it is true in parse time detect if the head splits is branches,
        /// if is true set after args in argument
        /// else parse in branch
        /// </summary>
        public bool param = false;

        /// <summary>
        /// abbreviation branch name
        /// </summary>
        public string abbreviation;


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

        public CommandBranch SetParamExecute<Tval, Tres>(Func<string,Tval> newer, Func<Tval[], Tres> replacer, Func<Tres, Result> result)
        {
            execute.Set((arg) =>
            {
                Tval[] vals = new Tval [arg.args.Count];
                for (int i = 0; i < vals.Length; i++)
                {
                    vals[i] = newer(arg.ParamGet(i));
                }
                Tres r = replacer(vals);
                return result(r);
            });

            return this;
        }

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
            var branchEqual = branches.Where(x => x.name == head || x.abbreviation == head).ToList();

            if (param && branchEqual.Count == 0)
            {
                //all after slipts is arg
                for (int i = 0; i < split.Count; i++)
                {
                    args.SetArg("param" + i, split[i]);
                }
                return this;
            }
            else
            {
                //when head is branch
                foreach (var b in branchEqual)
                {
                    split.RemoveAt(0);
                    return b.Parse(split, out args);
                }
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
        public (List<string> list, string tag) GetSuggestions(List<string> args)
        {
            var last = args.Last();
            args.RemoveAt(args.Count - 1);

            var node = Parse(args, out var a);
            int i = args.Count;

            var list = new List<string>();

            if (i == 0)
            {
                //add branches
                list.AddRange(node.branches.SelectMany(v => v.abbreviation == null
                    ? new[] { v.name }
                    : new[] { v.abbreviation , v.name }));
            }

            if (node.arguments.Count > i &&
                node.arguments[i].suggestion != null)
            {
                var l = node.arguments[i].suggestion.Invoke();
                if(l != null)
                    list.AddRange(l);
            }

            var rl = list.Where(x => x.StartsWith(last)).ToList();
            return new(rl, GetSuggestionTag(node, i,rl.Count == 0));
        }

        public string GetSuggestionTag(CommandBranch node,int count, bool brancheNotFound)
        {
            string t = "";

            void add(string s) => t += t == "" ? s : "/" + s;

            bool a = node.arguments.Count > 0;

            if (brancheNotFound)
            {
                if(!a)
                    add("<branch not found>");
            }
            else if (node.branches.Count > 0)
            {
                add("<branch>");
            }

            if (node.arguments.Count > 0 && count < node.arguments.Count)
            {
                add($"<{node.arguments[count].argument}>");
            }

            return t;
        }

        public CommandBranch ActiveParam()
        {
            param = true;
            return this;
        }

        public CommandBranch SetAbbreviation(string abbreviation)
        {
            this.abbreviation = abbreviation;
            return this;
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

            public string ParamGet(int index)
                => Get("param" + index);
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

        public static Result operator &(Result a, Result b)
        {
            Result r =  new Result
            {
                success = a.success & b.success
            };
            if (a.put == "default" || a.put == "")
            {
                r.put = b.put;
            }
            else
            {
                r.put = a.put + "\n" + b.put;
            }
            return r;
        }

        public static Result confirm = new("input confirm command to execute this command", true);
    }
}