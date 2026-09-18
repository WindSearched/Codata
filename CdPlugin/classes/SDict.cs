using System.Text;

namespace Codata.scripts.classes;
public class SDict<TKey, TVal>
{
    public int valueCount;

    public Dictionary<TKey, Dictionary<TKey, TVal>> dict = new();

    /// <summary>
    /// factory used when auto create value
    /// </summary>
    public Func<TVal> ValueFactory { get; set; }

    public SDict(Func<TVal> valueFactory = null)
    {
        ValueFactory = valueFactory;
    }

    /// <summary>
    /// Set val in dict
    /// </summary>
    public void Set(
        TKey key,
        TKey tag,
        TVal value,
        bool overwrite = true)
    {
        if (!dict.TryGetValue(key, out var line))
        {
            line = new Dictionary<TKey, TVal>();

            line.Add(tag, value);

            dict.Add(key, line);
        }
        else
        {
            if (!line.TryAdd(tag, value))
            {
                if (overwrite)
                    line[tag] = value;
            }
        }

        valueCount++;
    }

    public TVal Get(TKey key, TKey tag)
    {
        if (dict.TryGetValue(key, out var line) &&
            line.TryGetValue(tag, out var value))
        {
            return value;
        }

        return default;
    }

    /// <summary>
    /// if not present create one automatically
    /// </summary>
    public TVal GetAbs(TKey key, TKey tag, Func<TVal> createFunc)
    {
        Set(key, tag, createFunc.Invoke(), false);
        return Get(key, tag);
    }

    public TVal Get(TKey key)
    {
        if (dict.TryGetValue(key, out var line) &&
            line.Count > 0)
        {
            return line.First().Value;
        }

        return default;
    }

    public TVal Get(int index)
    {
        return Get(GetIndexKey(index));
    }

    public List<TVal> Gets(TKey tag)
    {
        var list = new List<TVal>();

        foreach (var line in dict.Values)
        {
            if (line.TryGetValue(tag, out var value))
            {
                list.Add(value);
            }
        }

        return list;
    }

    public int GetKeyIndex(TKey key)
    {
        return dict.Keys.ToList().IndexOf(key);
    }

    public TKey GetIndexKey(int index)
    {
        return dict.Keys.ToList()[index];
    }

    public bool ExistsLocation(TKey key, TKey tag)
    {
        return dict.TryGetValue(key, out var line)
               && line.ContainsKey(tag);
    }

    public bool TryGet(
        TKey key,
        TKey tag,
        out TVal value)
    {
        if (dict.TryGetValue(key, out var line) &&
            line.TryGetValue(tag, out value))
        {
            return true;
        }

        value = default;

        return false;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("{");
        foreach (var key in dict.Keys)
        {
            var line = dict[key];
            sb.Append(key);
            sb.Append(":{");
            foreach (var tag in line.Keys)
            {
                var val = line[tag];
                sb.Append(tag);
                sb.Append(":{");

                sb.Append(val);

                sb.Append("},");
            }

            if (sb[^1] == ',')
                sb.Length--;//remove comma
            sb.Append("},");
        }

        if (sb[^1] == ',')
            sb.Length--;//remove comma
        sb.Append("}");
        return sb.ToString();
    }
}