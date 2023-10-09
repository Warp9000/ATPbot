using System;
using System.Text.Json.Serialization;

namespace ATPbot.Users;

public class UserStats
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    [JsonIgnore]
    public int TotalGames => Wins + Losses + Ties;
    [JsonIgnore]
    public double WinLossRatio => Math.Round((double)Wins / (Losses == 0 ? 1 : Losses), 2);
}
