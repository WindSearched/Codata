namespace Codata.scripts;

public class LineCommand
{
    public SortedDictionary<int, string> commands = new();
    public SortedDictionary<int, string>.KeyCollection lines => commands.Keys;

    public void Reg(int lineIndex, string command, bool overwrite = true)
    {
        if (commands.ContainsKey(lineIndex))
        {
            if(overwrite)
                commands[lineIndex] = command;
        }
        else
        {
            commands.Add(lineIndex, command);
        }
    }

    public void Reg(int lineIndex ,Builder builder, bool overwrite) => Reg(lineIndex, builder.ToString(), overwrite);
    public string Get(int lineIndex, bool skipCanUse = false)
    {
        if (skipCanUse)
        {
            if(!Contain(lineIndex)) return "";
        }
        else
        {
            if(!CanUse(lineIndex)) return "";
        }

        var l = commands[lineIndex].Split('|');
        var c = l.Last();
        for (int i = 0; i < l.Length -1; i++)
        {
            c = PrefixParse(l[i], c);
        }
        return c;
    }

    public bool TryGet(int lineIndex, out string command, bool skipCanUse = false)
    {
        command = null;
        if (skipCanUse)
        {
            if(!Contain(lineIndex)) return false;
        }
        else
        {
            if(!CanUse(lineIndex)) return false;
        }

        var l = commands[lineIndex].Split('|');
        command = l.Last();
        for (int i = 0; i < l.Length -1; i++)
        {
            command = PrefixParse(l[i], command);
        }
        return !string.IsNullOrEmpty(command);
    }


    public bool CanUse(int lineIndex)
    {
        if(!commands.TryGetValue(lineIndex, out var c)) return false;
        return c.Split('|').SkipLast(1).Any(x => !x.StartsWith('n'));//if prefix start with n the command is not usable
    }
    public bool Contain(int lineIndex) => commands.ContainsKey(lineIndex);

    string PrefixParse(string prefix, string command)
    {
        var map = new Dictionary<char, Func<string, string, string>>
        {
            {'+', (prex, sufx) =>
                {
                    if (prex == "")
                    {
                        if(int.TryParse(sufx, out int i))
                            return command + Get(i, true);
                    }
                    else if(sufx == "")
                    {
                        if(int.TryParse(prex, out int i))
                            return Get(i) + command;
                    }

                    return command;
                }
            },
        };

        string s = "";

        foreach(var m in map)
        {
            int i = prefix.IndexOf(m.Key);
            if(i == -1) break;
            var px = prefix.Substring(0, i);
            var sx = prefix.Substring(i + 1);

            s = m.Value.Invoke(px, sx);
        }

        return s;
    }

    public int LineSearchRange(int start, int end)
    {
        if(start > end) return 0;
        var r = lines
            .SkipWhile(x => x < start)
            .FirstOrDefault();
        if (r <= end)
        {
            return r;
        }
        return 0;
    }
    public int LineSearchReverseRange(int start, int end)
    {
        if(start < end) return 0;
        var r = lines
            .Reverse()
            .SkipWhile(x => x > start)
            .FirstOrDefault();
        if(r >= end) return r;
        return 0;
    }

    public int LineSearch(int index, bool skipFirst, bool circling)
    {
        var p1 = lines.SkipWhile(skipFirst ? x => x <= index : x => x < index);
        var p2 = lines.TakeWhile(x => x < index);
        var r = p1.Concat(p2)
            .FirstOrDefault();
        return r;
    }

    public int LineSearchReverse(int index, bool skipFirst, bool circling)
    {
        var p1 = lines.Reverse()
            .SkipWhile(skipFirst ? x => x >= index :  x => x > index);
        var p2 = lines.Reverse()
            .TakeWhile(x => x > index);
        var r = p1.Concat(p2)
            .FirstOrDefault();
        return r;
    }

    public class Builder
    {
        private List<string> blocks;

        public void Insert(int index, string block)
        {
            blocks.Insert(index, block);
        }
        public void AddBefore(string block) => Insert(0, block);
        public void AddLast(string block) => blocks.Add(block);

        public string Get()
        {
            string s = "";
            foreach (var b in blocks)
            {
                s += b + "|";
            }
            return s.TrimEnd('|');
        }

        public override string ToString() => Get();
    }
}