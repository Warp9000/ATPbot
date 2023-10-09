using System;
using System.Text.Json.Serialization;

namespace ATPbot.Users;

public class User
{
    public ulong DiscordId { get; private set; }
    public int QuaverId { get; private set; }

    public User(ulong discordId, int quaverId)
    {
        DiscordId = discordId;
        QuaverId = quaverId;
    }

    /// <summary>
    ///    Checks if two users are equal by IDs, not wins/losses
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is User user)
        {
            return user.DiscordId == DiscordId && user.QuaverId == QuaverId;
        }

        return false;
    }

    /// <summary>
    ///   Gets the hashcode of the user by Discord and Quaver IDs
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(DiscordId, QuaverId);
    }
}
