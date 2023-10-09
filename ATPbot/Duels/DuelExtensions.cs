using System;
using Newtonsoft.Json;

namespace ATPbot.Duels;

public static class DuelExtensions
{
    public static bool HasEnded(this Duel duel)
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

    public static bool HasExpired(this Duel duel, TimeSpan expireTime)
    {
        if (!duel.Accepted)
        {
            return duel.CreatedAt + expireTime < DateTime.UtcNow;
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
        duel.MapId = mapIds[Random.Shared.Next(0, mapIds.Length)];
    }

    public static bool CanReroll(this Duel duel)
    {
        return duel.RerollCount < duel.MaxRerolls;
    }
}
