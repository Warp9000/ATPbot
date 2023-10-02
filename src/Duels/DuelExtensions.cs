using System;
using Newtonsoft.Json;

namespace ATPbot.Duels;

public static class DuelExtensions
{
    public static bool IsExpired(this Duel duel)
    {
        if (duel.Accepted)
        {
            return duel.EndAt != null && duel.EndAt.Value < DateTime.UtcNow;
        }
        else
        {
            return false;
        }
    }

    public static TimeSpan? TimeLeft(this Duel duel)
    {
        if (duel.Accepted && duel.EndAt != null)
        {
            return duel.EndAt.Value - DateTime.UtcNow;
        }
        else
        {
            return null;
        }
    }

    public static void SetRandomMap(this Duel duel, int[] mapIds, QuaverWebApi.Wrapper quaverWebApi)
    {
        var mapsetId = mapIds[Random.Shared.Next(0, mapIds.Length)];
        var mapset = quaverWebApi.Endpoints.GetMapset(mapsetId).Result;
        var map = mapset.Maps[Random.Shared.Next(0, mapset.Maps.Length)];
        duel.MapId = map.Id;
    }
}
