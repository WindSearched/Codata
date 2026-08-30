using System.Text.Json.Nodes;
using System.Text.Json;

namespace Codata.scripts.classes;

public class JsonBuilder
{
    public string path;
    public JsonObject root;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public JsonBuilder(string path)
    {
        this.path = path;
        root = File.Exists(path) ? JsonNode.Parse(File.ReadAllText(path))!.AsObject() : new JsonObject();
    }

    public void Save()
    {
        File.WriteAllText(path, root.ToString());
    }

    public T? Get<T>(string path)
    {
        var node = GetNode(path);

        if (node is null)
            return default;

        return node.Deserialize<T>(JsonOptions);
    }

    public T Get<T>(string path, T defaultValue)
    {
        return Get<T>(path) ?? defaultValue;
    }

    public void Set<T>(string path, T value)
    {
        var parts = path.Split('.');

        JsonObject current = root;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];

            if (current[part] is not JsonObject child)
            {
                child = new JsonObject();
                current[part] = child;
            }

            current = child;
        }

        current[parts[^1]] = JsonSerializer.SerializeToNode(
            value,
            JsonOptions
        );
    }

    public bool Exists(string path)
    {
        return GetNode(path) is not null;
    }

    public JsonNode? GetNode(string path)
    {
        JsonNode? current = root;

        foreach (var part in path.Split('.'))
        {
            if (current is JsonObject obj)
            {
                current = obj[part];
            }
            else
            {
                return null;
            }
        }

        return current;
    }
}