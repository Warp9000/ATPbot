using System;
using System.IO;
using Newtonsoft.Json;

namespace ATPbot;

public static class DataManager
{
    public static T? Get<T>(object owner, string key, string folder = "")
    {
        folder = '/' + folder.TrimStart('/').TrimEnd('/') + '/';
        string path = $"data/{owner.GetType().Name}{folder}{key}.json";
        if (!File.Exists(path))
            return default;
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static T? GetDirect<T>(string path)
    {
        if (!File.Exists(path))
            return default;
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static void Set<T>(object owner, string key, T value, string folder = "")
    {
        folder = '/' + folder.TrimStart('/').TrimEnd('/') + '/';
        string path = $"data/{owner.GetType().Name}{folder}";
        Directory.CreateDirectory(path);
        var json = JsonConvert.SerializeObject(value);
        File.WriteAllText($"{path}/{key}.json", json);
    }

    public static string[] GetFiles(object owner, string folder = "")
    {
        folder = '/' + folder.TrimStart('/').TrimEnd('/') + '/';
        string path = $"data/{owner.GetType().Name}{folder}";
        if (!Directory.Exists(path))
            return Array.Empty<string>();
        return Directory.GetFiles(path);
    }
}
