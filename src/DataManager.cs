using System.IO;
using Newtonsoft.Json;

namespace ATPbot
{
    public static class DataManager
    {
        public static T? Get<T>(object owner, string key)
        {
            string path = $"data/{owner.GetType().Name}/{key}.json";
            if (!File.Exists(path))
                return default;
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void Set<T>(object owner, string key, T value)
        {
            string path = $"data/{owner.GetType().Name}";
            Directory.CreateDirectory(path);
            var json = JsonConvert.SerializeObject(value);
            File.WriteAllText($"{path}/{key}.json", json);
        }
    }
}