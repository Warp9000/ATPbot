using Newtonsoft.Json;

namespace ATPbot;

public struct Config
{
    [JsonProperty("token")]
    public string Token { get; set; }
}